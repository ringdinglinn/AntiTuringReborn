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

    //Chatbot Visual
    public int playerVisualPalletID;
    public Sprite playerAliveBig;
    public Sprite playerAliveDead;
    public Sprite playerSmall;
    public Color playerColor;


    public int chatroomID = 99;
    public int currentSessionID = 0;

    private bool left;
    private bool conversationStarted = false;
    private bool inChatroom;
    private bool waitingToJoinChatroom;

    private int waitTimeBeforeStartConvo;
    private int startConvoCounter = 0;
    private int maxWaitTimeStartConvo = 500;
    private int minWaitTimeStartConvo = 100;

    private void Start() {
        networkManager = chatbotBehaviour.networkManager;
        chatbotAiID = chatbotBehaviour.chatbotAIs.Count;
        chatbotBehaviour.chatbotAIs.Add(this);
        GameStart();
    }

    public void GameStart() {
        playerID = chatbotAiID + networkManager.nrAwareAI;
        GetStartSetupNameAndVisuals();
        StartWaitToJoin();
    }

    private void GetStartSetupNameAndVisuals() {
        fakeName = networkManager.randomNames[playerID];
        playerVisualPalletID = networkManager.randomPalletsInt[playerID];
        //playerVisualPalletID = networkManager.GetRandomPlayerVisualPalletID();    
    } 

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Wait to join

    private void StartWaitToJoin() {
        waitTime = Random.Range(minWaitTimeJoin, maxWaitTimeJoin);
        currentState = state.WaitToJoin;
        StartCoroutine(WaitToJointCoroutine(waitTime));
    }

    IEnumerator WaitToJointCoroutine(float s) {
        waitingToJoinChatroom = true;
        yield return new WaitForSeconds(s);
        waitingToJoinChatroom = false;
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
            //chatroomID = 0;
            left = chatroomStates[chatroomID].leftFree;
            networkManager.GamePlayers[0].RequestJoinRoom(chatroomID, fakeName, true, playerVisualPalletID);
            currentSessionID = chatbotBehaviour.nextSessionID++;
            chatbotBehaviour.ChangeChatroomBotIndex(chatroomID, chatbotAiID, left);
            networkManager.othersJoinRooms.Play();
            conversationStarted = false;
            inChatroom = true;
        }
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Start Conversation Logic

    public void ConversationStarted() {
        conversationStarted = true;
    }
    private bool evaluatingStartingConvo = false;

    private void FixedUpdate() {
        if (inChatroom && !evaluatingStartingConvo) {
            if (!networkManager.GamePlayers[0].chatroomStates[chatroomID].leftFree && !networkManager.GamePlayers[0].chatroomStates[chatroomID].rightFree) {
                evaluatingStartingConvo = true;
                waitTimeBeforeStartConvo = Random.Range(minWaitTimeStartConvo, maxWaitTimeStartConvo);
                Debug.Log("convo start logic, " + "id = " + chatbotAiID + " waitTimeTotal = " + waitTimeBeforeStartConvo);
            }
        }

        if (evaluatingStartingConvo) {
            if (!conversationStarted) {
                Debug.Log("convo start logic, " + "id = " + chatbotAiID + " counter = " + startConvoCounter);
                startConvoCounter++;
                if (startConvoCounter >= waitTimeBeforeStartConvo) {
                    conversationStarted = true;
                    SendGreeting();
                    startConvoCounter = 0;
                }
            }
        }


        if (!inChatroom && !waitingToJoinChatroom) {
            StartCoroutine(WaitToJointCoroutine(Random.Range(minWaitTimeJoin, maxWaitTimeJoin)));
        }
    }

    private void SendGreeting() {
        ChatbotBehaviour.Response r = new ChatbotBehaviour.Response(chatroomID, "Hi, this is Linn's special conversation starter!", fakeName, playerVisualPalletID);
        chatbotBehaviour.SendResponseToServerDirectly(r);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // In Chatroom

    private void StartWaitToLeave() {

    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Leave Chatroom

    private void LeaveChatroom() {
        chatbotBehaviour.ChangeChatroomBotIndex(chatroomID, chatbotAiID, left);
        networkManager.othersLeaveRooms.Play();
        evaluatingStartingConvo = false;
    }

    //Destroy Bot
    public void DestroyBot()
    {
        LeaveChatroom();
        chatbotBehaviour.chatbotAIs.Remove(this);
        Destroy(this.gameObject);
    }
}
