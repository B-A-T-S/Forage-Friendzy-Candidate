using TMPro;
using Unity.Netcode;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
struct RoleComponentGroup
{

    public GameObject roleText;
    public GameObject roleBackground;

    public FadingCanvasGroup radioParent;
    public List<Toggle> characterRadios;

    public void Toggle(bool on)
    {

        roleBackground.SetActive(on);
        roleText.SetActive(on);

        if (on)
            radioParent.FadeIn();
        else
            radioParent.FadeOut();

    }

    public void SelectElement_NoResponse(int index)
    {
        if (index >= characterRadios.Count)
            index = 0;

        characterRadios[index].SetIsOnWithoutNotify(true);

    }

    public void SetInteractable(bool interactable)
    {
        foreach (Toggle go in characterRadios)
            go.interactable = interactable;
    }



}

public class RoomPlayerPanel : MonoBehaviour
{

    [SerializeField] private RoleComponentGroup preyComponentGroup, predatorComponentGroup;

    [SerializeField] private Button roleSwapper;

    [SerializeField] private TMP_Text nameText, statusText;

    public ulong PlayerId { get; private set; }

    public int FakePlayerId { get; private set; }

    public PlayerInfo info;

    [SerializeField] private Image ownershipIndicator;
    [SerializeField] private Color preyOwner, predOwner;

    [SerializeField] private RawImage previewImage;

    private PreviewObject activePreviewObject;

    public void Init(ulong playerId, ulong localId, string playerName)
    {
        PlayerId = playerId;
        FakePlayerId =  GetFakePlayerId(playerId);
        SetName(playerName);

        if (playerId != localId)
        {
            //this object defines another client
            SetInteractable(false);

            ownershipIndicator.gameObject.SetActive(false);

        }
        else
        {
            switch (ClientLaunchInfo.Instance.role)
            {
                case 0:
                    preyComponentGroup.Toggle(true);
                    predatorComponentGroup.Toggle(false);

                    preyComponentGroup.SelectElement_NoResponse(ClientLaunchInfo.Instance.character);
                    break;
                case 1:
                    predatorComponentGroup.Toggle(true);
                    preyComponentGroup.Toggle(false);

                    predatorComponentGroup.SelectElement_NoResponse(ClientLaunchInfo.Instance.character);
                    break;
            }


            ownershipIndicator.color = ClientLaunchInfo.Instance.role == 0 ? preyOwner : predOwner;
        }

        activePreviewObject = PreviewManager.Instance.GetPreviewObject();
        activePreviewObject.Loan();
        previewImage.texture = activePreviewObject.renderTexture;

    }

    private int GetFakePlayerId(ulong playerId)
    {
        List<KeyValuePair<ulong, PlayerInfo>> playerList = LobbyManager.Instance.PlayersInLobby.ToList();
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].Key == playerId)
                return i;
        }

        return -1;
    }

    public void SetName(string name)
    {
        nameText.text = string.IsNullOrEmpty(name) ? $"Player {FakePlayerId}" : name;
    }

    public void SetInteractable(bool isInteractable)
    {
        roleSwapper.interactable = isInteractable;
        preyComponentGroup.SetInteractable(isInteractable);
        predatorComponentGroup.SetInteractable(isInteractable);
    }

    public void ChangeTeam()
    {
        int currentRole = ClientLaunchInfo.Instance.role;

        //Assume switch to Prey
        int newRoleIndex = 0;
        RoleComponentGroup previousGroup = predatorComponentGroup;
        RoleComponentGroup newGroup = preyComponentGroup;
        if (currentRole == 0)
        {
            newRoleIndex = 1;
            //Player wants to switch to predator
            previousGroup = preyComponentGroup;
            newGroup = predatorComponentGroup;
        }

        newGroup.Toggle(true);
        previousGroup.Toggle(false);

        ownershipIndicator.color = newRoleIndex == 0 ? preyOwner : predOwner;

        LobbyManager.Instance.OnRoleChanged(newRoleIndex);
    }

    public void SelectCharacter(int signifierID)
    {
        LobbyManager.Instance.OnCharacterChanged(signifierID);
        activePreviewObject.SwitchSubjectPreview(signifierID + (3 * ClientLaunchInfo.Instance.role));
        AdvanceCosmeticIndex(currentCosmeticIndex - currentCosmeticIndex);
    }

    public void SetRole(int roleIndex)
    {

        if (roleIndex == 0)
        {
            //Prey Toggle
            preyComponentGroup.Toggle(true);
            predatorComponentGroup.Toggle(false);
        }
        else
        {
            //Pred Toggle
            predatorComponentGroup.Toggle(true);
            preyComponentGroup.Toggle(false);
        }

        info.roleIndex = roleIndex;
    }

    public void SetCharacter(int characterIndex)
    {
        if (ClientLaunchInfo.Instance.role == 0)
        {
            preyComponentGroup.SelectElement_NoResponse(characterIndex);
        }
        else
        {
            predatorComponentGroup.SelectElement_NoResponse(characterIndex);
        }

        info.characterIndex = characterIndex;
    }

    public void SetReady(bool isReady)
    {
        statusText.text = isReady ? "<color=#00FF00>Ready" : "<color=#D96565>Waiting";
        info.isReady = isReady;
    }

    [SerializeField] private int currentCosmeticIndex = 0;

    public void AdvanceCosmeticIndex(int value)
    {
        currentCosmeticIndex += value;
        int numCosmetics = activePreviewObject.GetActiveGroup().groupSubject.cosmeticGroups.Length;
        if (currentCosmeticIndex < 0)
            currentCosmeticIndex = numCosmetics;
        else if (currentCosmeticIndex > numCosmetics)
            currentCosmeticIndex = 0;

        LobbyManager.Instance.OnCosmeticChanged(currentCosmeticIndex);
    }

    public void SetCosmetic(int cosmeticIndex)
    {
        activePreviewObject?.SwitchSubjectCosmetic(cosmeticIndex);
        info.cosmeticIndex = cosmeticIndex;
    }
}

public struct PlayerInfo : INetworkSerializable
{
    public bool isReady;
    public int roleIndex;
    public int characterIndex;
    public int cosmeticIndex;
    public string playerName;


    //blank char, readyable
    public PlayerInfo(bool isReady)
    {
        this.isReady = isReady;
        roleIndex = 0;
        characterIndex = 0;
        cosmeticIndex = 0;
        playerName = "";
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref isReady);
        serializer.SerializeValue(ref roleIndex);
        serializer.SerializeValue(ref characterIndex);
        serializer.SerializeValue(ref cosmeticIndex);
        serializer.SerializeValue(ref playerName);
    }
}