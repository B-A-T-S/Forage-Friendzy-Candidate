using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollectableFood : AnonymousProvider
{

    private Food food;

    private void Start()
    {
        base.Start();
        food = GetComponent<Food>();
    }

    // Checks for PreyFood class to add, destroys self
    public override void AddAction(AnonymousActor actor)
    {
        OverrideActionContainerInput();
        heldAction.executableAction = () =>
        {

            Food tempFood = food;
            if (tempFood != null)
            {
                tempFood.OnCollect();
            }
            else
            {
                Debug.LogError($"Food Component Missing from Collect Provider {name}");
            }

            PreyFood pFood = actor.GetComponent<PreyFood>();
            if (pFood != null)
            {
                pFood.Addfood();
            }
            else
            {
                Debug.LogError($"Prey Food Component Missing from Actor {actor.name} despite" +
                    $"Provider {name} calling it.");
            }
        };
    }

    protected override void OverrideActionContainerInput()
    {
        heldAction.inputKey = inSceneController.interact;
    }

    public override bool Enter_IsValidActor(AnonymousActor actor)
    {
        return actor.tag == "Prey" && food.isAvailable.Value;
    }

    public override bool Stay_IsValidActor(AnonymousActor actor)
    {
        return actor.tag == "Prey" && food.isAvailable.Value;
    }

    protected override void WhenForgottenByActor()
    {
        base.WhenForgottenByActor();
    }

}