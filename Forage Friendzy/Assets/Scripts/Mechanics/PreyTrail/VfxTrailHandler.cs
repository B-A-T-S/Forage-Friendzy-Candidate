using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;
using static UnityEngine.GraphicsBuffer;

public class VfxTrailHandler : NetworkBehaviour
{
    [SerializeField] private VisualEffect vfx;
    [SerializeField] private int vfx_ThreshMax = 100;
    [SerializeField] private int vfx_ThreshMin = 1;
    [SerializeField] AnimalGeometryUtilities geoUtil;

    [SerializeField] private NetworkVariable<float> copyOfVelocityMagnitude;
    private BodyMovement bodyMovement;


    public override void OnNetworkSpawn()
    {
        copyOfVelocityMagnitude.Value = 0f;
        copyOfVelocityMagnitude.OnValueChanged += ServerUpdatedVelocity;
    }

    private void ServerUpdatedVelocity(float previousValue, float newValue)
    {
        //Debug.Log($"Velocity From Server: {previousValue} -> {newValue}");
    }

    private void Start()
    {
        bodyMovement = (BodyMovement)geoUtil.Body;
    }

    public void SetThreshVariable(int n)
    {
        if(vfx != null)
            vfx.SetInt("Velocity", n);
    }

    private void SetThresh(float trueSpeed)
    {

        if (bodyMovement != null && trueSpeed > bodyMovement.WalkSpeed)
            SetThreshVariable(vfx_ThreshMax);
        else
            SetThreshVariable(vfx_ThreshMin);
    }

    private void Update()
    {

        //if owned client
        //just grab vel from body like before

        //if host
        //grab vel from body, update the net var

        //if not owned client
        //use net var

        //local client, host
        if(IsOwner || IsServer)
        {

            if (bodyMovement == null)
                return;

            Vector3 noYMagnitude = bodyMovement.CurrentVelocity;
            noYMagnitude.y = 0;

            float trueSpeed = noYMagnitude.magnitude;

            SetThresh(trueSpeed);
            if(IsServer)
            {
                copyOfVelocityMagnitude.Value = trueSpeed;
            }
        }
        //non local client of this instance
        else if (!IsOwner)
        {
            SetThresh(copyOfVelocityMagnitude.Value);
        }

    }

    

}
