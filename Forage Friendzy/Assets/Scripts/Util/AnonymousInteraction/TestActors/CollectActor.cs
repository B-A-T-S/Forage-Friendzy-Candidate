using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CollectActor : AnonymousActor
{

    public NetworkVariable<bool> isCollecting;


    private void Start()
    {
        base.Start();
    }

    protected override void OnExecution()
    {
        SetCollectingServerRpc(false);
    }

    protected override void WhenForgottenAction()
    {
        SetCollectingServerRpc(false);
    }

    protected override void WhenInputActive()
    {
        SetCollectingServerRpc(true);
    }

    protected override void WhenInputInactive()
    {
        SetCollectingServerRpc(false);
    }

    [ServerRpc]
    private void SetCollectingServerRpc(bool newValue)
    {
        isCollecting.Value = newValue;
    }

}
