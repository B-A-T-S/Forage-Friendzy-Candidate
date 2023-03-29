using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnNight_UnityEventUtil : MonoBehaviour
{

    public UnityEvent toDo;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.isNight.OnValueChanged += Execute;
    }

    private void Execute(bool previousValue, bool newValue)
    {
        if(newValue)
            toDo?.Invoke();
    }
    
}
