using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class KeyInputOnClickInvoke : MonoBehaviour
{

    [SerializeField] private KeyCode input;
    private Button btn;

    // Start is called before the first frame update
    void Start()
    {
        btn = GetComponent<Button>();
    }

    private void Update()
    {
        if(Input.GetKeyUp(input))
        {
            btn.onClick?.Invoke();
        }
    }

}
