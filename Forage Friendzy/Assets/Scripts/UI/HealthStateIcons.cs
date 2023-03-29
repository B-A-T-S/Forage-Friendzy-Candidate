using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class HealthStateIcons : MonoBehaviour
{

    #region State Images References
    [Header("Hedgehog State Icons")]
    [Tooltip("Hedgehog healthy state icon")]
    [SerializeField] private Image hedgehogHealthyStateIcon;
    [Tooltip("Hedgehog injured state icon")]
    [SerializeField] private Image hedgehogInjuredStateIcon;
    [Tooltip("Hedgehog fainted state icon")]
    [SerializeField] private Image hedgehogFaintedStateIcon;
    [Header("Rabbit State Icons")]
    [Tooltip("Hedgehog healthy state icon")]
    [SerializeField] private Image rabbitHealthyStateIcon;
    [Tooltip("Hedgehog injured state icon")]
    [SerializeField] private Image rabbitInjuredStateIcon;
    [Tooltip("Hedgehog fainted state icon")]
    [SerializeField] private Image rabbitFaintedStateIcon;
    [Header("Chipmunk State Icons")]
    [Tooltip("Hedgehog healthy state icon")]
    [SerializeField] private Image chipmunkHealthyStateIcon;
    [Tooltip("Hedgehog injured state icon")]
    [SerializeField] private Image chipmunkInjuredStateIcon;
    [Tooltip("Hedgehog fainted state icon")]
    [SerializeField] private Image chipmunkFaintedStateIcon;
    #endregion

    #region Instance
    private static HealthStateIcons instance;
    #endregion

    public static HealthStateIcons Instance
    {
        get { return instance; }
    }

    private void Start()
    {
        if (instance == null)
            instance = this;
    }

    public void SetHealthyStateIcon(int character, bool isHealthy)
    {
        if(character == (int)(prey.HEDGEHOG))
        {
            hedgehogHealthyStateIcon.gameObject.SetActive(isHealthy);
        }else if(character == (int)(prey.RABBIT))
        {
            rabbitHealthyStateIcon.gameObject.SetActive(isHealthy);
        }
        else if(character == (int)(prey.CHIPMUNK))
        {
            chipmunkHealthyStateIcon.gameObject.SetActive(isHealthy);
        }
    }
    
    public void SetInjuredStateIcon(int character, bool isInjured)
    {
        if(character == (int)(prey.HEDGEHOG))
        {
            hedgehogInjuredStateIcon.gameObject.SetActive(isInjured);
        }else if(character == (int)(prey.RABBIT))
        {
            rabbitInjuredStateIcon.gameObject.SetActive(isInjured);
        }
        else if(character == (int)(prey.CHIPMUNK))
        {
            chipmunkInjuredStateIcon.gameObject.SetActive(isInjured);
        }
    }
    
    public void SetFaintedStateIcon(int character, bool isFainted)
    {
        if(character == (int)(prey.HEDGEHOG))
        {   
            hedgehogFaintedStateIcon.gameObject.SetActive(isFainted);
        }else if(character == (int)(prey.RABBIT))
        {
            rabbitFaintedStateIcon.gameObject.SetActive(isFainted);
        }
        else if(character == (int)(prey.CHIPMUNK))
        {
            chipmunkFaintedStateIcon.gameObject.SetActive(isFainted);
        }
    }
}
