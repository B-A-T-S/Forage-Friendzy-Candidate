using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// On Start or when refreshed, recursively finds all collider components on objects on and under the attached
/// </summary>
public class ColliderHolder : MonoBehaviour
{

    public List<Collider> presentColliders;

    [SerializeField] int childLimit = 5;
    [SerializeField] bool includeTriggers = false;

    // Use this for initialization
    void Start()
    {
        presentColliders = new();
        RefreshColliders(includeTriggers);
    }
    
    public Collider[] RefreshColliders(bool includeTriggers)
    {
        presentColliders.Clear();
        presentColliders.AddRange(FindColliders(gameObject, includeTriggers));
        return presentColliders.ToArray();
    }

    private List<Collider> FindColliders(GameObject currentObject, bool includeTriggers)
    {
        List<Collider> collidersOnCurrentObject = new List<Collider>();
        //get any colliders present on current object
        Collider[] allColliders = currentObject.GetComponents<Collider>();
        foreach(Collider c in allColliders)
        {
            if (!c.isTrigger)
                collidersOnCurrentObject.Add(c);
        }

        //get num of child objects
        int numChildren = currentObject.transform.childCount;
        //if more children than allowed to check, return
        if (numChildren > childLimit)
            return collidersOnCurrentObject;

        //if under childLimit, check each one recursively
        for(int childIndex = 0; childIndex < numChildren; childIndex++)
        {
            //add the colliders of the current child to the list
            collidersOnCurrentObject.AddRange(FindColliders(currentObject.transform.GetChild(childIndex).gameObject, includeTriggers));
        }

        return collidersOnCurrentObject;
    }
    
}