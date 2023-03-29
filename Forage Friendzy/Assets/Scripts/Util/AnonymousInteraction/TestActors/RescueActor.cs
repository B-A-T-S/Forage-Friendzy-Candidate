using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RescueActor : AnonymousActor
{

    public NetworkVariable<bool> isRescuing = new NetworkVariable<bool>();

    private PreyHealth pHealth;

    private void Start()
    {
        base.Start();
        pHealth = GetComponent<PreyHealth>();
    }

    [ServerRpc]
    private void SetRescuingServerRpc(bool newValue)
    {
        isRescuing.Value = newValue;
    }

    protected override void OnExecution()
    {
        SetRescuingServerRpc(false);
    }

    protected override void WhenForgottenAction()
    {
        SetRescuingServerRpc(false);
    }

    protected override void WhenInputActive()
    {
        SetRescuingServerRpc(true);
    }

    protected override void WhenInputInactive()
    {
        SetRescuingServerRpc(false);
    }

    protected virtual bool IsValidInputState()
    {
        return !pHealth.isFainted.Value;
    }

}
