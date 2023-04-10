using System.Collections;
using Unity.Netcode;
using UnityEngine;

public struct DiscoveryResponseData : INetworkSerializable
{

    public ushort port;
    public string lobbyName;
    public bool hasRestrictions;
    public int currentPlayerCount;
    public int maxPlayers;
    public bool hasPassword;
    public string password;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref port);
        serializer.SerializeValue(ref lobbyName);
        serializer.SerializeValue(ref hasRestrictions);
        serializer.SerializeValue(ref currentPlayerCount);
        serializer.SerializeValue(ref maxPlayers);
        serializer.SerializeValue(ref hasPassword);
        serializer.SerializeValue(ref password);
    }
}