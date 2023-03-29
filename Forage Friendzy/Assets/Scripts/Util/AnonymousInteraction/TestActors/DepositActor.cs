using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DepositActor : AnonymousActor
{

    public NetworkVariable<bool> isDepositing;

    private void Start()
    {
        base.Start();
    }

    protected override void OnExecution()
    {
        SetDepositingServerRpc(false);
    }

    protected override void WhenForgottenAction()
    {
        SetDepositingServerRpc(false);
    }

    protected override void WhenInputActive()
    {
        SetDepositingServerRpc(true);
    }

    protected override void WhenInputInactive()
    {
        //base.OnInvalidInputState();
        SetDepositingServerRpc(false);
    }

    [ServerRpc]
    private void SetDepositingServerRpc(bool newValue)
    {
        isDepositing.Value = newValue;
    }

}
