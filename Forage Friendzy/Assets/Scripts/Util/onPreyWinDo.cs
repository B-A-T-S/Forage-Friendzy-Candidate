using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class onPreyWinDo : MonoBehaviour
{

    [SerializeField]
    UnityEvent Alistor;

    // Use this for initialization
    void Start()
    {
        GameManager.Instance.onPreyWin += ObjectEnabler;
    }

    private void ObjectEnabler()
    {
        Alistor.Invoke();
    }

}
