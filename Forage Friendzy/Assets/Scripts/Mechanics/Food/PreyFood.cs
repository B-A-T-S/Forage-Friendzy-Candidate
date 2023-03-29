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

    [SerializeField] private float soundResetTime = 2f;
    [SerializeField] private int requiredDepositsForSound = 3;
    private float timeTillSoundReset;
    private int depositsTillSound = 3;
    private Coroutine currentSoundReset;

    public override void OnNetworkSpawn()
    {
        if (playerfood != null)
            playerfood.Value = 0;
    }

    #region movingfood
    public void Addfood()
    {
        AddFoodServerRpc();
        AudioManager.Instance.LoanOneShotSource(AudioCatagories.SFX, sound_OnCollect);
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

            depositsTillSound -= 1;
            if (depositsTillSound <= 0)
            {
                depositsTillSound = requiredDepositsForSound;
                AudioManager.Instance.LoanOneShotSource(AudioCatagories.SFX, sound_OnDeposit);
            }

            timeTillSoundReset = soundResetTime;
            if (currentSoundReset == null)
                currentSoundReset = StartCoroutine(SoundCountResetCoroutine());

        }
        
    }

    IEnumerator SoundCountResetCoroutine()
    {

        while (timeTillSoundReset >= 0)
        {
            timeTillSoundReset -= Time.deltaTime;
            yield return null;
        }

        currentSoundReset = null;
        depositsTillSound = requiredDepositsForSound;

    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerFoodServerRpc(int newValue)
    {
        playerfood.Value = newValue;
    }
    #endregion
}

