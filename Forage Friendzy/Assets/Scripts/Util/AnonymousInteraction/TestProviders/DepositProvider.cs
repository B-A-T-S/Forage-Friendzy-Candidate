using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DepositProvider : AnonymousProvider
{

    private void Start()
    {
        base.Start();
    }

    // Checks for PreyFood class to add, destroys self
    public override void AddAction(AnonymousActor actor)
    {
        heldAction.executableAction = () =>
        {

            PreyFood pFood = actor.GetComponent<PreyFood>();
            if (pFood != null)
            {
                pFood.Depositfood();
            }
            else
            {
                Debug.LogError($"Prey Food Component Missing from Actor {actor.name} despite" +
                    $"Provider {name} calling it.");
            }


        };
    }

    public override bool Enter_IsValidActor(AnonymousActor actor)
    {
        PreyFood pFood = actor.GetComponent<PreyFood>();
        if(pFood != null)
            return actor.tag == "Prey" && pFood.playerfood.Value > 0;

        return false;
    }

    public override bool Stay_IsValidActor(AnonymousActor actor)
    {
        PreyFood pFood = actor.GetComponent<PreyFood>();
        if (pFood != null)
            return actor.tag == "Prey" && pFood.playerfood.Value > 0;

        return false;
    }

    protected override void WhenForgottenByActor()
    {
        HideWorldCanvas();
    }

}