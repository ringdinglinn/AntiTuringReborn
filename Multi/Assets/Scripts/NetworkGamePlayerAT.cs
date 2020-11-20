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

    public string fakeName;
    public string newName;
    public Color32 color;

    public NetworkManagerAT room;
    public ChatBehaviour chatBehaviour;

    public bool isInvestigator = false;

    public GameObject investigatorText;

    public List<GameObject> joinButtonsList = new List<GameObject>();
    public List<GameObject> leaveButtonsList = new List<GameObject>();

    public MoveView moveView;
    public List<ChatroomStates> chatroomStates = new List<ChatroomStates>();

    public GameObject chatroomStatePrefab;
    public int chatroomID;

    public int nrOfChatrooms = 4;

    public int playerID;


    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------   
    private void InitializeChatroomStates() {
        for (int i = 0; i < chatBehaviour.chatDisplays.Count; i++) {
            GameObject newState = Instantiate(chatroomStatePrefab);
            ChatroomStates newCS = newState.GetComponent<ChatroomStates>();
            chatroomStates.Add(newCS);
            DontDestroyOnLoad(newState);
        }
        CmdUpdateChatroomState(chatBehaviour.chatDisplays.Count);
    }
    [Command]
    public void CmdUpdateChatroomState(int x) {
        if (chatroomStates.Count < nrOfChatrooms) {
            for (int i = 0; i < chatBehaviour.chatDisplays.Count; i++) {
                GameObject newState = Instantiate(chatroomStatePrefab);
                ChatroomStates newCS = newState.GetComponent<ChatroomStates>();
                chatroomStates.Add(newCS);
            }
        }
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------   
    // this returns and sometimes assigns the network manager to our player
    // maybe this is because we can't assign it to the prefab, since it exists in the scene
    public NetworkManagerAT Room {
        get {
            if (room != null) return room;
            return room = NetworkManager.singleton as NetworkManagerAT;
        }
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------   
    public override void OnStartClient() {
        DontDestroyOnLoad(gameObject);
        playerID = Room.GamePlayers.Count;
        Room.GamePlayers.Add(this);
        isInvestigator = Room.GetRole(Room.GamePlayers.Count - 1);
        investigatorText.SetActive(isInvestigator);
        InitializeChatroomStates();
        CmdAddToGamePlayers(playerID);
        CmdGamePlayerConnected();
        GetNameAndColor(Room.GamePlayers.Count - 1);
    }

    public override void OnStartServer() {
        base.OnStartServer();
        Debug.Log("OnStartServer");
    }

    [Command]
    private void CmdAddToGamePlayers(int id) {
        Debug.Log("CmdAddToGamePlayers by " + id);
        Room.GamePlayers.Add(this);
    }
    public override void OnStopClient() {
        Room.GamePlayers.Remove(this);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------   
    [Server]
    public void SetDisplayName(string displayName) {
        this.displayName = displayName;
    }
    public string GetDisplayName() {
        return displayName;
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------   
    public void ReceiveMessageFromChatbot(string r, int id) {
        chatBehaviour.ReceiveChatbotMessageFromPlayer(r, id);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------   
    //Start a Player Joins a Chatroom logic 
    public void CheckRequestFromJoinButton() {
        for (int x = 0; joinButtonsList.Count > x; x++) {
            if (EventSystem.current.currentSelectedGameObject == joinButtonsList[x]) {
                chatroomID = x;
                chatBehaviour.chatroomID = x;
                CmdRequestJoinRoom(chatroomID);
                return;
            }
        }
    }
    [Command]
    public void CmdRequestJoinRoom(int chatroomID) {
        RequestJoinRoom(chatroomID);
    }

    [Server]
    public void RequestJoinRoom(int chatroomID) {
        // will have to pass name in the future
        if (chatroomStates[chatroomID].leftFree || chatroomStates[chatroomID].rightFree) {
            RpcOpenChatroom();
        }

        foreach (NetworkGamePlayerAT player in room.GamePlayers) {
            string fakeName = displayName;
            if (player.chatroomStates[chatroomID].leftFree || player.chatroomStates[chatroomID].rightFree) {
                if (player.chatroomStates[chatroomID].leftFree) {
                    player.chatroomStates[chatroomID].leftFree = false;
                    player.chatroomStates[chatroomID].leftName = fakeName;
                    player.RpcUpdateChatroomStates(chatroomID, false, chatroomStates[chatroomID].rightFree, fakeName, chatroomStates[chatroomID].rightName);
                }
                else if (player.chatroomStates[chatroomID].rightFree) {
                    player.chatroomStates[chatroomID].rightFree = false;
                    player.chatroomStates[chatroomID].rightName = fakeName;
                    player.RpcUpdateChatroomStates(chatroomID, player.chatroomStates[chatroomID].leftFree, false, player.chatroomStates[chatroomID].leftName, fakeName);
                }
            }
            else {
                //room is full message
                Debug.Log("this should not happen");
            }
        }
    }

    [ClientRpc]
    private void RpcOpenChatroom() {
        moveView.MoveViewRight();
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------   
    //Start a Player Leaves a Chatroom logic 
    public void CheckRequestFromLeaveButton() {
        CmdLeaveChatroom(chatroomID);
    }
    [Command]
    public void CmdLeaveChatroom(int chatroomID) {
        RpcCloseChatroom();

        string fakeName = displayName;
        if (chatroomStates[chatroomID].leftName == fakeName) {
            foreach (NetworkGamePlayerAT player in room.GamePlayers) {
                player.UpdateChatroomStatesEvent(chatroomID, true, chatroomStates[chatroomID].rightFree, "", chatroomStates[chatroomID].rightName);
                player.RpcUpdateChatroomStates(chatroomID, true, player.chatroomStates[chatroomID].rightFree, "", player.chatroomStates[chatroomID].rightName);
            }

        }
        else if (chatroomStates[chatroomID].rightName == fakeName) {
            foreach (NetworkGamePlayerAT player in room.GamePlayers) {
                player.UpdateChatroomStatesEvent(chatroomID, chatroomStates[chatroomID].leftFree, true, chatroomStates[chatroomID].leftName, "");
                player.RpcUpdateChatroomStates(chatroomID, player.chatroomStates[chatroomID].leftFree, true, player.chatroomStates[chatroomID].leftName, "");
            }

        }
        else {
            Debug.Log("this shouldn't happen");
        }
    }
    [ClientRpc]
    private void RpcCloseChatroom() {
        moveView.MoveViewLeft();
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------   
    [ClientRpc]
    private void RpcUpdateChatroomStates(int id, bool leftFree, bool rightFree, string leftName, string rightName) {
        if (isClientOnly) {
            UpdateChatroomStatesEvent(id, leftFree, rightFree, leftName, rightName);
        }
    }
    private void UpdateChatroomStatesEvent(int id, bool leftFree, bool rightFree, string leftName, string rightName) {
        chatroomStates[id].leftFree = leftFree;
        chatroomStates[id].rightFree = rightFree;
        chatroomStates[id].leftName = leftName;
        chatroomStates[id].rightName = rightName;

        GetComponent<ChatBehaviour>().UpdateUI(id, leftFree, rightFree, leftName, rightName);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Random name and color


    [Command]
    private void CmdGamePlayerConnected() {
        Room.GamePlayerConnected();
    }

    private void GetNameAndColor(int index) {
        fakeName = Room.randomNames[index];
        Debug.Log(Room.randomColors[index].r + ", " + Room.randomColors[index].g + ", " + Room.randomColors[index].b);
        color = new Color32(Room.randomColors[index].r, Room.randomColors[index].g, Room.randomColors[index].b, 255);
    }
}