using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasFader : MonoBehaviour
{
   
    public float Duration = 0.4f;

    public void Fade(bool mFaded)
    {
        var canvasGroup = GetComponent<CanvasGroup>();



        // toggle end value depending on faded state
        StartCoroutine(DoFade(canvasGroup, canvasGroup.alpha, mFaded ? 1 : 0));
    }

    public IEnumerator DoFade(CanvasGroup canvasGroup, float start, float end)
    {

        float counter = 0f;

        while (counter < Duration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, counter / Duration);

            yield return null;

        }

    }
}
