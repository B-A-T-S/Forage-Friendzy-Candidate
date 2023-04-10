using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyRoomUI : MonoBehaviour
{

    [SerializeField]
    private TMP_Text nameText, playerCountText;

    [SerializeField]
    private Image restrictionImage, passwordLockImage;

    public bool IsLocal { get; private set; }

    public Lobby Lobby { get; private set; }
    public IPAddress IP { get; private set; }
    public DiscoveryResponseData ResponseData { get; private set; }

    public static event Action<Lobby> event_GlobalLobbySelected;
    public static event Action<IPAddress, DiscoveryResponseData> event_LocalLobbySelected;

    
    public void Init(KeyValuePair<IPAddress, DiscoveryResponseData> lobby)
    {
        UpdateDetails(lobby);
        IsLocal = true;
    }
    

    public void Init(Lobby lobby)
    {
        UpdateDetails(lobby);
        IsLocal = false;
    }

    public void UpdateDetails(KeyValuePair<IPAddress, DiscoveryResponseData> lobby)
    {
        IP = lobby.Key;
        ResponseData = lobby.Value;
        nameText.text = ResponseData.lobbyName;
        playerCountText.text = $"{ResponseData.currentPlayerCount}/{ResponseData.maxPlayers}";
        restrictionImage.gameObject.SetActive(ResponseData.hasRestrictions);
        passwordLockImage.gameObject.SetActive(ResponseData.hasPassword);
        
    }

    public void UpdateDetails(Lobby lobby)
    {
        Lobby = lobby;
        nameText.text = lobby.Name;
        playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        restrictionImage.gameObject.SetActive(Convert.ToBoolean(lobby.Data["r"].Value));
        passwordLockImage.gameObject.SetActive(Convert.ToBoolean(lobby.Data["l"].Value));
    }



    public void Clicked()
    {
        if (!IsLocal)
            event_GlobalLobbySelected?.Invoke(Lobby);
        else
            event_LocalLobbySelected?.Invoke(IP, ResponseData);
    }

}
