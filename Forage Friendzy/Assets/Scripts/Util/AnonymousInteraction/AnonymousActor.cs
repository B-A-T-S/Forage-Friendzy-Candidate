using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AnonymousActor : NetworkBehaviour
{
    [SerializeField] private int maximumActiveActions = 3;
    [SerializeField] private bool mustHoldInput = true;
    [SerializeField] private bool allowMultipleInputs = true;
    [SerializeField] private List<ActionContainer> activeActions = new();

    [SerializeField] private bool searchForProviders = false;
    [SerializeField] protected List<ActionCatagory> actionFilter = new List<ActionCatagory>();
    public List<ActionCatagory> ActionFilter { get { return actionFilter; } }
    [SerializeField] private List<AnonymousProvider> blackList = new();

    protected BodyMovement bodyMovement;
    private ActionContainer[] frameContainerArr;
    protected bool[] activeInputs;
    static protected PlayerController inSceneController;

    protected void Start()
    {
        bodyMovement = GetComponent<BodyMovement>();
        activeInputs = new bool[maximumActiveActions];
        if (!IsOwner)
            searchForProviders = false;
        if (inSceneController == null)
            inSceneController = FindObjectOfType<PlayerController>();
    }

    public bool TrySetAction(ActionContainer newAction)
    {
        //check if provider is banned from this actor
        if (IsProviderBlackListed(newAction.source))
            return false;

        //check if catagory exists in list
        if (!actionFilter.Contains(newAction.catagory))
            return false;

        bool setNewAction = true;
        //if the activeActions list already contains an action that uses this key...
        int indexOfMatch = GetIndexOfMatchingInput(newAction.inputKey);
        if(indexOfMatch != -1)
        {
            //The player is already interacting with some action
            //It would be disorienting to change the action at this time
            if (activeInputs[indexOfMatch])
                setNewAction = false;

            //The newer action has a worse priority than the current one
            if (setNewAction && newAction.priority > activeActions[indexOfMatch].priority)
                setNewAction = false;

            //The newer action is deemed "not the intended action" based on the player's perspective
            Transform existingProvider = activeActions[indexOfMatch].source.gameObject.transform;
            Transform newProvider = newAction.source.gameObject.transform;
            if (setNewAction && CameraDirectionSimilarity(newProvider) <= CameraDirectionSimilarity(existingProvider))
                setNewAction = false;

        }
        else
        {

            if (GetActiveActionCount() >= maximumActiveActions)
                setNewAction = false;
        }


        if (setNewAction)
            SetAction(newAction, indexOfMatch);

        return setNewAction;
              
    }

    protected void Update()
    {
        AddInputTime();
    }

    protected virtual void AddInputTime()
    {
        //for each active action in list...
        frameContainerArr = activeActions.ToArray();
        for (int i = 0; i < frameContainerArr.Length; i++)
        {
            //check to see if the input assosiated with that action
            //is down for this frame
            if (IsValidInputState() && Input.GetKey(frameContainerArr[i].inputKey))
            {
                //if multiInput is disabled and an input is already active, ignore this input
                //if an input is not yet active, set it as such
                if (!allowMultipleInputs && HasExistingInput(i))
                    continue;
                else if (!allowMultipleInputs)
                    activeInputs[i] = true;

                WhenInputActive();
                   
                frameContainerArr[i].CurrentHoldTime += Time.deltaTime;
                //has the input been held down for long enough?
                if (frameContainerArr[i].CurrentHoldTime >= frameContainerArr[i].requiredHoldTime)
                {
                    Execute(frameContainerArr[i]);
                }
            }
            else
            {
                //if progress can be lost, then set it to zero the frame the input is not active
                if (mustHoldInput)
                    frameContainerArr[i].CurrentHoldTime = 0;

                WhenInputInactive();

            }

        }

        if(frameContainerArr.Length > 0 && NoActiveInputs())
        {
            WhenNoActiveInputs();
        }
    } 

    protected void Execute(ActionContainer toExecute)
    {
        toExecute.executableAction?.Invoke();
        toExecute.source.WasExecuted(this);
        OnExecution();
    }

    /// <summary>
    /// Called when any action is executed
    /// </summary>
    protected virtual void OnExecution()
    {

    }

    /// <summary>
    /// Controls WHEN inputs are updated. If the actor is not in a valid input state, aka this returns false,
    /// inputs are ignored
    /// </summary>
    /// <returns>true if valid, false otherwise</returns>
    protected virtual bool IsValidInputState()
    {
        return true;
    }

    /// <summary>
    /// Called every frame the actor while IsValidInputState returns true and an actionInput is held down
    /// </summary>
    protected virtual void WhenInputActive()
    {

    }

    /// <summary>
    /// Called every frame the actor while IsValidInputState returns false or an actionInput is not held down
    /// </summary>
    protected virtual void WhenInputInactive()
    {

    }

    /// <summary>
    /// Called every frame while the actor has no active actions
    /// </summary>
    protected virtual void WhenNoActiveInputs()
    {

    }

    //returns true if there is an action's input that is already being held down
    //ignores the index of the input passed
    private bool HasExistingInput(int indexOfCurrentInput)
    {

        if (indexOfCurrentInput >= activeInputs.Length)
            return false;

        for(int i = 0; i < activeInputs.Length; i++)
        {
            if (i != indexOfCurrentInput && activeInputs[i] == true)
                return true;
        }

        return false;
    }

    //returns whether for a given frame, the actor is pressing any
    //of the inputs assosiated with their actions
    private bool NoActiveInputs()
    {
        for (int i = 0; i < activeInputs.Length; i++)
        {
            if (activeInputs[i] == true)
                return false;
        }

        return true;
    }

    private void SetAction(ActionContainer newAction, int index = -1)
    {
        if(index == -1)
        {
            activeActions.Add(newAction);
        }
        else
        {
            activeActions[index] = newAction;
        }
            
    }

    public bool ForgetAction(ActionContainer action)
    {

        if(activeActions.Remove(action))
        {
            WhenForgottenAction();
            return true;
        }
        else
        {
            return false;
        }
    }

    protected virtual void WhenForgottenAction()
    {

    }

    private int GetIndexOfMatchingInput(KeyCode key)
    {
        for (int i = 0; i < GetActiveActionCount(); i++)
        {
            if (activeActions[i].inputKey == key)
                return i;
        }
        return -1;
    }

    public bool ContainsMatchingInput(KeyCode key)
    {
        return activeActions.Find(x => x.inputKey == key) != null;
    }

    public bool ContainsMatchingContainer(int key)
    {
        return activeActions.Find(x => x.containerID == key) != null;
    }

    public ActionContainer GetMatchingInput(ActionContainer key)
    {
        return activeActions.Find(x => x == key);
    }

    //return the dot product of the NoY camera forward direction
    //and the directional vector from this object to another
    private float CameraDirectionSimilarity(Transform t)
    {
        //Get Camera Forward with no Y axis
        Vector3 camNoY = bodyMovement.cameraReference.MyCamera.transform.forward;
        camNoY.y = 0;
        camNoY.Normalize();

        //Get directional Vector from actor to provider
        Vector3 dir = t.position - bodyMovement.transform.position;
        dir.Normalize();

        //Debug.Log($"Calculated Camera Forward Dot {t.name} => {Vector3.Dot(camNoY, dir)}");

        return Vector3.Dot(camNoY, dir);
    }  

    public bool HasActiveAction()
    {
        return activeActions.Count > 0;
    }

    public int GetActiveActionCount()
    {
        return activeActions.Count;
    }

    /// <summary>
    /// returns true if the providerInQuestion exists in the actor's blackList, false otherwise
    /// </summary>
    public bool IsProviderBlackListed(AnonymousProvider providerInQuestion)
    {
        return blackList.Contains(providerInQuestion);
    }

    #region Provider Search Extention

    private void OnTriggerEnter(Collider other)
    {

        if (!searchForProviders) return;

        //check if collider is a provider or extention of provider
        AnonymousProvider provider = other.GetComponent<AnonymousProvider>();
        if (provider == null) return;

        //check if provider's action uses the input designated by filter
        if (actionFilter.Count != 0 && !actionFilter.Contains(provider.GetHeldAction().catagory)) return;

        //check the provider's valid Entity 
        if (!provider.Enter_IsValidActor(this)) return;

        //valid and existing provider for this actor, convey
        //send self to help provider specialize it's action
        provider.ShowWorldCanvas();
        provider.AddAction(this);

        //if successful in setting action to self, inform provider
        if (!TrySetAction(provider.GetHeldAction())) return;

        provider.AssignActorData(this);

    }

    private void OnTriggerStay(Collider other)
    {

        if (!searchForProviders) return;
        AnonymousProvider provider = other.GetComponent<AnonymousProvider>();
        if (provider == null) return;
        if (actionFilter.Count != 0 && !actionFilter.Contains(provider.GetHeldAction().catagory)) return;
        //check the provider's valid Entity 
        if (!provider.Stay_IsValidActor(this)) return;
        //if this actor contains a container with a matching ID, ignore
        if (ContainsMatchingContainer(provider.GetHeldAction().containerID)) return;
        //if the provider has reached it's execution limit, ignore
        if (provider.HasExceededExecutionLimit()) return;

        provider.AddAction(this);
        if (!TrySetAction(provider.GetHeldAction())) return;
            
        provider.AssignActorData(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!searchForProviders) return;
        AnonymousProvider provider = other.GetComponent<AnonymousProvider>();
        if (provider == null) return;

        provider.HideWorldCanvas();
        provider.UnassignActor(this);
    }

    #endregion

}
