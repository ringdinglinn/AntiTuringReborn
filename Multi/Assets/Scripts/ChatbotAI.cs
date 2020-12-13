﻿using System.Collections;
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

    private float minWaitTimeJoinInit = 5f;
    private float maxWaitTimeJoinInit = 15f;
    private float minWaitTimeJoin = 2f;
    private float maxWaitTimeJoin = 10f;
    private float minWaitTimeLeave = 10f;
    private float maxWaitTimeLeave = 30f;

    private List<ChatroomStates> chatroomStates = new List<ChatroomStates>();

    public string fakeName;

    public string pandoraBotsClientName;

    //Chatbot Visual
    public int playerVisualPalletID;
    public Sprite playerAliveBig;
    public Sprite playerAliveDead;
    public Sprite playerSmall;
    public Color playerColor;


    public int chatroomID = 99;
    public int currentSessionID = 0;

    public bool left;
    private bool conversationStarted = false;
    private bool inChatroom;
    private bool waitingToJoinChatroom;

    private int waitTimeBeforeStartConvo;
    private int startConvoCounter = 0;
    private int maxWaitTimeStartConvo = 200;
    private int minWaitTimeStartConvo = 10;

    private bool dead;

    public bool typing = false;
    public bool typingVisual = false;

    private void Start() {
        networkManager = chatbotBehaviour.networkManager;
        chatbotAiID = chatbotBehaviour.chatbotAIs.Count;
        chatbotBehaviour.chatbotAIs.Add(this);
        pandoraBotsClientName = chatbotBehaviour.clientNameList[networkManager.GamePlayers.Count + chatbotAiID];
    }

    public void GameStart() {
        Debug.Log("chatbotAI: GameStart()");
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
        Debug.Log("start wait to join");
        waitTime = Random.Range(minWaitTimeJoinInit, maxWaitTimeJoinInit);
        currentState = state.WaitToJoin;
        StartCoroutine(WaitToJoinCoroutine(waitTime));
    }

    IEnumerator WaitToJoinCoroutine(float s) {
        Debug.Log("wait to join routine");
        waitingToJoinChatroom = true;
        yield return new WaitForSeconds(s);
        waitingToJoinChatroom = false;
        JoinChatroom();
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Join Chatroom

    private void JoinChatroom() {
        Debug.Log("join chatroom");
        chatroomStates = networkManager.GamePlayers[0].chatroomStates;
        List<int> indeces = new List<int>();
        List<int> higherPrioIndeces = new List<int>();
        left = false;
        for (int i = 0; i < chatroomStates.Count; i++) {
            if (chatroomStates[i].leftFree || chatroomStates[i].rightFree) {
                indeces.Add(i);
                if (!chatroomStates[i].leftFree || !chatroomStates[i].rightFree) {
                    higherPrioIndeces.Add(i);
                }
            }
        }
        Debug.Log("high priority indeces = " + higherPrioIndeces.Count);
        Debug.Log("indeces = " + indeces.Count);
        if (higherPrioIndeces.Count > 0) {
            chatroomID = higherPrioIndeces[Random.Range(0, higherPrioIndeces.Count)];
            //chatroomID = 0;
            left = chatroomStates[chatroomID].leftFree;
            networkManager.GamePlayers[0].RequestJoinRoom(chatroomID, fakeName, true, playerVisualPalletID);
            currentSessionID = chatbotBehaviour.nextSessionID++;
            chatbotBehaviour.ChangeChatroomBotIndex(chatroomID, chatbotAiID, left, true);
            networkManager.othersJoinRooms.Play(); // wait, this will only play on server
            conversationStarted = false;
            inChatroom = true;
            StartCoroutine(StartWaitToLeave());
        } else if (indeces.Count > 0) {
            chatroomID = indeces[Random.Range(0, indeces.Count)];
            //chatroomID = 0;
            left = chatroomStates[chatroomID].leftFree;
            networkManager.GamePlayers[0].RequestJoinRoom(chatroomID, fakeName, true, playerVisualPalletID);
            currentSessionID = chatbotBehaviour.nextSessionID++;
            chatbotBehaviour.ChangeChatroomBotIndex(chatroomID, chatbotAiID, left, true);
            networkManager.othersJoinRooms.Play(); // wait, this will only play on server
            conversationStarted = false;
            inChatroom = true;
            StartCoroutine(StartWaitToLeave());
        } else {
            StartCoroutine(WaitToJoinCoroutine(Random.Range(minWaitTimeJoin, maxWaitTimeJoin)));
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
            }
        }

        if (evaluatingStartingConvo) {
            //Debug.Log("converstation started = " + conversationStarted);
            if (!conversationStarted) {
                startConvoCounter++;
                if (startConvoCounter == waitTimeBeforeStartConvo) {
                    conversationStarted = true;
                    SendGreeting();
                    startConvoCounter = 0;
                }
            }
        }


        //if (!inChatroom && !waitingToJoinChatroom) {
        //    StartCoroutine(WaitToJoinCoroutine(Random.Range(minWaitTimeJoin, maxWaitTimeJoin)));
        //}
    }

    private void SendGreeting() {
        ChatbotBehaviour.Response r = new ChatbotBehaviour.Response(chatroomID, "Hi, this is Linn's special conversation starter!", fakeName, playerVisualPalletID, 0);
        chatbotBehaviour.SendResponseToServerDirectly(r);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // In Chatroom

    IEnumerator StartWaitToLeave() {
        float waitToLeaveTime = Random.Range(minWaitTimeLeave, maxWaitTimeLeave);
        yield return new WaitForSeconds(waitToLeaveTime);
        StartCoroutine(WaitToLeaveTillMessageIsSent());
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Leave Chatroom

    private void LeaveChatroom() {
        Debug.Log("leave chatroom");
        networkManager.othersLeaveRooms.Play(); // only plays on server :(
        evaluatingStartingConvo = false;
        inChatroom = false;
        networkManager.GamePlayers[0].LeaveChatroom(chatroomID, fakeName, true);
        chatbotBehaviour.ChangeChatroomBotIndex(chatroomID, chatbotAiID, left, false);
        float waitTime = Random.Range(minWaitTimeJoin, maxWaitTimeJoin);
        if (!dead) StartCoroutine(WaitToJoinCoroutine(waitTime));
    }

    IEnumerator WaitToLeaveTillMessageIsSent() {
        Debug.Log("start to leave till message is sent");
        while (typing) {
            yield return new WaitForEndOfFrame();
        }
        LeaveChatroom();
    }

    public void StartTyping() {
        typing = true;
        networkManager.GamePlayers[0].RpcUpdateYourTypingVisualInYouChatroom(chatroomID, fakeName, true, true);
    }

    public void StopTypingAfterDelay(float time) {
        StartCoroutine(WaitToStopTypingAfterDelay(time));
    }

    IEnumerator WaitToStopTypingAfterDelay(float time) {
        networkManager.GamePlayers[0].RpcUpdateYourTypingVisualInYouChatroom(chatroomID, fakeName, false, true);
        yield return new WaitForSeconds(time/4f);
        typing = false;
    }

    //Destroy Bot
    public void DestroyBot()
    {
        dead = true;
        LeaveChatroom();
        Destroy(gameObject);
    }
}
