using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using System.Threading;
using System;

public static class Matchmaking
{

    //must be constants, static classes cannot have non-constant instance vars
    private const int heartbeat = 15;
    private const int lobbyRefreshRate = 2;
    private const int siblingSearchRate = 2;

    private static UnityTransport transport;

    private static Lobby currentLobby;

    public static Lobby GetCurrentLobby() 
    {
        return currentLobby;
    }

    //don't know alot about these, but they get used in the async heartbeat and update examples
    //so I included them
    //pretty sure they function as blocks for the processes
    private static CancellationTokenSource heartbeatSource, updateLobbySource, foundSiblingQueueSource;

    public static event Action<Lobby> CurrentLobbyRefreshed;
    public static event Action<Lobby, CancellationTokenSource> FoundSiblingQueue;

    //get/set for Unity Transport instance
    private static UnityTransport Transport
    {
        get
        {
            if (transport != null)
                return transport;
            else
                return UnityEngine.Object.FindObjectOfType<UnityTransport>();
        }

        set
        {
            transport = value;
        }
    }

    public static void Reset()
    {
        if (Transport != null)
        {
            Transport.Shutdown();
            Transport = null;
        }

        currentLobby = null;
    }

    public static async Task<List<Lobby>> GetOpenPreyLobbies()
    {
        //Setup Query Options
        QueryLobbiesOptions queryOptions = new QueryLobbiesOptions
        {
            //The maximum number of lobbies to return via this query
            Count = 15,
            Filters = new List<QueryFilter>
            {
                new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                new(QueryFilter.FieldOptions.IsLocked, "0", QueryFilter.OpOptions.EQ),
                new(QueryFilter.FieldOptions.N1, "2", QueryFilter.OpOptions.EQ)
            }
        };

        //request lobbies that fit defined conditions
        var validLobbies = await Lobbies.Instance.QueryLobbiesAsync(queryOptions);
        return validLobbies.Results;
    }

    public static async Task<List<Lobby>> GetFullPredatorLobbies()
    {
        //Setup Query Options
        QueryLobbiesOptions queryOptions = new QueryLobbiesOptions
        {
            //The maximum number of lobbies to return via this query
            Count = 15,
            Filters = new List<QueryFilter>
            {
                //filters for lobbies that have 3 available slots (2 Predators)
                new(QueryFilter.FieldOptions.AvailableSlots, "3", QueryFilter.OpOptions.EQ),
                new(QueryFilter.FieldOptions.IsLocked, "0", QueryFilter.OpOptions.EQ),
                new(QueryFilter.FieldOptions.N1, "1", QueryFilter.OpOptions.EQ)
            }

        };

        //request lobbies that fit defined conditions
        var validLobbies = await Lobbies.Instance.QueryLobbiesAsync(queryOptions);
        return validLobbies.Results;
    }

    public static async Task<List<Lobby>> GetOpenPredatorLobbies()
    {
        //Setup Query Options
        QueryLobbiesOptions queryOptions = new QueryLobbiesOptions
        {
            Count = 15,

            Filters = new List<QueryFilter>
            {
                new(QueryFilter.FieldOptions.AvailableSlots, "1", QueryFilter.OpOptions.EQ),
                new(QueryFilter.FieldOptions.IsLocked, "0", QueryFilter.OpOptions.EQ),
                new(QueryFilter.FieldOptions.N1, "1", QueryFilter.OpOptions.EQ)
            }

        };

        //request lobbies that fit defined conditions
        var validLobbies = await Lobbies.Instance.QueryLobbiesAsync(queryOptions);
        return validLobbies.Results;
    }

    //Lobby Query Process
    public static async Task<List<Lobby>> GetLobbies()
    {

        //Setup Query Options
        QueryLobbiesOptions queryOptions = new QueryLobbiesOptions
        {
            //The maximum number of lobbies to return via this query
            //Count = #,
            Count = 15,

            //a list of filters to apply to any lobbies found (applied to Lobby object class)
            //Filers =  new List<QueryFilter> {}
            //Ideally, we want to filter for;
            //Open Predator Slot
            //Open Prey Slot
            Filters = new List<QueryFilter>
            {
                //filters for lobbies that have more than 0 available slots
                //cuz yknow that would mean it was full
                new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                //filters for unlocked lobbies
                new(QueryFilter.FieldOptions.IsLocked, "0", QueryFilter.OpOptions.EQ),
                new(QueryFilter.FieldOptions.N1, "0", QueryFilter.OpOptions.EQ)
            }

        };

        //request lobbies that fit defined conditions
        var validLobbies = await Lobbies.Instance.QueryLobbiesAsync(queryOptions);
        return validLobbies.Results;
    }

    //Lobby Creation Process
    public static async Task CreateFreeLobby(LobbyData data)
    {
        //Create Relay Allocation
        Allocation relayAllocation = await RelayService.Instance.CreateAllocationAsync(data.maxPlayers, "northamerica-northeast1");
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(relayAllocation.AllocationId);

        //Create Lobby using Relay Allocation
        //include the JoinKey
        CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                { CustomLobbyData.JoinKey, new DataObject(DataObject.VisibilityOptions.Member, joinCode) },
                { CustomLobbyData.MatchRestricted, new DataObject(DataObject.VisibilityOptions.Public, data.hasRestrictions.ToString(), DataObject.IndexOptions.S1) },
                { CustomLobbyData.LobbyType, new DataObject(DataObject.VisibilityOptions.Public, ((int)data.lobbyType).ToString(), DataObject.IndexOptions.N1) },
                { CustomLobbyData.PasswordLocked, new DataObject(DataObject.VisibilityOptions.Public, (!string.IsNullOrEmpty(data.password)).ToString(), DataObject.IndexOptions.S2)},
                { CustomLobbyData.Password, new DataObject(DataObject.VisibilityOptions.Public, data.password, DataObject.IndexOptions.S3)}
            }
        };

        //Create Lobby Instance and Set this sessions currentLobby
        currentLobby = await LobbyService.Instance.CreateLobbyAsync(data.name, data.maxPlayers, lobbyOptions);

        //Set Transport's Host Data to the Relay
        RelayServer server = relayAllocation.RelayServer;
        Transport.SetHostRelayData(server.IpV4, (ushort) server.Port, relayAllocation.AllocationIdBytes, relayAllocation.Key, relayAllocation.ConnectionData);

        //Start heartbeating and refreshing the user's current lobby
        Heartbeat();
        PeriodicallyRefreshLobby();
    }

    public static async Task CreatePredatorOnlyLobby(LobbyData data)
    {
        Allocation relayAllocation = await RelayService.Instance.CreateAllocationAsync(data.maxPlayers, "northamerica-northeast1");
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(relayAllocation.AllocationId);

        CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                { CustomLobbyData.JoinKey, new DataObject(DataObject.VisibilityOptions.Member, joinCode) },
                { CustomLobbyData.MatchRestricted, new DataObject(DataObject.VisibilityOptions.Public, data.hasRestrictions.ToString(), DataObject.IndexOptions.S1) },
                { CustomLobbyData.LobbyType, new DataObject(DataObject.VisibilityOptions.Public, ((int)data.lobbyType).ToString(), DataObject.IndexOptions.N1) },
                { CustomLobbyData.PasswordLocked, new DataObject(DataObject.VisibilityOptions.Public, (!string.IsNullOrEmpty(data.password)).ToString(), DataObject.IndexOptions.S2)},
                { CustomLobbyData.Password, new DataObject(DataObject.VisibilityOptions.Public, data.password, DataObject.IndexOptions.S3)}
            }
        };

        currentLobby = await LobbyService.Instance.CreateLobbyAsync(data.name, data.maxPlayers, lobbyOptions);

        RelayServer server = relayAllocation.RelayServer;
        Transport.SetHostRelayData(server.IpV4, (ushort)server.Port, relayAllocation.AllocationIdBytes, relayAllocation.Key, relayAllocation.ConnectionData);

        Heartbeat();
        PeriodicallyRefreshLobby();
    }

    public static async Task CreatePreyOnlyLobby(LobbyData data)
    {
        Allocation relayAllocation = await RelayService.Instance.CreateAllocationAsync(data.maxPlayers, "northamerica-northeast1");
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(relayAllocation.AllocationId);

        CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                { CustomLobbyData.JoinKey, new DataObject(DataObject.VisibilityOptions.Member, joinCode) },
                { CustomLobbyData.MatchRestricted, new DataObject(DataObject.VisibilityOptions.Public, data.hasRestrictions.ToString(), DataObject.IndexOptions.S1) },
                { CustomLobbyData.LobbyType, new DataObject(DataObject.VisibilityOptions.Public, ((int)data.lobbyType).ToString(), DataObject.IndexOptions.N1) },
                { CustomLobbyData.PasswordLocked, new DataObject(DataObject.VisibilityOptions.Public, (!string.IsNullOrEmpty(data.password)).ToString(), DataObject.IndexOptions.S2)},
                { CustomLobbyData.Password, new DataObject(DataObject.VisibilityOptions.Public, data.password, DataObject.IndexOptions.S3)}
            }
        };

        currentLobby = await LobbyService.Instance.CreateLobbyAsync(data.name, data.maxPlayers, lobbyOptions);

        RelayServer server = relayAllocation.RelayServer;
        Transport.SetHostRelayData(server.IpV4, (ushort)server.Port, relayAllocation.AllocationIdBytes, relayAllocation.Key, relayAllocation.ConnectionData);

        Heartbeat();
        PeriodicallyRefreshLobby();
        //Start Looking for Predator Lobbies

    }


    public static async Task JoinLobby(string lobbyId)
    {
        //Join the lobby
        currentLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId);
        JoinAllocation relayAllocation = await RelayService.Instance.JoinAllocationAsync(currentLobby.Data[CustomLobbyData.JoinKey].Value);

        //set transports client information
        RelayServer server = relayAllocation.RelayServer;
        Transport.SetClientRelayData(server.IpV4, (ushort)server.Port, relayAllocation.AllocationIdBytes,
            relayAllocation.Key, relayAllocation.ConnectionData, relayAllocation.HostConnectionData);

        //refresh lobby
        PeriodicallyRefreshLobby();

    }

    //prevent players from entering the current lobby
    public static async Task LockLobby()
    {
        try
        {
            await Lobbies.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions { IsLocked = true });
        }
        catch (Exception e)
        {
            Debug.Log($"Failed closing lobby: {e}");
        }
    }

    public static async Task UnlockLobby()
    {
        try
        {
            await Lobbies.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions { IsLocked = false });
        }
        catch (Exception e)
        {
            Debug.Log($"Failed opening lobby: {e}");
        }
    }

    //async heartbeat process from example
    //users who create lobbies start running this process to keep the lobby active
    private static async void Heartbeat()
    {
        heartbeatSource = new CancellationTokenSource();
        //while heartbeat is not canceled and this user (host) is in a lobby
        while (!heartbeatSource.IsCancellationRequested && currentLobby != null)
        {
            await Lobbies.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            await Task.Delay(heartbeat * 1000);
        }
    }

    //async refresh process from example
    //all users in a lobby need to refresh it
    private static async void PeriodicallyRefreshLobby()
    {
        updateLobbySource = new CancellationTokenSource();
        await Task.Delay(lobbyRefreshRate * 1000);
        //while lobby update is not canceled and this user is in a lobby
        while (!updateLobbySource.IsCancellationRequested && currentLobby != null)
        {
            currentLobby = await Lobbies.Instance.GetLobbyAsync(currentLobby.Id);
            CurrentLobbyRefreshed?.Invoke(currentLobby);
            await Task.Delay(lobbyRefreshRate * 1000);
        }
    }

    //async periodic check for lobby during role queue
    //perform query for 
    private static async void SearchForSiblingLobby()
    {
        foundSiblingQueueSource = new CancellationTokenSource();
        await Task.Delay(siblingSearchRate * 1000);

        while(!foundSiblingQueueSource.IsCancellationRequested && currentLobby != null)
        {
            List<Lobby> foundLobbies = await GetFullPredatorLobbies();
            if (foundLobbies.Count >= 1)
            {
                FoundSiblingQueue?.Invoke(foundLobbies[0], foundSiblingQueueSource);
            }
                
            await Task.Delay(siblingSearchRate * 1000);
        }
    }

    public static async Task LeaveLobby()
    {
        heartbeatSource?.Cancel();
        updateLobbySource?.Cancel();
        foundSiblingQueueSource?.Cancel();

        if (currentLobby != null)
            try
            {
                //when leaving, if I was host, delete lobby
                if (currentLobby.HostId == Authentication.PlayerId) 
                    await Lobbies.Instance.DeleteLobbyAsync(currentLobby.Id);
                //if I was a client, then remove myself from the lobby
                else 
                    await Lobbies.Instance.RemovePlayerAsync(currentLobby.Id, Authentication.PlayerId);
                //my current lobby is now empty
                currentLobby = null;
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
    }

}
