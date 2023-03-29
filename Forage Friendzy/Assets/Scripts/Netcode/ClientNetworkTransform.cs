using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

/**
 * Client Authoritative Network Transform
 *   Client is treated as SOT for the position, rotation, and scale
 *   of attached objects
 */ 
public class ClientNetworkTransform : NetworkTransform
{

    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }

}
