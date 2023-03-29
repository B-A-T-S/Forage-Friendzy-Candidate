using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{

    private static ObjectPool instance;

    public static ObjectPool Instance
    {
        get { return instance; }
    }

    [SerializeField]
    private GameObject poolableObject;

    [SerializeField]
    private int pooledAmount = 20;

    [SerializeField]
    private bool willGrow = false;

    [SerializeField]
    private string objectPrefix = "";

    private List<GameObject> pooledObjects;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("Object Pool " + this.name + " Init");
        pooledObjects = new List<GameObject>();

        for(int i = 0; i < pooledAmount; i++)
        {
            GameObject obj = Instantiate(poolableObject);
            obj.SetActive(false);

            obj.transform.position = transform.position;
            obj.transform.SetParent(transform);

            obj.name = objectPrefix + i;

            pooledObjects.Add(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        for(int i = 0; i < pooledObjects.Count; i++)
        {
            //Debug.Log($"{pooledObjects[i].name} is currently {pooledObjects[i].activeSelf}");
            if(!pooledObjects[i].activeSelf)
            {
                return pooledObjects[i];
            }
        }

        if(willGrow)
        {
            GameObject obj = Instantiate(poolableObject);
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            pooledObjects.Add(obj);
            return obj;
        }

        //Debug.LogError("Object Pool for " + poolableObject.name + "is empty.");
        return null;
    }

    public T GetPooledObjectComponent<T>()
    {
        GameObject go = GetPooledObject();
        return go.GetComponent<T>();
    }

    public void Reset()
    {
        for(int i = 0; i < pooledObjects.Count; i++)
        {

            MatchTransform(pooledObjects[i].transform, transform);
            pooledObjects[i].transform.parent = transform;
            pooledObjects[i].SetActive(false);
        }
    }

    //given two transforms A and B, set A to the same pos, rot as B
    private void MatchTransform(Transform a, Transform b)
    {
        a.position = b.position;
        a.rotation = b.rotation;
    }

}
