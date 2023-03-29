using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

//The view visible when a player has successfully entered a lobby
//Character Customization should occur here
public class RoomView : MonoBehaviour
{

    [SerializeField]
    private RoomPlayerPanel playerPanelPrefab;

    [SerializeField]
    private Transform roomPanelParent;

    [SerializeField] 
    private TMP_Text waitingText, restrictionMessage;

    [SerializeField]
    private GameObject startButton, readyButton, unreadyButton;

    private readonly List<RoomPlayerPanel> playerPanels = new();
    private bool allReady;
    private bool ready;

    //events
    public static event Action StartGamePressed;
    public static event Action LobbyLeft;

    //this screen gets turned off and on alot, so reset on enable
    private void OnEnable()
    {
        //Clear any lingering room panels
        foreach (Transform child in roomPanelParent) 
            Destroy(child.gameObject);

        //Clear list of current panels
        playerPanels.Clear();

        //subscribe to events
        LobbyManager.LobbyPlayersUpdated += NetworkLobbyPlayersUpdated;
        Matchmaking.CurrentLobbyRefreshed += OnCurrentLobbyRefreshed;

        //ui setup
        startButton?.SetActive(false);
        readyButton?.SetActive(false);
        unreadyButton?.SetActive(false);
        ready = false;
        if(restrictionMessage != null)
            restrictionMessage.text = "";

        NetworkLobbyPlayersUpdated(LobbyManager.Instance.PlayersInLobby);
    }

    private void OnDisable()
    {
        //when this is disabled, unsubscribe. pretty sure it would error trying to call methods on disabled objects
        LobbyManager.LobbyPlayersUpdated -= NetworkLobbyPlayersUpdated;
        Matchmaking.CurrentLobbyRefreshed -= OnCurrentLobbyRefreshed;
    }

    //handles the deletion and creation of RoomPanels
    //handles the updating of existing RoomPanels
    public void NetworkLobbyPlayersUpdated(Dictionary<ulong, PlayerInfo> players)
    {
        Dictionary<ulong, PlayerInfo>.KeyCollection allActivePlayerIds = players.Keys;

        // Remove all inactive panels
        List<RoomPlayerPanel> toDestroy = playerPanels.Where(p => !allActivePlayerIds.Contains(p.PlayerId)).ToList();
        foreach (var panel in toDestroy)
        {
            playerPanels.Remove(panel);
            Destroy(panel.gameObject);
        }

        //Create new panels / edit existing ones

        List<ulong> keys = new List<ulong>(players.Keys);
        foreach(ulong key in keys)
        {
            RoomPlayerPanel currentPanel = playerPanels.FirstOrDefault(p => p.PlayerId == key);

            if(currentPanel != null)
            {
                //edit
                currentPanel.SetReady(players[key].isReady);
                currentPanel.SetRole(players[key].roleIndex);
                currentPanel.SetCharacter(players[key].characterIndex);
            }
            else
            {
                //create
                RoomPlayerPanel panel = Instantiate(playerPanelPrefab, roomPanelParent);
                panel.Init(key, NetworkManager.Singleton.LocalClientId);
                playerPanels.Add(panel);
                
            }
        }

        bool validMatch = MeetsRestrictions(players);
        startButton.SetActive(NetworkManager.Singleton.IsHost && players.All(p => p.Value.isReady) && validMatch);
        readyButton.SetActive(!ready);
    }

    private bool MeetsRestrictions(Dictionary<ulong, PlayerInfo> players)
    {
        //if the lobby doesn't have restrictions enabled, just return true
        DataObject hasRestrictionsDO;
        Matchmaking.GetCurrentLobby().Data.TryGetValue("r", out hasRestrictionsDO);
        if(!Convert.ToBoolean(hasRestrictionsDO.Value))
            return true;

        //count nums of role in player list
        ushort[] indexedAnimalArr = new ushort[2];
        List<PlayerInfo> playerInfos = new List<PlayerInfo>(players.Values);
        bool meetsRestrictions = true;
        string restrictionErrorMessage = "";
        foreach(PlayerInfo info in playerInfos)
            indexedAnimalArr[info.roleIndex]++;

        //check against restrictions

        //There must be at least 1 Prey 
        if (indexedAnimalArr[0] <= 0)
        {
            //cool ui things here
            restrictionErrorMessage += "<sprite=0>There must be at least 1 Prey\r\n";
            meetsRestrictions = false;
        }

        //There must be at least 1 Predator
        if (indexedAnimalArr[1] <= 0)
        {
            //cool ui things here
            restrictionErrorMessage += "<sprite=0>There must be at least 1 Predator\r\n";
            meetsRestrictions = false;
        }

        //There should not be more Predator than Prey
        if(indexedAnimalArr[1] > indexedAnimalArr[0])
        {
            restrictionErrorMessage += "<sprite=0>Predators > Prey\r\n";
            meetsRestrictions = false;
        }

        restrictionMessage.text = restrictionErrorMessage;

        return meetsRestrictions;
    }


    public void OnLeaveLobby()
    {
        LobbyLeft?.Invoke();
    }

    private void OnCurrentLobbyRefreshed(Lobby lobby)
    {
        waitingText.text = $"Waiting on players... {lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    public void OnReadyClicked()
    {
        readyButton.SetActive(false);
        unreadyButton.SetActive(true);
        ready = true;

        playerPanels.Find(x => x.PlayerId == NetworkManager.Singleton.LocalClientId).SetInteractable(false);

    }

    public void OnUnreadyClicked()
    {
        readyButton.SetActive(true);
        unreadyButton.SetActive(false);
        ready = false;

        playerPanels.Find(x => x.PlayerId == NetworkManager.Singleton.LocalClientId).SetInteractable(true);
    }

    public void OnStartClicked()
    {
        StartGamePressed?.Invoke();
    }
}
