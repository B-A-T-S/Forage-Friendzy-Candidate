using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectCollection : MonoBehaviour
{
    [SerializeField] bool playSounds = true;
    
    [SerializeField] CustomLayers grassLayer;
    [SerializeField] float rayDistance = 0.3f;

    enum State
    {
        AboveGround,
        AboveGrass
    }

    private State state = State.AboveGround;
    private RaycastHit raycastHit;


    [Tooltip("A Dictionary of strings. Used to change key to match when in StateTwo")]
    [SerializeField] List<StringPair> keyMap = new();

    [SerializeField] List<StringListMap> availableSoundLists = new();
    
    private PooledAudioSource loanedAudioSource;
    private AudioSource aSource;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == (int) grassLayer)
            state = State.AboveGrass;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == (int)grassLayer)
            state = State.AboveGround;
    }

    void Start()
    {
        loanedAudioSource = AudioManager.Instance.LoanLoopingSource(AudioCatagories.SFX, null);
        aSource = loanedAudioSource.GetAudioSource();
        aSource.loop = false;
    }

    public void PlaySoundByIndex(string listKey, int soundIndex)
    {

        if (!playSounds)
            return;

        if (state == State.AboveGrass)
            listKey = GetKeyMatch(listKey);

        List<PitchedSound> sounds = GetMapGivenKey(listKey).list;
        if (soundIndex >= sounds.Count)
            return;
        PitchedSound chosenClip = sounds[soundIndex];
        aSource.clip = chosenClip.clip;
        aSource.pitch = UnityEngine.Random.Range(chosenClip.pitchRange.x, chosenClip.pitchRange.y);
        aSource.Play();
    }

    public void PlayRandomSound(string listKey)
    {

        if (!playSounds)
            return;

        if (state == State.AboveGrass)
            listKey = GetKeyMatch(listKey);

        List<PitchedSound> sounds = GetMapGivenKey(listKey).list;
        int randomIndex = RandomIndex(sounds.Count);
        PitchedSound chosenClip = sounds[randomIndex];
        aSource.clip = chosenClip.clip;
        aSource.pitch = UnityEngine.Random.Range(chosenClip.pitchRange.x, chosenClip.pitchRange.y);
        aSource.Play();
    }

    #region helpers

    public string GetKeyMatch(string key)
    {
        StringPair result = keyMap.Find(x => x.key == key);
        if (result != null)
            return result.value;

        return key;
    }
    public StringListMap GetMapGivenKey(string key)
    {
        return availableSoundLists.Find(x => x.key == key);
    }

    public int RandomIndex(int size)
    {
        return UnityEngine.Random.Range(0, size - 1);
    }

    #endregion

}

[Serializable]
public class StringPair
{
    public string key = "StateOne";
    public string value = "StateTwo";
}

[Serializable]
public class StringListMap
{
    public string key;
    public List<PitchedSound> list;
}

[Serializable]
public class PitchedSound
{
    public AudioClip clip;
    public Vector2 pitchRange;
}
