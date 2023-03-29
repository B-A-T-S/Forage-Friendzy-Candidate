using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RescueInfo : MonoBehaviour
{
    private PreyRescue rescuingPrey;

    private void OnTriggerStay(Collider other)
    {
        //check for collision with self
        if (other.gameObject.GetComponentInChildren<RescueInfo>() == this)
            return;
        //Debug.Log(other);
        if (rescuingPrey == null && other.gameObject.CompareTag("Prey"))
        {
            rescuingPrey = other.GetComponent<PreyRescue>();
            rescuingPrey.PreyBeingRescuedHealth = transform.GetComponentInParent<PreyHealth>();
            rescuingPrey.CanRescue = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(rescuingPrey != null && other.gameObject.CompareTag("Prey"))
        {
            rescuingPrey.CanRescue = false;
            rescuingPrey.PreyBeingRescuedHealth = null;
            rescuingPrey = null;
        }       
    }

    private void OnDisable()
    {
        if (rescuingPrey)
        {
            rescuingPrey.CanRescue = false;
            rescuingPrey.PreyBeingRescuedHealth = null;
            rescuingPrey = null;
        }
    }
}
