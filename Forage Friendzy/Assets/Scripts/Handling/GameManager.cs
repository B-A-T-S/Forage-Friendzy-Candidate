using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.SceneManagement;

//Persists through scene loads
//Holds information regarding Match params
//Holds Food Related Collection stuff
public class GameManager : NetworkBehaviour
{
    private static GameManager instance;
    public static GameManager Instance {
        get { return instance; }
    }

    public GameObject localPlayer;
    [SerializeField] GameObject networkManagerGO;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip sound_NightTransition;
    private AudioSource audioSource;

    public event Action onPreyWin;
    public event Action onPredatorWin;

    public event Action<bool, ClientStatus, ClientStatus> event_EndOfMatch;
    public static event Action event_VolumeSettingsChanged;

    #region Match Variables
    //these are variables that pertain to the conditions of the match
    [Header("Match Variables")]
    [Tooltip("Amount of food needed to be collected for prey victory.")]
    public int foodLimit;
    [Tooltip("Time limit for the match (in seconds).")]
    public float matchDuration, totalDaytime=180.0f, remainingDaytime;

    public NetworkVariable<int> matchFoodCollected; //temporarily serialized to test win conditions
    public int matchFoodHeld;
    [SerializeField] private bool allPreyCaught; //temporarily serialized to test win conditions
    private bool matchTimerComplete;

    public TextMeshProUGUI winText;
    //End game menus
    [Tooltip("Menu display for the winning team")]
    [SerializeField] private GameObject winMenu;
    [Tooltip("Menu display for the lossing team")]
    [SerializeField] private GameObject lossMenu;
    private GameObject instantiatedWinMenu;
    private GameObject instantiatedLossMenu;

    public int numPlayersInMatch;

    [SerializeField]
    private float pollingInterval;
    private float timeElapsed;

    [SerializeField, HideInInspector]
    public NetworkVariable<float> sliderValue;
    [SerializeField, HideInInspector]
    public NetworkVariable<bool> isNight;
    public float sunsetAnimDur;
    [SerializeField] private Material skyboxMat, dayMat, nightMat;
    [SerializeField]
    private List<Material> blend;

    private Skybox skybox;
    private Color daySky, nightSky, currentSky;
    //game objs for sun slider and moon icon

    public bool isMatch;
    public List<ClientStatus> clientStatus = new();

    public GameObject predatorDoor, centerDepo;

    #endregion

    #region Team Variables

    private const int NUM_OF_PREDATORS = 2;
    private const int NUM_OF_PREY = 4;
    [Tooltip("Prefab to generate for prey team.")]
    [SerializeField]
    private GameObject preyPrefab;
    [Tooltip("Prefab to generate for predator team.")]
    [SerializeField]
    private GameObject predatorPrefab;
    //private GameObject[] predatorTeam;
    //private GameObject[] preyTeam;
    public Dictionary<ulong, GameObject> predatorTeam;
    public Dictionary<ulong, GameObject> preyTeam;

    #endregion

    #region Local Variables

    [Header("Audio Control")]
    public float masterVol = 1;
    public float musicVol = 1, sfxVol = 1; // some day load this from player prefs
    public float baseMusicVol = 1, baseSFXVol = 1;
    public float mouseSensitivity = 1;

    [Header("")]
    public string playerName; //maybe this goes in the client launch info

    #endregion

    #region Start

    private bool hasSpawnedNetworkManager;

    private void Awake()
    {
        
    }

    private void Start()
    {
        if(instance == null)
        {
            instance = this;
            if(!hasSpawnedNetworkManager)
            {
                Instantiate(networkManagerGO);
                hasSpawnedNetworkManager = true;
                isNight.OnValueChanged += IsNight_OVC;
            }
        }
        else
            Destroy(gameObject);

        Reset();

        audioSource = GetComponent<AudioSource>();
        matchFoodCollected.OnValueChanged += FoodCollected_OnValueChanged;
        DontDestroyOnLoad(this.gameObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        clientStatus.Clear();
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong disconnectedClientID)
    {
        int indexOfMatch = clientStatus.FindIndex(x => x.clientId == disconnectedClientID);
        if (IsServer && isMatch)
            if(indexOfMatch >= 0)
            {
                ClientStatus match = clientStatus[indexOfMatch];
                match.isConnected = false;
                clientStatus[indexOfMatch] = match;
            }
    }

    public void Reset()
    {
        if (IsServer)
            matchFoodCollected.Value = 0;

        allPreyCaught = false;
        matchTimerComplete = false;
        isMatch = false;
        predatorTeam = new();
        preyTeam = new();

        remainingDaytime = totalDaytime;
    }

    private void FoodCollected_OnValueChanged(int previousValue, int newValue)
    {
        //Debug.Log($"Prey Team's Collected Food went from {previousValue} to {newValue}");

        //check for win
        CheckMatchOver();
    }

    #endregion

    #region Update

    void Update()
    {

        if (!isMatch)
            return;

        timeElapsed += Time.deltaTime;
        if (timeElapsed >= pollingInterval)
        {
            //behaviours we only want to poll periodically

            //CheckNighttime();

            FoodHeld();

            CheckAllPreyCaught();

            //Check for win
            CheckMatchOver();


            timeElapsed -= pollingInterval;
        }
    }

    #endregion

    #region Reason

    public void CheckMatchOver()
    {

        if (matchFoodCollected.Value >= foodLimit)
            PreyWin();
        else if (allPreyCaught)
            PredatorWin();
    }



    public void CheckNighttime()
    {
        if (!IsServer) return;

        remainingDaytime -= timeElapsed;
        if (remainingDaytime > 0)
        {

            sliderValue.Value = remainingDaytime / totalDaytime;
            //move slider on each user (might be a clientRpc situation)
            /*
             sunSlider.Value=remainingDaytime/totalDaytime; // have the slider go from 1 to 0 instead of 0 to 1
             */

            return;
        }

        isNight.Value = true;

        //NIGHT
        //replace slider with night icon

        //GameManager.Instance.AssertMatchComplete();
        //AssertMatchComplete() will ask the server to check if the match is actually complete, and push the rele

        // onValueChanged, replace an array of values with another array of values (say defaultVals[] with nightVals[], which mods whatever it mods
        // (food spawn rate, attack recovery speed, idk)

        //heres a coroutine:
        /*
         public IEnumerator NightFalls()
    {
        float i = 0, fadeTime = 0.75f;
        /*
        
        while(i<fadeTime)
        {
        //probably a layoutGroup or whatever they are
        sunUIParent.alpha = 1-(i/fadeTime);

        yield return null;

        }
        
        sunUIParent.gameobject.setActive(false);
        moonUIParent.gameobject.setActive(true);
        i=0;

        while(i<fadeTime)
        {
        //probably a layoutGroup or whatever they are
        moonUIParent.alpha = (i/fadeTime);

        yield return null;

        }

        moonUIParent.alpha = 1;

         *//*

        //to appease the monobehaviour gods
        yield return null; //take this out in final
    }
         
         */
    }

    public void IsNight_OVC(bool prev, bool next)
    {
        //find the skybox, then run a dimming coroutine
        StartCoroutine(Sunset());

        //play night transition sound
        audioSource?.PlayOneShot(sound_NightTransition);




        /* NIGHT REQUESTS:

        
        








        //overwrite trail code to always spawn trails
        //change any movement speeds?
        //amplify any perks?
        //activate fireflies ArrayList<GamObj>
        //coroutine to lerp skybox
        */
    }

    public IEnumerator Sunset()
    {
        float therskold = sunsetAnimDur / blend.Count;
        //Debug.Log(therskold);
        float targ = therskold;
        int j = 0;
        float i = 0;

        //RenderSettings.skybox = nightMat;
        while (i < sunsetAnimDur)
        {
            if (i >= targ)
            {
                targ += therskold;


                RenderSettings.skybox = blend[j];
                j++;
            }
            //Debug.Log(Time.deltaTime);
            //Debug.Log("j = " + j);

            yield return null;
            i += Time.deltaTime;
        }
        RenderSettings.skybox = blend[j];

    }

    #endregion

    #region Act

    /*
     * Call this every time a prey reaches the fainted state.
     * This way we arent constantly checking if all prey are fainted and only checking each time a new prey enters the fainted state.
     */
    public void CheckAllPreyCaught()
    {

        if (!(preyTeam.Count >= 1))
            return;

        //traverse through all prey, if all are fainted then tell game manager
        for (int i = 0; i < preyTeam.Count; i++)
        {
            //if we find a prey that is not fainted then we can leave this function
            if (!preyTeam.ElementAt(i).Value.GetComponent<PreyHealth>().isFainted.Value)
            {
                return;
            }
        }
        //reaching this point means all prey are fainted
        allPreyCaught = true;
    }

    //Called whenever food is deposited by prey
    public void FoodDeposited(int amount)
    {
        //matchFoodCollected.Value += amount;

        //inform server
        FoodDepositedServerRpc(amount);


    }

    [ServerRpc(RequireOwnership = false)]
    public void FoodDepositedServerRpc(int amount)
    {
        matchFoodCollected.Value += amount;
    }

    //iterate through prey list and sum up their food values
    public void FoodHeld()
    {
        int sum = 0;
        foreach(KeyValuePair<ulong,GameObject> preyObj in preyTeam)
        {
            if (preyObj.Value == null)
            {
                preyTeam.Remove(preyObj.Key);
                continue;
            }

            PreyFood preyFood = preyObj.Value.GetComponent<PreyFood>();
            if(preyFood != null)
            {
                sum += preyFood.playerfood.Value;
            }
        }

        //assign value
        matchFoodHeld = sum;
    }

    //in future will be used to display win screen for predators and loss for prey
    public void PredatorWin()
    {
        onPredatorWin.Invoke();
        /*
        //winText.text = "<color=#88FF00>Predator Players Win!</color>";
        if (IsClient)
        {
            if (predatorTeam.ContainsKey(NetworkManager.Singleton.LocalClientId))
            {
                //Display Victory
                instantiatedWinMenu = Instantiate(winMenu);
            }
            else if (preyTeam.ContainsKey(NetworkManager.Singleton.LocalClientId))
            {
                //Display loss menu
                instantiatedLossMenu = Instantiate(lossMenu);
            }
        }
        if(IsHost)
            StartCoroutine(DelayedFunction(() => { TryExitMatch(); }, 3f));
        */

        //Determine MVP
        event_EndOfMatch?.Invoke(ClientLaunchInfo.Instance.role == 1, 
            clientStatus.Find(x => x.clientId == NetworkManager.Singleton.LocalClientId), DetermineMVP(1));
    }

    private ClientStatus DetermineMVP(int role)
    {

        int currentMaxScore = int.MinValue;
        ClientStatus currentMVP = clientStatus.ElementAt(0);
        foreach(ClientStatus cs in clientStatus)
        {
            if (cs.role != role)
                continue;

            int sum = 0;
            if (role == 0)
                sum = (cs.metrics[0] * 1) + (cs.metrics[1] * 2);
            else
                sum = (cs.metrics[2] * 1) + (cs.metrics[3] * 2);
            
            if(sum > currentMaxScore)
            {
                currentMaxScore = sum;
                currentMVP = cs;
            }

        }

        return currentMVP;
    }

    //in future will be used to display win screen for prey and loss for predators
    public void PreyWin()
    {
        onPreyWin.Invoke();
        //winText.text = "<color=#88FF00>Prey Players Win!</color>";
        /*
        if (IsClient)
        {
            if (preyTeam.ContainsKey(NetworkManager.Singleton.LocalClientId))
            {
                //Display Victory
                instantiatedWinMenu = Instantiate(winMenu);
            }
            else if (predatorTeam.ContainsKey(NetworkManager.Singleton.LocalClientId))
            {
                //Display loss menu
                instantiatedLossMenu = Instantiate(lossMenu);
            }
        }

        if (IsHost)
            StartCoroutine(DelayedFunction(() => { TryExitMatch(); }, 3f));
        */
        event_EndOfMatch?.Invoke(ClientLaunchInfo.Instance.role == 0,
            clientStatus.Find(x => x.clientId == NetworkManager.Singleton.LocalClientId), DetermineMVP(0));
    }

    //call after everyone is loaded into game, possibly after a prematch timer?
    public void StartMatchTimer()
    {
        StartCoroutine(MatchTimer());
    }

    IEnumerator DelayedFunction(Action toPerform, float toWait)
    {
        yield return new WaitForSeconds(toWait);
        toPerform?.Invoke();
    }

    IEnumerator MatchTimer()
    {
        yield return new WaitForSeconds(matchDuration);
        matchTimerComplete = true;
    }

    public void TryExitMatch()
    {
        if (!isMatch)
            return;

        if(IsHost)
        {
            ExitMatch();
        }
        else if (IsClient)
        {
            InformServerClientExitedServerRpc(NetworkManager.Singleton.LocalClientId);
            ExitMatch();
        }
    }

    [ClientRpc]
    private void UnlockClientMouseClientRpc()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    [ClientRpc]
    public void NetworkSceneLoadingScreenClientRpc(string text)
    {

        if (IsServer)
            return;

        using (new LoadNetworkScene(text, NetworkManager.Singleton))
        {

        }
    }

    public async void ExitMatch()
    {

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isMatch = false;
        Reset();
        LobbyManager.Instance.ResetPlayerReadyStatusClientRpc();

        //unlock lobby and load lobby scene
        if(IsServer)
        {
            using (new LoadNetworkScene("Exiting Match...", NetworkManager.Singleton))
            {
                UnlockClientMouseClientRpc();
                await Matchmaking.UnlockLobby();
                //LoadSceneUtil.Instance.NM_BySceneName("LobbyScene");
                NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
            }
        }
        else
        {
            using (new LoadScene("Exiting Match..."))
            {
                NetworkManager.Singleton.Shutdown();
                await Matchmaking.LeaveLobby();
                LoadSceneUtil.Instance.PreviousBuildIndex();
            }
        }
    }

    public void CloseApplication()
    {
#if UNITY_STANDALONE || UNITY_STANDALONE_WIN 
        Application.Quit();
#endif
#if UNITY_EDITOR_WIN || UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif

    }

    public void AddClientStatus(ulong clientId, ClientLaunchInfo launchInfo)
    {
        clientStatus.Add(new ClientStatus(clientId, launchInfo));
    }

    [ServerRpc]
    public void EditClientMetricServerRpc(ulong clientId, int metricIndex, int amount)
    {
        int indexOfMatch = clientStatus.FindIndex(x => x.clientId == clientId);
        if (indexOfMatch >= 0)
        {
            ClientStatus match = clientStatus[indexOfMatch];
            match.metrics[metricIndex] += amount;
            clientStatus[indexOfMatch] = match;
        }
                
    }

    [ClientRpc]
    public void ClientExitMatchClientRpc()
    {
        if (IsHost)
            return;
        InformServerClientExitedServerRpc(NetworkManager.LocalClientId);
        ExitMatch();
    }

    [ServerRpc(RequireOwnership = false)]
    public void InformServerClientExitedServerRpc(ulong clientId)
    {
        ClientStatus cs = clientStatus.Find(x => x.clientId == clientId);
        int indexOfClient = clientStatus.IndexOf(cs);
        if (indexOfClient != -1)
        {
            //update the list ref
            cs.isConnected = false;
            clientStatus[indexOfClient] = cs;
            //Debug.Log($"Client {clientId}'s Connection Status has been set to {clientStatus[indexOfClient].isConnected}");
        }
    }

#endregion

    #region Helpers
    //This will contain functions for other scripts to call (eg. food needs a list of players)
    public List<GameObject> GetPlayerBodies()
    {
        return null; //for now
    }

    //add/remove for player lists
    public void AddToPlayerList(ulong clientId, GameObject toAdd, bool isPred)
    {

        //Debug.Log($"{toAdd.name} has been added to a player list");

        if (isPred)
            predatorTeam.Add(clientId, toAdd);
        else
            preyTeam.Add(clientId, toAdd);
    }

    public void RemoveFromPlayerList(ulong clientId, GameObject toRemove)
    {
        if (predatorTeam.ContainsKey(clientId))
            predatorTeam.Remove(clientId);
        else if (preyTeam.ContainsKey(clientId))
            preyTeam.Remove(clientId);

    }

    public void EditVal(float resultant, int signature) //-99 for mouse sens, 0 master, 1 music, 2 sfx
    {
        if (signature == -99)
        {
            mouseSensitivity = resultant;
            return;
        }

        if (signature == 0) masterVol = resultant;
        else if (signature == 1) baseMusicVol = resultant;
        else baseSFXVol = resultant;

        sfxVol = baseSFXVol * masterVol;
        musicVol = baseMusicVol * masterVol;

        event_VolumeSettingsChanged?.Invoke();

    }

    #endregion

}

[Serializable]
public struct ClientStatus
{
    public ulong clientId;
    public int role;
    public int character;
    public int cosmetic;
    public string playerName;

    public int[] metrics;

    public bool isConnected;

    public ClientStatus(ulong _clientId, ClientLaunchInfo launchInfo)
    {
        clientId = _clientId;
        role = launchInfo.role;
        character = launchInfo.character;
        cosmetic = launchInfo.cosmetic;
        playerName = launchInfo.playerName;

        metrics = new int[4];

        isConnected = true;
    }

    public enum StatIndex
    {
        FoodDeposited,
        NumRevives,
        AttacksLanded,
        Knockouts
    }

}

public enum CustomLayers
{
    Prey = 11,
    Predator,
    Food,
    Depo,
    Scurry,
    ScurryBreak,
    PreyXRay,
    PredXRay,
    Terrain,
    Grass,
    PreyTopLevel,
    PredatorTopLevel
}

public enum Findable
{
    PredatorDoor,
    CenterDepo
}
