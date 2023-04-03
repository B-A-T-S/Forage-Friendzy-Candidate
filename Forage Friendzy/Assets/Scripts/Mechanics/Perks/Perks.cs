using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum prey
{
    HEDGEHOG,
    RABBIT,
    CHIPMUNK
};

public enum predator
{
    FOX,
    WOLF
};

public class Perks : NetworkBehaviour
{

    #region References
    private BodyMovement bodyMovement;
    private PreySelfHeal preySelfHeal;
    private AnimalScurry animalScurry;
    #endregion

    #region Prey Perk Variables
    [Header("Prey Perk Variables")]
    [Tooltip("The time a prey will be stunned when they attack a prey with Counter Spikes perk")]
    [SerializeField] private float stunTime;
    [Tooltip("This is the value that will be set to the walk speed of prey with the Faster Walk perk")]
    [SerializeField] private float fasterWalkSpeed;
    [Tooltip("The amount that will be added to the walk and sprint speed values for the Quick Getaway perk")]
    [SerializeField] private float quickGetawayBoost;
    [Tooltip("The duration of the Quick Getaway perk in seconds")]
    [SerializeField] private float quickGetawayDuration;
    [Tooltip("The value that will be added to the sprint speed value when Hop To It perk is activated")]
    [SerializeField] private float hopToItBoost;
    [Tooltip("The duration of the Hop To It perk in seconds")]
    [SerializeField] private float hopToItDuration;
    [Tooltip("The cool down for the Hop To It perk in seconds")]
    [SerializeField] private float hopToItCooldown;
    [Tooltip("The duration of self heal boost in seconds")]
    [SerializeField] private float boostedHealTime;
    [Tooltip("The amount of food the prey can carry with Deep Pocket perk")]
    [SerializeField] private int deepPocketValue;
    public float StunTime { get { return stunTime; } }
    #endregion

    #region Predator Perk Variables
    [Header("Predator Perk Variables")]
    [Tooltip("*SET ON PREY* This is the value that will be multiplied to the distance of the prey from the fox when calculating sniff volume")]
    [SerializeField] private float slyPerkMultiplier;
    public float SlyPerkMultiplier { get { return slyPerkMultiplier; } }
    [Tooltip("The time in seconds it will take for fox to get through scurries")]
    [SerializeField] private float boostedScurrySpeed;
    [Tooltip("The boosted attack cooldown given to the wolf")]
    [SerializeField] private float boostedAttackCooldown;
    #endregion

    #region Helper Variables
    public NetworkVariable<bool> activateQuickGetaway;
    private bool isPrey;
    [Header("Preadtor Line of Sight Variables")]
    [Tooltip("Distance of the raycast")]
    [SerializeField]
    private float maxDistance;
    [Tooltip("Angle of the raycast")]
    [SerializeField]
    [Range(0,360)]
    private float angle;
    [Tooltip("Toggle the debug line draws in game")]
    [SerializeField]
    private bool drawDebugLines;
    [Tooltip("Layer that we are targeting")]
    [SerializeField]
    private LayerMask targetMask;
    private float quickSpeed;
    private float normalSpeed;
    #endregion

    

    // Start is called before the first frame update
    void Start()
    {
        activateQuickGetaway.Value = false;
        bodyMovement = GetComponent<BodyMovement>();

        isPrey = (GetComponent<PreyHealth>() != null);

        //for perks that need to be initialized at the start
        if (!isPrey)
        {
            //set up pred perks and other related stuff
            if(bodyMovement.characterId.Value == (int)(predator.WOLF))
            {
                FasterScurry();
            }
        }
        else
        {
            normalSpeed = bodyMovement.SprintSpeed;
            quickSpeed = bodyMovement.SprintSpeed + quickGetawayBoost;
            if (bodyMovement.characterId.Value == (int)(prey.CHIPMUNK))
            {
                QuickHeal();
            }
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (activateQuickGetaway.Value)
        {
            DeactivateQuickGetawayIndicatorServerRpc();
            QuickGetaway();
        }
    }

    #region Prey Perks

    private void QuickHeal()
    {
        preySelfHeal = GetComponent<PreySelfHeal>();
        preySelfHeal.HealTime = boostedHealTime;
    }

    public void QuickGetaway()
    {
        if (!IsOwner)
            return;
        bodyMovement.WalkSpeed = quickSpeed;
        bodyMovement.SprintSpeed = quickSpeed;
        StartCoroutine(QuickGetawayBoost());
    }

    #endregion

    #region Predator Perks

    private void FasterScurry()
    {
        if (!IsOwner) return;
        animalScurry = GetComponent<AnimalScurry>();
        animalScurry.scurryTime = boostedScurrySpeed;
    }

    #endregion

    #region Helpers

    IEnumerator QuickGetawayBoost()
    {
        yield return new WaitForSeconds(quickGetawayDuration);
        bodyMovement.WalkSpeed = normalSpeed;
        bodyMovement.SprintSpeed = normalSpeed;
    }

    [ServerRpc]
    private void DeactivateQuickGetawayIndicatorServerRpc()
    {
        activateQuickGetaway.Value = false;
    }

    #endregion

    #region Archived Perks
    //FASTER WALK
    //
    //private void FasterWalk()
    //{
    //    if (!IsOwner) return;
    //    bodyMovement.WalkSpeed = fasterWalkSpeed;
    //}


    //HOP TO IT
    //
    //public void HopToIt()
    //{
    //    if (!IsOwner || onCooldownHTI) return;

    //    if (bodyMovement.characterId.Value == (int)(prey.RABBIT))
    //    {
    //        bodyMovement.SprintSpeed += hopToItBoost;
    //        Debug.Log($"Speed: {bodyMovement.SprintSpeed}");
    //        onCooldownHTI = true;
    //        StartCoroutine(HopToItSpeedBoost());
    //        StartCoroutine(HopToItCooldown());
    //    }
    //}
    //IEnumerator HopToItSpeedBoost()
    //{
    //    yield return new WaitForSeconds(hopToItDuration);
    //    bodyMovement.SprintSpeed -= hopToItBoost;
    //    Debug.Log($"Speed: {bodyMovement.SprintSpeed}");
    //}
    //IEnumerator HopToItCooldown()
    //{
    //    yield return new WaitForSeconds(hopToItCooldown);
    //    onCooldownHTI = false;
    //    Debug.Log($"COOLDOWN OVER Speed: {bodyMovement.SprintSpeed}");

    //}


    //DANGER SENSE
    //
    //private void PredatorLineOfSight()
    //{
    //    Collider[] rangeChecks = Physics.OverlapSphere(predatorLOS.transform.position, maxDistance, targetMask);

    //    if (rangeChecks.Length != 0)
    //    {
    //        Transform currentTarget;
    //        Vector3 directionToTarget;
    //        foreach (Collider collider in rangeChecks)
    //        {
    //            currentTarget = collider.transform;
    //            directionToTarget = (currentTarget.position - predatorLOS.transform.position).normalized;
    //            //check if player is in f.o.v.
    //            if (Vector3.Angle(predatorLOS.transform.forward, directionToTarget) < angle / 2)
    //            {
    //                float distanceToTarget = Vector3.Distance(predatorLOS.transform.position, currentTarget.position);
    //                Ray ray = new Ray(predatorLOS.transform.position, directionToTarget);
    //                RaycastHit hit;
    //                Physics.Raycast(ray, out hit, distanceToTarget);
    //                if (hit.collider != null)
    //                {
    //                    if (targetMask == (1 << hit.collider.gameObject.layer))
    //                    {
    //                        collider.GetComponentInParent<Perks>().PreyInLineOfSight();
    //                        if (drawDebugLines)
    //                        {
    //                            Debug.DrawLine(predatorLOS.transform.position, currentTarget.position, Color.green);
    //                        }
    //                    }
    //                    else
    //                    {
    //                        collider.GetComponentInParent<Perks>().DeactivateDangerIcon();
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                collider.GetComponentInParent<Perks>().DeactivateDangerIcon();
    //            }
    //        }
    //    }
    //}
    //public void DeactivateDangerIcon()
    //{
    //    if (!IsOwner) return;

    //    if ((bodyMovement.characterId.Value == (int)(prey.RABBIT)) || (bodyMovement.characterId.Value == (int)(prey.HEDGEHOG)))
    //    {
    //        if (dangerSenseIcon != null && dangerSenseIcon.GetComponent<Image>().IsActive())
    //        {
    //            Debug.Log("no longer seen");
    //            dangerSenseIcon.SetActive(false);
    //        }
    //    }
    //}
    //public void PreyInLineOfSight()
    //{
    //    if (!IsOwner) return;

    //    if ((bodyMovement.characterId.Value == (int)(prey.RABBIT)) || (bodyMovement.characterId.Value == (int)(prey.HEDGEHOG)))
    //    {
    //        if (dangerSenseIcon != null && !dangerSenseIcon.GetComponent<Image>().IsActive())
    //        {
    //            Debug.Log("Predator can see prey... uh oh danger!");
    //            dangerSenseIcon.SetActive(true);
    //        }
    //    }
    //}
    //private void OnDrawGizmosSelected()
    //{
    //    if (predatorLOS == null)
    //        return;

    //    Gizmos.color = Color.white;
    //    Gizmos.DrawWireSphere(predatorLOS.transform.position, maxDistance);
    //    Gizmos.color = Color.yellow;
    //    Vector3 drawViewAngle1 = DirectionFromAngleY(predatorLOS.transform.eulerAngles.y, -angle / 2);
    //    Vector3 drawViewAngle2 = DirectionFromAngleY(predatorLOS.transform.eulerAngles.y, angle / 2);
    //    Gizmos.DrawLine(predatorLOS.transform.position, predatorLOS.transform.position + drawViewAngle1 * maxDistance);
    //    Gizmos.DrawLine(predatorLOS.transform.position, predatorLOS.transform.position + drawViewAngle2 * maxDistance);
    //}
    //private Vector3 DirectionFromAngleY(float eulerY, float angleInDegrees)
    //{
    //    angleInDegrees += eulerY;
    //    return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    //}


    //DEEP POCKET
    //
    //private void DeepPocket()
    //{
    //    if (!IsOwner) return;
    //    preyFood.FoodCarryLimit = deepPocketValue;
    //}

    //FASTER ATTACK RECOVERY
    //private void FasterAttackRecover()
    //{
    //    if (!IsOwner) return;
    //    predatorAttack.attackCooldown = boostedAttackCooldown;
    //}
    #endregion
}
