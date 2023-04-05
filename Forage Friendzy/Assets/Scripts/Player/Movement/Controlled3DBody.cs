using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Runtime.Remoting;

public class Controlled3DBody: ControlledBody
{
    [Header("References")]

    public ThirdPersonCamera cameraReference;
    [HideInInspector] public AnimalGeometryUtilities geoUtility;
    [SerializeField] protected PredatorAttack predatorAttack;
    [SerializeField] protected PreyRescue preyRescue;
    [SerializeField] protected PreyHealth preyHealth;
    [SerializeField] protected AnimalScurry scurryComponent;
    [SerializeField] protected Perks perkComponent;
    public NetworkVariable<int> characterId;
    public Transform geometryParent;

    [Header("Information")]
    [SerializeField] protected Vector3 currentLookDirection;


    #region Animation
    [Header("Animator Floats")]
    [SerializeField] protected string animFloat_Speed;

    [Header("Animator Bools")]
    [SerializeField] protected string animBool_ZeroInput;
    [SerializeField] protected string animBool_IsGrounded;
    [SerializeField] protected string animBool_IsInjured; 
    [SerializeField] protected string animBool_IsFainted; 
    [SerializeField] protected string animBool_IsScurrying; 

    [Header("Animator Triggers")]
    [SerializeField] protected string animTrigger_Attack; 
    [SerializeField] protected string animTrigger_React;
    [SerializeField] protected string animTrigger_Dig;

    protected AnimatorCollection animCollection;

    #endregion


    public override void OnNetworkSpawn()
    {
        //object attempts to link with an inscene controller
        base.OnNetworkSpawn();

        //object attempts to find an unlinked geometry object
        GameObject[] geos = GameObject.FindGameObjectsWithTag("BodyGeometry");
        foreach(GameObject geometryObject in geos)
        {
            Pair(geometryObject);
        }

        //Enable Cosmetic Prefab


    }

    public void Pair(GameObject toPair)
    {
        NetworkObject no = toPair.GetComponent<NetworkObject>();
        AnimalGeometryUtilities geoUtil = toPair.GetComponent<AnimalGeometryUtilities>();

        if (geoUtil != null && geoUtil.Body == null && no != null && no.OwnerClientId == OwnerClientId)
        {
            AssignGeometry(toPair, NetworkManager.Singleton.IsServer);
            AssignAnimator(toPair);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        //remove me from gamemanager lists
        GameManager.Instance.RemoveFromPlayerList(OwnerClientId, this.gameObject);
    }

    #region Animation

    public void AssignAnimator(GameObject newAnimator)
    {
        animCollection = newAnimator.GetComponent<AnimatorCollection>();

        if(preyHealth != null)
        {
            preyHealth.event_OnTookDamage += OnTookDamage;
            preyHealth.event_OnRescued += OnRescued;



        }

        if(predatorAttack != null)
        {
            predatorAttack.event_OnAttack += OnAttack;
        }
            
        if(scurryComponent != null)
        {
            scurryComponent.event_StartedScurry += OnScurry;
            scurryComponent.event_EndedScurry += OnScurry;
        }

    }

    #region Event Reacts
    private void OnRescued()
    {
        Anim_SetBoolean(animBool_IsFainted, false);
        Anim_SetBoolean(animBool_IsInjured, true);
    }

    private void OnTookDamage(bool isInjured, bool isFainted)
    {
        if (!isFainted && isInjured)
            Anim_SetTrigger(animTrigger_React);

        Anim_SetBoolean(animBool_IsInjured, isInjured);
        Anim_SetBoolean(animBool_IsFainted, isFainted);
    }

    private void OnAttack()
    {
        Anim_SetTrigger(animTrigger_Attack);
    }

    private void OnScurry()
    {
        bool isScurrying = scurryComponent.IsScurrying;
        Anim_SetBoolean(animBool_IsScurrying, isScurrying);
    }

    #endregion

    public void Anim_SetFloat(string varName, float newValue)
    {
        if (!string.IsNullOrEmpty(varName))
            animCollection?.SetFloat(varName, newValue);
    }

    public void Anim_SetBoolean(string varName, bool newValue)
    {
        if (!string.IsNullOrEmpty(varName))
            animCollection?.SetBool(varName, newValue);
    }

    public void Anim_SetTrigger(string varName)
    {
        if (!string.IsNullOrEmpty(varName))
            animCollection?.SetTrigger(varName);
    }
    #endregion

    #region Geometry Setup

    public void AssignGeometry(GameObject newGeometry, bool isServer)
    {

        //get fmd
        AnimalGeometryUtilities newGeo = newGeometry.GetComponent<AnimalGeometryUtilities>();
        if(newGeo != null )
        {
            geoUtility = newGeo;
            geoUtility.Body = this;
        }

        bool isPredator = false;
        if(predatorAttack != null)
        {
            //get attack transform
            Transform attackTransform = newGeometry.GetComponentInChildren<AttackInfo>().gameObject.transform;
            predatorAttack.attackTransform = attackTransform;
            isPredator = true;

            ToggleXRayCameras(false, true);
            ToggleXRayGeometry(true);

        }
        
        if(preyRescue != null)
        {
            GameObject rescueAreaGO = newGeometry.GetComponentInChildren<RescueInfo>().gameObject;
            preyHealth.RescueArea = rescueAreaGO;
            preyHealth.RescueArea.SetActive(false);
            preyRescue.CanRescue = false;

            ToggleXRayCameras(true, false);
            ToggleXRayGeometry(true);

        }

        if(IsServer)
            newGeometry.transform.parent = geometryParent;
        newGeometry.transform.localPosition = Vector3.zero;

        //add this object to object lists in GameManager
        GameManager.Instance.AddToPlayerList(OwnerClientId, this.gameObject, isPredator);

        TeamStateIcons.Instance?.TeammateStateUpdateClientRpc(isPredator);
    }

    public void ParentGeometry(GameObject newGeometry)
    {
        newGeometry.transform.parent = geometryParent;
        newGeometry.transform.localPosition = Vector3.zero;
    }

    public void UnassignGeometry()
    {
        geoUtility.Body = null;
        geoUtility = null;
    }

    #endregion
    /*
    #region Slope and Ground Checks

    public bool OnSlope()
    {
        float angle = GetAngleOfGround();

        currentSlopeAngle = angle;

        return angle < maxSlopeAngle && angle != 0f;
    }

    private float GetAngleOfGround()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeRaycastHit, groundCheckDistance) )
        {
            currentSlopeNormal = slopeRaycastHit.normal;
            return Vector3.Angle(Vector3.up, slopeRaycastHit.normal);
        }

        //if there's no ground within the checkDistance, then we're probably in the air
        return 0f;
    }

    protected bool IsGrounded()
    {
        
        Anim_SetGrounded(isGrounded);
        return isGrounded;
    }

    #endregion
    */
    #region X Ray

    public bool XRayIsVisible()
    {
        return geoUtility.xRay_IsVisible.Value;
    }

    public bool XRayCanSeePrey()
    {
        return cameraReference.xRay_CanSeePrey.Value;
    }

    public bool XRayCanSeePred()
    {
        return cameraReference.xRay_CanSeePred.Value;
    }

    public void ToggleXRayCameras(bool canSeePrey, bool canSeePred)
    {
        ToggleXRayCamerasServerRpc(canSeePrey, canSeePred);
    }

    [ServerRpc(RequireOwnership = false)]
    void ToggleXRayCamerasServerRpc(bool canSeePrey, bool canSeePred)
    {
        cameraReference.xRay_CanSeePrey.Value = canSeePrey;
        cameraReference.xRay_CanSeePred.Value = canSeePred;
    }

    public void ToggleXRayGeometry(bool isVisible)
    {
        ToggleXRayGeometryServerRpc(isVisible);
    }

    [ServerRpc(RequireOwnership = false)]
    void ToggleXRayGeometryServerRpc(bool isVisible)
    {
        geoUtility.xRay_IsVisible.Value = isVisible;
    }

    #endregion
    

}