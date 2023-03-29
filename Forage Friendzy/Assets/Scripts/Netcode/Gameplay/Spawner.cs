using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

[System.Serializable]
public struct AnimalSet
{
    public Controlled3DBody controlledBody;
    public NetworkObject geometry;
}

public class Spawner : NetworkBehaviour
{

    public static Spawner Instance { get; private set; }

    [SerializeField]
    private AnimalSet[] preyPrefabs;

    [SerializeField]
    private AnimalSet[] predatorPrefabs;

    public PlayerController inSceneController;

    [SerializeField]
    private List<Transform> preySpawns, predatorSpawns;

    private bool[] preySpawnsUsed, predatorSpawnsUsed;

    [SerializeField]
    private List<Transform> foodSpawnLocations;
    private bool[] foodSpawnLocationsUsed;

    private int numSpawned;

    [SerializeField]
    private NetworkObject foodToSpawn;

    //public Dictionary<ulong, BodyMovement> clientPrefabs;

    private List<NetworkObject> spawnedObjects = new();

    public override void OnNetworkSpawn()
    {
        if (Instance == null)
            Instance = this;

        Debug.developerConsoleVisible = true;
        //Debug.LogError("Force Console Open - NOT A REAL ERROR");

        //tell GM to start doing things
        GameManager.Instance.isMatch = true;

        preySpawnsUsed = new bool[preySpawns.Count];
        predatorSpawnsUsed = new bool[predatorSpawns.Count];
        SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, ClientLaunchInfo.Instance);

        if (IsHost && foodSpawnLocations.Count > 0)
            SpawnFoodServerRpc();
    }

    private void Start()
    {
        //clientPrefabs = new Dictionary<ulong, BodyMovement>();
        
        

    }

    //when spawning a prefab, the client tells the server to run this
    //it does not occur on the client side whatsoever
    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ulong playerId, ClientLaunchInfo launchInfo)
    {

        //numSpawned++;

        Controlled3DBody bodyToSpawn;
        NetworkObject geoToSpawn;
        AnimalSet chosenSet;

        byte randomPlaceIndex;
        Transform randomPlace;

        //check the role information passed
        if(launchInfo.role == 0)
        {
            //this player is Prey

            //check character
            chosenSet = preyPrefabs[launchInfo.character];

            //Choose a spawn
            do
            {
                randomPlaceIndex = (byte) UnityEngine.Random.Range(0, preySpawns.Count - 1);
            } while (preySpawnsUsed[randomPlaceIndex]);

            preySpawnsUsed[randomPlaceIndex] = true;
            randomPlace = preySpawns[randomPlaceIndex];

        }
        else
        {
            //this player is a Predator

            //check character
            chosenSet = predatorPrefabs[launchInfo.character];

            //Choose a spawn
            do
                randomPlaceIndex = (byte) UnityEngine.Random.Range(0, predatorSpawns.Count - 1);
            while (predatorSpawnsUsed[randomPlaceIndex]);

            preySpawnsUsed[randomPlaceIndex] = true;
            randomPlace = predatorSpawns[randomPlaceIndex];
        }

        bodyToSpawn = chosenSet.controlledBody;
        geoToSpawn = chosenSet.geometry;

        //spawn the geometry assosiated with that prefab
        NetworkObject spawnedGeo = Instantiate(geoToSpawn);
        spawnedGeo.SpawnWithOwnership(playerId);

        //spawn prefab with owner being the client who sent this

        //pick a random place
        //byte randomPlaceIndex = (byte) Random.Range(0,placesToSpawn.Count - 1);
        //Transform randomPlace = placesToSpawn[randomPlaceIndex];

        Controlled3DBody spawned = Instantiate(bodyToSpawn, randomPlace.position, randomPlace.rotation);
        spawned.name = $"SpawnedPlayer_Player{playerId}";
        spawned.NetworkObject.SpawnWithOwnership(playerId);
        spawned.NetworkObject.name = $"SpawnedPlayer_Player{playerId}";
        spawned.characterId.Value = launchInfo.character;

        numSpawned++;

        //add the spawned client to the clientStatus list
        GameManager.Instance.AddClientStatus(playerId, launchInfo);

        if(numSpawned == GameManager.Instance.numPlayersInMatch)
        {
            //OnFinishedSpawningPlayers?.Invoke();
        }

        spawnedObjects.Add(spawned.GetComponent<NetworkObject>());
        spawnedObjects.Add(spawnedGeo);


    }

    [ServerRpc(RequireOwnership = false)] //it probably would be fine if it did require
    private void SpawnFoodServerRpc()
    {
        foodSpawnLocationsUsed = new bool[foodSpawnLocations.Count];
        byte randomPlaceIndex;
        Transform randomPlace;

        for(int i = 0; i < foodSpawnLocations.Count; i+=2)
        {
            //pick a random place

            do
                randomPlaceIndex = (byte) UnityEngine.Random.Range(0, foodSpawnLocations.Count - 1);
            while (foodSpawnLocationsUsed[randomPlaceIndex]);

            foodSpawnLocationsUsed[randomPlaceIndex] = true;

            randomPlace = foodSpawnLocations[randomPlaceIndex];


            //then reselect if randomPlaceIndex is in the busyIndices array

            //Debug.Log($"I'm gonna make a food that is so at {foodSpawnLocations[randomPlaceIndex].name}");

            //NetworkObject foodSpawned = Instantiate(foodToSpawn, randomPlace.position, randomPlace.rotation);
            NetworkObject foodSpawned = Instantiate(foodToSpawn, Vector3.zero, Quaternion.identity);
            foodSpawned.SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);

            foodSpawned.GetComponent<Food>().OnEnable();
            foodSpawned.GetComponent<Food>().locationID = randomPlaceIndex;
            foodSpawned.GetComponent<Food>().foodPos.Value = randomPlace.position;

            spawnedObjects.Add(foodSpawned);
        }
    }

    public Vector3 GetFreeFoodLocation(Food food, int freed)
    {
        byte randomPlaceIndex;

        foodSpawnLocationsUsed[freed] = false;
        
        do
            randomPlaceIndex = (byte) UnityEngine.Random.Range(0, foodSpawnLocations.Count - 1);
        while (foodSpawnLocationsUsed[randomPlaceIndex]);

        foodSpawnLocationsUsed[randomPlaceIndex] = true;

        food.locationID = randomPlaceIndex;

        //Debug.Log(foodSpawnLocations[randomPlaceIndex].position);

        return foodSpawnLocations[randomPlaceIndex].position;

        return Vector3.zero;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        foreach(NetworkObject no in spawnedObjects.ToArray())
        {
            try
            {
                if (no != null)
                    no?.Despawn();
            } 
            catch (NullReferenceException e)
            {

            }

        }
        //Matchmaking.LeaveLobby();
        //if (NetworkManager.Singleton != null) 
            //NetworkManager.Singleton.Shutdown();
    }

}
