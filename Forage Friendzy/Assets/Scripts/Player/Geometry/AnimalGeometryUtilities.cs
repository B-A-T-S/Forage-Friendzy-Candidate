using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Unity.Netcode;
using System;

public class AnimalGeometryUtilities : NetworkBehaviour
{

    [Header("References")]
    [SerializeField] private Controlled3DBody body;
    public Controlled3DBody Body 
    {
        get { return body; }
        set { body = value; } 
    }

    [Header("FMD System")]
    [SerializeField] private Transform FMD_toRotate;
    [SerializeField] private float FMD_turnSpeed;

    [Header("Slope Alignment")]
    [SerializeField] CustomLayers alignToLayer;
    [SerializeField] float rayDistance;
    [SerializeField] bool lerpAlignment = true;
    [SerializeField] float lerpDuration = 1f;
    private float elapsedTime = 0f;
    private Quaternion startingQuat = Quaternion.identity;
    private Quaternion targetQuat = Quaternion.identity;

    [Header("X-Ray")]
    [SerializeField] private GameObject xRayGeometry;
    [HideInInspector] public NetworkVariable<bool> xRay_IsVisible;

    public override void OnNetworkSpawn()
    {
        xRay_IsVisible.OnValueChanged += xRayIsVisible_OnValueChanged;
        //StartCoroutine(LerpQuaternion_Loop());
    }

    #region Face Movement Direction System
    public void AlignToNormal()
    {

        RaycastHit slopeHit;
        if(Physics.Raycast(new Ray(transform.position, Vector3.down), out slopeHit, rayDistance, (int) alignToLayer))
        {
            Vector3 slopeNormal = slopeHit.normal;
            FMD_toRotate.rotation = Quaternion.FromToRotation(transform.up, slopeNormal) * transform.rotation;
            //startingQuat = FMD_toRotate.rotation;
            //targetQuat = Quaternion.FromToRotation(transform.up, slopeNormal) * transform.rotation;
        }

    }

    IEnumerator LerpQuaternion_Loop()
    {
        while(lerpAlignment)
        {
            FMD_toRotate.rotation = Quaternion.Lerp(FMD_toRotate.rotation, targetQuat, elapsedTime / lerpDuration);
            elapsedTime += Time.deltaTime;
            if(elapsedTime >= 1f)
            {
                elapsedTime = 0;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void DetermineDirectionViaInput(InputPayload input)
    {
        if (body == null || FMD_toRotate == null)
            return;

        //continue only if input is being sent
        if (input.inputVector != Vector3.zero)
            FaceDirectionSlerp(input, input.inputVector);

    }

    public void FaceDirectionSlerp(InputPayload inputPayload, Vector3 inputVector)
    {
        Vector3 cameraForward = inputPayload.cameraForward;
        cameraForward.y = 0;
        Vector3 cameraRight = inputPayload.cameraRight;
        cameraRight.y = 0;

        Vector3 currentLookDir = cameraForward * inputVector.z + cameraRight * inputVector.x;

        Quaternion targetRotation = Quaternion.LookRotation(currentLookDir, math.up());
        FMD_toRotate.rotation = math.slerp(FMD_toRotate.rotation, targetRotation, FMD_turnSpeed);
    }

    public void FaceDirectionBlink(Vector3 newLook)
    {
        Quaternion targetRotation = Quaternion.LookRotation(newLook, Vector3.up);
        FMD_toRotate.rotation = targetRotation;
    }
    #endregion

    #region X Ray Geometry

    private void xRayIsVisible_OnValueChanged(bool previousValue, bool newValue)
    {
        Debug.Log($"{body.name}'s X-Ray Geometry is now {newValue}");
        xRayGeometry.SetActive(newValue);
    }

    public void ToggleXRayGeometry(bool isVisible)
    {
        ToggleXRayGeometryServerRpc(isVisible);
    }

    [ServerRpc(RequireOwnership = false)]
    void ToggleXRayGeometryServerRpc(bool isVisible)
    {
        xRay_IsVisible.Value = isVisible;
    }

    #endregion

}
