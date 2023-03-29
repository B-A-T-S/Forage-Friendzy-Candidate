using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRadius : MonoBehaviour
{
    public float damage;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.parent != null)
        {
            if (other.gameObject.transform.parent.gameObject.TryGetComponent<PreyController>(out PreyController preyController))
            {
                gameObject.SetActive(false);
                preyController.HandleAttack(damage);
            }
        }
    }

}
