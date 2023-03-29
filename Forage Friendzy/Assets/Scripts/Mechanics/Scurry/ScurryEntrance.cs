using System;
using System.Collections;
using UnityEngine;

public class ScurryEntrance : MonoBehaviour
{
    [HideInInspector] public ScurryData parentData;

    public bool IsDestructible()
    {
        return parentData.IsDestuctible();
    }

    public bool CanBeDestroyed()
    {
        return parentData.CanBeDestroyed();
    }

    public bool IsDestroyed()
    {
        return parentData.IsDestroyed();
    }
}