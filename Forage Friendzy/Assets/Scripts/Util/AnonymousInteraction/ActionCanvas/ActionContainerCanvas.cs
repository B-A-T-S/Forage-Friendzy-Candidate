using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FadingCanvasGroup))]
public class ActionContainerCanvas : NetworkBehaviour
{
    [SerializeField] protected GameObject promptParent;
    [SerializeField] protected Image radialImage;
    [SerializeField] protected Image inputIconImage;
    [SerializeField] protected TextMeshProUGUI actionNameText;
    [SerializeField] protected string inputIconFilePath = "InputIcons/";

    protected FadingCanvasGroup fcg;
    [HideInInspector] public bool isPromptVisible = false;

    protected void Start()
    {
        fcg = GetComponent<FadingCanvasGroup>();
        //ensure radial image is radial
        if (radialImage != null)
        {
            radialImage.type = Image.Type.Filled;
            radialImage.fillMethod = Image.FillMethod.Radial360;
            radialImage.fillOrigin = 2;
            radialImage.fillAmount = 0;
        }
    }

    public void Show(ActionContainer action)
    {
        //Load input icon
        Sprite iconSprite = Resources.Load<Sprite>($"{inputIconFilePath}{action.inputKey.ToString()}");
        inputIconImage.sprite = iconSprite;
        //Set name text
        actionNameText.text = action.displayName;
        //Fade in UI
        fcg.FadeIn();
        isPromptVisible = true;
    }

    public void Hide()
    {
        //Fade out UI
        fcg.FadeOut();
        isPromptVisible = false;
    }

    public void UpdateRadialProgress(AnonymousProvider source, float currentTime, float requiredTime)
    {
        if (requiredTime == 0)
            return;

        radialImage.fillAmount = currentTime / requiredTime;
    }

}