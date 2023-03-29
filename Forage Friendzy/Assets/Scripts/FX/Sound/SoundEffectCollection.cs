using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
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
    private AudioSource audioSource;

    private void Update()
    {
        //fire ray, if ray hits, change to state two
        if (Physics.Raycast(new Ray(transform.position, Vector3.down), out raycastHit, rayDistance, (int)grassLayer))
            state = State.AboveGrass;
        else
            state = State.AboveGround;
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
        audioSource.clip = chosenClip.clip;
        audioSource.pitch = UnityEngine.Random.Range(chosenClip.pitchRange.x, chosenClip.pitchRange.y);
        audioSource.Play();
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
        audioSource.clip = chosenClip.clip;
        audioSource.pitch = UnityEngine.Random.Range(chosenClip.pitchRange.x, chosenClip.pitchRange.y);
        audioSource.Play();
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

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
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
