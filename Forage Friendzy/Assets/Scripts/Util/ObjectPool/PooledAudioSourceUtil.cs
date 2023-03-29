using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledAudioSourceUtil : MonoBehaviour
{
    public void PlaySound(AudioClip clip)
    {
        PooledAudioSource pas = ObjectPoolManager.Instance.GetPooledObjectComponent<PooledAudioSource>(PoolTypes.AudioSource);
        pas.gameObject.SetActive(true);
        pas.PlayAudio(clip);
    }


}
