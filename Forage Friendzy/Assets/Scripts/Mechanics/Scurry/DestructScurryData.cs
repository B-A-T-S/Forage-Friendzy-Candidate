using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class DestructScurryData : ScurryData
{


    [Header("Destructible Scurry Data")]
    public NetworkVariable<ScurryState> destrucState;
    [Header("Unity Events")]
    [Tooltip("Invoked when a <b>Prey<\\b> interacts with this Scurry while it is <b>unblocked<\\b>")]
    public UnityEvent interact_Unblocked;
    [Tooltip("Invoked when a <b>Predator<\\b> interacts with this Scurry while it is <b>blocked<\\b>")]
    public UnityEvent interact_Blocked;

    // Use this for initialization
    void Start()
    {
        AssignSelfToEntrances();
        RefreshColliderList();
        destrucState.OnValueChanged += DestrucState_OnValueChanged;
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

    //runs whenever a change in state of destructable scurry is percieved
    private void DestrucState_OnValueChanged(ScurryState previousValue, ScurryState newValue)
    {
        if (newValue == ScurryState.Blocked)
        {
            interact_Unblocked.Invoke();
        }
        else if (newValue == ScurryState.Destroyed)
        {
            interact_Blocked.Invoke();
        }
    }

    public void MakeDestructible()
    {
        //do animation things
        SetDestructibleStateServerRpc(ScurryState.Blocked);
    }

    public void DestroyScurry()
    {
        //do animation things
        SetDestructibleStateServerRpc(ScurryState.Destroyed);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetDestructibleStateServerRpc(ScurryState newState)
    {
        destrucState.Value = newState;       
    }

    //returns whether the scurry is able to be destroyed IN CURRENT STATE
    //IE if the scurry point is not active, it therefore can not be destroyed at that time
    public override bool CanBeDestroyed()
    {
        return IsDestuctible() && destrucState.Value == ScurryState.Blocked;
    }

    //returns whether the scurry is currently destroyed
    public override bool IsDestroyed()
    {
        return destrucState.Value == ScurryState.Destroyed;
    }
}