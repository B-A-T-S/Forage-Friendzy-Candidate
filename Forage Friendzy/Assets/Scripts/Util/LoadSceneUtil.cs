using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;



public class LoadSceneUtil : MonoBehaviour
{
    
    public static LoadSceneUtil Instance { get; private set; }
    [SerializeField] private string[] buildIndexScenes;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        //scenes = EditorBuildSettings.scenes;
        //buildIndexScenes = scenes.Select(x => Path.GetFileNameWithoutExtension(x.path)).ToArray();
    }

    public void NM_BySceneName(string sceneName)
    {
        if (!NetworkManager.Singleton.IsHost)
            return;
        
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void NM_ByBuildIndex(int buildIndex)
    {
        if (!NetworkManager.Singleton.IsHost)
            return;

        NetworkManager.Singleton.SceneManager.LoadScene(buildIndexScenes[buildIndex], LoadSceneMode.Single);
    }

    public void NM_NextBuildIndex()
    {
        if (!NetworkManager.Singleton.IsHost)
            return;

        NetworkManager.Singleton.SceneManager.LoadScene(buildIndexScenes[SceneManager.GetActiveScene().buildIndex + 1], LoadSceneMode.Single);
    }


    public AsyncOperation ByBuildIndex(int buildIndex)
    {
        return SceneManager.LoadSceneAsync(buildIndex);
    }

    public AsyncOperation NextBuildIndex()
    {
        return SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1); 
    }

    public AsyncOperation PreviousBuildIndex()
    {
        return SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex - 1);
    }

}
