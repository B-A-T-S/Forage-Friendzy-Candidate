using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhenIPressJYeetMe : MonoBehaviour
{
    bool free;
    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.J)) free = !free;

        if(free)
        {
            transform.position = new Vector3(0, 10000, 0);
        }

        

    }
}
