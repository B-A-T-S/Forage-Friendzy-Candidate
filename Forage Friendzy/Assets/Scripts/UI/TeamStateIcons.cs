using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TeamStateIcons : MonoBehaviour
{
    #region Teammates
    public List<GameObject> iconSpaces;
    public List<StateIcon> activeIcons;
    #endregion

    [Header("Hedgehog State Icons")]
    [Tooltip("Hedgehog healthy state icon")]
    [SerializeField] private Image[] hedgehogHealthyStateIcon;
    [Tooltip("Hedgehog injured state icon")]
    [SerializeField] private Image[] hedgehogInjuredStateIcon;
    [Tooltip("Hedgehog fainted state icon")]
    [SerializeField] private Image[] hedgehogFaintedStateIcon;
    [Header("Rabbit State Icons")]
    [Tooltip("Hedgehog healthy state icon")]
    [SerializeField] private Image[] rabbitHealthyStateIcon;
    [Tooltip("Hedgehog injured state icon")]
    [SerializeField] private Image[] rabbitInjuredStateIcon;
    [Tooltip("Hedgehog fainted state icon")]
    [SerializeField] private Image[] rabbitFaintedStateIcon;
    [Header("Chipmunk State Icons")]
    [Tooltip("Hedgehog healthy state icon")]
    [SerializeField] private Image[] chipmunkHealthyStateIcon;
    [Tooltip("Hedgehog injured state icon")]
    [SerializeField] private Image[] chipmunkInjuredStateIcon;
    [Tooltip("Hedgehog fainted state icon")]
    [SerializeField] private Image[] chipmunkFaintedStateIcon;

    #region Instance
    private static TeamStateIcons instance;
    public static TeamStateIcons Instance { get { return instance; } }
    #endregion

    private void Start()
    {
        if (instance == null)
            instance = this;
    }

    [ClientRpc]
    public void TeammateStateUpdateClientRpc()
    {
        ////if passed info is of a pred OR localClient is a pred...
        //if (info.role == 1 || ClientLaunchInfo.Instance.role == 1)
        //    return;

        ////if passed info belongs to this localClient...
        //if (NetworkManager.Singleton.LocalClientId == id)
        //    return;

        //At this point...
        //Info belongs to a teammate of role prey of the local player
        if (GameManager.Instance.predatorTeam.ContainsKey(NetworkManager.Singleton.LocalClientId))
            return;

        int index = 0;
        foreach (KeyValuePair<ulong, GameObject> p in GameManager.Instance.preyTeam)
        {
            if (p.Key == NetworkManager.Singleton.LocalClientId)
                continue;

            BodyMovement bodyMovement = p.Value.GetComponent<BodyMovement>();
            PreyHealth preyHealth = p.Value.GetComponent<PreyHealth>();

            if (bodyMovement.characterId.Value == (int)(prey.HEDGEHOG))
            {
                hedgehogHealthyStateIcon[index].gameObject.SetActive(!(preyHealth.isInjured.Value || preyHealth.isFainted.Value));
                hedgehogInjuredStateIcon[index].gameObject.SetActive(preyHealth.isInjured.Value);
                hedgehogFaintedStateIcon[index].gameObject.SetActive(preyHealth.isFainted.Value);
            }
            else if (bodyMovement.characterId.Value == (int)(prey.RABBIT))
            {
                rabbitHealthyStateIcon[index].gameObject.SetActive(!(preyHealth.isInjured.Value || preyHealth.isFainted.Value));
                rabbitInjuredStateIcon[index].gameObject.SetActive(preyHealth.isInjured.Value);
                rabbitFaintedStateIcon[index].gameObject.SetActive(preyHealth.isFainted.Value);
            }
            else if (bodyMovement.characterId.Value == (int)(prey.CHIPMUNK))
            {
                chipmunkHealthyStateIcon[index].gameObject.SetActive(!(preyHealth.isInjured.Value || preyHealth.isFainted.Value));
                chipmunkInjuredStateIcon[index].gameObject.SetActive(preyHealth.isInjured.Value);
                chipmunkFaintedStateIcon[index].gameObject.SetActive(preyHealth.isFainted.Value);
            }
            index++;
        }
    }
}
