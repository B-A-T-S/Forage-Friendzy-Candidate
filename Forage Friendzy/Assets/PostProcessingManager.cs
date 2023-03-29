using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessingManager : MonoBehaviour
{
    public Volume ppVol;

    public VolumeProfile ppDay, ppNight;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.isNight.OnValueChanged += NightTransition;
    }

    void NightTransition(bool wasNight, bool isNight)
    {
        StartCoroutine(NightTransitionCoroutine(isNight));
    }

    public IEnumerator NightTransitionCoroutine(bool isNight)
    {
        yield return new WaitForSeconds(GameManager.Instance.sunsetAnimDur);

        if (isNight) ppVol.profile = ppNight;
        else ppVol.profile = ppDay;
    }

    
}
