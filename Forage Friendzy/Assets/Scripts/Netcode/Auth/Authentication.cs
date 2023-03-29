using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;

#if UNITY_EDITOR
using ParrelSync;
#endif

public static class Authentication
{
    public static string PlayerId { get; private set; }

    public static async Task LogIn()
    {
        //if services are not up and running this session, get them
        if(UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            InitializationOptions initOptions = new InitializationOptions();

            //Parallel Sync - Ensure Clone Project does not have the same profileName as Origin Project
            #if UNITY_EDITOR
                // Remove this if ParrelSync is not installed. 
                // It's used to differentiate the clients, otherwise lobby will count them as the same
                if (ClonesManager.IsClone()) 
                    initOptions.SetProfile(ClonesManager.GetArgument());
                else
                    initOptions.SetProfile("Primary");
            #endif

            await UnityServices.InitializeAsync(initOptions);
        } //end if service check

        //if not authorized for session, do that
        if(!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            //Set PlayerID based on provided ID
            PlayerId = AuthenticationService.Instance.PlayerId;
        }
    }
}
