using System.Collections;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

[RequireComponent(typeof(PlayerController))]
public class DeveloperControls : MonoBehaviour
{

    private bool devToolsEnabled;

    [SerializeField]
    private KeyCode swapKey;

    private PlayerController myController;

    private ushort preyIndex = 0;
    private ushort predIndex = 0;

    [SerializeField]
    private float delay = 2f;
    private bool canDevTool = true;

    #region NumLock Check Reqs
    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)] private static extern short GetKeyState(int keyCode);


    [DllImport("user32.dll")] private static extern int GetKeyboardState(byte[] lpKeyState);


    [DllImport("user32.dll", EntryPoint = "keybd_event")] private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        myController = GetComponent<PlayerController>();


        bool GetNumLock()
        {
            return (((ushort)GetKeyState(0x90)) & 0xffff) != 0;
        }

        //Requres WindowsBase.dll
        if (GetNumLock())
        {
            //if numLock is already down on startup, inform user
            Debug.Log("[DEV TOOL] Developer Tools is ENABLED." +
                "\nDeveloper Tools are accessed via the Num Pad Digit Keys." +
                "\n1: Body Swap - Cycle linkage of attached controller between Prey/Preds in Scene");
            devToolsEnabled = true;
        } 
        else
        {
            Debug.Log("[DEV TOOL] Developer Tools is DISABLED." +
                "\nDeveloper Tools are accessed via the Num Pad Digit Keys." +
                "\n1: Body Swap - Cycle linkage of attached controller between Prey/Preds in Scene");
            devToolsEnabled = false;
        }
        

    }

    // Update is called once per frame
    void Update()
    {

        if (!canDevTool)
            return;

        if(Input.GetKey(KeyCode.Numlock))
        {
            devToolsEnabled = !devToolsEnabled;
            if(devToolsEnabled)
                Debug.Log($"[DEV TOOL] Developer Tools are ENABLED on Controller {gameObject.name}.");
            else
                Debug.Log($"[DEV TOOL] Developer Tools are DISABLED on Controller {gameObject.name}.");

            StartCoroutine(DevToolDelay());
        }
            

        if (Input.GetKey(swapKey))
            BodySwap();
    }

    IEnumerator DevToolDelay()
    {
        float counter = 0f;
        canDevTool = false;
        while (counter < delay)
        {
            counter += Time.deltaTime;
            yield return null;
        }
        counter = 0;
        canDevTool = true;
        Debug.Log($"[DEV TOOL] {gameObject.name} Ready");
    }

    #region Body Swap
    //Dev Tool
    //Given a Singleton containing lists of Predators and Prey in Scene,
    //  allows cycling of which body this controller is linked to
    void BodySwap()
    {

        if(EntitiesInScene.Instance == null)
            Debug.LogError("[DEV TOOL] Body Swapper requires the EntitiesInScene Singleton.");

        //check what the controller is currently linked to
        if (myController.isLinked)
        {
            //determine WHAT it is linked to
            bool isPrey = myController.linkedBody.tag == "Prey" ? true : false;
            if (isPrey)
                SwapToPredator();
            else
                SwapToPrey();

        }
        else
        {
            //It is currently linked to nothing, link it to some default
            SwapToPrey();

        }
    }

    void SwapToPrey()
    {

        GameObject[] preyInScene = EntitiesInScene.Instance.preyInScene.ToArray();
        if(preyIndex >= preyInScene.Length)
            preyIndex = 0;

        GameObject currentPrey = preyInScene[preyIndex];
        ControlledBody bodyRef = currentPrey.GetComponent<ControlledBody>();
        if(bodyRef != null)
        {
            myController.Link(bodyRef);
            Debug.Log($"[DEV TOOL] Linked {myController.name} and {bodyRef.name}");
        }
        preyIndex++;
        StartCoroutine(DevToolDelay());
        
    }

    void SwapToPredator()
    {
        GameObject[] predInScene = EntitiesInScene.Instance.predatorsInScene.ToArray();
        if (predIndex >= predInScene.Length)
            predIndex = 0;

        GameObject currentPrey = predInScene[predIndex];
        ControlledBody bodyRef = currentPrey.GetComponent<ControlledBody>();
        if (bodyRef != null)
        {
            myController.Link(bodyRef);
            Debug.Log($"[DEV TOOL] Linked {myController.name} and {bodyRef.name}");
        }
        predIndex++;
        StartCoroutine(DevToolDelay());
    }

    #endregion

}
