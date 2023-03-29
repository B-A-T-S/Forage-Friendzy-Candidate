using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsOverlay : MonoBehaviour
{
    public GameObject overlayPrefab;

    public void OpenSettings()
    {
        overlayPrefab?.SetActive(true);
    }

    public void CloseSettings()
    {
        overlayPrefab?.SetActive(false);
    }

    public void AdjustMouseSensitivity(float value)
    {
        GameManager.Instance.EditVal(value, -99);
    }

    public void AdjustMaster(float value)
    {
        GameManager.Instance.EditVal(value, 0);
    }

    public void AdjustMusic(float value)
    {
        GameManager.Instance.EditVal(value, 1);
    }

    public void AdjustSFX(float value)
    {
        GameManager.Instance.EditVal(value, 2);
    }

}
