using System;
using System.Collections;
using System.Net;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
//using UnityEditor.Build.Content;
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
    private IPAddress cachedIP;
    private DiscoveryResponseData cachedResponseData;

    #region Password

    public void OpenPasswordWindow(Lobby lobby, string password)
    {
        cachedLobbyPassword = password;
        cachedPasswordLobby = lobby;
        passwordInput.text = "";
        incorrectPasswordTLO.SetActive(false);
        passwordWindow.SetActive(true);
    }

    public void OpenPasswordWindow(IPAddress ip, DiscoveryResponseData responseData, string password)
    {
        cachedLobbyPassword = password;
        cachedIP = ip;
        cachedResponseData = responseData;
        passwordInput.text = "";
        incorrectPasswordTLO.SetActive(false);
        passwordWindow.SetActive(true);
    }

    public void ClosePasswordWindow()
    {
        cachedLobbyPassword = null;
        cachedPasswordLobby = null;
        cachedIP = null;
        passwordWindow.SetActive(false);
    }

    public void CheckPassword()
    {
        if (string.Equals(passwordInput.text, cachedLobbyPassword))
        {
            //If Correct
            if(cachedPasswordLobby != null)
            {
                LobbyManager.Instance.JoinLobby(cachedPasswordLobby, roomView.gameObject, lobbyViewer.gameObject);
            }
            else if (cachedIP != null)
            {
                LobbyManager.Instance.JoinLobby(cachedIP, cachedResponseData.port, roomView.gameObject, lobbyViewer.gameObject);
            }
            
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

        LobbyViewer.event_OnExitClicked += ExitLobbyViewer;
        LobbyViewer.event_OnHostClicked += Transition_ViewerToCreator;

        LobbyCreator.event_OnCreateClicked += CreateLobby;
        LobbyCreator.event_OnExitClicked += Transition_CreatorToViewer;

        LobbyRoomUI.event_GlobalLobbySelected += TryJoinGlobalLobby;
        LobbyRoomUI.event_LocalLobbySelected += TryJoinLocalLobby;

        RoomView.event_LobbyLeft += OnLobbyLeft;
        RoomView.event_StartGamePressed += OnGameStart;

        LobbyManager.event_OnClientDisconnect += LobbyManager_OnClientDisconnect;
    }

    private void ExitLobbyViewer()
    {
        using (new LoadScene("Loading..."))
        {
            LoadSceneUtil.Instance.PreviousBuildIndex();
        }
    }

    private void Transition_ViewerToCreator()
    {
        Transition(lobbyViewer.gameObject, lobbyCreator.gameObject);
    }

    private void Transition_CreatorToViewer()
    {
        Transition(lobbyCreator.gameObject, lobbyViewer.gameObject);
    }

    private void Transition(GameObject prevScreen, GameObject newScreen)
    {
        using (new Load("Loading..."))
        {
            newScreen.SetActive(true);
            prevScreen.SetActive(false);
        }
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

    private void TryJoinGlobalLobby(Lobby lobby)
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

    private void TryJoinLocalLobby(IPAddress ip, DiscoveryResponseData responseData)
    {
        if (responseData.hasPassword)
        {
            OpenPasswordWindow(ip, responseData, responseData.password);
        }
        else
        {
            LobbyManager.Instance.JoinLobby(ip, responseData.port, roomView.gameObject, lobbyViewer.gameObject);
        }
    }

    public void OnLobbyLeft()
    {
        LobbyManager.Instance.OnLobbyLeft(lobbyViewer.gameObject, roomView.gameObject);
    }

    public void OnLobbyCreatorLeft()
    {
        lobbyViewer.gameObject.SetActive(true);
        lobbyCreator.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        LobbyViewer.event_OnExitClicked -= ExitLobbyViewer;
        LobbyViewer.event_OnHostClicked -= Transition_ViewerToCreator;

        LobbyCreator.event_OnCreateClicked -= CreateLobby;
        LobbyCreator.event_OnExitClicked -= Transition_CreatorToViewer;

        LobbyRoomUI.event_GlobalLobbySelected -= TryJoinGlobalLobby;
        LobbyRoomUI.event_LocalLobbySelected -= TryJoinLocalLobby;

        RoomView.event_LobbyLeft -= OnLobbyLeft;
        RoomView.event_StartGamePressed -= OnGameStart;

        LobbyManager.event_OnClientDisconnect -= LobbyManager_OnClientDisconnect;
    }
}