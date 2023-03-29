using System.Collections;
using UnityEngine;


public class BreakableScurryProvider : AnonymousProvider
{

    private ScurryEntrance scurryEntrance;
    private DestructScurryData destrucData;

    // Use this for initialization
    protected void Start()
    {
        base.Start();
        scurryEntrance = GetComponent<ScurryEntrance>();
    }

    protected override void OverrideActionContainerInput()
    {
        heldAction.inputKey = inSceneController.scurry;
    }

    public override bool Enter_IsValidActor(AnonymousActor actor)
    {
        if (destrucData == null)
            destrucData = scurryEntrance.parentData as DestructScurryData;

        if (destrucData.IsDestuctible() && !destrucData.CanBeDestroyed() && !destrucData.IsDestroyed())
        {
            return actor.tag == "Prey";
        }
        else if (destrucData.IsDestuctible() && destrucData.CanBeDestroyed() && !destrucData.IsDestroyed())
        {
            return actor.tag == "Predator";
        }
            

        return true;
    }

    public override bool Stay_IsValidActor(AnonymousActor actor)
    {
        if (destrucData == null)
            destrucData = scurryEntrance.parentData as DestructScurryData;

        if (destrucData.IsDestuctible() && !destrucData.CanBeDestroyed() && !destrucData.IsDestroyed())
            return actor.tag == "Prey";
        else if (destrucData.IsDestuctible() && destrucData.CanBeDestroyed() && !destrucData.IsDestroyed())
            return actor.tag == "Predator";

        return false;
    }

    protected override void WhenForgottenByActor()
    {
        HideWorldCanvas();
    }

    public override void AddAction(AnonymousActor actor)
    {
        if (destrucData == null)
            destrucData = scurryEntrance.parentData as DestructScurryData;

        heldAction.executableAction = () =>
        {
            AnimalScurry actorScurryComp = actor.GetComponent<AnimalScurry>();
            if (actor.tag == "Prey")
            {
                actorScurryComp.Prey_MakeScurryDestrucible(destrucData);
            }
            else if (actor.tag == "Predator")
            {
                actorScurryComp.Predator_DestroyScurry(destrucData);
            }

        };
    }
}