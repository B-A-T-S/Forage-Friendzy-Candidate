using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ClientInputQueue: NetworkBehaviour
{

    public static ClientInputQueue Instance { get; private set; }

    private Queue<InputPayload> inputQueue;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //A client doesn't need a q except their own
        if (!IsServer || !IsOwner)
            Destroy(this.gameObject);

    }

    public void QueueInput(InputPayload inputPayload)
    {

        if (!IsOwner)
            return;

        inputQueue.Enqueue(inputPayload);
        AddToInputQueueServerRpc(inputPayload);
    }

    [ServerRpc]
    private void AddToInputQueueServerRpc(InputPayload inputPayload)
    {
        inputQueue.Enqueue(inputPayload);
    }
}
