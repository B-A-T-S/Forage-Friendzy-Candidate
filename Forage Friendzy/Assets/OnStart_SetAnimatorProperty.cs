using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class OnStart_SetAnimatorProperty : MonoBehaviour
{

    [SerializeField] private List<AnimAction> actions = new List<AnimAction>();
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        foreach (AnimAction action in actions)
        {
            action.Apply(animator);
        }
    }

    [Serializable]
    struct AnimAction
    {

        public enum PropertyMode
        {
            Float,
            Bool,
            Trigger
        }

        public PropertyMode mode;
        public string propertyName;
        public float floatValue;
        public bool boolValue;

        public void Apply(Animator anim)
        {
            switch(mode)
            {
                case PropertyMode.Float:
                    anim.SetFloat(propertyName, floatValue);
                    break;
                case PropertyMode.Bool:
                    anim.SetBool(propertyName, boolValue);
                    break;
                case PropertyMode.Trigger:
                    anim.SetTrigger(propertyName);
                    break;
            }
        }
    }

}
