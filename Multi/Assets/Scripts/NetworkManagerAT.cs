using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;
using FMODUnity;

public class NetworkManagerAT : NetworkManager {

    //Game Setup Related
    [SerializeField] private int minPlayers = 1;
    [Scene] [SerializeField] private string menuScene;

    //Sounds
    [Header("Sounds")]
    public StudioEventEmitter startGameSound;
    public StudioEventEmitter isReadySound;
    public StudioEventEmitter isNotReadySound;
    public StudioEventEmitter lobbyMusic;
    public StudioEventEmitter loadingRoleMusic;
    public StudioEventEmitter revealRoleMusic;
    public StudioEventEmitter invTheme;
    public StudioEventEmitter aiTheme;
    public StudioEventEmitter activateInputFieldSound;
    public StudioEventEmitter otherPersonJoinsRoom;
    public StudioEventEmitter otherPersonLeavesRoom;
    public StudioEventEmitter othersJoinRooms;
    public StudioEventEmitter othersLeaveRooms;
    public StudioEventEmitter openConfirmWindow;
    public StudioEventEmitter closeConfirmWindow;
    public StudioEventEmitter loadingBar;
    public StudioEventEmitter loadingComplete;
    public StudioEventEmitter taggingFailure;
    public StudioEventEmitter ai_AIConnectionMade;
    public StudioEventEmitter botTerminated;
    public StudioEventEmitter inv_AIConnectionFailed;
    public StudioEventEmitter inv_AIConnectionMade;
    public StudioEventEmitter youDied;
    public StudioEventEmitter victorySound;
    public StudioEventEmitter defeatSound;

    [Header("Key Sounds")]
    public StudioEventEmitter normalKeyDownSound;
    public StudioEventEmitter normalKeyUpSound;
    public StudioEventEmitter spaceDownSound;
    public StudioEventEmitter spaceUpSound;
    public StudioEventEmitter enterDownSound;
    public StudioEventEmitter enterUpSound;
    public StudioEventEmitter escapeDownSound;
    public StudioEventEmitter escapeUpSound;

    public KeySoundEffectHandler keySoundEffectHandler;

    //Prefabs
    [Header("Room")]
    [SerializeField] private NetworkRoomPlayerAT roomPlayerPrefab;
    [Header("Game")]
    [SerializeField] private NetworkGamePlayerAT gamePlayerPrefab;
    [Header("ChatbotAI")]
    [SerializeField] private GameObject chatbotAIPrefab;

    //Actions
    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

    //Lists
    public List<NetworkRoomPlayerAT> RoomPlayers = new List<NetworkRoomPlayerAT>();
    public List<NetworkGamePlayerAT> GamePlayers = new List<NetworkGamePlayerAT>();
    public List<Sprite> playerVisualList = new List<Sprite>();
    //Related to Roles
    private bool[] roleIndex;
    public int nrInvestigators;
    public int nrAwareAI;
    public int nrPlayers;
    public int nrOfChatbots;

    //ChatBotRelated
    public ChatbotBehaviour chatbot;

    //Server Status
    public bool isSeverOnly = false;

    //Client Connections
    private int nrOfWaitingClients;
    public event Action OnAllPlayersConnected;

    //Game State
    public event Action OnGameStart;

    public int nrChatrooms = 4;

    //Visual Pallet List
    public List<PlayerVisualPallet> playerVisualPalletsList = new List<PlayerVisualPallet>();

    //Nr of bots by nr of ai players
    private int[] botNrIndex = new int[] { 3 ,  5 ,  7, 9,  10 , 10, 10, 10 , 10 , 10, 10 , 10 };

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void OnStartClient() {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");
   
        foreach (GameObject prefab in spawnablePrefabs) {
            ClientScene.RegisterPrefab(prefab);
        }     
    }

    public override void OnClientConnect(NetworkConnection conn) {
        base.OnClientConnect(conn);
        //Debug.Log("onclientconnect");
    }

    public override void OnClientDisconnect(NetworkConnection conn) {
        base.OnClientDisconnect(conn);
        OnClientDisconnected?.Invoke();
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void OnStartServer() {
        spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList<GameObject>();
        lobbyMusic.Stop();
    }
    public override void OnServerConnect(NetworkConnection conn) {

       
        if (numPlayers >= maxConnections) {
            conn.Disconnect();
            return;
        }

        if ("Assets/Scenes/" + SceneManager.GetActiveScene().name + ".unity" != menuScene) {
            conn.Disconnect();
            return;
        }

        OnClientConnected?.Invoke();
        RandomizeNameAndColorLists(randomNames, randomPalletsInt);
        base.OnServerConnect(conn);
    }
    public override void OnServerAddPlayer(NetworkConnection conn) {      
        if ("Assets/Scenes/" + SceneManager.GetActiveScene().name + ".unity" == menuScene) {
            bool isLeader = RoomPlayers.Count == 0;
            NetworkRoomPlayerAT roomPlayerInstance = Instantiate(roomPlayerPrefab);
            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
            roomPlayerInstance.SetLeader(isLeader);
            roomPlayerInstance.SyncNameAndColorAndAiBotsLists();
        }    
    }
    public override void OnServerDisconnect(NetworkConnection conn) {
        if (conn.identity != null) {
            var player = conn.identity.GetComponent<NetworkRoomPlayerAT>();
            RoomPlayers.Remove(player);
            NotifyPlayersOfReadyState();
        }
        base.OnServerDisconnect(conn);
    }
    public override void OnStopServer() {
        RoomPlayers.Clear();
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void NotifyPlayersOfReadyState() {
        foreach (var player in RoomPlayers) {
            player.HandleReadyToStart(IsReadyToStart());  
        }
    }
    private bool IsReadyToStart() {
        if (numPlayers < minPlayers) {
            return false;
        }

        foreach(var player in RoomPlayers) {
            if (!player.IsReady) {
                return false;
            }
        }
        return true;
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void StartGame() {
        if (SceneManager.GetActiveScene().path == menuScene) {
            if (!IsReadyToStart()) return;

            StopLobbyMusic();
            RoomPlayers[0].RpcPlayStartSound();
            nrPlayers = RoomPlayers.Count;
            nrOfWaitingClients = nrPlayers;
            nrAwareAI = nrPlayers - nrInvestigators;
            //nrOfChatbots = botNrIndex[nrAwareAI];
            //nrOfChatbots = 7; // PLACE HOLDER: TO BE BALANCED!!!
            chatbot.GameStart();
            ServerChangeScene("SampleScene");
        }     
    }

    private void StopLobbyMusic() {
        foreach (NetworkRoomPlayerAT player in RoomPlayers) {
            player.RpcStopLobbyMusic();
        }
    }

    public void StartLoadingRoleMusic() {
        loadingRoleMusic.Play();
    }

    public void StopLoadingRoleMusic() {
        loadingRoleMusic.Stop();
    }

    public void StartRevealRoleMusic() {
      //  UnityEngine.Debug.Log("Start Reveal Role Music");
        revealRoleMusic.Play();
    }

    public void StopRevealRoleMusic() {
     //   UnityEngine.Debug.Log("Stop Reveal Role Music");
        revealRoleMusic.Stop();
    }

    public void StartAITheme() {
        aiTheme.Play();
    }

    public void StartInvTheme() {
        invTheme.Play();
    }

    public override void ServerChangeScene(string newSceneName) {
        if (SceneManager.GetActiveScene().path == menuScene && newSceneName == "SampleScene") {
            for (int i = RoomPlayers.Count - 1; i >= 0; i--) {
                var conn = RoomPlayers[i].connectionToClient;
                var gamePlayerInstance = Instantiate(gamePlayerPrefab);
                gamePlayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);
         
                NetworkServer.Destroy(conn.identity.gameObject);             
                NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject);

                DontDestroyOnLoad(gamePlayerInstance.gameObject);        
            }
            for (int i = 0; i < nrOfChatbots; i++) {
                var chatbotAIinstance = Instantiate(chatbotAIPrefab);
                chatbotAIinstance.GetComponent<ChatbotAI>().chatbotBehaviour = chatbot;
                DontDestroyOnLoad(chatbotAIinstance);
            }
        }
        base.ServerChangeScene(newSceneName);
    }

  
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public bool GetRole(int i) {
        return roleIndex[i];
    }
    public void SetRoleIndex(bool[] newIndex) {
       // Debug.Log("SetRoleIndex()");
        roleIndex = new bool[RoomPlayers.Count];
        roleIndex = newIndex;
    }
    public void SetNrInvestigators(int n) {
        nrInvestigators = n;
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Random name and color
    private int maxNrOfChatParticitpants = 16;
    public List<string> randomNames = new List<string>() { "Hildebrand", "Neko", "Mica", "Vladimir",
                                                           "Berthold", "Maximus", "Dirk", "Gaia",
                                                           "Veronica", "Bob", "Susan", "Bernard",
                                                           "Augustus", "Klaus", "Mortimer", "Yusuke" };

    public List<int> randomPalletsInt = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15};


    public void GamePlayerConnected() {
        nrOfWaitingClients--;
     //   Debug.Log("nrOfWaitingClients = " + nrOfWaitingClients);
        if (nrOfWaitingClients <= 0) {
             OnAllPlayersConnected?.Invoke();
        }
    }

    public static void RandomizeNameAndColorLists(List<string> names, List<int> randomPallets) {
        int n = names.Count;
        for (var i = n - 1; i > 0; i--) {
            var r = UnityEngine.Random.Range(0, n);
            var tmp = names[i];
            names[i] = names[r];
            names[r] = tmp;
        }

        int x = randomPallets.Count;
        for (var i = x - 1; i > 0; i--)
        {
            var r = UnityEngine.Random.Range(0, x);
            var tmp = randomPallets[i];
            randomPallets[i] = randomPallets[r];
            randomPallets[r] = tmp;
        }
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
  
}

