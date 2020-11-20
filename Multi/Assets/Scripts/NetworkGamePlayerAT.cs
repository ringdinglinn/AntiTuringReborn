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
        for (int i = 0; i < chatBehaviour.chatDisplayContents.Count; i++) {
            GameObject newState = Instantiate(chatroomStatePrefab);
            ChatroomStates newCS = newState.GetComponent<ChatroomStates>();
            chatroomStates.Add(newCS);
            DontDestroyOnLoad(newState);
        }
        CmdUpdateChatroomState(chatBehaviour.chatDisplayContents.Count);
    }
    [Command]
    public void CmdUpdateChatroomState(int x)
    {
        if (chatroomStates.Count < nrOfChatrooms)
        {
            for (int i = 0; i < chatBehaviour.chatDisplayContents.Count; i++)
            {
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

        CmdGamePlayerConnected();
        GetNameAndColor(Room.GamePlayers.Count - 1);

        CmdAddToGamePlayers();
    }

    public override void OnStartServer() {
        base.OnStartServer();
        Debug.Log("OnStartServer");
    }

    [Command]

    private void CmdAddToGamePlayers()
    {
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
                CmdRequestJoinRoom(chatroomID, fakeName);
                return;
            }
        }
    }

 
    [Command]
    public void CmdRequestJoinRoom(int chatroomID, string fakeName) {
        RequestJoinRoom(chatroomID, fakeName);
    }

    [Server]
    public void RequestJoinRoom(int roomID, string fakeName) {

        if (chatroomStates[roomID].leftFree || chatroomStates[roomID].rightFree)
        {
            RpcOpenChatroom();
            chatroomID = roomID;


            foreach (NetworkGamePlayerAT player in Room.GamePlayers)
            {
                if (player.chatroomStates[roomID].leftFree || player.chatroomStates[roomID].rightFree)
                {
                    if (player.chatroomStates[roomID].leftFree)
                    {
                        player.chatroomStates[roomID].leftFree = false;
                        player.chatroomStates[roomID].leftName = fakeName;
                        player.RpcUpdateChatroomStates(roomID, false, chatroomStates[roomID].rightFree, fakeName, chatroomStates[roomID].rightName);

                        if (player.chatroomID == chatroomID)
                        {
                            Debug.Log("In 1");

                            player.GetComponent<ChatBehaviour>().RpcFillUpMainCanvasOnlyUI(roomID, chatroomStates[roomID].leftFree, chatroomStates[roomID].rightFree, chatroomStates[roomID].leftName, chatroomStates[roomID].rightName);
                        }

                    }
                    else if (player.chatroomStates[roomID].rightFree)
                    {
                        player.chatroomStates[roomID].rightFree = false;
                        player.chatroomStates[roomID].rightName = fakeName;
                        player.RpcUpdateChatroomStates(roomID, player.chatroomStates[roomID].leftFree, false, player.chatroomStates[roomID].leftName, fakeName);
                        if (player.chatroomID == chatroomID)
                        {
                            Debug.Log("In 2");
                            player.GetComponent<ChatBehaviour>().RpcFillUpMainCanvasOnlyUI(roomID, chatroomStates[roomID].leftFree, chatroomStates[roomID].rightFree, chatroomStates[roomID].leftName, chatroomStates[roomID].rightName);
                        }
                    }
                }
                else
                {
                    //room is full message
                    Debug.Log("this should not happen");
                }
            }
            GetComponent<ChatBehaviour>().RpcFillUpMainCanvasTextAndUI(roomID, chatroomStates[roomID].leftFree, chatroomStates[roomID].rightFree, chatroomStates[roomID].leftName, chatroomStates[roomID].rightName);
        }

    }

    [ClientRpc]
    private void RpcOpenChatroom() {
        moveView.MoveViewRight();
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------   
    //Start a Player Leaves a Chatroom logic 
    public void CheckRequestFromLeaveButton() {
        CmdLeaveChatroom(chatroomID, fakeName);
    }
    [Command]
    public void CmdLeaveChatroom(int ID, string fakeName)
    {
        RpcCloseChatroom();
       
     
        //string fakeName = displayName;
        if (chatroomStates[ID].leftName == fakeName)
        {
            foreach (NetworkGamePlayerAT player in room.GamePlayers)
            {
                player.UpdateChatroomStatesEvent(ID, true, chatroomStates[ID].rightFree, "", chatroomStates[ID].rightName);
                player.RpcUpdateChatroomStates(ID, true, player.chatroomStates[ID].rightFree, "", player.chatroomStates[ID].rightName);
                if (player.chatroomID == ID)
                {
                    Debug.Log("Out 1");
                    player.GetComponent<ChatBehaviour>().RpcLeaveMainCanvas(ID, true, player.chatroomStates[ID].rightFree, "", player.chatroomStates[ID].rightName);
                 
                   
                }
            }
            chatroomID = 99;
            GetComponent<ChatBehaviour>().RpcClearMainCanvas(ID, true, chatroomStates[ID].rightFree, "", chatroomStates[ID].rightName);
        }
        else if (chatroomStates[ID].rightName == fakeName)
        {
            foreach (NetworkGamePlayerAT player in room.GamePlayers)
            {
                player.UpdateChatroomStatesEvent(ID, chatroomStates[ID].leftFree, true, chatroomStates[ID].leftName, "");
                player.RpcUpdateChatroomStates(ID, player.chatroomStates[ID].leftFree, true, player.chatroomStates[ID].leftName, "");
                if (player.chatroomID == ID)
                {
                    Debug.Log("Out 2");
                    player.GetComponent<ChatBehaviour>().RpcLeaveMainCanvas(ID, player.chatroomStates[ID].leftFree, true, player.chatroomStates[ID].leftName, "");
                   
                   
                }
            }

            chatroomID = 99;
            GetComponent<ChatBehaviour>().RpcClearMainCanvas(ID, chatroomStates[ID].leftFree, true, chatroomStates[ID].leftName, "");
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
    private void RpcUpdateChatroomStates(int id, bool leftFree, bool rightFree, string leftName, string rightName)
    {
        
       UpdateChatroomStatesEvent(id, leftFree, rightFree, leftName, rightName);
         
    }   
    private void UpdateChatroomStatesEvent(int id, bool leftFree, bool rightFree, string leftName, string rightName)
    {
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