using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadingCanvasGroup : MonoBehaviour
{
    private CanvasGroup cg;

    public event Action event_OnFadedIn;
    public event Action event_OnFadedOut;
    public event Action event_OnFadeComplete;
    public event Action event_OnFadeKilled;

    private Coroutine currentRoutine;

    private void Start()
    {
        cg = GetComponent<CanvasGroup>();
    }

    public void FadeIn(float duration = 0.2f)
    {

        Kill();

        if (cg == null)
            cg = GetComponent<CanvasGroup>();

        if (duration > 0.0f)
            currentRoutine = StartCoroutine(Fade(true, duration));
        else
        {
            cg.alpha = 1f;
            ToggleInteract(true, true);
            event_OnFadedIn?.Invoke();
            event_OnFadeComplete?.Invoke();
        }
    }

    public void FadeOut(float duration = 0.2f)
    {

        Kill();

        if (cg == null)
            cg = GetComponent<CanvasGroup>();

        if (duration > 0.0f)
            currentRoutine = StartCoroutine(Fade(false, duration));
        else
        {
            cg.alpha = 0f;
            ToggleInteract(false, false);
            event_OnFadedOut?.Invoke();
            event_OnFadeComplete?.Invoke();
        }
    }

    public void Kill()
    {
        if (currentRoutine == null)
            return;
        
        StopCoroutine(currentRoutine);
        currentRoutine = null;
        event_OnFadeKilled?.Invoke();
    }

    private void ToggleInteract(bool raycasts, bool interact)
    {
        cg.blocksRaycasts = raycasts;
        cg.interactable = interact;
    }

    private IEnumerator Fade(bool fadeIn, float duration)
    {
        float counter = 0;
        float startAlpha = cg.alpha;

        if (!fadeIn)
        {
            while (counter < 1)
            {
                counter += Time.deltaTime / duration;
                cg.alpha = Mathf.Lerp(startAlpha, 0, counter);
                yield return null;
            }

            cg.alpha = 0;
            ToggleInteract(false, false);
            event_OnFadedOut?.Invoke();
        }
        else
        {
            while (counter < 1)
            {
                counter += Time.deltaTime / duration;
                cg.alpha = Mathf.Lerp(startAlpha, 1, counter);
                yield return null;
            }

            cg.alpha = 1;
            ToggleInteract(true, true);
            event_OnFadedIn?.Invoke();
        }

        event_OnFadeComplete?.Invoke();
    }
}
