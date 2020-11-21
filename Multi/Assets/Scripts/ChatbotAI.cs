using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatbotAI : MonoBehaviour
{
    public ChatbotBehaviour chatbotBehaviour;
    private NetworkManagerAT networkManager;

    private int chatbotAiID;
    private int playerID;

    private enum state {Idle, WaitToJoin, JoinChat, InChat};
    private state currentState = state.Idle;

    private float waitTime;

    private float minWaitTimeJoin = 2f;
    private float maxWaitTimeJoin = 10f;

    private List<ChatroomStates> chatroomStates = new List<ChatroomStates>();

    public string fakeName;
    public Color32 color;

    public int chatroomID = 99;
    public int currentSessionID = 0;

    private bool left;

    private void Start() {
        networkManager = chatbotBehaviour.networkManager;
        chatbotAiID = chatbotBehaviour.chatbotAIs.Count;
        chatbotBehaviour.chatbotAIs.Add(this);
        GameStart();
    }

    public void GameStart() {
        Debug.Log("game start");
        playerID = chatbotAiID + networkManager.nrAwareAI;
        GetNameAndColor();
        StartWaitToJoin();
    }

    private void GetNameAndColor() {
        fakeName = networkManager.randomNames[playerID];
        color = networkManager.randomColors[playerID];
    } 

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Wait to join

    private void StartWaitToJoin() {
        waitTime = Random.Range(minWaitTimeJoin, maxWaitTimeJoin);
        currentState = state.WaitToJoin;
        StartCoroutine(WaitToJointCoroutine(waitTime));
    }

    IEnumerator WaitToJointCoroutine(float s) {
        yield return new WaitForSeconds(s);
        JoinChatroom();
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Join Chatroom

    private void JoinChatroom() {
        chatroomStates = networkManager.GamePlayers[0].chatroomStates;
        List<int> indeces = new List<int>();
        left = false;
        for (int i = 0; i < chatroomStates.Count; i++) {
            if (chatroomStates[i].leftFree || chatroomStates[i].rightFree) {
                indeces.Add(i);
            }
        }
        if (indeces.Count > 0) {
            chatroomID = indeces[Random.Range(0, indeces.Count)];
            left = chatroomStates[chatroomID].leftFree;
            networkManager.GamePlayers[0].RequestJoinRoom(chatroomID, fakeName, true);
            currentSessionID = chatbotBehaviour.nextSessionID++;
            Debug.Log(fakeName +"\n change chatroom bot index: " + chatroomID + ", " + chatbotAiID + ", " + left);
            chatbotBehaviour.ChangeChatroomBotIndex(chatroomID, chatbotAiID, left);
        }
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // In Chatroom

    private void StartWaitToLeave() {

    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Leave Chatroom

    private void LeaveChatroom() {
        chatbotBehaviour.ChangeChatroomBotIndex(chatroomID, chatbotAiID, left);
    }


    private void Update() {
        switch (currentState) {
            case (state.Idle): {
                    break;
                }
            case (state.WaitToJoin): {
                    break;
                }
            case (state.InChat): {
                    break;
                }
        }
    }
}
