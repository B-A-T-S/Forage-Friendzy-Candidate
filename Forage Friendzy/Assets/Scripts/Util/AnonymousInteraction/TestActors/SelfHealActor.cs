using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfHealActor : AnonymousActor
{
    private PreyHealth pHealth;
    private PreyFood pFood;
    private PreySelfHeal pSelfHeal;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        pHealth = GetComponent<PreyHealth>();
        pFood = GetComponent<PreyFood>();
    }

    protected virtual bool IsValidInputState()
    {
        return pHealth.isInjured.Value && (pFood.playerfood.Value >= pSelfHeal.FoodCost);
    }
}
