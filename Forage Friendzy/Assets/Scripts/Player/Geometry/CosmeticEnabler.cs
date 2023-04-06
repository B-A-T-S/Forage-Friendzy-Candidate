using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CosmeticEnabler : MonoBehaviour
{

    [Header("Cosmetics")]
    public CosmeticGroup[] cosmeticGroups;

    public void EnableCosmeticByIndex(int cosmeticIndex)
    {
        if (cosmeticIndex != 0)
        {
            for (int i = 0; i < cosmeticGroups.Length; i++)
                cosmeticGroups[i].Toggle(cosmeticIndex - 1 == i);
        }
        else
        {
            foreach (CosmeticGroup cg in cosmeticGroups)
                cg.Toggle(false);
        }
    }
}

[Serializable]
public struct CosmeticGroup
{
    [SerializeField] private List<GameObject> toEnable;

    public void Toggle(bool on)
    {
        foreach (GameObject go in toEnable)
            go.SetActive(on);
    }
}