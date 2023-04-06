using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

public class LobbyManager : NetworkBehaviour
{

    public static LobbyManager Instance { get; private set; }

    //populate this with any players currently in the lobby
    private Dictionary<ulong, PlayerInfo> playersInLobby = new Dictionary<ulong, PlayerInfo>();
    public Dictionary<ulong, PlayerInfo> PlayersInLobby
    {
        get { return playersInLobby; }
    }



    //meant to be invoked whenever a player joins and we need to update the lobby
    public static event Action<Dictionary<ulong, PlayerInfo>> event_LobbyPlayersUpdated;
    public static event Action<bool> event_OnClientDisconnect;
    private float nextLobbyUpdate;
    private bool hasRanStart = false;

    private async void OnApplicationQuit()
    {
        playersInLobby.Clear();
        await Matchmaking.LeaveLobby();
    }

    void Start()
    {
        if (hasRanStart)
            return;

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        hasRanStart = true;
    }

    public async void JoinLobby(Lobby lobby, GameObject toEnable, GameObject toDisable)
    {
        try
        {
            using(new Load("Joining Lobby..."))
            {
                await Matchmaking.JoinLobby(lobby.Id);

                //enable/disable related screen
                toEnable.gameObject.SetActive(true);
                toDisable.gameObject.SetActive(false);

                NetworkManager.Singleton.StartClient();
            }
                
        }
        catch (Exception e)
        {
            CanvasUtil.Instance.ShowError("Failed to Join Lobby");
            Debug.LogError(e);
        }
    }

    public async void JoinLobby(string lobbyId, GameObject toEnable, GameObject toDisable)
    {
        try
        {
            await Matchmaking.JoinLobby(lobbyId);

            //enable/disable related screen
            toEnable.gameObject.SetActive(true);
            toDisable.gameObject.SetActive(false);

            NetworkManager.Singleton.StartClient();

        }
        catch (Exception e)
        {
            CanvasUtil.Instance.ShowError("Failed to Join Lobby");
            Debug.LogError(e);
        }
    }

    private async void ForceJoin(string lobbyId)
    {
        Debug.Log($"Forced to Join: {lobbyId}");
        /*
        await Matchmaking.LeaveLobby();
        NetworkManager.Singleton.Shutdown();
        await Matchmaking.JoinLobby(lobbyId);
        roomView.gameObject.SetActive(true);
        NetworkManager.Singleton.StartClient();
        */

    }

    [ClientRpc]
    public void ForceJoinClientRpc(string lobbyId)
    {
        ForceJoin(lobbyId);   
    }

    //Ran when user finishes inputing lobby data and wants to host
    public async void CreateLobby(LobbyData data, GameObject toEnable, GameObject toDisable)
    {

        try
        {
            using(new Load("Creating Lobby..."))
            {
                await Matchmaking.CreateFreeLobby(data);

                //enable/disable related screen
                toEnable?.gameObject.SetActive(true);
                toDisable?.gameObject.SetActive(false);

                //Starting host immediately will keep the relay server alive
                NetworkManager.Singleton.StartHost();
            }
        } 
        catch (Exception e)
        {
            CanvasUtil.Instance.ShowError("Failed to Create Lobby");
            Debug.LogError(e);
        }
    }

    //Network behaviour override
    public override void OnNetworkSpawn()
    {
        //ensure start is run at least once
        if (!hasRanStart)
            Start();

        //when spawned, is this object currently on the host?
        if (IsServer)
        {

            //if I am the server, listen for client connection events
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            //add myself to the list of local clients as not ready (because I am in the lobby)
            if(!playersInLobby.ContainsKey(NetworkManager.Singleton.LocalClientId))
                playersInLobby.Add(NetworkManager.Singleton.LocalClientId, new PlayerInfo(false));
            //EnqueueNameUpdateRequestServerRpc(NetworkManager.Singleton.LocalClientId, ClientLaunchInfo.Instance.playerName);
            
            //and update UI
            UpdateUI();
        }

        //regardless of who I am, start listening for disconnections
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

        //After a client networkSpawn, update playerName on server
        //if(IsClient)
            //EnqueueNameUpdateRequestServerRpc(NetworkManager.Singleton.LocalClientId, ClientLaunchInfo.Instance.playerName);
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
    }

    //called on client connection invokations
    //should never be run by non server users
    private void OnClientConnectedCallback(ulong playerId)
    {

        if (!IsServer) return;

        if (playerId == NetworkManager.Singleton.LocalClientId) return;

        //if (playersInLobby.ContainsKey(playerId)) return;

        //Add this client to the local list of lobby members
        //as not ready
        playersInLobby.Add(playerId, new PlayerInfo(false));

        //tell every other member that a new client has connected
        PropagateToClients();
        UpdateUI();

    }

    //iterates through list of current lobby players and sends update rpcs
    private void PropagateToClients()
    {
        foreach (var player in playersInLobby)
        {
            UpdatePlayerClientRpc(player.Key, player.Value);
        }
            
    }

    //sent from server to clients
    //informs client when to update their room view and player list
    [ClientRpc]
    private void UpdatePlayerClientRpc(ulong clientId, PlayerInfo playerInfo)
    {
        if (IsServer) return;

        //add new player to local list
        if (!playersInLobby.ContainsKey(clientId))
            playersInLobby.Add(clientId, playerInfo);
        //if they already exist, then update their status
        else
            playersInLobby[clientId] = playerInfo;
            
        UpdateUI();
    }

    private void OnClientDisconnectCallback(ulong playerId)
    {
        Lobby currentLobby = Matchmaking.GetCurrentLobby();
        bool hostDisconnect = (currentLobby != null && playerId == 0);

        if (IsServer)
        {
            // Handle locally
            if (playersInLobby.ContainsKey(playerId))
                playersInLobby.Remove(playerId);
                

            // Propagate all clients
            RemovePlayerClientRpc(playerId);
            UpdateUI();
        }
        else
        {
            //was the player who left the host?
            if (hostDisconnect)
            {
                if(GameManager.Instance.isMatch)
                    GameManager.Instance.ExitMatch();
                else
                    OnLobbyLeft(null, null);
            }
        }

        event_OnClientDisconnect?.Invoke(hostDisconnect);
    }

    //server to client
    //informs client to remove a given player from their local list of lobby members
    [ClientRpc]
    private void RemovePlayerClientRpc(ulong clientId)
    {
        if (IsServer) return;

        if (playersInLobby.ContainsKey(clientId)) 
            playersInLobby.Remove(clientId);

        UpdateUI();
    }

    public void OnReadyClicked()
    {
        SetReadyServerRpc(NetworkManager.Singleton.LocalClientId, true);
    }

    public void OnUnreadyClicked()
    {
        SetReadyServerRpc(NetworkManager.Singleton.LocalClientId, false);
    }

    public void OnRoleChanged(int value)
    {

        //the user has interacted with the role dropdown
        //update the launch info and notify the server

        ClientLaunchInfo.Instance.role = value;
        SetRoleServerRpc(NetworkManager.Singleton.LocalClientId, value);
    }

    public void OnCharacterChanged(int value)
    {
        ClientLaunchInfo.Instance.character = value;
        SetCharacterServerRpc(NetworkManager.Singleton.LocalClientId, value);
    }

    public void OnCosmeticChanged(int value)
    {
        ClientLaunchInfo.Instance.cosmetic = value;
        SetCosmeticServerRpc(NetworkManager.Singleton.LocalClientId, value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetCosmeticServerRpc(ulong playerId, int cosmeticIndex)
    {
        PlayerInfo copyOfInfo = playersInLobby[playerId];
        copyOfInfo.cosmeticIndex = cosmeticIndex;
        playersInLobby[playerId] = copyOfInfo;
        PropagateToClients();
        UpdateUI();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetRoleServerRpc(ulong playerId, int roleIndex)
    {
        PlayerInfo copyOfInfo = playersInLobby[playerId];
        copyOfInfo.roleIndex = roleIndex;
        playersInLobby[playerId] = copyOfInfo;
        PropagateToClients();
        UpdateUI();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetCharacterServerRpc(ulong playerId, int characterIndex)
    {
        PlayerInfo copyOfInfo = playersInLobby[playerId];
        copyOfInfo.characterIndex = characterIndex;
        playersInLobby[playerId] = copyOfInfo;
        PropagateToClients();
        UpdateUI();
    }

    //client to server
    //inform server that you are ready
    //and tell it to tell all other clients to update as well
    [ServerRpc(RequireOwnership = false)]
    private void SetReadyServerRpc(ulong playerId, bool isReady)
    {
        PlayerInfo copyOfInfo = playersInLobby[playerId];
        copyOfInfo.isReady = isReady;
        playersInLobby[playerId] = copyOfInfo;
        PropagateToClients();
        UpdateUI();
    }

    struct PlayerNameUpdateRequest
    {
        public ulong playerId;
        public string playerName;
        public bool status;

        public PlayerNameUpdateRequest(ulong id, string name)
        {
            playerId = id;
            playerName = name;
            status = false;
        }
    }

    Queue<PlayerNameUpdateRequest> nameUpdateQueue = new();
    Coroutine queueProcessor;
    float failedProcessWaitTime = 1f;

    IEnumerator ProcessNameQueueCoroutine()
    {
        while(nameUpdateQueue.Count > 0)
        {
            if (ProcessUpdateRequest(nameUpdateQueue.Peek()))
            {
                nameUpdateQueue.Dequeue();
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(failedProcessWaitTime);
            }
        }
        queueProcessor = null;
    }

    private bool ProcessUpdateRequest(PlayerNameUpdateRequest request)
    {

        ulong[] existingIds = playersInLobby.Keys.ToArray();
        if (existingIds.Contains<ulong>(request.playerId))
        {
            //player exists in dictionary
            SetPlayerName(request.playerId, request.playerName);
            return true;
        }

        return false;
    } 

    private void SetPlayerName(ulong id, string name)
    {

        KeyValuePair<ulong, PlayerInfo>[] copy = playersInLobby.ToArray();

        //returns the number of times a specific name turns up 
        // in a basic search
        int DoesNameExist(string inQuestion)
        {
            int i = 0;
            foreach (KeyValuePair<ulong, PlayerInfo> info in copy)
            {
                if (info.Key == id)
                    continue;

                if (info.Value.playerName == name)
                    i++;
            }

            return i;
        }

        //does given name already exist?
        // if so, add postfix
        int numDuplicates = DoesNameExist(name);
        if (numDuplicates > 0)
            name = $"{name} ({numDuplicates})";

        //edit information in dictionary
        PlayerInfo copyOfInfo = playersInLobby[id];
        copyOfInfo.playerName = name;
        playersInLobby[id] = copyOfInfo;
        PropagateToClients();
        UpdateUI();
    }

    //client to server
    //inform server of Client Name
    [ServerRpc(RequireOwnership = false)]
    private void EnqueueNameUpdateRequestServerRpc(ulong playerId, string playerName)
    {

        nameUpdateQueue.Enqueue(new PlayerNameUpdateRequest(playerId, playerName));
        if (queueProcessor == null)
            queueProcessor = StartCoroutine(ProcessNameQueueCoroutine());
    }

    [ClientRpc]
    public void ResetPlayerReadyStatusClientRpc()
    {

        KeyValuePair<ulong, PlayerInfo>[] copyOfDictionary = playersInLobby.ToArray();

        foreach (KeyValuePair<ulong, PlayerInfo> info in copyOfDictionary)
        {
            PlayerInfo p = info.Value;
            p.isReady = false;
            playersInLobby[info.Key] = p;
        }
    }


    //throws the lobby update event
    private void UpdateUI()
    {
        event_LobbyPlayersUpdated?.Invoke(playersInLobby);
    }

    public async void OnLobbyLeft(GameObject toEnable, GameObject toDisable)
    {
        using (new Load("Leaving Lobby..."))
        {
            playersInLobby.Clear();
            NetworkManager.Singleton.Shutdown();
            await Matchmaking.LeaveLobby();

            toEnable?.SetActive(true);
            toDisable?.SetActive(false);
        }
            
    }

    public async void OnGameStart()
    {
        
        using (new LoadNetworkScene("Starting Game...", NetworkManager.Singleton))
        {
            //locking the lobby means noone can join
            await Matchmaking.LockLobby();

            //fill in GM's information
            GameManager.Instance.numPlayersInMatch = Matchmaking.GetCurrentLobby().Players.Count;

            //tell client to show loading screen
            GameManager.Instance.NetworkSceneLoadingScreenClientRpc("Starting Game...");

            //load the game scene here
            LoadSceneUtil.Instance.NM_NextBuildIndex();
        }
        
    }

    public void OnExitClicked()
    {
        GameManager.Instance.CloseApplication();
    }

    public int GetConnectedClientCosmeticIndex(ulong clientId)
    {
        PlayerInfo copyOfInfo = playersInLobby[clientId];
        return copyOfInfo.cosmeticIndex;
    }

}

public struct LobbyData
{
    public string name;
    public int lobbyType;
    public bool hasRestrictions;
    public int maxPlayers;
    public bool hasPassword;
    public string password;
}

public enum LobbyType
{
    FreeLobby,
    PredOnly,
    PreyOnly
}