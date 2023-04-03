using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchInfo_SetNameUtil : MonoBehaviour
{

    public void SetName(string newValue)
    {
        ClientLaunchInfo.Instance.playerName = newValue.Trim(); 
    }


}
