using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorCollection : MonoBehaviour
{

    [SerializeField] List<Animator> animators = new();
    private RuntimeAnimatorController cachedControllerReference;

    // Start is called before the first frame update
    void Start()
    {
        foreach(Animator animator in animators)
        {
            if (cachedControllerReference == null)
                cachedControllerReference = animator.runtimeAnimatorController;
            else
            {
                if (animator.runtimeAnimatorController != cachedControllerReference)
                {
                    Debug.LogError($"AnimatorCollection | Controller Mismatch", this);
                    this.enabled = false;
                }
            }   
        }
    }

    public void AddAnimator(Animator newAnim)
    {
        animators.Add(newAnim);
    }

    public void AddAnimator(Animator[] newAnims)
    {
        animators.AddRange(newAnims);
    }

    public bool SetInteger(string name, int n)
    {
        try
        {
            foreach(Animator animator in animators)
            {
                animator.SetInteger(name, n);
            }
            return true;
        } 
        catch (Exception e)
        {
            Debug.LogError($"AnimatorCollection | Unknown Integer {name}", this);
            return false;
        }
    }

    public bool SetFloat(string name, float f)
    {
        try
        {
            foreach (Animator animator in animators)
            {
                animator.SetFloat(name, f);
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"AnimatorCollection | Unknown Float {name}", this);
            return false;
        }
    }

    public bool SetBool(string name, bool b)
    {
        try
        {
            foreach (Animator animator in animators)
            {
                animator.SetBool(name, b);
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"AnimatorCollection | Unknown Bool {name}", this);
            return false;
        }
    }

    public bool SetTrigger(string name)
    {
        try
        {
            foreach (Animator animator in animators)
            {
                animator.SetTrigger(name);
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"AnimatorCollection | Unknown Trigger {name}", this);
            return false;
        }
    }
}
