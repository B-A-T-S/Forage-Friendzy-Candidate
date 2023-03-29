using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class onPredWinDo : MonoBehaviour
{

    [SerializeField]
    UnityEvent Alistor;

    // Use this for initialization
    void Start()
    {
        GameManager.Instance.onPredatorWin += ObjectEnabler;
    }

    private void ObjectEnabler()
    {
        Alistor.Invoke();
    }

}
