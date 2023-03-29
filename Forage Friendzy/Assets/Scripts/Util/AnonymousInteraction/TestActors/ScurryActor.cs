using System.Collections;
using UnityEngine;

public class ScurryActor : AnonymousActor
{


    private AnimalScurry animalScurryComponent;

    protected void Start()
    {
        base.Start();
        animalScurryComponent = GetComponent<AnimalScurry>();
    }

    protected override bool IsValidInputState()
    {
        return !animalScurryComponent.IsScurrying && (animalScurryComponent.CanScurry && !animalScurryComponent.CanScurryOverride);
    }



}