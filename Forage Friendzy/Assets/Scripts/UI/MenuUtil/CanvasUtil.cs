using System;
using UnityEngine;
using TMPro;
using System.Collections;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class CanvasUtil : MonoBehaviour
{
    public static CanvasUtil Instance { get; set; }

    [SerializeField] private FadingCanvasGroup loadGroup, errorGroup;
    [SerializeField] private TextMeshProUGUI loadText, errorText;

    [Header("Properties")]
    [SerializeField] private float loadFadeDuration = 0.2f;
    [SerializeField] private float errorFadeDuration = 0.2f;
    [SerializeField] private float errorFadeDelay = 1f;

    private void Start()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            SelfDestruct();
            return;
        }

        if(loadGroup != null)
            ToggleLoad(false, instant: true);

    }

    public void ToggleLoad(bool on, string text = null, bool instant = false)
    {
        loadText.text = text;
        if (on)
            loadGroup.FadeIn(instant ? 0.0f : loadFadeDuration);
        else
            loadGroup.FadeOut(instant ? 0.0f : loadFadeDuration);
    }

    public void ToggleSceneLoad(bool on, AsyncOperation task, string text = null, bool instant = false)
    {
        loadText.text = text;
        if (on)
            loadGroup.FadeIn(instant ? 0.0f : loadFadeDuration);
        else
            loadGroup.FadeOut(instant ? 0.0f : loadFadeDuration);

        StartCoroutine(SceneLoadCoroutine(task));

    }

    private IEnumerator SceneLoadCoroutine(AsyncOperation task)
    {

        //if loading progress is setup, assign here

        while(!task.isDone)
        {
            yield return null;
        }
        ToggleLoad(false);
    }

    public void ShowError(string error)
    {
        errorText.text = error;
        errorGroup.FadeIn(errorFadeDuration);
        Invoke("HideError", errorFadeDelay);
    }

    public void HideError()
    {
        errorText.text = null;
        errorGroup.FadeOut(errorFadeDuration);
    }

    private void SelfDestruct()
    {
        Destroy(gameObject);
    }


}

//Disposable Class
//used similarly to efficient Java File Manip
//  don't reserve memory for lifetime, just use it like
//  a non-relevant reference
// using(new Load(<text to display>))
// {
//     stuff occurs here
// }
//  < calls Dispose here
public class Load : IDisposable
{
    public Load(string text)
    {
        CanvasUtil.Instance.ToggleLoad(true, text);
    }

    public void Dispose()
    {
        CanvasUtil.Instance.ToggleLoad(false);
    }
}

public class LoadScene : IDisposable
{
    public LoadScene(string text)
    {
        CanvasUtil.Instance.ToggleLoad(true, text);
        SceneManager.sceneLoaded += Toggle;
    }

    public void Toggle(Scene scene, LoadSceneMode loadMode)
    {
        SceneManager.sceneLoaded -= Toggle;
        CanvasUtil.Instance.ToggleLoad(false);
    }

    public void Dispose()
    {

    }
}

public class LoadNetworkScene : IDisposable
{

    NetworkManager nm;

    public LoadNetworkScene(string text, NetworkManager _nm)
    {
        nm = _nm;
        CanvasUtil.Instance.ToggleLoad(true, text);
        nm.SceneManager.OnLoadComplete += Toggle;
    }

    public void Toggle(ulong id, string sceneName, LoadSceneMode loadMode)
    {
        nm.SceneManager.OnLoadComplete -= Toggle;
        CanvasUtil.Instance.ToggleLoad(false);
    }

    public void Dispose()
    {
        
    }
}