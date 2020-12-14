using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using FMODUnity;

public class NetworkRoomPlayerAT : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[16];
    [SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[16];
    [SerializeField] private Image[] playerReadyIcons = new Image[16];
    [SerializeField] private Button startGameButton;
    [SerializeField] private GameObject gameSettings;
    [SerializeField] private TMP_Text nrInvestigatorsText;
    [SerializeField] private Sprite readySprite;
    [SerializeField] private Sprite notReadySprite;
    [SerializeField] private GameObject warningSymbol;
    [SerializeField] private GameObject warningSymbol2;

    [Header("Logic")]
    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;
    [SyncVar]
    public int nrInvestigators;

    public bool isLeader;
    public bool isInvestigator;

    public bool[] roleIndex;

    [SyncVar]
    public int nrOfPlayers;
    [SyncVar]
    public int nrOfBots;
    
    private NetworkManagerAT room;


   
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void OnStartAuthority() {
        CmdSetDisplayName(PlayerNameInput.DisplayName);
        lobbyUI.SetActive(true);

     
    }


    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void SetLeader(bool value) {
        IsLeader = value;
        if (value) RpcSetAsLeader();
    }
    public bool IsLeader {
        set {
            isLeader = value;
            startGameButton.gameObject.SetActive(value);
            gameSettings.SetActive(value);
        }
    }
    [Command]
    private void CmdSetAsLeader() {
        IsLeader = true;
        RpcSetAsLeader();
    }
    [ClientRpc]
    private void RpcSetAsLeader() {      
        IsLeader = true;
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------   
    // this returns and sometimes assigns the network manager to out player
    // maybe this is because we can't assign it to the prefab, since it exists in the scene
    public NetworkManagerAT Room {
        get {
            if (room != null) return room;
            return room = NetworkManager.singleton as NetworkManagerAT;
        }
    }
    public override void OnStartClient() {      
        Room.RoomPlayers.Add(this);
        CmdAddToRoomPlayers(); // Hier muss man checken ob es Localhost gibt oder nicht, wenn server dann ausführen
        CmdChangeNrOfPlayers(Room.RoomPlayers.Count);
        UpdateDisplay();      
    }
    [Command]
    private void CmdAddToRoomPlayers() {
        Room.RoomPlayers.Add(this);
    }
    [Command]
    private void CmdChangeNrOfPlayers(int n) {
        nrOfPlayers = n;
    }
    public override void OnStopClient() {
        Room.RoomPlayers.Remove(this);
        UpdateDisplay();
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------   
    public void HandleReadyStatusChanged(bool oldValue, bool newValue) {
        if (oldValue != newValue) {
            if (newValue) {
                Room.isReadySound.Play();
            } else {
                Room.isNotReadySound.Play();
            }
        }
        UpdateDisplay();
    }
    public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateDisplay();
    private void UpdateDisplay() {
        if (!isLocalPlayer) {
            // i dont understand this. this method is called by a sync var. if it is called
            // on all clients then we have to check if this is the local player so it isn't
            // called on each several times? but then we still have to execute on the local player
            // if it is called on everyone, why wouldn't it execute once for each as they are their
            // local players?
            foreach (var player in Room.RoomPlayers) {
                if (player.isLocalPlayer) {
                    player.UpdateDisplay();
                    break;
                }
            }
            return;
        }

        // clear all player names and ready states
        for (int i = 0; i < playerNameTexts.Length; i++) {
            playerNameTexts[i].text = "Waiting for player...";
            playerReadyTexts[i].text = string.Empty;
            playerReadyIcons[i].sprite = null;
            playerReadyIcons[i].gameObject.SetActive(false);
        }

        //for all the players set their current names and ready states
        for (int i = 0; i < Room.RoomPlayers.Count; i++) {
            playerNameTexts[i].text = Room.RoomPlayers[i].DisplayName;
            playerReadyTexts[i].text = Room.RoomPlayers[i].IsReady ? "Ready" : "Not Ready";
            playerReadyIcons[i].sprite = Room.RoomPlayers[i].IsReady ? readySprite : notReadySprite;
            playerReadyIcons[i].gameObject.SetActive(true);
        }
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------   
    public void HandleReadyToStart(bool readyToStart) {
        if (!isLeader) return;

        startGameButton.interactable = readyToStart;
    }
    [Command]
    private void CmdSetDisplayName(string displayName) {
        DisplayName = displayName;
    }
    [Command]
    public void CmdReadyUp() {
        IsReady = !IsReady;
        Room.NotifyPlayersOfReadyState();
    }
    [Command]
    public void CmdStartGame() {
        DistributeRoles();
        // Server ensures that this client connection is the leader
        if (Room.RoomPlayers[0].connectionToClient != connectionToClient) return;

        Room.StartGame();
    }
    [Server]
    private void DistributeRoles() {
        roleIndex = new bool[Room.RoomPlayers.Count];
        for (int i = 0; i < nrInvestigators; i++) {
            roleIndex[i] = true;
        }
        roleIndex = RandomizeArray(roleIndex);
        HandleRoleIndexChanged();
        RpcRoleIndexChanged(roleIndex);
    }
    static bool[] RandomizeArray(bool[] arr) {
        for (var i = arr.Length - 1; i > 0; i--) {
            var r = Random.Range(0, arr.Length);
            var tmp = arr[i];
            arr[i] = arr[r];
            arr[r] = tmp;
        }
        return arr;
    }
    [ClientRpc]
    public void RpcRoleIndexChanged(bool[] newIndex) {
        roleIndex = newIndex;
        HandleRoleIndexChanged();
    }
    private void HandleRoleIndexChanged() {
        Room.SetRoleIndex(roleIndex);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------   
    public void ChangeNrInvestigators(string input) {
        if (isLeader) {
            if (int.TryParse(input, out int result)) {
                if ((nrOfPlayers - result + nrOfBots) <= 16) {
                    CmdSetNrInvestigators(result);
                    warningSymbol.SetActive(false);
                } else {
                    warningSymbol.SetActive(true);
                }
            }
            else {
                warningSymbol.SetActive(true);
            }
        }
    }
    public void SetColorBlack(string input) {
        //nrInvestigatorsText.color = Color.white;
    }
    [Command]
    public void CmdSetNrInvestigators(int n) {
        nrInvestigators = n;
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------   
    public void ChangeNrBots(string input) {
        if (isLeader) {
            if (int.TryParse(input, out int result)) {
                if ((nrOfPlayers - nrInvestigators + result) <= 16) {
                    CmdSetNrBots(result);
                    warningSymbol2.SetActive(false);
                } else {
                    warningSymbol2.SetActive(true);
                }
            }
            else {
                warningSymbol2.SetActive(true);
            }
        }
    }

    [Command]
    public void CmdSetNrBots(int n) {
        nrOfBots = n;
        Room.nrOfChatbots = n;
    }


    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Random name and color

    [Server]
    public void SyncNameAndColorAndAiBotsLists() {
        for (int i = 0; i < Room.randomNames.Count; i++) {
            RpcUpdateRndName(Room.randomNames[i], i);          
        }

        for (int i = 0; i < Room.randomPalletsInt.Count; i++)
        {
            RpcUpdateRndVisualIDs(Room.randomPalletsInt[i], i);
        }

        for (int i = 0; i < Room.randomPalletsInt.Count; i++)
        {
            RpcUpdateRndVisualIDs(Room.randomPalletsInt[i], i);
        }
    }

    [ClientRpc]
    private void RpcUpdateRndName(string rndName, int index) {
        Room.randomNames[index] = rndName;


    }
    [ClientRpc]
    private void RpcUpdateRndVisualIDs(int value, int index)
    {
        Room.randomPalletsInt[index] = value;
    }


    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Sounds

    [ClientRpc]
    public void RpcStopLobbyMusic() {
        Room.lobbyMusic.Stop();
    }

    [ClientRpc]
    public void RpcStartLobbyMusic() {
        Room.lobbyMusic.Play();
    }
}
