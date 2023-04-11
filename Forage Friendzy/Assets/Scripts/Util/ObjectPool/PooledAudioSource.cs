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
        GameManager.event_VolumeSettingsChanged += UpdateVolume;
    }

    private void OnDisable()
    {

        if(aSource.isPlaying)
        {
            aSource.Stop();
        }

        Reset();

        GameManager.event_VolumeSettingsChanged -= UpdateVolume;
    }

    private void Reset()
    {
        aSource.clip = null;
        aSource.loop = false;
        aSource.volume = 1;
        aSource.pitch = 1;
        listenForVolumeChange = true;
    }

    private void UpdateVolume()
    {
        
        if (!listenForVolumeChange)
            return;

        aSource.volume = AudioManager.Instance.GetVolume(volumeSource) / settingConversionValue;
    }

    public float GetExpectedVolume()
    {
        return AudioManager.Instance.GetVolume(volumeSource) / settingConversionValue;
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

    public void PlayAudioLoop(AudioClip clip)
    {
        if (aSource.isPlaying)
            return;

        aSource.clip = clip;
        aSource.loop = true;
        aSource.Play();
    }

    public void StopAudioLoop()
    {
        if (!aSource.isPlaying)
            return;

        aSource.Stop();
    }

    public AudioSource GetAudioSource()
    {
        return aSource;
    }

}