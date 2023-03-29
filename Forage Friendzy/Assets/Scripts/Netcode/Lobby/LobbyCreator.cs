using System.Collections;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEditor;


//Handles the pre-room creation input
//Match params go here
public class LobbyCreator : MonoBehaviour
{

    [SerializeField]
    private TMP_InputField nameInput;

    [SerializeField]
    private TMP_Dropdown lobbySizeDropdown;

    [SerializeField]
    private Toggle hasRestrictionsToggle;

    [SerializeField]
    private GameObject passwordParent;

    [SerializeField]
    private Toggle hasPasswordToggle;

    [SerializeField]
    private TMP_InputField passwordInput;

    [SerializeField]
    private Button createLobbyButton;

    public static event Action<LobbyData> LobbyCreated;

    private void OnEnable()
    {
        nameInput.text = "";
        lobbySizeDropdown.value = 2;
#if UNITY_STANDALONE || UNITY_STANDALONE_WIN
        hasRestrictionsToggle.isOn = true;
#endif
#if UNITY_EDITOR_WIN || UNITY_EDITOR
        hasRestrictionsToggle.isOn = false;
#endif
        hasPasswordToggle.isOn = false;
        OnPasswordToggled(false);
        createLobbyButton.interactable = true;
    }

    public void OnPasswordChanged()
    { 
        createLobbyButton.interactable = !hasPasswordToggle.isOn || !(passwordInput.text.Length <= 0);
    }

    public void OnPasswordToggled(bool isOn)
    {
        passwordParent.SetActive(isOn);
        OnPasswordChanged();
    }

    public void CreatePredatorLobby()
    {
        LobbyData lobbyData = new LobbyData
        {
            name = "PredatorLobby",
            lobbyType = (int)LobbyType.PredOnly,
            maxPlayers = 5,
            hasRestrictions = false,
            hasPassword = false,
            password = ""
        };
    }

    public void CreatePreyLobby()
    {
        LobbyData lobbyData = new LobbyData
        {
            name = "PreyLobby",
            lobbyType = (int)LobbyType.PreyOnly,
            maxPlayers = 3,
            hasRestrictions = false,
            hasPassword = false,
            password = ""
        };
    }

    public void OnCreateClicked()
    {
        LobbyData lobbyData = new LobbyData
        {
            name = nameInput.text == "" ? "Lobby" : nameInput.text,
            lobbyType = (int)LobbyType.FreeLobby,
            maxPlayers = lobbySizeDropdown == null ? 5 : Convert.ToInt32(lobbySizeDropdown.options[lobbySizeDropdown.value].text),
            hasRestrictions = hasRestrictionsToggle == null ? false : hasRestrictionsToggle.isOn,
            hasPassword = hasPasswordToggle.isOn,
            password = hasPasswordToggle.isOn ? passwordInput.text : ""
        };

        LobbyCreated?.Invoke(lobbyData);
    }
}