using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyController : MonoBehaviour
{

    #region Prey Match Variables
    private float health;
    private int currentFoodCount;
    #endregion

    #region Movement Variables
    [SerializeField] private float speed;
    private float currentSpeed;
    [SerializeField] private float distance;
    private float max;
    private float min;
    #endregion

    #region Injured Variables
    private bool isInjured;
    [SerializeField] private float injuryDuration;
    #endregion

    #region Fainted Variables
    private bool isFainted;
    [SerializeField] private float initialFaintLockTime;
    private float faintLockTime;
    private int matchFaintCount;
    #endregion

    private void Start()
    {
        health = 100;
        currentFoodCount = 0;

        currentSpeed = speed;
        min = transform.position.x;
        max = min + distance;

        isInjured = false;

        isFainted = false;
        matchFaintCount = 0;
    }

    #region Update

    // Update is called once per frame
    void Update()
    {
    /*
     * 
     * This movement is to show how the Prey will change as a result of the predator catching them.
     * This is intended to be replaced with the actual prey controller.
     * As of now I have the prey ping ponging back and forth to show off the movement changes.
     * The pingpong is very glitchy when you change the speed, I figured there is no point wasting time fixing it,
     * seeing how this will be replaced with actual input movement.
     * 
     */

        if (!isFainted)
        {
            transform.position = new Vector3(Mathf.PingPong(Time.time * currentSpeed, max - min) + min, transform.position.y, transform.position.z);
        }
    }

    #endregion

    public void HandleAttack(float damage)
    {
        //I am assuming we dont want the prey to be able to be attacked while they are fainted
        if (!isFainted) {
            if (isInjured)
            {
                isInjured = false;
                isFainted = true;
                currentSpeed = 0f;
                matchFaintCount++;
                TakeDamage(damage);

                Debug.Log("Help, I've fallen, and I can't get up (without the help of a teammate).");

                //here we can calculate how long we want the prey to be locked for based on number of times they fainted in the match
                //this is just a filler calculation that can be replaced
                faintLockTime = initialFaintLockTime * matchFaintCount;

                StartCoroutine(FaintCooldown());
            }
            else
            {
                Debug.Log("I'm one of Lifehouse's biggest songs - Halfway Gone");

                isInjured = true;
                currentSpeed = speed * 2;
                currentFoodCount = 0; //maybe in the future also call a function before this to physically drop food assets
                TakeDamage(damage);
                StartCoroutine(InjuryCooldown());
            }
        }
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;

        if (health <= 0)
        {
            //I am just destroying the game object for now. we can replace this with whatever we want to happen to the prey when they die.
            Destroy(gameObject);
        }
    }

    IEnumerator FaintCooldown()
    {
        yield return new WaitForSeconds(faintLockTime);
        isFainted = false;
        currentSpeed = speed;
    }

    IEnumerator InjuryCooldown()
    {
        yield return new WaitForSeconds(injuryDuration);
        if (!isFainted)
        {
            currentSpeed = speed;
            isInjured = false;
        }
    }

    public bool IsFainted()
    {
        return isFainted;
    }
}
