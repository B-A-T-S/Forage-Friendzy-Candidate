using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventUtil : MonoBehaviour
{
    [SerializeField] UnityEvent ue_OnEnable;
    [SerializeField] UnityEvent ue_OnAwake;
    [SerializeField] UnityEvent ue_OnStart;
    
    
    [SerializeField] UnityEvent ue_OnDisable;

    void OnEnable()
    {
        ue_OnEnable?.Invoke();
    }

    void OnDisable()
    {
        ue_OnDisable?.Invoke();
    }

    void Awake()
    {
        ue_OnAwake?.Invoke();
    }

    void Start()
    {
        ue_OnStart?.Invoke();
    }
}
