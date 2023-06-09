using System.Collections;
using System;
using UnityEngine;
using Unity.Netcode;

public class PredatorAttack : NetworkBehaviour
{
    public Transform attackTransform;
    public float attackRadius, attackCooldown, attackFrameDelay;
    private bool canAttack;
    public NetworkVariable<bool> isStunned;

    private PreyHealth currentPrey;
    private BodyMovement bodyMovement;

    public event Action event_OnAttack;
    NetworkVariable<int> attackVariableAbstract = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField] private AudioClip sound_foxAttack;
    [SerializeField] private AudioClip sound_wolfAttack;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    // Start is called before the first frame update
    void Start()
    {
        canAttack = true;
        isStunned.Value = false;
        isStunned.OnValueChanged += OnStunChanged;
        attackVariableAbstract.OnValueChanged += OnAbstractChanged;
        bodyMovement = GetComponent<BodyMovement>();
    }

    private void OnAbstractChanged(int prev, int curr)
    {
        event_OnAttack?.Invoke();
    }

    #region Update

    void Update()
    {

        if (!IsOwner)
            return;
        
        if (Input.GetButton(bodyMovement.linkedController.interact))
        {
            //initiate an attack 
            if (canAttack)
            {
                Attack();
            }
        }
    }

    #endregion

    private AudioClip GetSoundByPredID()
    {
        int predID = ClientLaunchInfo.Instance.character;
        switch(predID)
        {
            case (int)predator.FOX:
                return sound_foxAttack;
            case (int)predator.WOLF:
                return sound_wolfAttack;
            default:
                return null;
        }
    }

    public void Attack()
    {

        currentPrey = null;

        if (attackTransform == null)
        {
            //Debug.Log($"{gameObject.name}'s AttackTransform is NULL");
            return;
        }
            

        Debug.Log("Attacking... NOW");

        canAttack = false;
        attackVariableAbstract.Value++;

        AudioManager.Instance.LoanOneShotSource(AudioCatagories.SFX, GetSoundByPredID());

        /*
        LayerMask mask = new LayerMask();
        mask.value = 0;

        Debug.Log(mask.value);
        */
        StartCoroutine(AttackCoroutine());
    }

    IEnumerator AttackCoroutine()
    {
        yield return new WaitForSeconds(attackFrameDelay/24.0f);

        Collider[] hits = Physics.OverlapSphere(attackTransform.position, attackRadius);

        bool metricUpdated = false;

        foreach (Collider preyHit in hits)
        {

            if (preyHit.transform.parent != null)
                if (preyHit.transform.parent.parent != null)
                {
                    if (preyHit.gameObject.layer == (int)CustomLayers.Prey)
                    {
                        //Debug.Log("I hit " + prey.transform.parent.parent.name);
                    }

                    if (preyHit.transform.parent.parent.CompareTag("Prey"))
                    {
                        currentPrey = preyHit.gameObject.transform.parent.GetComponentInParent<PreyHealth>();
                    }

                    //currentPrey = prey.gameObject.GetComponent<PreyHealth>();

                    if (currentPrey != null)
                    {
                        //Debug.Log("I Hit Sone");
                        if (!currentPrey.isInjured.Value && !currentPrey.isFainted.Value && !metricUpdated)
                        {
                            GameManager.Instance.EditClientStatus((int)ClientStatus.StatIndex.AttacksLanded, 1);
                            metricUpdated = true;
                        }
                            

                        if(currentPrey.isInjured.Value && !metricUpdated)
                        {
                            GameManager.Instance.EditClientStatus((int)ClientStatus.StatIndex.Knockouts, 1);
                            metricUpdated = true;
                        }
                            

                        currentPrey.ProcessAttack(NetworkManager.Singleton.LocalClientId);
                        if (currentPrey.GetComponent<BodyMovement>().characterId.Value == (int)(prey.HEDGEHOG))
                        {
                            StunPredatorServerRpc(currentPrey.GetComponent<Perks>().StunTime);
                        }
                    }
                }
        }

        StartCoroutine(AttackCooldownReset());
    }

    [ServerRpc]
    private void StunPredatorServerRpc(float duration)
    {
        isStunned.Value = true;
        StartCoroutine(StunTimer(duration));
    }

    private void OnStunChanged(bool previous, bool current)
    {
        //Debug.Log($"Stun changed from {previous} to {current}");
    }
    
    //assumes ownership is required
    [ServerRpc]
    public void AnimateAttackServerRpc()
    {
        //grahhh animate here
    }

    IEnumerator AttackCooldownReset()
    {
        yield return new WaitForSeconds(attackCooldown);



        canAttack = true;
    }
    
    IEnumerator StunTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        isStunned.Value = false;
    }
}
