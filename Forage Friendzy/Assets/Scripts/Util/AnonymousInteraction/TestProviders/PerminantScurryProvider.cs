using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PerminantScurryProvider : AnonymousProvider
{

    private ScurryEntrance scurryEntrance;

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
        return actor.tag == "Predator";
    }

    public override bool Stay_IsValidActor(AnonymousActor actor)
    {
        return actor.tag == "Predator";
    }

    public override void AddAction(AnonymousActor actor)
    {
        heldAction.executableAction = () => 
        {
            worldCanvas.Hide();
            AnimalScurry actorScurryComp = actor.GetComponent<AnimalScurry>();
            actorScurryComp.Animal_PerformScurry(scurryEntrance);
        
        };
    }

    
}
