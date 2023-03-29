using Unity.Netcode;
using UnityEngine;

public struct InputPayload : INetworkSerializable
{
    public int tick;
    public ulong clientId;
    public Vector3 inputVector;
    public MovementTypes moveType;
    public Vector3 cameraPosition;
    public Vector3 cameraForward;
    public Vector3 cameraRight;

    public InputPayload(bool interact)
    {

        tick = -1;
        clientId = NetworkManager.Singleton.LocalClientId;
        inputVector = Vector3.zero;
        moveType = MovementTypes.Walking;
        cameraPosition = new Vector3(0, 0, 0);
        cameraForward = new Vector3(0, 0, 1);
        cameraRight = new Vector3(1, 0, 0);

    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref inputVector);
        serializer.SerializeValue(ref cameraPosition);
        serializer.SerializeValue(ref cameraForward);
        serializer.SerializeValue(ref cameraRight);
        serializer.SerializeValue(ref moveType);
    }
}

public struct StatePayload : INetworkSerializable
{

    public int tick;
    public Vector3 position;
    public Vector3 velocity;

    public StatePayload(int _tick, Vector3 _position, Vector3 _velocity)
    {
        tick = _tick;
        position = _position;
        velocity = _velocity;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref velocity);
    }
}