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
using System.Net;
using Unity.Netcode;

public static class Matchmaking
{

    //must be constants, static classes cannot have non-constant instance vars
    private const int heartbeat = 15;
    private const int lobbyRefreshRate = 2;

    private static UnityTransport transport;

    private static Lobby currentLobby;

    private static DiscoveryResponseData currentResonseData;

    public static Lobby GetCurrentLobby() 
    {
        return currentLobby;
    }

    private static CancellationTokenSource heartbeatSource, updateLobbySource;

    public static event Action<Lobby> event_CurrentLobbyRefreshed;

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

    //Lobby Query Process
    public static async Task<List<Lobby>> GetGlobalLobbies()
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
    public static async Task CreateGlobalLobby(LobbyData data)
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

    public static void CreateLocalLobby(LobbyData data)
    {
        currentResonseData = new DiscoveryResponseData
        {
            port = ((UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport).ConnectionData.Port,
            lobbyName = data.name,
            currentPlayerCount = 1,
            maxPlayers = data.maxPlayers,
            hasRestrictions = data.hasRestrictions,
            hasPassword = data.hasPassword,
            password = data.password
        };
        NetworkManager.Singleton.StartHost();
    }

    public static async Task JoinGlobalLobby(string lobbyId)
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

    public static void JoinLocalLobby(IPAddress ip, ushort port)
    {
        Transport.SetConnectionData(ip.ToString(), port);
        NetworkManager.Singleton.StartClient();
    }

    //prevent players from entering the current lobby
    public static async Task LockGlobalLobby()
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
    
    //prevent players from entering the current lobby
    public static async Task LockLocalLobby()
    {
        //discovery.StopDiscovery();
    }

    public static async Task UnlockLocalLobby()
    {
        //discovery.StartServer();
    }

    public static async Task UnlockGlobalLobby()
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
            event_CurrentLobbyRefreshed?.Invoke(currentLobby);
            await Task.Delay(lobbyRefreshRate * 1000);
        }
    }

    public static async Task LeaveLobby()
    {
        heartbeatSource?.Cancel();
        updateLobbySource?.Cancel();

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
