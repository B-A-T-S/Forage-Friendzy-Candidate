using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class PooledAudioSource : MonoBehaviour
{

    [Tooltip("Used to convert values in settings to [0.0, 1.0]\n" +
        "Should match the maximum value of this source's corresponding setting")]
    [SerializeField][Range(1, float.MaxValue)] private float settingConversionValue = 1f;

    [Tooltip("The audio type this source belongs too")]
    [SerializeField] private AudioCatagories volumeSource;

    [Tooltip("AudioSource Component used to play sounds from this source")]
    [SerializeField] private AudioSource aSource;

    [Header("Properties")]
    private bool listenForVolumeChange = true;

    private void OnEnable()
    {
        UpdateVolume();
        AudioManager.Instance.event_VolumeValueChanged += UpdateVolume;
    }

    private void OnDisable()
    {

        if(aSource.isPlaying)
        {
            aSource.Stop();
        }

        Reset();

        AudioManager.Instance.event_VolumeValueChanged -= UpdateVolume;

        gameObject.SetActive(false);
    }

    private void Reset()
    {
        aSource.clip = null;
        aSource.loop = false;
        listenForVolumeChange = true;

    }

    private void UpdateVolume()
    {
        if (!listenForVolumeChange)
            return;

        aSource.volume = (AudioManager.Instance.GetMasterVolume() / settingConversionValue) *
            (AudioManager.Instance.GetVolume(volumeSource) / settingConversionValue);
    }

    public float GetExpectedVolume()
    {
        return (AudioManager.Instance.GetMasterVolume() / settingConversionValue) *
            (AudioManager.Instance.GetVolume(volumeSource) / settingConversionValue);
    }

    public void Return()
    {
        Reset();
        gameObject.SetActive(false);
    }

    public void PlayAudio(AudioClip clip)
    {
        if (aSource.isPlaying)
            return;

        aSource.clip = clip;
        aSource.Play();

        Invoke("Return", clip.length);
    }

    public AudioSource GetAudioSource()
    {
        return aSource;
    }

}