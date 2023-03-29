using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkTimeManager : NetworkBehaviour
{

    public static NetworkTimeManager Instance { get; private set; }

    public NetworkVariable<float> serverDeltaTime = new NetworkVariable<float>();

    private float currentServerDeltaTime = 0;
    private float previousServerDeltaTime = 0;

    private float[] previousDeltaTimes = new float[10];
    private int index;

    private NetworkTimeSystem networkTimeSystem;

    private void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        networkTimeSystem = NetworkTimeSystem.ServerTimeSystem();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        networkTimeSystem = null;
    }

    // Update is called once per frame
    void Update()
    {
        if(IsServer && networkTimeSystem != null)
        {
            //Advance time
            networkTimeSystem?.Advance(Time.deltaTime);

            //Recal delta
            currentServerDeltaTime = (float) networkTimeSystem.ServerTime - previousServerDeltaTime;
            previousServerDeltaTime = (float) networkTimeSystem.ServerTime;

            //Store in delta list
            previousDeltaTimes[index] = currentServerDeltaTime;
            index++;

            if(index >= previousDeltaTimes.Length - 1)
            {
                index %= previousDeltaTimes.Length;
                float sum = 0;
                foreach (float f in previousDeltaTimes)
                    sum += f;

                float average = sum/previousDeltaTimes.Length;

                serverDeltaTime.Value = average;
                //Debug.Log($"Average Delta: {average}");
            }
        }
            
    }
}
