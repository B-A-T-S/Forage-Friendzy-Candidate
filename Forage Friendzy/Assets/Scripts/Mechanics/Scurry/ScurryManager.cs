using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Maps numbers to in-scene ScurryData instances
/// </summary>
public class ScurryManager : MonoBehaviour
{

    static Dictionary<int, ScurryData> scurryDicitonary;

    // Use this for initialization
    void Start()
    {
        //fill hash with active scurry points
        scurryDicitonary = new Dictionary<int, ScurryData>();
        ScurryData[] dataInScene = FindObjectsOfType<ScurryData>();
        for(int i = 0; i < dataInScene.Length; i++)
        {
            scurryDicitonary.Add(i, dataInScene[i]);
        }
    }

    public static ScurryData GetScurry(int id)
    {
        return scurryDicitonary[id];
    }

    public static int GetID(ScurryData sd)
    {
        return scurryDicitonary.Keys.FirstOrDefault(i => scurryDicitonary[i] == sd);
    }

}