using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControllerMenuNavigation : MonoBehaviour
{
    private bool controllerConnected;
    [SerializeField] private Selectable selectOnConnection;
    [SerializeField] private GameObject joinButtons;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button backButton;
    [SerializeField] private bool updateNav;
    string[] controllers;

    private void Start()
    {
        controllers = null;
        StartCoroutine(CheckForControllers());
    }

    //private void Update()
    //{
    //    if (updateNav) { 
    //        JoinLobbyNavigation();
    //    }
    //}

    IEnumerator CheckForControllers()
    {
        while (true)
        {

            controllers = Input.GetJoystickNames();
            if (!controllerConnected && controllers.Length != 0 && !string.IsNullOrEmpty(controllers[0]))
            {
                Debug.Log("CONTROLLER CONNECTED");
                controllerConnected = true;
                selectOnConnection.Select();
            }else if (controllerConnected)
            {
                if (controllers == null)
                {
                    Debug.Log("CONTROLLER DISCONNECTED");
                    controllerConnected = false;
                    EventSystem.current.SetSelectedGameObject(null);
                }
                else if (string.IsNullOrEmpty(controllers[0]))
                {
                    Debug.Log("CONTROLLER DISCONNECTED");
                    controllerConnected = false;
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public void SelectNextButton(Selectable button)
    {
        if(controllerConnected)
            button.Select();
    }

    public void JoinLobbyNavigation()
    {
        for(int i = 0; i < joinButtons.transform.childCount; i++)
        {
            GameObject currentLobby = joinButtons.transform.GetChild(i).gameObject;
            Navigation nav = new Navigation();
            Navigation hostNav = new Navigation();
            nav.mode = Navigation.Mode.Explicit;
            if (i == 0)
            {
                nav.selectOnUp = hostButton;
                hostNav.selectOnUp = currentLobby.GetComponent<Selectable>();
            }
            else
                nav.selectOnUp = joinButtons.transform.GetChild(i - 1).gameObject.GetComponent<Selectable>();

            if ((i + 1) >= joinButtons.transform.childCount)
            {
                nav.selectOnDown = hostButton;
                hostNav.selectOnDown = currentLobby.GetComponent<Selectable>();
            }
            else
                nav.selectOnDown = joinButtons.transform.GetChild(i + 1).gameObject.GetComponent<Selectable>();

            hostButton.navigation = hostNav;
            currentLobby.GetComponent<Button>().navigation = nav;
        }
    }

    public void InsertButtonNavigation(Selectable addedSelectable, Selectable left = null, Selectable right= null, Selectable up = null, Selectable down= null)
    {
        if (up != null)
        {
            Navigation navAdded = new Navigation();
            Navigation navExisting = new Navigation();
            navAdded.mode = Navigation.Mode.Explicit;
            navExisting.mode = Navigation.Mode.Explicit;

        }
    }
}
