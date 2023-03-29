using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;

public delegate void BodyUnlink();
public delegate void BodyLink();

public class ControlledBody : NetworkBehaviour
{
    public PlayerController linkedController;
    [HideInInspector]
    public bool isLinked;

    [HideInInspector]
    public Vector3 currentInputVector;

    public event BodyLink OnBodyLink;
    public event BodyUnlink OnBodyUnlink;

    private void Start()
    {
        isLinked = linkedController != null ? true : false;
    }

    
    public override void OnNetworkSpawn()
    {

        if (IsOwner)
        {
            GameManager.Instance.localPlayer = this.gameObject;
            Link(Spawner.Instance.inSceneController);
        } 

    }

    public void Link(PlayerController newController)
    {

        PlayerController previousController = linkedController;

        linkedController = newController;
        if (newController.linkedBody != this)
            newController.Link(this);

        if(previousController != null)
            previousController.Unlink();

        isLinked = true;
        OnBodyLink?.Invoke();

    }

    public void Unlink()
    {

        PlayerController tempRef = linkedController;
        linkedController = null;

        if (tempRef.linkedBody == this)
            tempRef.Unlink();

        isLinked = false;
        OnBodyUnlink?.Invoke();
    }

}

