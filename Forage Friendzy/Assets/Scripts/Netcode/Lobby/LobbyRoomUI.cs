using System;
using System.Collections;
using System.Collections.Generic;
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

    public Lobby Lobby { get; private set; }

    public static event Action<Lobby> LobbySelected;

    public void Init(Lobby lobby)
    {
        UpdateDetails(lobby);
    }

    public void UpdateDetails(Lobby lobby)
    {
        Lobby = lobby;
        nameText.text = lobby.Name;
        playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        restrictionImage.gameObject.SetActive(Convert.ToBoolean(lobby.Data["r"].Value));
        passwordLockImage.gameObject.SetActive(Convert.ToBoolean(lobby.Data["l"].Value));
        int GetValue(string key)
        {
            return int.Parse(lobby.Data[key].Value);
        }
            
    }



    public void Clicked()
    {
        LobbySelected?.Invoke(Lobby);
    }

}
