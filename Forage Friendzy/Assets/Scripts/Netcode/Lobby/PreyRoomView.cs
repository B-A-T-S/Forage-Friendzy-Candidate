using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PreyRoomView : MonoBehaviour
{

    [SerializeField] private RoomPlayerPanel preyPanel;
    [SerializeField] private Transform panelParent;

    private readonly List<RoomPlayerPanel> playerPanels = new();

    public static event Action LobbyLeft;

    private void OnEnable()
    {
        //Clear any lingering room panels
        foreach (Transform child in panelParent)
            Destroy(child.gameObject);

        //Clear list of current panels
        playerPanels.Clear();

        //subscribe to events
        LobbyManager.LobbyPlayersUpdated += NetworkLobbyPlayersUpdated;
        Matchmaking.CurrentLobbyRefreshed += OnCurrentLobbyRefreshed;
        Matchmaking.FoundSiblingQueue += MigrateToSiblingLobby;
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
        var toDestroy = playerPanels.Where(p => !allActivePlayerIds.Contains(p.PlayerId)).ToList();
        foreach (var panel in toDestroy)
        {
            playerPanels.Remove(panel);
            Destroy(panel.gameObject);
        }

        //Create new panels / edit existing ones

        List<ulong> keys = new List<ulong>(players.Keys);
        foreach (ulong key in keys)
        {
            var currentPanel = playerPanels.FirstOrDefault(p => p.PlayerId == key);
            if (currentPanel != null)
            {
                //edit
                currentPanel.SetReady(players[key].isReady);
                currentPanel.SetRole(players[key].roleIndex);
                currentPanel.SetCharacter(players[key].characterIndex);
            }
            else
            {
                //create
                var panel = Instantiate(preyPanel, panelParent);
                panel.Init(key, NetworkManager.Singleton.LocalClientId);
                playerPanels.Add(panel);
            }
        }
    }

    private async void MigrateToSiblingLobby(Lobby sibling, CancellationTokenSource tokenSource)
    {
        try
        {
            await Matchmaking.LeaveLobby();
        }
        catch(Exception e)
        {
            CanvasUtil.Instance.ShowError("Failed Lobby Migration");
            Debug.LogError(e);
        }
    }

    public void OnLeaveLobby()
    {
        LobbyLeft?.Invoke();
    }

    private void OnCurrentLobbyRefreshed(Lobby lobby)
    {
        //waitingText.text = $"Waiting on players... {lobby.Players.Count}/{lobby.MaxPlayers}";
    }

}
