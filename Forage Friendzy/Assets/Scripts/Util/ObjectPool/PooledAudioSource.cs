using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class PooledAudioSource : MonoBehaviour
{

    private static ObjectPool pool;
    [SerializeField] private AudioSource aSource;

    void Start()
    {
        if(pool == null)
        {
            pool = ObjectPoolManager.Instance.GetPool(PoolTypes.AudioSource);
        }    
    }

    private void OnDisable()
    {

        if(aSource.isPlaying)
        {
            aSource.Stop();
        }

        aSource.clip = null;
        aSource.loop = false;

        gameObject.SetActive(false);
    }

    public void PlayAudio(AudioClip clip)
    {
        if (aSource.isPlaying)
            return;

        aSource.clip = clip;
        aSource.Play();

        DelayedExecute(clip.length, () =>
        {
            gameObject.SetActive(false);
        });
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

    public void DelayedExecute(float delay, Action onExecute)
    {
        // Check if delay is valid.
        if (delay < 0)
        {
            onExecute?.Invoke();
            return;
        }
        

        StartCoroutine(DelayRoutine(delay, onExecute));
    }

    private IEnumerator DelayRoutine(float delay, Action onExecute)
    {
        // Wait for given delay
        yield return new WaitForSeconds(delay);

        //if action isn't null, invoke
        onExecute?.Invoke();
    }


}