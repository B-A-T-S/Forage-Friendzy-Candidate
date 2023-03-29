using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewManager : MonoBehaviour
{

    public static PreviewManager Instance { get; private set; }

    [SerializeField] private List<PreviewObject> availablePreviewObjects;


    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    public PreviewObject GetPreviewObject()
    {
        return availablePreviewObjects.Find(x => !x.IsLoaned());
    }
}
