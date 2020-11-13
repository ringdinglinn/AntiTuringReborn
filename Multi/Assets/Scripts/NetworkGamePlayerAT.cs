using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;



public class NetworkGamePlayerAT : NetworkBehaviour {
    [SyncVar]
    private string displayName = "Loading...";

    private NetworkManagerAT room;
    public ChatBehaviour chatBehaviour;

    public bool isInvestigator = false;

    public GameObject investigatorText;

    public static event Action<int, bool, bool, string, string> OnChatroomStateUpdate;

    public List<GameObject> joinButtonsList = new List<GameObject>();
    public List<GameObject> leaveButtonsList = new List<GameObject>();

    public class ChatroomState {
        public int id;
        public bool leftFree;
        public bool rightFree;
        public string leftName;
        public string rightName;
        public ChatroomState(int id, bool leftFree, bool rightFree, string leftName, string rightName) {
            this.id = id;
            this.leftFree = leftFree;
            this.rightFree = rightFree;
            this.leftName = leftName;
            this.rightName = rightName;
        }
    }

    private List<ChatroomState> chatroomStates = new List<ChatroomState>();

    public int chatroomID;

    private void InitializeChatroomStates() {
        for (int i = 0; i < chatBehaviour.chatDisplays.Count; i++) {
            ChatroomState newCS = new ChatroomState(i, true, true, "", "");
            chatroomStates.Add(newCS);
        }
    }

    public override void OnStartServer() {
        base.OnStartServer();
        //InitializeChatroomStates();
    }

    public override void OnStartAuthority() {
        OnChatroomStateUpdate += UpdateChatroomStatesEvent;
    }

    public void CheckRequestFromJoinButton() {

        for(int x = 0; joinButtonsList.Count > x; x++) {
            if(EventSystem.current.currentSelectedGameObject == joinButtonsList[x]) {

                chatroomID = x;
                chatBehaviour.chatroomID = x;
                CmdRequestJoinRoom(chatroomID);
                return;
            }
        }
    }

    public void CheckRequestFromLeaveButton() {
        for (int x = 0; leaveButtonsList.Count > x; x++) {
            if (EventSystem.current.currentSelectedGameObject == leaveButtonsList[x]) {

                chatroomID = x;
                chatBehaviour.chatroomID = x;
                CmdLeaveChatroom(chatroomID);
                return;
            }
        }
    }
    
    [Command]
    public void CmdRequestJoinRoom(int chatroomID) {
        string fakeName = displayName;
        
        Debug.Log("in cmd request left is " + chatroomStates[chatroomID].leftFree);
        if (chatroomStates[chatroomID].leftFree || chatroomStates[chatroomID].rightFree) {
            if (chatroomStates[chatroomID].leftFree) {
                Debug.Log("I am the left player");
                chatroomStates[chatroomID].leftFree = false;
                chatroomStates[chatroomID].leftName = fakeName;
                RpcUpdateChatroomStates(chatroomID, false, chatroomStates[chatroomID].rightFree, fakeName, chatroomStates[chatroomID].rightName);
                OnChatroomStateUpdate?.Invoke(chatroomID, false, chatroomStates[chatroomID].rightFree, fakeName, chatroomStates[chatroomID].rightName);
            }
            else if (chatroomStates[chatroomID].rightFree) {
                Debug.Log("I am right player");
                chatroomStates[chatroomID].rightFree = false;
                chatroomStates[chatroomID].rightName = fakeName;
                RpcUpdateChatroomStates(chatroomID, chatroomStates[chatroomID].leftFree, false, chatroomStates[chatroomID].leftName, fakeName);
                OnChatroomStateUpdate?.Invoke(chatroomID, chatroomStates[chatroomID].leftFree, false, chatroomStates[chatroomID].leftName, fakeName);
            }
        } else {
            //room is full message
            Debug.Log("this should not happen");
        }
        Debug.Log("in cmd request 2 left is " + chatroomStates[chatroomID].leftFree);
    }

    [Command]
    public void CmdUpdateChatroomStates(int id, bool leftFree, bool rightFree, string leftName, string rightName) {
        Debug.Log("information from server, left is " + leftFree);
        chatroomStates[id].leftFree = leftFree;
        Debug.Log("left is " + chatroomStates[id].leftFree);
        chatroomStates[id].rightFree = rightFree;
        chatroomStates[id].leftName = leftName;
        chatroomStates[id].rightName = rightName;
        Debug.Log("in network player: " + chatBehaviour.chatDisplayContents.Count);
        Debug.Log("in np: " + chatBehaviour.chatDisplays.Count);
        //GetComponent<ChatBehaviour>().UpdateUI(id, leftFree, rightFree, leftName, rightName);
    }

    [ClientRpc]
    private void RpcUpdateChatroomStates(int id, bool leftFree, bool rightFree, string leftName, string rightName) {
        OnChatroomStateUpdate?.Invoke(id, leftFree, rightFree, leftName, rightName);
    }

    private void Update() {
        if (isServer) {
            Debug.Log("update server left is free: " + chatroomStates[1].leftFree + ", " + displayName);
        }
    }

    private void UpdateChatroomStatesEvent(int id, bool leftFree, bool rightFree, string leftName, string rightName){
        Debug.Log("information from server, left is " + leftFree);
        chatroomStates[id].leftFree = leftFree;
        Debug.Log("left is " + chatroomStates[id].leftFree);
        chatroomStates[id].rightFree = rightFree;
        chatroomStates[id].leftName = leftName;
        chatroomStates[id].rightName = rightName;
        Debug.Log("in network player: " + chatBehaviour.chatDisplayContents.Count);
        Debug.Log("in np: " + chatBehaviour.chatDisplays.Count);
        GetComponent<ChatBehaviour>().UpdateUI(id, leftFree, rightFree, leftName, rightName);
        CmdUpdateChatroomStates(id, leftFree, rightFree, leftName, rightName);
    }

    // this returns and sometimes assigns the network manager to our player
    // maybe this is because we can't assign it to the prefab, since it exists in the scene
    public NetworkManagerAT Room {
        get {
            if (room != null) return room;
            return room = NetworkManager.singleton as NetworkManagerAT;
        }
    }

    public override void OnStartClient() {
        DontDestroyOnLoad(gameObject);
        Room.GamePlayers.Add(this);
        isInvestigator = Room.GetRole(Room.GamePlayers.Count - 1);
        investigatorText.SetActive(isInvestigator);
        InitializeChatroomStates();
    }

    public override void OnNetworkDestroy() {
        Room.GamePlayers.Remove(this);
    }

    [Server]
    public void SetDisplayName(string displayName) {
        this.displayName = displayName;
    }

    public string GetDisplayName() {
        return displayName;
    }

    public void ReceiveMessageFromChatbot(string r, int id) {
        chatBehaviour.ReceiveChatbotMessageFromPlayer(r, id);
    }

    [Command]
    public void CmdLeaveChatroom(int chatroomID) {
        string fakeName = displayName;
        if (chatroomStates[chatroomID].leftName == fakeName) {
            RpcUpdateChatroomStates(chatroomID, true, chatroomStates[chatroomID].rightFree, "", chatroomStates[chatroomID].rightName);
        } else if (chatroomStates[chatroomID].rightName == fakeName) {
            RpcUpdateChatroomStates(chatroomID, chatroomStates[chatroomID].leftFree, true, chatroomStates[chatroomID].leftName, "");
        } else {
            Debug.Log("this shouldn't happen");
        }
    }
} 
