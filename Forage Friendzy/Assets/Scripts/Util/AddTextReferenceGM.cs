using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class AddTextReferenceGM : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        GameManager.Instance.winText = GetComponent<TextMeshProUGUI>();

    }
}
