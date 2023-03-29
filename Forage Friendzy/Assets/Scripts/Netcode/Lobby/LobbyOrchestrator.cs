using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyOrchestrator : MonoBehaviour
{


    [SerializeField] private LobbyViewer lobbyViewer;

    [SerializeField] private LobbyCreator lobbyCreator;

    [SerializeField] private RoomView roomView;

    [Header("Password Lobbies")]
    private string cachedLobbyPassword;
    [SerializeField] GameObject passwordWindow;
    [SerializeField] TMP_InputField passwordInput;
    [SerializeField] GameObject incorrectPasswordTLO;
    private Lobby cachedPasswordLobby;


    #region Password

    public void OpenPasswordWindow(Lobby lobby, string password)
    {
        cachedLobbyPassword = password;
        cachedPasswordLobby = lobby;
        passwordInput.text = "";
        incorrectPasswordTLO.SetActive(false);
        passwordWindow.SetActive(true);
    }

    public void ClosePasswordWindow()
    {
        cachedLobbyPassword = null;
        cachedPasswordLobby = null;
        passwordWindow.SetActive(false);
    }

    public void CheckPassword()
    {
        if (string.Equals(passwordInput.text, cachedLobbyPassword))
        {
            //If Correct
            LobbyManager.Instance.JoinLobby(cachedPasswordLobby, roomView.gameObject, lobbyViewer.gameObject);
            ClosePasswordWindow();
        }
        else
        {
            //If Incorrect
            incorrectPasswordTLO.SetActive(true);
        }
    }

    #endregion

    // Use this for initialization
    void Start()
    {
        //ensure proper screen is visible
        if (Matchmaking.GetCurrentLobby() != null)
        {
            using (new Load("Loading Lobby..."))
            {
                //Lobby Scene loaded while in lobby -> coming from game scene
                lobbyViewer.gameObject.SetActive(false);
                lobbyCreator.gameObject.SetActive(false);
                roomView.gameObject.SetActive(true);
                roomView.NetworkLobbyPlayersUpdated(LobbyManager.Instance.PlayersInLobby);
            } 
        }
        else
        {
            lobbyViewer.gameObject.SetActive(true);
            lobbyCreator.gameObject.SetActive(false);
            roomView.gameObject.SetActive(false);
        }

        //subscribe to events
        LobbyCreator.LobbyCreated += CreateLobby;
        LobbyRoomUI.LobbySelected += OnLobbySelected;
        RoomView.LobbyLeft += OnLobbyLeft;
        RoomView.StartGamePressed += OnGameStart;
        LobbyManager.OnClientDisconnect += LobbyManager_OnClientDisconnect;
    }

    public void OnReadyClicked()
    {
        LobbyManager.Instance.OnReadyClicked();
    }

    public void OnUnreadyClicked()
    {
        LobbyManager.Instance.OnUnreadyClicked();
    }

    public void OnRoleChanged(int value)
    {
        LobbyManager.Instance.OnRoleChanged(value);
    }

    public void OnCharacterChanged(int value)
    {
        LobbyManager.Instance.OnCharacterChanged(value);
    }

    private void LobbyManager_OnClientDisconnect(bool hostDisconnect)
    {
        if(hostDisconnect)
        {
            roomView.gameObject.SetActive(false);
            lobbyViewer.gameObject.SetActive(true); 
        }
    }

    private void OnGameStart()
    {
        LobbyManager.Instance.OnGameStart();
    }

    private void CreateLobby(LobbyData data)
    {
        LobbyManager.Instance.CreateLobby(data, roomView.gameObject, lobbyCreator.gameObject);
    }

    private void OnLobbySelected(Lobby lobby)
    {
        if (Convert.ToBoolean(lobby.Data["l"].Value))
        {
            OpenPasswordWindow(lobby, lobby.Data["p"].Value);
        }
        else
        {
            LobbyManager.Instance.JoinLobby(lobby, roomView.gameObject, lobbyViewer.gameObject);
        }
    }

    public void OnLobbyLeft()
    {
        LobbyManager.Instance.OnLobbyLeft(lobbyViewer.gameObject, roomView.gameObject);
    }

    public void OnExitClicked()
    {
        GameManager.Instance.CloseApplication();
    }

    private void OnDestroy()
    {
        LobbyCreator.LobbyCreated -= CreateLobby;
        LobbyRoomUI.LobbySelected -= OnLobbySelected;
        RoomView.LobbyLeft -= OnLobbyLeft;
        RoomView.StartGamePressed -= OnGameStart;
        LobbyManager.OnClientDisconnect -= LobbyManager_OnClientDisconnect;
    }
}