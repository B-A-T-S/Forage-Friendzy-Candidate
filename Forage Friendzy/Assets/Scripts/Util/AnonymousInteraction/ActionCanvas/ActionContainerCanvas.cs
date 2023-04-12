using System.Collections;
using System.Linq;
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

    [SerializeField] protected string xboxSuffix = "_xctrl";
    [SerializeField] protected string psSuffix = "_psctrl";
    [SerializeField] protected string keyboardSuffix = "_kb";

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
        Sprite iconSprite = LoadInputIcon(action);
        inputIconImage.sprite = iconSprite;
        //Set name text
        actionNameText.text = action.displayName;
        //Fade in UI
        fcg.FadeIn();
        isPromptVisible = true;
    }

    public Sprite LoadInputIcon(ActionContainer action)
    {
        string resultingPath = "" + inputIconFilePath + "" + action.inputKey;
        string[] gamepad = Input.GetJoystickNames();
        if (gamepad.Length != 0)
        {
            if(gamepad[0].ToLower().Contains("xbox"))
            {
                //add xbox controller suffix
                resultingPath += xboxSuffix;
            }
            else if (gamepad[0].ToLower().Contains("ps"))
            {
                //add ps controller suffix
                resultingPath += psSuffix;
            }
            else
            {
                //add keyboard suffix if controller is not xbox or ps
                resultingPath += keyboardSuffix;
            }
        }
        else
        {
            //add the keyboard suffix
            resultingPath += keyboardSuffix;
        }

        Debug.Log($"Returned {resultingPath}");
        return Resources.Load<Sprite>(resultingPath);
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