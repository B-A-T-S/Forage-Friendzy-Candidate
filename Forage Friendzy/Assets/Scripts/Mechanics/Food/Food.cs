using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class Food : NetworkBehaviour
{

    #region Declarations

    public enum TYPE {Common=10, Uncommon=15, Rare=25};

    public int numFoods; //reeval
    public TYPE rarity;

    [SerializeField]
    private List<GameObject> foodMeshes, playerList;
    private GameObject foodMesh;
    [SerializeField, HideInInspector]
    public NetworkVariable<int> foodMeshIndex = new NetworkVariable<int>();
    public NetworkVariable<int> foodValue = new NetworkVariable<int>();
    [SerializeField, HideInInspector]
    public NetworkVariable<bool> isAvailable = new NetworkVariable<bool>();
    [SerializeField,HideInInspector]
    public NetworkVariable<Vector3> foodPos = new NetworkVariable<Vector3>();

    public int locationID;

    #endregion

    public override void OnNetworkSpawn()
    {
        /*
        foodMeshIndex = new NetworkVariable<int>();
        foodValue = new NetworkVariable<int>();
        isAvailable = new NetworkVariable<bool>();
        foodPos = new NetworkVariable<Vector3>();
        */
        //Debug.Log($"This {gameObject.name} ran it's OnNetworkSpawn");
        isAvailable.OnValueChanged += Availability_OVC;
        foodPos.OnValueChanged += RelocateFood_OVC;
    }

    public IEnumerator SubscribeOnDelay(float delay)
    {
        //Debug.Log($"This {gameObject.name} ran the opening of the coroutine");
        yield return new WaitForSeconds(delay);
        

        //Debug.Log($"This {gameObject.name} ran the end of the coroutine");
    }

    private void Availability_OVC(bool previousValue, bool newValue)
    {
        //Debug.Log("OVC - Should be setting food to " + newValue);

        
        if(IsServer)
        {
            foodMeshes[foodMeshIndex.Value].SetActive(newValue);
            AvailabilityClientRpc(foodMeshIndex.Value, newValue);
        }
            

    }

    [ClientRpc]
    void AvailabilityClientRpc(int foodMeshIndex, bool active)
    {

        //GameObject go = foodMeshes.Find(g => g == foodMesh);
        GameObject go = foodMeshes[foodMeshIndex];
        if (go != null)
            go.SetActive(active);
        else
            Debug.LogError($"Food Mesh was {foodMesh.name} but was not found by Predicate");

        //bool b = foodMesh == foodMeshes[newMeshIndex];
        //foodMeshes[newMeshIndex].SetActive(active);
    }

    private void RelocateFood_OVC(Vector3 prev, Vector3 next)
    {
        //Debug.Log("OVC - food is at " + next);
        transform.position = next;
        //if(IsServer)
            //RelocateFoodClientRpc(next);

    }

    [ClientRpc]
    void RelocateFoodClientRpc(Vector3 newPos)
    {
        //Debug.Log($"New Position : {newPos}");
        transform.position = newPos;
    }

    #region OnEnable Behaviours
    public void OnEnable() 
    {
        /*
        isAvailable.OnValueChanged -= Availability_OVC;
        foodPos.OnValueChanged -= RelocateFood_OVC;

        isAvailable.OnValueChanged += Availability_OVC;
        foodPos.OnValueChanged += RelocateFood_OVC;
        */
        //when a food is spawned, do what?

        //choose a food
        //weighted random, switch case prob
        int weightedRand = (int) (Random.value * 100.99f);

        if (weightedRand < 60) rarity = TYPE.Common;
        else if (weightedRand < 85) rarity = TYPE.Uncommon;
        else rarity = TYPE.Rare;

        //Debug.Log("rarity = " + rarity);

        //choose mesh based on rarity
        //if(common) random between apple, cherry, pepper

        //playerList = gameManager.GetPlayerBodies();

        StartCoroutine(WaitUntilUnseen());
    }

    private IEnumerator WaitUntilUnseen()
    {
        bool observed = AmIObserved();
        while (observed)
        {
            observed = AmIObserved();
            yield return null;
        }
        EnableMesh();
    }

    private bool AmIObserved()
    {
        return false; //for now
    }

    private void EnableMesh()
    {
        if (rarity == TYPE.Common) EnableCommonMesh();
        else if (rarity == TYPE.Uncommon) EnableUncommonMesh();
        else EnableRareMesh();

        

    }

    private void EnableCommonMesh()
    {
        //nothing yet
        //mesh, and foodValue
        SetMeshIndexServerRpc(0, 1);
    }
    
    private void EnableUncommonMesh()
    {
        //nothing
        SetMeshIndexServerRpc(1, 2);
    }

    private void EnableRareMesh()
    {
        //nothing
        SetMeshIndexServerRpc(2, 3);
    }

    [ServerRpc (RequireOwnership = false)]
    void SetMeshIndexServerRpc(int index, int value)
    {
        foodMeshIndex.Value = index;
        foodValue.Value = value;

        //foodMeshes[foodMeshIndex.Value].SetActive(true);

        isAvailable.Value = true;
    }

    #endregion
    
    #region OnCollect Behaviours
    /*
    In pickup, make the food spawner aware, and add some oncollect behaviour that adds the passed food into the pool and calls OnCollect in here
    // add OnCollect() Behaviours that disable foodMesh and 
    */

    public int OnCollect()
    {
        //Debug.Log("OnCollect");
        //call an rpc to disable itself
        OnCollectServerRpc();

        return foodValue.Value;
    }

    [ServerRpc (RequireOwnership = false)]
    void OnCollectServerRpc()
    {
        //Debug.Log("rpc");
        isAvailable.Value = false;
        foodValue.Value = 0;

        Vector3 tempVec = FindObjectOfType<Spawner>().GetFreeFoodLocation(this, locationID);
        //Debug.Log($"On Collect Relocated to : {tempVec}");
        foodPos.Value = tempVec;
        //Debug.Log(foodPos.Value);
        //RelocateFoodClientRpc(foodPos.Value);

        
        OnEnable();
    }

    

    #endregion

}