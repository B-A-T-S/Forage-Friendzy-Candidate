using System.Collections;
using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(BodyMovement))]
public class AnimalScurry : NetworkBehaviour
{

    public float scurryTime = 2;

    bool isScurrying, canScurry, canScurryOverride;
    public bool IsScurrying { get { return isScurrying; } }
    public bool CanScurry { get { return canScurry; } }
    public bool CanScurryOverride { get { return canScurryOverride; } }

    BodyMovement bodyMovement;
    CharacterController controller;

    public event Action event_StartedScurry;
    public event Action event_EndedScurry;

    // Use this for initialization
    void Start()
    {
        bodyMovement = GetComponent<BodyMovement>();
        controller = bodyMovement.GetCharacterController();
        isScurrying = false;
        canScurry = true;
        canScurryOverride = false;
    }

    public void Predator_DestroyScurry(DestructScurryData scurry)
    {
        //predator destroy anim here
        scurry.DestroyScurry();
    }

    public void Prey_MakeScurryDestrucible(DestructScurryData scurry)
    {
        //prey set up destroy anim here
        scurry.MakeDestructible();
    }

    public void Animal_PerformScurry(ScurryEntrance entrancePoint)
    {
        StartLocalScurry(entrancePoint);
    }

    public void StartLocalScurry(ScurryEntrance entrancePoint)
    {
        StartCoroutine(LocalScurryCoroutine(entrancePoint));
    }

    IEnumerator LocalScurryCoroutine(ScurryEntrance entrance)
    {
        isScurrying = true;
        canScurry = false;
        Collider[] toIgnore = entrance.parentData.colliderHolder.presentColliders.ToArray();
        DisableScurryCollisions(toIgnore);
        event_StartedScurry?.Invoke();

        ScurryData scurryData = entrance.parentData;
        Transform startingPoint = entrance.transform;
        Transform targetPoint = entrance.parentData.GetTargetViaStart(entrance).transform;

        //Blink to Start Rotation, switch from playerCam to startCam
        bodyMovement.geoUtility.FaceDirectionBlink(scurryData.GetScurryDirection(entrance));

        //Start of Movement
        float elapsedTime = 0f;
        Vector3 startingPos = startingPoint.position;
        Vector3 targetPos = targetPoint.position;

        while (elapsedTime < scurryTime)
        {
            Vector3 newPos = Vector3.Lerp(startingPos, targetPos, elapsedTime / scurryTime);
            transform.Translate(newPos - transform.position, Space.World);
            //controller.Move(newPos - transform.position);
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        //End of Movement

        //snap to target
        transform.position = targetPos;

        isScurrying = false;
        canScurry = true;
        EnableScurryCollisions(toIgnore);
        event_EndedScurry?.Invoke();

    }

    private void DisableScurryCollisions(Collider[] toIgnore)
    {   
        bodyMovement.ToggleCollisionsWithCollection(toIgnore, false);
        bodyMovement.ToggleCharacterController(false);
    }

    private void EnableScurryCollisions(Collider[] areIgnored)
    {
        bodyMovement.ToggleCollisionsWithCollection(areIgnored, true);
        bodyMovement.ToggleCharacterController(true);
    }
}