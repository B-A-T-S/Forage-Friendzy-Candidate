using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class am_I_Moving : MonoBehaviour
{

    [SerializeField] private BodyMovement bodyMovement;
    [SerializeField] private List<Camera> camerasToChange;
    [SerializeField] private FadingCanvasGroup vignetteEffect;

    [Header("Properties")]
    [SerializeField] private float effectFadeInTime = 1f;
    [SerializeField] private float effectFadeOutTime = 1f;
    [SerializeField] private float cooldown = 2f;
    [Tooltip("Determines at what speed predator is considered \"Moving\". A speed less than this is considered \"Full Rest\"")]
    [SerializeField] private float speedThreshold = 0.1f;



    public bool isEnabled;
    [HideInInspector] public bool isMovingThisFrame = true;
    [HideInInspector] public float timeSinceLastDisable;

    /// <summary>
    /// while at full rest, check to see if can enable mechanic
    /// After enabling mechanic, start cooldown
    /// </summary>

    private void Start()
    {
        timeSinceLastDisable = cooldown + 1;
        vignetteEffect.event_OnFadedOut += () => 
        {

            foreach (Camera c in camerasToChange)
                c.cullingMask -= LayerMask.GetMask("SmellTrail");

        };
    }

    // Update is called once per frame
    void Update()
    {
        //zero out vertical component of speed
        Vector3 noYMagnitude = bodyMovement.CurrentVelocity;
        noYMagnitude.y = 0;

        //if moving
        if(noYMagnitude.magnitude >= speedThreshold)
        {
            //if started moving this frame
            if (!isMovingThisFrame)
                DisableMechanic();
            else
                timeSinceLastDisable += Time.deltaTime;

            isMovingThisFrame = true;
        }
        //if not moving
        else if (noYMagnitude.magnitude < speedThreshold)
        {
            //if cooldown is up
            if (timeSinceLastDisable >= cooldown)
                EnableMechanic();
            else
                timeSinceLastDisable += Time.deltaTime;

            isMovingThisFrame = false;
        }
    }

    private void EnableMechanic()
    {
        if (isEnabled)
            return;

        isEnabled = true;

        foreach (Camera c in camerasToChange)
            c.cullingMask += LayerMask.GetMask("SmellTrail");

        vignetteEffect.FadeIn(effectFadeInTime);
    }

    private void DisableMechanic()
    {

        if (!isEnabled)
            return;

        isEnabled = false;
        timeSinceLastDisable = 0f;
        vignetteEffect.FadeOut(effectFadeOutTime);
    }
}
