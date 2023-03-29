using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabMenuController : MonoBehaviour
{
    [Header("Objective Menu")]
    [Tooltip("Any images you would like appear in the objective menu")]
    [SerializeField] private Image[] tabMenuImages;
    [SerializeField] private GameObject preyMenu, predMenu;
    [Tooltip("The time length that the objective menu will display in the initial phase")]
    [SerializeField] private float initialVisibilityTime;
    [Tooltip("Reference to get players keybind")]
    [SerializeField] private PlayerController playerController;
    [Tooltip("Check to get rid of inital phase menu for debugging purposes")]
    [SerializeField] private bool deactivateInitialTimer;
    private bool initialPhaseOver;

    private bool isPrey;

    private void Start()
    {
        if (deactivateInitialTimer)
        {
            initialPhaseOver = true;
            return;
        }

        StartCoroutine(StartPhaseVisibility());
    }

    private void Update()
    {
        if (Input.GetKeyDown(playerController.scoreboard))
        {
            (isPrey ? preyMenu : predMenu).gameObject.SetActive(true);
        }

        if (Input.GetKeyUp(playerController.scoreboard))
        {
            (isPrey ? preyMenu : predMenu).gameObject.SetActive(false);
            //Debug.Log("just disabled " + (isPrey ? "preyMenu" : "predMenu"));
        }
    }

    IEnumerator StartPhaseVisibility()
    {
        yield return new WaitForSeconds(0.5f);

        //am I prey
        isPrey = (ClientLaunchInfo.Instance.role == 0);


        (isPrey ? preyMenu : predMenu).gameObject.SetActive(true);


        yield return new WaitForSeconds(initialVisibilityTime);

        (isPrey ? preyMenu : predMenu).gameObject.SetActive(false);
        GameManager.Instance.predatorDoor.SetActive(false);

        initialPhaseOver = true;
    }
}
