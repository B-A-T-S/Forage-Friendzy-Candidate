using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boot_LoadNext : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        LoadSceneUtil.Instance.NextBuildIndex();

        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 30;

    }

}
