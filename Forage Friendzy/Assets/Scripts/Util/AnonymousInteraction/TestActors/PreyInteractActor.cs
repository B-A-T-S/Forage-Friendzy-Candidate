using System.Collections;
using UnityEngine;

public class PreyInteractActor : AnonymousActor
{

    public bool isInteracting = false;

    private PreyHealth pHealth;

    private void Start()
    {
        base.Start();
        pHealth = GetComponent<PreyHealth>();

    }

    protected override void OnExecution()
    {
        
    }

    protected override void WhenForgottenAction()
    {
        isInteracting = false;
    }

    protected override void WhenInputActive()
    {
        isInteracting = true;
    }

    protected override void WhenInputInactive()
    {
        isInteracting = false;
    }

    protected override bool IsValidInputState()
    {
        return !pHealth.isFainted.Value;
    }

}