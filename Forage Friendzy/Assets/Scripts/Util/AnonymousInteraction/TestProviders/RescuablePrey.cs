using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RescuablePrey : AnonymousProvider
{

    private PreyHealth pHealth;

    private void Start()
    {
        base.Start();
        pHealth = GetComponent<PreyHealth>();
    }

    protected override void OverrideActionContainerInput()
    {
        //heldAction.inputKey = inSceneController.interact;
    }

    public override void AddAction(AnonymousActor actor)
    {
        heldAction.executableAction = () =>
        {

            //Get PreyHealth component from Rescueie 
            PreyHealth rescueie = GetComponent<PreyHealth>();
            //Get PreyHealth component from Rescuer
            PreyHealth rescuer = actor.GetComponent<PreyHealth>();

            if (rescueie == null || rescuer == null)
                return;

            rescueie.ProcessRescue();
            rescuer.ToggleRescuingTeammate();

        };
    }

    //case Prey : Self
    //case Prey : Not Self : Self Fainted : Fainted Other
       // T && !F && T && !T -> F
    //case Prey : Not Self : Self Not Fainted : Fainted Other
       // T && !F && F && !T -> F
    //case Prey : Not Self : Self Not Fainted : Not Fainted Other
       // T && !F && F && !F -> F
    //case Prey : Not Self : Self Fainted : Not Fainted Other
       // T &7 !F && T && !F -> T


    public override bool Enter_IsValidActor(AnonymousActor actor)
    {
        return actor.tag == "Prey" && !actor.IsProviderBlackListed(this) && pHealth.isFainted.Value && !actor.GetComponent<PreyHealth>().isFainted.Value;
    }

    public override bool Stay_IsValidActor(AnonymousActor actor)
    {
        return actor.tag == "Prey" && !actor.IsProviderBlackListed(this) && pHealth.isFainted.Value && !actor.GetComponent<PreyHealth>().isFainted.Value;
    }

    protected override void WhenForgottenByActor()
    {
        base.WhenForgottenByActor();
        HideWorldCanvas();
    }

}