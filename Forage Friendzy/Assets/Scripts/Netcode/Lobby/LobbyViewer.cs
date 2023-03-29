using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using System.Linq;
using System;


//includes a non-threaded approach to delays between method calls
//that was used by an example
public class LobbyViewer : MonoBehaviour
{

    [Tooltip("The UI Prefab that represents an active Room")]
    [SerializeField]
    private LobbyRoomUI lobbyRoomPrefab;

    [Tooltip("Instantiated LobbyRoomUI objects are placed underneath this")]
    [SerializeField]
    private Transform lobbyViewParent;

    [Tooltip("Object visible when no lobbies can be found")]
    [SerializeField]
    private GameObject noLobbiesText;

    [Tooltip("Controls how often this object attempts to update the visible lobbies")]
    [SerializeField]
    private float lobbyRefreshRate = 2f;

    //a list of all currently displayed LobbyRoomUI objects
    private List<LobbyRoomUI> currentlyDisplayedLobbies;

    //internal storage for next frametime to update displayed lobbies
    private float nextRefreshTime;

    private void Update()
    {
        if (Time.time >= nextRefreshTime)
            FetchLobbies();
    }

    private void OnEnable()
    {
        foreach (Transform child in lobbyViewParent)
            Destroy(child.gameObject);

        if (currentlyDisplayedLobbies != null)
            currentlyDisplayedLobbies.Clear();
        else
            currentlyDisplayedLobbies = new List<LobbyRoomUI>();
    }

    private async void SearchForQueueLobby(LobbyType toSearchFor)
    {
        List<Lobby> allLobbies;
        //ask Matchmaking for current lobbies
        switch(toSearchFor)
        {
            case LobbyType.PredOnly:
                allLobbies = await Matchmaking.GetOpenPredatorLobbies();
                break;

            default:
                allLobbies = await Matchmaking.GetOpenPreyLobbies();
                break;
        }

        if (allLobbies.Count < 1)
        {
            //no lobbies were found, create one
            await Matchmaking.CreateFreeLobby(CreateRoleQueueLobbyData(toSearchFor));

        }
        else
        {
            //a lobby was found, join it
            await Matchmaking.LeaveLobby();
            await Matchmaking.JoinLobby(allLobbies[0].Id);
        }


    }

    private LobbyData CreateRoleQueueLobbyData(LobbyType toCreate)
    {
        LobbyData data;
        switch(toCreate)
        {
            case LobbyType.PredOnly:
                data = new LobbyData
                {
                    name = "PredLobby",
                    lobbyType = (int)LobbyType.PredOnly,
                    maxPlayers = 5,
                    hasRestrictions = false,
                    hasPassword = false
                };
                break;
            default:
                data = new LobbyData
                {
                    name = "PreyLobby",
                    lobbyType = (int)LobbyType.PreyOnly,
                    maxPlayers = 3,
                    hasRestrictions = false,
                    hasPassword = false
                };
                break;
        }

        return data;
    }

    private async void FetchLobbies()
    {
        try
        {
            //add to the refresh time
            //if this continues to get big, wouldn't it go over the float limit?
            nextRefreshTime = Time.time + lobbyRefreshRate;

            //ask Matchmaking for current lobbies
            var allLobbies = await Matchmaking.GetLobbies();

            // Exclude our owned lobbies
            var lobbyIds = allLobbies.Where(l => l.HostId != Authentication.PlayerId).Select(l => l.Id);

            //remove inactive lobbies
            var notActive = currentlyDisplayedLobbies.Where(l => !lobbyIds.Contains(l.Lobby.Id)).ToList();
            foreach (LobbyRoomUI ui in notActive)
            {
                Destroy(ui.gameObject);
                currentlyDisplayedLobbies.Remove(ui);
            }

            //create new/update existing lobbies
            foreach (Lobby lobby in allLobbies)
            {
                var current = currentlyDisplayedLobbies.FirstOrDefault(p => p.Lobby.Id == lobby.Id);
                if (current != null)
                {
                    current.UpdateDetails(lobby);
                }
                else
                {
                    LobbyRoomUI panel = Instantiate(lobbyRoomPrefab, lobbyViewParent);
                    panel.Init(lobby);
                    currentlyDisplayedLobbies.Add(panel);
                }
            }

            noLobbiesText.SetActive(!currentlyDisplayedLobbies.Any());
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }


}