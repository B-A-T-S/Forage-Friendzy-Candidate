using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviewObject : MonoBehaviour
{

    [SerializeField] private bool isLoaned = false;
    [SerializeField] private PreviewGroup[] previewGroups;
    [SerializeField] private PreviewGroup activePreviewGroup;

    private void Start()
    {
        activePreviewGroup = previewGroups[0];
    }

    public void SwitchSubjectPreview(int index, RenderTexture renderView)
    {
        previewGroups[index].Toggle(true, renderView);
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
    public Camera groupCamera;
    public AnimalGeometryUtilities groupSubject;

    public void Toggle(bool on, RenderTexture renderView = null)
    {
        groupCamera.gameObject.SetActive(on);
        groupCamera.targetTexture = renderView;
        groupSubject.EnableCosmeticByIndex(0);
    }

    public void UpdateCosmetic(int cosmeticIndex)
    {
        groupSubject.EnableCosmeticByIndex(cosmeticIndex);
    }
}
