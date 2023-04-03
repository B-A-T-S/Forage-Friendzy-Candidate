using System.Collections;
using UnityEngine;
using Unity.Netcode;

//Data class designed to exist in the lobby scene
//stores all information regarding client customization
//that takes place within lobby/matchmaking
public class ClientLaunchInfo : MonoBehaviour, INetworkSerializable
{

    public static ClientLaunchInfo Instance { get; private set; }

    public int role;
    public int character;
    public int cosmetic;
    public string playerName = "";


    // Use this for initialization
    void Start()
    {

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref role);
        serializer.SerializeValue(ref character);
        serializer.SerializeValue(ref cosmetic);
        serializer.SerializeValue(ref playerName);
    }
}