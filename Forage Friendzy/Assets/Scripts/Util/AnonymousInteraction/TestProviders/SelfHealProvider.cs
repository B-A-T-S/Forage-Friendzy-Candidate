using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SelfHealProvider : AnonymousProvider
{

    private PreyFood pFood;
    private PreyHealth pHealth;
    private PreySelfHeal pSelfHeal;

    private void Start()
    {
        base.Start();
        pFood = GetComponent<PreyFood>();
        pHealth = GetComponent<PreyHealth>();
        pSelfHeal = GetComponent<PreySelfHeal>();
    }

    public override void AddAction(AnonymousActor actor)
    {
        heldAction.executableAction = () =>
        {
            pHealth.HandleSelfHealServerRpc();
            pFood.SetPlayerFoodServerRpc(pFood.playerfood.Value - pSelfHeal.FoodCost);
        };
    }

    protected override void OverrideActionContainerInput()
    {
        heldAction.inputKey = inSceneController.selfHeal;
    }

    public override bool Enter_IsValidActor(AnonymousActor actor)
    {
        return actor.gameObject == gameObject && actor.tag == "Prey" && pHealth.isInjured.Value && (pFood.playerfood.Value >= pSelfHeal.FoodCost);
    }

    public override bool Stay_IsValidActor(AnonymousActor actor)
    {
        return actor.gameObject == gameObject && actor.tag == "Prey" && pHealth.isInjured.Value && (pFood.playerfood.Value >= pSelfHeal.FoodCost);
    }
    protected override void WhenForgottenByActor()
    {
        HideWorldCanvas();
    }


}
