using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthenticationManager : MonoBehaviour
{

    public void OnStartClicked(float loadDelay)
    {
        AuthenticateAnonymously();
    }

    public async void AuthenticateAnonymously()
    {

        using (new LoadScene("Authenticating..."))
        {
            await Authentication.LogIn();
            LoadSceneUtil.Instance.NextBuildIndex();
        }
        
    }
}
