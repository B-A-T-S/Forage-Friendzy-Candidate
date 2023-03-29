using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PreyRescue : NetworkBehaviour
{
    #region Rescue Variables

    [Tooltip("How long the player will need to hold interact to rescue their buddy in seconds.")]
    [SerializeField]
    private float interactHoldLength = 3.0f;
    [Tooltip("Transform for the overlap sphere used for healing.")]
    public Transform rescueTransform;
    [Tooltip("Radius of the healing interaction.")]
    [SerializeField]
    private float rescueRadius;
    private PlayerController controller;
    private PreyHealth playerHealth;
    private Coroutine rescueCoroutine;
    private bool rescueActive;
    private Transform playerParent;
    private PreyHealth preyBeingRescuedHealth;
    public PreyHealth PreyBeingRescuedHealth { set { preyBeingRescuedHealth = value; } }
    [SerializeField] private bool canRescue;
    public bool CanRescue { get { return canRescue; } set { canRescue = value; } }

    #endregion

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    private void Start()
    {
        preyBeingRescuedHealth = null;
        controller = GetComponent<BodyMovement>().linkedController;
        playerHealth = GetComponent<PreyHealth>();
        rescueActive = false;
        canRescue = false;
        playerParent = transform;
        while (playerParent.parent != null)
        {
            playerParent = playerParent.parent;
        }
    }

    #region Update

    private void Update()
    {

        
        /*
        //should only be running on the owner
        if (!IsOwner) 
            return;

        //remains true while the key is being held down
        if (Input.GetKey(controller.interact) && !playerHealth.isFainted.Value)
        {
            if (!rescueActive)
            {
                //check if an injured prey is in front of us
                if (!canRescue)
                    return;
                
                Debug.Log("Rescue commencing");
                rescueActive = true;
                playerHealth.ToggleRescuingTeammate();
                rescueCoroutine = StartCoroutine(RescueTimer());
            }
        }
        else
        {
            if (rescueActive)
            {
                Debug.Log("Rescue Interrupted");
                rescueActive = false;
                StopCoroutine(rescueCoroutine);
                playerHealth.ToggleRescuingTeammate();
            }
        }
        */
    }

    #endregion

    #region Helpers

    IEnumerator RescueTimer()
    {
        yield return new WaitForSeconds(interactHoldLength);
        //rescue
        rescueActive = false;
        preyBeingRescuedHealth.ProcessRescue();
        playerHealth.ToggleRescuingTeammate();
        Debug.Log("Rescue Finished");

    }

    #endregion
}
