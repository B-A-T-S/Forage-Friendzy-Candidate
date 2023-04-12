using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControllerMenuNavigation : MonoBehaviour
{
    private bool controllerConnected;
    [SerializeField] private Selectable selectOnConnection;
    string[] controllers;

    private void Start()
    {
        controllers = new string[1];
        StartCoroutine(CheckForControllers());
    }

    IEnumerator CheckForControllers()
    {
        while (true)
        {
            controllers = Input.GetJoystickNames();
            if (!controllerConnected && controllers.Length > 0)
            {
                Debug.Log("CONTROLLER CONNECTED");
                controllerConnected = true;
                selectOnConnection.Select();
            }else if (controllerConnected && controllers.Length == 0)
            {
                Debug.Log("CONTROLLER DISCONNECTED");
                controllerConnected = false;
                EventSystem.current.SetSelectedGameObject(null);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public void SelectNextButton(Selectable button)
    {
        if(controllerConnected)
            button.Select();
    }
}
