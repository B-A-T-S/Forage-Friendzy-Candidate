using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class ProximityTerror: MonoBehaviour
{
    #region Declarations

    #region Participants

    [SerializeField]
    private AudioSource localAudioSource; //the camera itself will have an audio source - DO NOT NETWORK THIS AUDIO SOURCE
    [SerializeField]
    private Transform myLocation;
    [SerializeField]
    private float activationDistance = 25;
    [SerializeField]
    private List<GameObject> isAfraidOf;
    private float slyFoxPerkMultiplier;

    #endregion

    #region Internal Variables

    private float lowestDistOfFrame, currentMemberDist;
private Transform workingTransform;
private float volumeMultiplier;

#endregion

#endregion

    private void Start()
    {
        StartCoroutine(NetConnectionDelay());
        slyFoxPerkMultiplier = GetComponent<Perks>().SlyPerkMultiplier;
    }

    private IEnumerator NetConnectionDelay()
    {

        yield return new WaitForSeconds(1);

        //grab list of preds from GameManager
        isAfraidOf.AddRange(GameManager.Instance.predatorTeam.Values.ToList());
        //Debug.Log("now googling the sex offender registry");
        //Debug.Log(isAfraidOf.Count);
        foreach (GameObject sexOffender in isAfraidOf)
        {
            Debug.Log(sexOffender.name);
        }

        NetworkManager.Singleton.OnClientConnectedCallback += RefreshAfraidOf;
        NetworkManager.Singleton.OnClientDisconnectCallback += RefreshAfraidOf;

    }

    private void RefreshAfraidOf(ulong playerId)
    {
        isAfraidOf.Clear();
        isAfraidOf.AddRange(GameManager.Instance.predatorTeam.Values.ToList());
    }

    // Update is called once per frame
    void Update()
    {
        lowestDistOfFrame = float.MaxValue;

        foreach (GameObject currentPred in isAfraidOf)
        {

            if (currentPred == null)
                continue;

            workingTransform = currentPred.transform;
            currentMemberDist = Vector3.Distance(myLocation.position, workingTransform.position);
            if (currentPred.GetComponent<BodyMovement>().characterId.Value == (int)(predator.FOX))
                currentMemberDist *= slyFoxPerkMultiplier;
            if (currentMemberDist < lowestDistOfFrame) lowestDistOfFrame = currentMemberDist;
        }

        volumeMultiplier = ApplyEquation();

        //Debug.Log(volumeMultiplier);

        localAudioSource.volume = volumeMultiplier;
    }

    private float ApplyEquation()
    {
        //Debug.Log("distance is " + lowestDistOfFrame);

        return (-0.00128571f * (MathF.Pow(lowestDistOfFrame * (25.0f / activationDistance), 2)) + (-0.00785714f * lowestDistOfFrame * (25.0f / activationDistance)) + 1); // for now
    }
}