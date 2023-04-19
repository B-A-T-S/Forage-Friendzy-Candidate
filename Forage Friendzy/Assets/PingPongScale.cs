using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PingPongScale : MonoBehaviour
{

    public bool isOn = true;

    [SerializeField] private Vector3 initialScale = Vector3.one;
    [SerializeField] private Vector3 resultingScale = Vector3.one;

    [SerializeField] private float cycleDelay = 4;
    [SerializeField] private float timeToComplete = 2;

    private Coroutine pingPong;
    private RectTransform rectTransform;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        pingPong = StartCoroutine(PingPongScaleCoroutine());
    }

    IEnumerator PingPongScaleCoroutine()
    {
        if(rectTransform == null) 
            rectTransform = GetComponent<RectTransform>();
        while(isOn)
        {
            float elapsedTime = 0;
            Vector3 startingVector = transform.localScale;
            Vector3 endingVector = startingVector == initialScale ? resultingScale : initialScale;
            while(elapsedTime < timeToComplete)
            {

                elapsedTime += Time.deltaTime;
                float completion = elapsedTime / timeToComplete;

                rectTransform.localScale = Vector3.Slerp(startingVector, endingVector, completion);
                yield return null;

            }

            yield return new WaitForSeconds(cycleDelay);
        }


    }
}
