using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Netcode;
using UnityEngine;

public class ActiveAbilities : NetworkBehaviour
{
    #region Inspector Variables
    [Header("Prefabs")]
    [Tooltip("The prefab for the fox trap.")] //pretty self explanatory
    [SerializeField] private GameObject foxTrapPrefab;
    [Header("Fox Trap variables")]
    [Tooltip("Time it takes to place trap (in seconds).")]
    [SerializeField] private float trapPlacementTime;
    [Tooltip("Cooldown until fox can place next trap (in seconds).")]
    [SerializeField] private float trapCooldown;
    #endregion

    #region References
    private BodyMovement bodyMovement;
    private PlayerController playerController;
    #endregion

    #region Helper Variables
    private bool isOnCooldownFT;
    Transform geometry;
    #endregion

    private void Start()
    {
        bodyMovement = GetComponent<BodyMovement>();
        playerController = bodyMovement.linkedController;
        isOnCooldownFT = false;
        foreach(Transform child in transform)
        {
            if (child.CompareTag("BodyGeometry"))
            {
                geometry = child;
            }
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        //looks for Fox Active ability input
        if (bodyMovement.characterId.Value == (int)predator.FOX)
        {
            if (Input.GetKeyDown(playerController.activeAbility))
                SpawnFoxTrap();
        }
        
        //looks for Wolf Active ability input
        if (bodyMovement.characterId.Value == (int)predator.WOLF)
        {
            if (Input.GetKeyDown(playerController.activeAbility))
                PerformDeafeningHowl();
        }
    }

    private void SpawnFoxTrap()
    {
        //Check for cooldown
        if (isOnCooldownFT)
        {
            UnityEngine.Debug.Log("Trap on cooldown");
            return;
        }

        SpawnFoxTrapServerRpc();
    }

    private void PerformDeafeningHowl()
    {

    }

    #region Helper Functions
    private IEnumerator FoxTrapCooldown()
    {
        isOnCooldownFT = true;
        yield return new WaitForSeconds(trapCooldown);
        isOnCooldownFT = false;
    }

    [ServerRpc]
    private void SpawnFoxTrapServerRpc()
    {
        GameObject foxTrap = Instantiate(foxTrapPrefab, transform.position, geometry.rotation);
        foxTrap.GetComponent<NetworkObject>().Spawn();
        StartCoroutine(FoxTrapCooldown());
    }
    #endregion
}
