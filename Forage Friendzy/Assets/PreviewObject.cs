using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviewObject : MonoBehaviour
{
    public Camera groupCamera;

    [SerializeField] private bool isLoaned = false;
    [SerializeField] private PreviewGroup[] previewGroups;
    [SerializeField] private PreviewGroup activePreviewGroup;

    public RenderTexture renderTexture;

    private void Start()
    {
        activePreviewGroup = previewGroups[0];
    }

    public void SwitchSubjectPreview(int index)
    {
        previewGroups[index].Toggle(true);
        activePreviewGroup.Toggle(false);

        activePreviewGroup = previewGroups[index];
    }

    public void SwitchSubjectCosmetic(int cosmeticIndex)
    {
        activePreviewGroup.UpdateCosmetic(cosmeticIndex);
    }


    public void Return()
    {
        isLoaned = false;
    }

    public bool IsLoaned() {
        return isLoaned;
    }

}

[Serializable]
public struct PreviewGroup
{
    public GameObject groupSubject;

    public void Toggle(bool on)
    {
        groupSubject.gameObject.SetActive(on);
        //groupSubject.EnableCosmeticByIndex(0);
    }

    public void UpdateCosmetic(int cosmeticIndex)
    {
        //groupSubject.EnableCosmeticByIndex(cosmeticIndex);
    }
}
