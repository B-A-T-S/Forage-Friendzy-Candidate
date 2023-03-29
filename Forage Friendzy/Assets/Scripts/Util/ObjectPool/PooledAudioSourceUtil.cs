using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledAudioSourceUtil : MonoBehaviour
{

    [Tooltip("The audio type this utility will request")]
    [SerializeField] private AudioCatagories toRequest;

    //current active aSource
    private PooledAudioSource currentSource;

    public void PlaySound(AudioClip clip)
    {
        currentSource = AudioManager.Instance.LoanOneShotSource(toRequest, clip);
    }

    public void StopPlayingSound()
    {
        if (currentSource != null)
            currentSource.Return();
    }


}
