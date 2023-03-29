using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Action Canvas that exists in World Space. Billboards to player character
/// </summary>
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(FadingCanvasGroup))]
public class ActionContainerWorldCanvas : ActionContainerCanvas
{
    private Transform target;

    protected void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    { 
        if(target == null && GameManager.Instance.localPlayer != null)
            target = GameManager.Instance.localPlayer.GetComponent<Controlled3DBody>().cameraReference.MyCamera.transform;

        if (isPromptVisible && target != null)
            promptParent.transform.LookAt(target);
    }

}