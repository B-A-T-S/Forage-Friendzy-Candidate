using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewObject : MonoBehaviour
{

    [SerializeField] private bool isLoaned = false;
    [SerializeField] private PreviewGroup[] previewGroups;
    [SerializeField] private PreviewGroup activePreviewGroup;

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
    public GameObject groupCamera;
    public AnimalGeometryUtilities groupSubject;

    public void Toggle(bool on)
    {
        groupCamera.SetActive(on);
        groupSubject.EnableCosmeticByIndex(0);
    }

    public void UpdateCosmetic(int cosmeticIndex)
    {
        groupSubject.EnableCosmeticByIndex(cosmeticIndex);
    }
}
