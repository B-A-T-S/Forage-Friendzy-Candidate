using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerTypeDetection : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var gamepad = Input.GetJoystickNames();

        if (gamepad.Length == 0)
            Debug.Log("No Controller Connected");
        
        foreach(string s in gamepad)
        {
            Debug.Log(s);
        }
    }
}
