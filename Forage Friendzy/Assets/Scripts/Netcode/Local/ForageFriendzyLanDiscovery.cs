using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ForageFriendzyLanDiscovery : NetworkDiscovery<DiscoveryBroadcastData, DiscoveryResponseData>
{

    public static ForageFriendzyLanDiscovery Instance { get; private set; }

    public static Dictionary<IPEndPoint, DiscoveryResponseData> currentlyKnownLobbies = new();
    public static event Action<IPEndPoint, DiscoveryResponseData> event_OnServerFound;
    private Coroutine broadcastCoroutine;


    [Header("Properties")]
    [SerializeField] private float shoutDelay = 2.0f;


    private void Start()
    {
        Instance = this;
        event_OnServerFound += StoreFoundServer;
    }

    public void StartClientBroadcast()
    {
        if (broadcastCoroutine != null)
            return;

        broadcastCoroutine = StartCoroutine(ClientBroadcast());
    }

    public void StopClientBroadcast()
    {
        if (broadcastCoroutine == null)
            return;
        StopCoroutine(broadcastCoroutine);
        broadcastCoroutine = null;
    }

    IEnumerator ClientBroadcast()
    {

        while(!IsClient)
        {
            ClientBroadcast(new DiscoveryBroadcastData());
            yield return new WaitForSeconds(shoutDelay);
        }
    }

    private void StoreFoundServer(IPEndPoint ip, DiscoveryResponseData response)
    {
        if(!currentlyKnownLobbies.ContainsKey(ip))
            currentlyKnownLobbies.Add(ip, response);
    }

    //This is run by Hosts when they recieve a client ping
    protected override bool ProcessBroadcast(IPEndPoint sender, DiscoveryBroadcastData broadCast, out DiscoveryResponseData response)
    {
        response = Matchmaking.GetResponseData();
        return true;
    }

    //This is run by Clients when they recieve a host pong
    protected override void ResponseReceived(IPEndPoint sender, DiscoveryResponseData response)
    {
        event_OnServerFound?.Invoke(sender, response);
    }
}