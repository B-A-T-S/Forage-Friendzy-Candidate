using System;
using UnityEngine;

[Serializable]
public class ActionContainer
{

    public static System.Random rand = new System.Random(System.DateTime.Now.Millisecond);

    public Action executableAction = () => { };
    public int containerID;
    public string displayName;
    public KeyCode inputKey;
    public ActionCatagory catagory;
    [Tooltip("lower number is prioritized")]
    public int priority;
    public float requiredHoldTime;

    public float currentHoldTime;
    public EventHandler<ActionContainerEventArgs> event_OnHeldTimeValueChanged;
    public float CurrentHoldTime
    {
        get { return currentHoldTime; }
        set 
        { 
            currentHoldTime = value; 
            event_OnHeldTimeValueChanged?.Invoke(this, new ActionContainerEventArgs { currentTime = currentHoldTime, requiredTime = requiredHoldTime});
        }
    }


    public AnonymousProvider source;

    public ActionContainer()
    {
        containerID = rand.Next();
        inputKey = KeyCode.E;
        catagory = ActionCatagory.Scurry;
        priority = 0;
        requiredHoldTime = 0.01f;
        displayName = "Unnamed Action";
        currentHoldTime = 0f;
    }

    public ActionContainer(ActionContainer existingContainer)
    {
        executableAction = existingContainer.executableAction;
        containerID = existingContainer.containerID;
        displayName = existingContainer.displayName;
        inputKey = existingContainer.inputKey;
        catagory = existingContainer.catagory;
        priority = existingContainer.priority;
        requiredHoldTime = existingContainer.requiredHoldTime;
        currentHoldTime = 0f;
        event_OnHeldTimeValueChanged = existingContainer.event_OnHeldTimeValueChanged;
        source = existingContainer.source;
    }
}

public class ActionContainerEventArgs : EventArgs
{
    public float currentTime;
    public float requiredTime;
}

public enum ActionCatagory
{
    NoFilter,
    Collect,
    Deposit,
    Scurry,
    Rescue,
    SelfHeal
}