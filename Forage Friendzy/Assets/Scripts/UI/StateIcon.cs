using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StateIcon : MonoBehaviour
{
    public bool initialized;
    public int character;
    public string[] names = { "HedgehogGEO", "RabbitGEO", "ChipmunkGEO"};

    [HideInInspector]
    public GameObjectCollection activeStateCollection;

    [Header("Hedgehog State Icons")]
    [Tooltip("Hedgehog healthy state icon")]
    [SerializeField] private GameObjectCollection hedgehogStateCollection;
    [Header("Rabbit State Icons")]
    [Tooltip("Hedgehog healthy state icon")]
    [SerializeField] private GameObjectCollection rabbitStateCollection;
    [Header("Chipmunk State Icons")]
    [Tooltip("Hedgehog healthy state icon")]
    [SerializeField] private GameObjectCollection chipmunkStateCollection;

    public void InitIcon(ulong playerId, GameObject geometry, Controlled3DBody controlled3dBody)
    {
        if (NetworkManager.Singleton.LocalClientId == playerId)
            return;
        int i;
        for(i = 0; i < names.Length; i++)
        {
            if (geometry.name == names[i]) 
                break;
        }
        switch (i)
        {
            case 0:
                activeStateCollection = hedgehogStateCollection;
                break;
            case 1:
                activeStateCollection = rabbitStateCollection;
                break;
            case 2:
                activeStateCollection = chipmunkStateCollection;
                break;
        }
        activeStateCollection?.ToggleByIndex((int)StateIndex.HEALTHY, true);
        controlled3dBody.GetComponent<PreyHealth>().event_OnTookDamage += OnStateChanged;
        activeStateCollection.transform.SetParent(TeamStateIcons.Instance.iconSpaces.ElementAt(TeamStateIcons.Instance.activeIcons.Count).transform);
        TeamStateIcons.Instance.activeIcons.Add(this);
    }

    private void OnStateChanged(bool isInjured, bool isFainted)
    {
        activeStateCollection.ToggleByIndex((int)StateIndex.HEALTHY, !(isInjured || isFainted));
        activeStateCollection.ToggleByIndex((int)StateIndex.INJURED, isInjured);
        activeStateCollection.ToggleByIndex((int)StateIndex.FAINTED, isFainted);
    }

    enum StateIndex
    {
        HEALTHY,
        INJURED,
        FAINTED
    }
}
