using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTracking : MonoBehaviour
{
    [SerializeField]
    private bool ui, tracking, trailing, loop;
    public GameObject objectTracked, startCoordinate; //in case you wanna switch objects mid play
    public float trailingRate;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        

        if (tracking)
        {
            TrackObject();
            return;
        }
        if (trailing) TrailObject();

    }

    void TrackObject()
    {
        if (ui)
        {
            GetComponent<RectTransform>().position = objectTracked.GetComponent<RectTransform>().position; // * Transform.localToWorldMatrix
        }

        transform.position = objectTracked.transform.position;

        
    }
    
    void TrailObject() //approaches by rate per sec
    {
        if (loop) LoopTrail();
    }

    void LoopTrail()
    {
        if (Vector3.Distance(objectTracked.transform.position, transform.position) < 1) transform.position = startCoordinate.transform.position;

        transform.position+=((objectTracked.transform.position - transform.position).normalized * trailingRate * Time.deltaTime);
        
    }

}
