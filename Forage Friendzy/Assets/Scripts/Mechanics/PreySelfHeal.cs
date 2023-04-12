using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PreySelfHeal : NetworkBehaviour
{
    #region Self Heal Inspector Variables
    [Header("Self Heal Variables")]
    [Tooltip("The amount of food it will take to heal yourself.")]
    [SerializeField] private int foodCost;
    public int FoodCost { set { foodCost = value; } get { return foodCost; } }

    [Tooltip("The amount of time that it will take to heal yourself..")]
    [SerializeField] private float healTime;
    public float HealTime { set { healTime = value; } }
    [Tooltip("The amount of speed reduction when healing")]
    [SerializeField] private float speedReduction;
    #endregion

    #region Component Variables
    private PreyFood preyFoodComponent;
    private PreyHealth preyHealthComponent;
    private PlayerController playerControllerComponent;
    private BodyMovement bodyMovement;
    #endregion

    #region Helper Variables
    [HideInInspector]
    public NetworkVariable<bool> selfHealActive;
    private Coroutine selfHealCoroutine;
    private float normalSpeed;
    private float reducedSpeed;
    #endregion

    public override void OnNetworkSpawn()
    {
        selfHealActive.Value = false;
        selfHealActive.OnValueChanged += OnSelfHealChanged;
    }

    // Start is called before the first frame update
    void Start()
    {
        preyFoodComponent = GetComponent<PreyFood>();
        preyHealthComponent = GetComponent<PreyHealth>();
        bodyMovement = GetComponent<BodyMovement>();
        playerControllerComponent = bodyMovement.linkedController;
        normalSpeed = bodyMovement.SprintSpeed;
        reducedSpeed = bodyMovement.SprintSpeed - speedReduction;
    }

    // Update is called once per frame
    void Update()
    {
        //CheckForSelfHeal();
    }

    private void CheckForSelfHeal()
    {
        if (!IsOwner)
            return;

        if (preyHealthComponent.isInjured.Value)
        {
            if (Input.GetKeyDown(playerControllerComponent.selfHeal))
            {
                if (!selfHealActive.Value && (foodCost <= preyFoodComponent.playerfood.Value))
                {
                    Debug.Log("Self Healing..");
                    SetSelfHealActivityServerRpc(false);
                    selfHealCoroutine = StartCoroutine(SelfHealTimer());
                }
            }
            else
            {
                if (selfHealActive.Value)
                {
                    Debug.Log("Self Healing cancelled..");
                    SetSelfHealActivityServerRpc(false);
                    StopCoroutine(selfHealCoroutine);
                }
            }
        }
    }

    IEnumerator SelfHealTimer()
    {
        yield return new WaitForSeconds(healTime); 
        SetSelfHealActivityServerRpc(false);
        preyHealthComponent.HandleSelfHealServerRpc();
        preyFoodComponent.SetPlayerFoodServerRpc(preyFoodComponent.playerfood.Value-foodCost);
        Debug.Log("Self Healing Finished..");
    }

    private void OnSelfHealChanged(bool previous, bool current)
    {
        Debug.Log($"Self heal set from {previous} to {current}");
        if (current)
        {
            bodyMovement.SprintSpeed = reducedSpeed;
            bodyMovement.WalkSpeed = reducedSpeed;
        }
        else
        {
            bodyMovement.SprintSpeed = normalSpeed;
            bodyMovement.WalkSpeed = normalSpeed;
        }
    }

    [ServerRpc]
    public void SetSelfHealActivityServerRpc(bool isActive)
    {
        selfHealActive.Value = isActive;
    }
}
