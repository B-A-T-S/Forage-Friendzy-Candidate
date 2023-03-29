using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class PreyFood : NetworkBehaviour
{
    #region variables

    //amount of food on player or in nest
    [Tooltip("The max amount of food the prey can carry")]
    [SerializeField] private int foodCarryLimit;
    public int FoodCarryLimit { set { foodCarryLimit = value; } }
    public NetworkVariable<int> playerfood;

    //we need to hold the food itself
    Food currFood;
    #endregion

    [SerializeField] AudioClip sound_OnCollect;
    [SerializeField] AudioClip sound_OnDeposit;
    private AudioSource audioSource;

    public override void OnNetworkSpawn()
    {
        if (playerfood != null)
            playerfood.Value = 0;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    #region movingfood
    public void Addfood()
    {
        AddFoodServerRpc();
        audioSource?.PlayOneShot(sound_OnCollect);
    }

    [ServerRpc]
    public void AddFoodServerRpc()
    {
        //Debug.Log("Server - AddFood");
        playerfood.Value++;
        
    }

    public void Depositfood()
    {
        //Debug.Log("deposit is working");
        if(playerfood.Value > 0)
        {
            //Debug.Log("deposit is adding to nest");
            //playerfood = playerfood - 1;
            int amount = playerfood.Value;
            SetPlayerFoodServerRpc(amount - 1);
            GameManager.Instance.EditClientMetricServerRpc(NetworkManager.LocalClientId, (int)ClientStatus.StatIndex.FoodDeposited, 1);
            GameManager.Instance.FoodDeposited(1);
            //nestfood = nestfood + 1;
            //player puts their food into nest

            audioSource?.PlayOneShot(sound_OnDeposit);

        }
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerFoodServerRpc(int newValue)
    {
        playerfood.Value = newValue;
    }
    #endregion
}

