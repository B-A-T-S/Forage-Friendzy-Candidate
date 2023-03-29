using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameObjectCollection : MonoBehaviour
{
    public List<GameObject> list;
    public void ToggleByIndex(int index, bool on)
    {
        list.ElementAt(index).SetActive(on);
    }
}
