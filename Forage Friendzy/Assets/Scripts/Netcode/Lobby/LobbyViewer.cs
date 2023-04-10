using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using System.Linq;
using System;
using System.Net;


//includes a non-threaded approach to delays between method calls
//that was used by an example
public class LobbyViewer : MonoBehaviour
{

    [Tooltip("The UI Prefab that represents an active Room")]
    [SerializeField] private LobbyRoomUI lobbyRoomPrefab;

    [Tooltip("Instantiated LobbyRoomUI objects are placed underneath this")]
    [SerializeField] private Transform lobbyViewParent;

    [Tooltip("Object visible when no lobbies can be found")]
    [SerializeField] private GameObject noLobbiesText;

    [Tooltip("Controls how often this object attempts to update the visible lobbies")]
    [SerializeField] private float lobbyRefreshRate = 2f;

    //determines the process used to find active lobbies
    private bool globalDiscovery;

    //a list of all currently displayed LobbyRoomUI objects
    private List<LobbyRoomUI> currentlyDisplayedLobbies;

    //internal storage for next frametime to update displayed lobbies
    private float nextRefreshTime;

    public static event Action event_OnExitClicked;
    public static event Action event_OnHostClicked;

    private void Update()
    {
        if (Time.time >= nextRefreshTime)
        {

            //determine next refresh time
            nextRefreshTime = Time.time + lobbyRefreshRate;

            if (globalDiscovery)
                FetchGlobalLobbies();

            FetchLocalLobbies();
        }
            

    }

    private void OnEnable()
    {
        foreach (Transform child in lobbyViewParent)
            Destroy(child.gameObject);

        if (currentlyDisplayedLobbies != null)
            currentlyDisplayedLobbies.Clear();
        else
            currentlyDisplayedLobbies = new List<LobbyRoomUI>();

        globalDiscovery = Authentication.IsAuthenticated;

        if (!globalDiscovery)
        {
            ForageFriendzyLanDiscovery.Instance.StartClient();
            ForageFriendzyLanDiscovery.Instance.StartClientBroadcast();
        }
            
    }

    private void OnDisable()
    {
        ForageFriendzyLanDiscovery.Instance.StopClientBroadcast();
    }

    private void FetchLocalLobbies()
    {

        //Get Discovered Lobbies from Discovery Component (array of KeyValuePair<IP, ResponseData>)
        KeyValuePair<IPEndPoint, DiscoveryResponseData>[] discoveredLobbies = ForageFriendzyLanDiscovery.currentlyKnownLobbies.ToArray();

        Debug.Log($"Found {discoveredLobbies.Length} local lobbies.");

        foreach(KeyValuePair<IPEndPoint, DiscoveryResponseData> lobby in discoveredLobbies)
        {
            LobbyRoomUI current = currentlyDisplayedLobbies.FirstOrDefault(p => p.IP == lobby.Key);
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
        

    }

    private async void FetchGlobalLobbies()
    {
        try
        {
            
            //ask Matchmaking for current lobbies
            var allLobbies = await Matchmaking.GetGlobalLobbies();

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
        catch (InvalidOperationException e)
        {
            Debug.LogError("LobbyAPI not Initialized");
        }
    }

    public void OnExitClicked()
    {
        event_OnExitClicked?.Invoke();
    }

    public void OnHostClicked()
    {
        event_OnHostClicked?.Invoke();
    }


}