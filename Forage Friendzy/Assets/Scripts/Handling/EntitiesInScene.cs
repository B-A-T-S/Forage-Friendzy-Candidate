using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class EntitiesInScene : MonoBehaviour
{

    private static EntitiesInScene instance;

    public static EntitiesInScene Instance
    {
        get { return instance; }
    }

    private enum SearchFor
    {
        Tag,
        Layer
    }

    [SerializeField]
    private SearchFor searchFor = SearchFor.Tag;

    public List<GameObject> preyInScene;
    public List<GameObject> predatorsInScene;



    void Awake()
    {
        if (instance == null) instance = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        switch(searchFor)
        {
            case SearchFor.Tag:
                preyInScene.AddRange(GameObject.FindGameObjectsWithTag("Prey"));
                predatorsInScene.AddRange(GameObject.FindGameObjectsWithTag("Predator"));
                break;
            case SearchFor.Layer:
                preyInScene.AddRange(FindObjectsWithLayer(CustomLayers.Prey));
                predatorsInScene.AddRange(FindObjectsWithLayer(CustomLayers.Predator));
                break;
        }
        

    }

    //Given a Layer enum or number, compile a set of all
    //objects in the scene that are on that layer
    GameObject[] FindObjectsWithLayer(CustomLayers layer)
    {
        GameObject[] goArray = FindObjectsOfType<GameObject>();
        List<GameObject> goList = new List<GameObject>();

        foreach(GameObject go in goArray)
        {
            if (go.layer == ((int)layer))
                goList.Add(go);
        }

        if(goList.Count == 0)
        {
            return null;
        }

        return goList.ToArray();
    }
    GameObject[] FindObjectsWithLayer(int layerNum)
    {
        GameObject[] goArray = FindObjectsOfType<GameObject>();
        List<GameObject> goList = new List<GameObject>();

        foreach (GameObject go in goArray)
        {
            if (go.layer == layerNum)
                goList.Add(go);
        }

        if (goList.Count == 0)
        {
            return null;
        }

        return goList.ToArray();
    }

    //if this gets used in the future, utilize Events to subscribe to some Spawning method call
    //and add to the arrays

}


