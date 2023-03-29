using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindSelfInGM : MonoBehaviour
{
    [SerializeField]
    private Findable me;

    // Start is called before the first frame update
    void Start()
    {
        if (me.Equals(Findable.PredatorDoor))
        {
            GameManager.Instance.predatorDoor = this.gameObject;
            return;
        }

        if (me.Equals(Findable.CenterDepo))
        {
            GameManager.Instance.centerDepo = this.gameObject;
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
