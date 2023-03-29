using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

//Data required for Scurry Mechanic
//Includes:
// StartingPosition - Animals are to be blinked to this position/rotation at the beginning of the process
// TargetPosition - The endpoint of the process. Animals end up here

[RequireComponent(typeof(ColliderHolder))]
public class ScurryData : NetworkBehaviour
{
    [Header("Type")]
    public ScurryType scurryType;

    [Header("Entrances")]
    public ScurryEntrance pointA;
    public ScurryEntrance pointB;

    public ColliderHolder colliderHolder;

    private void Start()
    {
        AssignSelfToEntrances();
        RefreshColliderList();
    }

    private void AssignSelfToEntrances()
    {
        if (pointA != null)
            pointA.parentData = this;
        else
            Debug.LogError($"ScurryData - PointA of {gameObject.name} is null");

        if (pointB != null)
            pointB.parentData = this;
        else
            Debug.LogError($"ScurryData - PointB of {gameObject.name} is null");
    }

    private void RefreshColliderList()
    {
        if (colliderHolder == null)
            colliderHolder = GetComponent<ColliderHolder>();
    }

    #region Data Getters

    //helper to determine target based on start
    public ScurryEntrance GetTargetViaStart(ScurryEntrance startingPoint)
    {
        if (startingPoint == pointA)
            return pointB;

        return pointA;
    }

    //returns the direction vector of entrance -> target
    public Vector3 GetScurryDirection(ScurryEntrance startingPoint)
    {
        ScurryEntrance targetPoint = GetTargetViaStart(startingPoint);
        Vector3 heading = targetPoint.transform.position - startingPoint.transform.position;
        Vector3 direction = heading / heading.magnitude;
        //Debug.Log($"Direction of Scurry: {direction}");

        return direction;
    }

    #endregion

    #region State Getters
    //returns whether or not this is a destructible type scurry
    public bool IsDestuctible()
    {
        return scurryType == ScurryType.Destructible ? true : false;
    }

    public virtual bool CanBeDestroyed()
    {
        return false;
    }

    public virtual bool IsDestroyed()
    {
        return false;
    }

    #endregion
}

public enum ScurryType 
{
    Permanent,
    Destructible
}

public enum ScurryState
{
    Unblocked,
    Blocked,
    Destroyed
}