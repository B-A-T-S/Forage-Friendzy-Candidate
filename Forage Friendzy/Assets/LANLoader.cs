using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LANLoader : MonoBehaviour
{

    [SerializeField] string sceneName = "";

    public void OnClick_LoadLANScene()
    {
        SceneManager.LoadSceneAsync(sceneName);
    }

}
