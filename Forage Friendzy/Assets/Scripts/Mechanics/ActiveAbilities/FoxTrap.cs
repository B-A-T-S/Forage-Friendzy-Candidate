using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxTrap : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Prey")) {
            PreyHealth preyHealth = other.GetComponent<PreyHealth>();
            preyHealth.isInjured.Value = false;
            preyHealth.isFainted.Value = true;
        }
    }
}
