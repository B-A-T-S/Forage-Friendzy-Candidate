using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AnonymousProvider : NetworkBehaviour
{

    [SerializeField] protected ActionContainer heldAction;
    [SerializeField] protected int numTimesExecutable;
    [SerializeField] protected bool checkForActor;

    [SerializeField] protected ActionContainerCanvas worldCanvas;

    protected int numTimesExecuted = 0;

    protected bool hasActiveActor;
    protected ActorData actorData;
    static protected PlayerController inSceneController;

    public ActionContainer GetHeldAction()
    {
        return heldAction;
    }

    protected void Start()
    {
        heldAction.source = this;
        heldAction.event_OnHeldTimeValueChanged += UpdateRadialProgress;
        if (inSceneController == null)
            inSceneController = FindObjectOfType<PlayerController>();
    }

    protected virtual void OverrideActionContainerInput()
    {

    }

    protected void UpdateRadialProgress(object sender, ActionContainerEventArgs e)
    {
        worldCanvas?.UpdateRadialProgress(this, e.currentTime, e.requiredTime);
    }

    public virtual void AddAction(AnonymousActor actor)
    {
        OverrideActionContainerInput();
        heldAction.executableAction = () =>
        {
            //do stuff here
            Debug.Log($"From {name}, called via {actor.name}");
        };
    }

    protected virtual void OnTriggerEnter(Collider other)
    {

        if (!checkForActor)
            return;

        NetworkBehaviour nb = other.GetComponent<NetworkBehaviour>();
        AnonymousActor[] presentActors = other.GetComponents<AnonymousActor>();
        foreach(AnonymousActor actor in presentActors)
        {

            if (!(nb.OwnerClientId == NetworkManager.Singleton.LocalClientId && actor != null))
                continue;

            if (!Enter_IsValidActor(actor))
                continue;

            if (!actor.ActionFilter.Contains(heldAction.catagory))
                continue;

            //Debug.Log($" {heldAction.catagory} Catagory Match at {transform.position}", gameObject);

            ShowWorldCanvas();
            AddAction(actor);

            if (!(actor.TrySetAction(heldAction)))
                continue;

            //Debug.Log($"SetAction Successful");

            AssignActorData(actor);
        }
        
    }

    public virtual bool Enter_IsValidActor(AnonymousActor actor)
    {
        return true;
    }

    public void ShowWorldCanvas()
    {
        if(!worldCanvas.isPromptVisible)
            worldCanvas.Show(heldAction);
    }

    public void HideWorldCanvas()
    {
        if (worldCanvas.isPromptVisible)
            worldCanvas.Hide();
    }

    protected virtual void OnTriggerStay(Collider other)
    {

        if (!checkForActor)
            return;

        NetworkBehaviour nb = other.GetComponent<NetworkBehaviour>();
        AnonymousActor[] presentActors = other.GetComponents<AnonymousActor>();
        foreach (AnonymousActor actor in presentActors)
        {
            if (!(nb.OwnerClientId == NetworkManager.Singleton.LocalClientId && actor != null))
                continue;

            if (!Stay_IsValidActor(actor))
            {
                HideWorldCanvas();
                UnassignActor(actor);
                continue;
            }
                

            //if this actor contains a container from me (aka same containerID), do not attempt to set
            if (actor.ContainsMatchingContainer(heldAction.containerID))
                continue;

            //if I have an actor and that actor has exceeded the executionlimit of my action, do not attempt to set
            if (hasActiveActor && HasExceededExecutionLimit())
                continue;

            if (!worldCanvas.isPromptVisible)
                ShowWorldCanvas();

            AddAction(actor);

            if (!(actor.TrySetAction(heldAction)))
                continue;

            AssignActorData(actor);
        }

        
    }

    public virtual bool Stay_IsValidActor(AnonymousActor actor)
    {
        return true;
    }

    protected virtual void OnTriggerExit(Collider other)
    {

        if (!checkForActor)
            return;

        NetworkBehaviour nb = other.GetComponent<NetworkBehaviour>();
        AnonymousActor[] presentActors = other.GetComponents<AnonymousActor>();
        foreach (AnonymousActor actor in presentActors)
        {
            if (!(nb.OwnerClientId == NetworkManager.Singleton.LocalClientId && actor != null))
                return;

            HideWorldCanvas();
            UnassignActor(actor);
        }
    }



    public void WasExecuted(AnonymousActor executor)
    {
        numTimesExecuted++;
        //the actor has executed this action the intended number of times
        //tell the actor to forget this action and perform approriate actions
        if (HasExceededExecutionLimit())
            if(executor.ForgetAction(heldAction))
                WhenForgottenByActor();

        heldAction.CurrentHoldTime = 0;
        worldCanvas.UpdateRadialProgress(this, 0, 1);

    }

    protected virtual void WhenForgottenByActor()
    {

    }

    public bool HasExceededExecutionLimit()
    {
        return numTimesExecuted >= numTimesExecutable;
    }

    public void AssignActorData(AnonymousActor actor)
    {
        hasActiveActor = true;
        actorData = new ActorData(actor, false);
    }

    public void UnassignActor(AnonymousActor actor)
    {
        //if this isn't the current actor, no reason to forget
        if (actorData.actorRef != actor)
            return;

        if (actor.ForgetAction(heldAction))
            WhenForgottenByActor();
        hasActiveActor = false;
        actorData = new ActorData(null, false);
        numTimesExecuted = 0;
    }

    protected struct ActorData
    {
        public AnonymousActor actorRef;
        public bool hasExecuted;

        public ActorData(AnonymousActor _actorRef, bool _hasExecuted = false)
        {
            actorRef = _actorRef;
            hasExecuted = _hasExecuted;
        }


    }

}
