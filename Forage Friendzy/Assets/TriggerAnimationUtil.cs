using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerAnimationUtil : MonoBehaviour
{

    [SerializeField] private Animator animator;

    public void TriggerAnimation(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }
}
