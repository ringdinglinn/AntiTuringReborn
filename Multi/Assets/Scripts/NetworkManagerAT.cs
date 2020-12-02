using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class NetworkManagerAT : NetworkManager {

    //Game Setup Related
    [SerializeField] private int minPlayers = 1;
    [Scene] [SerializeField] private string menuScene;

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
    public override void OnStartServer() => spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList<GameObject>();
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
        RandomizeNameAndColorLists(randomNames);
        base.OnServerConnect(conn);
    }
    public override void OnServerAddPlayer(NetworkConnection conn) {      
        if ("Assets/Scenes/" + SceneManager.GetActiveScene().name + ".unity" == menuScene) {
            bool isLeader = RoomPlayers.Count == 0;
            Debug.Log(isLeader);
            NetworkRoomPlayerAT roomPlayerInstance = Instantiate(roomPlayerPrefab);
            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
            roomPlayerInstance.SetLeader(isLeader);
            roomPlayerInstance.SyncNameAndColorLists();
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
        if (numPlayers < minPlayers) return false;

        foreach(var player in RoomPlayers) {
            if (!player.IsReady) return false;
        }
        return true;
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void StartGame() {
        if (SceneManager.GetActiveScene().path == menuScene) {
            if (!IsReadyToStart()) return;

            nrPlayers = RoomPlayers.Count;
            nrOfWaitingClients = nrPlayers;
            nrAwareAI = nrPlayers - nrInvestigators;
            nrOfChatbots = 7; // PLACE HOLDER: TO BE BALANCED!!!
            chatbot.GameStart();
            ServerChangeScene("SampleScene");
        }     
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
                Debug.Log("instantiate chatbot");
                var chatbotAIinstance = Instantiate(chatbotAIPrefab);
                chatbotAIinstance.GetComponent<ChatbotAI>().chatbotBehaviour = chatbot;
                DontDestroyOnLoad(chatbotAIinstance);
            }
        }
        base.ServerChangeScene(newSceneName);
    }
    public Sprite GetRandomPic()
    {
        int randomPictureNumber = UnityEngine.Random.Range(0, playerVisualList.Count);

        Sprite spriteReferene = playerVisualList[randomPictureNumber];
        playerVisualList.RemoveAt(randomPictureNumber);
    //    Debug.Log(spriteReferene);
        return spriteReferene;
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
    public List<string> randomNames = new List<string>() { "Hildebrand", "Neko", "Mica", "Jùlian",
                                                           "Berthold", "Maximus", "Dirk", "Gaia",
                                                           "Veronica", "Bob", "Susan", "Bernard",
                                                           "Augustus", "Klaus", "Mortimer", "Yusuke" };
  
    public int GetRandomPlayerVisualPalletID()
    {
        int playerVisualPalletID = UnityEngine.Random.Range(0, playerVisualPalletsList.Count);      
        playerVisualPalletsList.RemoveAt(playerVisualPalletID);
        return playerVisualPalletID;
    }

    public void GamePlayerConnected() {
        nrOfWaitingClients--;
     //   Debug.Log("nrOfWaitingClients = " + nrOfWaitingClients);
        if (nrOfWaitingClients <= 0) {
             OnAllPlayersConnected?.Invoke();
        }
    }

    public static void RandomizeNameAndColorLists(List<string> names) {
        int n = names.Count;
        for (var i = n - 1; i > 0; i--) {
            var r = UnityEngine.Random.Range(0, n);
            var tmp = names[i];
            names[i] = names[r];
            names[r] = tmp;
        }

     
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void UpdateBotListForAllTagManagementsOnClients()
    {
        foreach (NetworkGamePlayerAT x in GamePlayers)
        {
            x.tagManagement.allTagableBotPlayersList.AddRange(chatbot.chatbotAIs);
        }
    }
}

