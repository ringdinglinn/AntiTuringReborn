﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.Events;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FMODUnity;


public class ChatBehaviour : NetworkBehaviour
{


    public List<GameObject> chatDisplayContents = new List<GameObject>();
    public GameObject mainChatDisplay;
   // public List<TMP_InputField> inputFields;
    public GameObject chatUI;

    public NetworkGamePlayerAT networkPlayer;

    private static event Action<String, String, int, int> OnMessage;

    public int chatroomID = 99;
    public Dictionary<int, bool> chatbotRoomsIndex = new Dictionary<int, bool>();

    public GameManagerAT gameManagerAT;

    //botname placeholder
    string botname = "???";

    public string myFakeName = "";

    public GameObject textPrefab;
    public List<List<GameObject>> listOfChatroomLists = new List<List<GameObject>>();
    public List<GameObject> mainChatDisplayContentList = new List<GameObject>();

    private bool left;

    public NetworkGamePlayerAT gamePlayerAT;

    public TMP_InputField mainInputField;

    private bool botIsWaitingForMessage = false;

    public Action<int> BuildTextDone;
    private bool[] buildTextIsDoneInChatroomsList = new bool[8]; // hard coded nr of chatrooms

    private string newMessage;

    public override void OnStartAuthority()
    {
        chatbotRoomsIndex[0] = true;
        chatUI.SetActive(true);
        OnMessage += HandleNewMessage;
        BuildTextDone += HandleBuildTextIsDone;

        chatbotRoomsIndex[0] = true;
        chatbotRoomsIndex[1] = false;

        chatbotRoomsIndex[2] = false;
        chatbotRoomsIndex[3] = false;

        CmdchatbotRooIndexSetup();
        CmdChatbotRoomsIndex();


        StartCoroutine(ShortDelay());
        // CmdServerAddNetworkPlayerAndShit();

        mainInputField.onSubmit.AddListener(Send);
    }

    private void HandleBuildTextIsDone(int chatroomID) {
        buildTextIsDoneInChatroomsList[chatroomID] = true;
        CmdBuildTextIsDone(chatroomID);
    }

    [Command]
    private void CmdBuildTextIsDone(int chatroomID) {
        buildTextIsDoneInChatroomsList[chatroomID] = true;
    }

    IEnumerator ShortDelay()
    {
        yield return new WaitForSeconds(1);
        CmdServerAddNetworkPlayerAndShit();
    }


    public void CmdchatbotRooIndexSetup()
    {
        chatbotRoomsIndex[0] = true;
        chatbotRoomsIndex[1] = false;

        chatbotRoomsIndex[2] = false;
        chatbotRoomsIndex[3] = false;

        networkPlayer = GetComponent<NetworkGamePlayerAT>();
    }
    [Command]
    private void CmdChatbotRoomsIndex()
    {
       // for (int i = 1; i < inputFields.Count; i++) chatbotRoomsIndex.Add(i, false);
    }
 
    [Header("SoundEffect")]
    public StudioEventEmitter digitalLetterVersion1;
    public StudioEventEmitter digitalLetterVersion2;
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void OnStartClient()
    {
        base.OnStartClient();
        networkPlayer = GetComponent<NetworkGamePlayerAT>();
        myFakeName = networkPlayer.fakeName;

        for (int x = 0; 8 > x; x++)
        {
            listOfChatroomLists.Add(new List<GameObject>());
           // Debug.Log(listOfChatroomLists.Count);

        }
    }

    [Command]
    private void CmdServerAddNetworkPlayerAndShit()
    {
        networkPlayer = GetComponent<NetworkGamePlayerAT>();
        for (int x = 0; 8 > x; x++)
        {
            listOfChatroomLists.Add(new List<GameObject>());
            //Debug.Log(listOfChatroomLists.Count);

        }
        RpcClientsDoTheFuckingSameAddNetworkPlayerAndShit();
    }

    [ClientRpc]
    private void RpcClientsDoTheFuckingSameAddNetworkPlayerAndShit()
    {
        networkPlayer = GetComponent<NetworkGamePlayerAT>();

        if (listOfChatroomLists.Count < 8)
        {
            for (int x = 0; 8 > x; x++)
            {
                listOfChatroomLists.Add(new List<GameObject>());
                //Debug.Log(listOfChatroomLists.Count);

            }
        }
    }

    [ClientCallback]
    private void OnDestroy()
    {
        if (!hasAuthority) return;

        OnMessage -= HandleNewMessage;
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    private void HandleNewMessage(string message, string name, int chatroomID, int visualIDOfPlayerWhoSendMessage)
    {

        //Create and add new Messag   
        GameObject newMessage1 = Instantiate(textPrefab);
        newMessage1.transform.SetParent(chatDisplayContents[chatroomID].GetComponent<ChatDisplayContent>().scrollPanelContent.transform);
        newMessage1.transform.localScale = new Vector3(1, 1, 1);

      
        //Set Left or right Bound
        if (networkPlayer.chatroomStates[chatroomID].rightName == name)
        {
            newMessage1.GetComponent<Text>().alignment = TextAnchor.LowerRight;
            newMessage1.GetComponent<Text>().rectTransform.anchorMin = new Vector2(1, 0);
            newMessage1.GetComponent<Text>().rectTransform.anchorMax = new Vector2(1, 0);
            newMessage1.GetComponent<Text>().rectTransform.pivot = new Vector2(1, 0);
            newMessage1.GetComponent<Text>().rectTransform.anchoredPosition = new Vector3(0, 0, 0);
                     
            newMessage1.GetComponent<Text>().fontSize = 12;
            newMessage1.GetComponent<Text>().GetComponent<RectTransform>().sizeDelta = new Vector2(230, 60);
            listOfChatroomLists[chatroomID].Add(newMessage1);
           // mainChatDisplayContentList.Add(newMessage1);
            StartCoroutine(BuildText(newMessage1.GetComponent<Text>(), name + "\n"   + message  , 0.02f,false, chatroomID));
        }

        if (networkPlayer.chatroomStates[chatroomID].leftName == name)
        {
            newMessage1.GetComponent<Text>().alignment = TextAnchor.LowerLeft;
            newMessage1.GetComponent<Text>().fontSize = 12;
            newMessage1.GetComponent<Text>().GetComponent<RectTransform>().sizeDelta = new Vector2(230, 60);
            listOfChatroomLists[chatroomID].Add(newMessage1);
            StartCoroutine(BuildText(newMessage1.GetComponent<Text>(), name + "\n"   + message , 0.02f,false, chatroomID));

        }
            if (chatroomID == networkPlayer.chatroomID)
            {
                GameObject newMessage2 = Instantiate(textPrefab);
                newMessage2.transform.SetParent(mainChatDisplay.GetComponent<ChatDisplayContent>().scrollPanelContent.transform);

                newMessage2.transform.localScale = new Vector3(1, 1, 1);
                //Set Left or right Bound
                if (networkPlayer.chatroomStates[chatroomID].rightName == name)
                {
                    newMessage2.GetComponent<Text>().alignment = TextAnchor.LowerRight;
                    newMessage2.GetComponent<Text>().fontSize = 20;
                    newMessage2.GetComponent<Text>().GetComponent<RectTransform>().sizeDelta = new Vector2(230, 90);
                    newMessage2.GetComponent<Text>().rectTransform.anchorMin = new Vector2(1, 0);
                    newMessage2.GetComponent<Text>().rectTransform.anchorMax = new Vector2(1, 0);
                    newMessage2.GetComponent<Text>().rectTransform.pivot = new Vector2(1, 0);
                    newMessage2.GetComponent<Text>().rectTransform.anchoredPosition = new Vector3(0, 0, 0);

                 //   listOfChatroomLists[chatroomID].Add(newMessage2);
                    StartCoroutine(BuildText(newMessage2.GetComponent<Text>(), name + "\n"   + message , 0.02f,true, chatroomID));
                }
                if (networkPlayer.chatroomStates[chatroomID].leftName == name)
                {
                    newMessage2.GetComponent<Text>().alignment = TextAnchor.LowerLeft;
                    newMessage2.GetComponent<Text>().fontSize = 20;
                    newMessage2.GetComponent<Text>().GetComponent<RectTransform>().sizeDelta = new Vector2(230, 90);
                  //  listOfChatroomLists[chatroomID].Add(newMessage2);
                    StartCoroutine(BuildText(newMessage2.GetComponent<Text>(), name + "\n"   + message  , 0.02f,true, chatroomID));
                }
                mainChatDisplayContentList.Add(newMessage2);
            }
        
    }


    private IEnumerator BuildText(Text text,  string message, float textSpeed, bool textForMainCanvas, int chatroomID = 0)
    {
        int soundCounter = 0;
        for (int i = 0; i < message.Length; i++)
        {
            if (text != null)
            {
                text.text = string.Concat(text.text, message[i]);
                if (textForMainCanvas == true)
                {
                    if (soundCounter % 2 == 0)
                    {
                        
                        digitalLetterVersion1.Play();
                    }
                    soundCounter++;
                }
                yield return new WaitForSeconds(textSpeed);
            }
        }

        BuildTextDone?.Invoke(chatroomID);
    }
    [Client]
    public void Send(string message)
    {
     

       if( mainInputField.wasCanceled)
       {
          return;
       }
        if (string.IsNullOrWhiteSpace(message)) return;
        Clear();
        newMessage = message;
        CmdSendMessage(newMessage, chatroomID, networkPlayer.fakeName, networkPlayer.playerVisualPalletID, networkPlayer.playerID);
        mainInputField.text = "";
        networkPlayer.CmdUpdateYourTypingVisualInYouChatroom(chatroomID, networkPlayer.fakeName, false, false);
        //inputFields[chatroomID].text = string.Empty;        
    }
    public void Clear()
    {
        mainInputField.text = "";
    }
    [Command]
    private void CmdSendMessage(string message, int chatroomID, string name, int visualIDOfPlayerWhoSendMessage, int playerID)
    {
        //Add new Messege on Sever Entity

        //GameObject newMessage = Instantiate(textPrefab, chatDisplayContents[chatroomID].GetComponent<ChatDisplayContent>().scrollPanelContent.transform);
        //newMessage.GetComponent<Text>().text = name + "" + message;

        //listOfChatroomLists[chatroomID].Add(newMessage);

        //RpcHandleMessage(message, name, chatroomID, visualIDOfPlayerWhoSendMessage);

        //var chatroomBotIndex = networkPlayer.Room.chatbot.chatroomBotIndex;
        //if (chatroomBotIndex[chatroomID][0] != -1 || chatroomBotIndex[chatroomID][1] != -1)
        //{
        //    int chatbotID;
        //    if (!networkPlayer.left)
        //    {
        //        chatbotID = chatroomBotIndex[chatroomID][0];
        //    }
        //    else
        //    {
        //        chatbotID = chatroomBotIndex[chatroomID][1];
        //    }
        //    SendMessageToChatbot(message, chatroomID, chatbotID);
        //}

        ProcessMessage(message, chatroomID, name, visualIDOfPlayerWhoSendMessage, false, playerID);
    }

    [Server]
    public void ChatbotSendsMessage(string message, int chatroomID, string name, int visualIDOfPlayerWhoSendMessage) {
        ProcessMessage(message, chatroomID, name, visualIDOfPlayerWhoSendMessage, true, -1);
    }

    [Server]
    private void ProcessMessage(string message, int chatroomID, string name, int visualIDOfPlayerWhoSendMessage, bool comingFromChatbot, int playerID) {
        GameObject newMessage = Instantiate(textPrefab, chatDisplayContents[chatroomID].GetComponent<ChatDisplayContent>().scrollPanelContent.transform);
        newMessage.GetComponent<Text>().text = name + "" + message;

        listOfChatroomLists[chatroomID].Add(newMessage);

        buildTextIsDoneInChatroomsList[chatroomID] = false;
        RpcHandleMessage(message, name, chatroomID, visualIDOfPlayerWhoSendMessage);

        var chatroomBotIndex = networkPlayer.Room.chatbot.chatroomBotIndex;
        if (chatroomBotIndex[chatroomID][0] != -1 || chatroomBotIndex[chatroomID][1] != -1) {
            int chatbotID;
            if (!comingFromChatbot)
            {
                if (networkPlayer.chatroomStates[chatroomID].leftName != name)
                {
                    chatbotID = chatroomBotIndex[chatroomID][0];
                }
                else
                {
                    chatbotID = chatroomBotIndex[chatroomID][1];

                }
            } else
            {
                if (networkPlayer.chatroomStates[chatroomID].leftName == name)
                {
                    chatbotID = chatroomBotIndex[chatroomID][1];
                }
                else
                {
                    chatbotID = chatroomBotIndex[chatroomID][0];
                }
            }
            if (chatbotID != -1) networkPlayer.Room.chatbot.chatbotAIs[chatbotID].ConversationStarted();
            if (chatbotID != -1) StartCoroutine(WaitForBuildTextIsDone(message, chatbotID, chatroomID, comingFromChatbot, playerID));
        } else {
        }
    }

    IEnumerator WaitForBuildTextIsDone(string message, int chatbotID, int chatroomID, bool fromChatbot, int playerID) {
        while (!buildTextIsDoneInChatroomsList[chatroomID]) {
            yield return new WaitForSeconds(Time.deltaTime);
        }
        SendMessageToChatbot(message, chatbotID, chatroomID, fromChatbot, playerID);
    }

    [ClientRpc]
    private void RpcHandleMessage(string message, string name, int chatroomID, int visualIDOfPlayerWhoSendMessage)
    {
        OnMessage?.Invoke($"\n{message}", name, chatroomID, visualIDOfPlayerWhoSendMessage);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [Client]
    public void SelectChatroom(string s)
    {
        chatroomID = int.Parse(EventSystem.current.currentSelectedGameObject.name);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void SendMessageToChatbot(string text, int chatbotID, int chatroomID, bool fromChatbot, int playerID)
    {
        networkPlayer = GetComponent<NetworkGamePlayerAT>();
        networkPlayer.Room.chatbot.SendTextToChatbot(text, chatroomID, chatbotID, fromChatbot, playerID);
    }
    [Command]
    public void CmdSendOutResponseFromChatbot(string r, int id, string chatbotName, int BotVisualId)
    {
        RpcHandleMessage(r, chatbotName, id, BotVisualId);
    }
    public void ReceiveChatbotMessageFromPlayer(string r, int id, string chatbotName, int BotVisualId)
    {       
        if (isClientOnly)
        {
            CmdSendOutResponseFromChatbot(r, id, chatbotName, BotVisualId);
            return;
        }
        RpcHandleMessage(r, chatbotName, id, BotVisualId);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void UpdateUI(int id, bool leftFree, bool rightFree, string leftName, string rightName, int leftVisualID, int rightVisualID)
    {
      
        // manzgöggeli, name uswertig
        ChatDisplayContent cdc = chatDisplayContents[id].GetComponent<ChatDisplayContent>();

        cdc.leftPerson.gameObject.SetActive(!leftFree);
        cdc.rightPerson.gameObject.SetActive(!rightFree);
        cdc.leftName.text = leftName;
        cdc.rightName.text = rightName;

        if (leftVisualID != 99)
        {
            cdc.leftPerson.enabled = true;
         
            cdc.leftPerson.sprite = gameManagerAT.playerVisualPalletsList[leftVisualID].playerSmall;
        }
        else
        {
            cdc.leftPerson.enabled = false;
            cdc.leftPerson.sprite = null;
        }
        if (rightVisualID != 99)
        {
            cdc.rightPerson.enabled = true;
            cdc.rightPerson.sprite = gameManagerAT.playerVisualPalletsList[rightVisualID].playerSmall;
        }
        else
        {
            cdc.rightPerson.enabled = false;
            cdc.rightPerson.sprite = null;
        }

        // join button uswertig
        foreach (GameObject x in chatDisplayContents)
        {
            if (networkPlayer.isInvestigator == false )
            {
                if (networkPlayer.playerIsDead == false)
                {
                    ChatDisplayContent newCdc = x.GetComponent<ChatDisplayContent>();
                    newCdc.joinButton.interactable = true;
                }
            }
        }

        foreach (GameObject x in chatDisplayContents)
        {
            if (networkPlayer.isInvestigator == false)
            {
                if (networkPlayer.playerIsDead == false)
                {
                    ChatDisplayContent newCdc = x.GetComponent<ChatDisplayContent>();
                    if (newCdc.rightName.text == networkPlayer.fakeName || newCdc.leftName.text == networkPlayer.fakeName)
                    {
                        foreach (GameObject y in chatDisplayContents)
                        {
                            ChatDisplayContent newCdc2 = y.GetComponent<ChatDisplayContent>();
                            newCdc2.joinButton.interactable = false;
                        }
                        // FillUmMainCanvas(id);
                    }
                }
            }
        }

        foreach (GameObject x in chatDisplayContents)
        {
            ChatDisplayContent newCdc = x.GetComponent<ChatDisplayContent>();
            if (newCdc.rightName.text != "" && newCdc.leftName.text != "")
            {
                if (networkPlayer.isInvestigator == false)
                {
                    if (networkPlayer.playerIsDead == false)
                    {
                        newCdc.joinButton.interactable = false;
                    }
                }
            }
        }
    }
    [ClientRpc]
    public void RpcFillUpMainCanvasTextAndUI(int id, bool leftFree, bool rightFree, string leftName, string rightName, int leftVisualID, int rightVisualID)
    {      
        ChatDisplayContent cdc = mainChatDisplay.GetComponent<ChatDisplayContent>();

        if (leftVisualID != 99)
        {
            cdc.leftPerson.enabled = true;
            cdc.leftPerson.sprite = gameManagerAT.playerVisualPalletsList[leftVisualID].playerAliveBig;
        }
        else
        {
            cdc.leftPerson.enabled = false;
            cdc.leftPerson.sprite = null;
        }
        if (rightVisualID != 99)
        {
            cdc.rightPerson.enabled = true;
            cdc.rightPerson.sprite = gameManagerAT.playerVisualPalletsList[rightVisualID].playerAliveBig;
        }
        else
        {
            cdc.rightPerson.enabled = false;
            cdc.rightPerson.sprite = null;
        }
       
        networkPlayer.chatroomID = id;

        if (networkPlayer.chatroomStates[id].numberOfInvestigators > 0)
        {
            cdc.investigatorVisual.SetActive(true);
        }

        if (networkPlayer.isInvestigator == true)
        {
            cdc.inputField.SetActive(false);
        }

        if (networkPlayer.playerIsDead == true)
        {
            cdc.inputField.SetActive(false);
        }

        //Make sure dead Player or Investigator cannot write anything
        if (networkPlayer.isInvestigator == false)
        {
            if (networkPlayer.playerIsDead == false)
            {
                //mainInputField.gameObject.SetActive(true);
            }
            else
            {
                mainInputField.gameObject.SetActive(false);
            }
        }
        else
        {
            mainInputField.gameObject.SetActive(false);
        }
                StartCoroutine(BuildPreviewsTextInMainCanvas(id));

    }

    private IEnumerator BuildPreviewsTextInMainCanvas(int id )
    {
        if(listOfChatroomLists[id].Count > 30) 
        {
            for (int x = 0; listOfChatroomLists[id].Count-24 > x; x++)
            {
                GameObject newMessage = Instantiate(textPrefab, mainChatDisplay.GetComponent<ChatDisplayContent>().scrollPanelContent.transform);
                newMessage.GetComponent<Text>().text = listOfChatroomLists[id][x].GetComponent<Text>().text;
                newMessage.GetComponent<Text>().alignment = listOfChatroomLists[id][x].GetComponent<Text>().alignment;
                newMessage.GetComponent<Text>().fontSize = 20;
                mainChatDisplayContentList.Add(newMessage);
                digitalLetterVersion1.Play();
                yield return new WaitForSeconds(0.06f);
            }           
        }
        else
        {
            for (int x = 0; listOfChatroomLists[id].Count > x; x++)
            {
                GameObject newMessage = Instantiate(textPrefab, mainChatDisplay.GetComponent<ChatDisplayContent>().scrollPanelContent.transform);
                newMessage.GetComponent<Text>().text = listOfChatroomLists[id][x].GetComponent<Text>().text;
                newMessage.GetComponent<Text>().alignment = listOfChatroomLists[id][x].GetComponent<Text>().alignment;
                newMessage.GetComponent<Text>().fontSize = 20;
                mainChatDisplayContentList.Add(newMessage);
                digitalLetterVersion1.Play();
              
                // float waitTime = listOfChatroomLists[id][x].GetComponent<Text>().text.Length * 0.001f;
                //     StartCoroutine(BuildText(newMessage.GetComponent<Text>(), listOfChatroomLists[id][x].GetComponent<Text>().text, 0.001f));
                yield return new WaitForSeconds(0.06f);
                // StartCoroutine(BuildPreviewsTextInMainCanvas(id));
            }
        }
        yield return new WaitForEndOfFrame();
    }

    [ClientRpc]
    public void RpcFillUpMainCanvasOnlyUI(int id, bool leftFree, bool rightFree, string leftName, string rightName, int leftVisualID, int rightVisualID)
    {
        ChatDisplayContent cdc = mainChatDisplay.GetComponent<ChatDisplayContent>();

        cdc.leftPerson.gameObject.SetActive(!leftFree);
        cdc.rightPerson.gameObject.SetActive(!rightFree);
        cdc.leftName.text = leftName;
        cdc.rightName.text = rightName;

        if (leftVisualID != 99)
        {
            cdc.leftPerson.enabled = true;
            cdc.leftPerson.sprite = gameManagerAT.playerVisualPalletsList[leftVisualID].playerAliveBig;
        }
        else
        {
            cdc.leftPerson.enabled = false;
            cdc.leftPerson.sprite = null;
        }
        if (rightVisualID != 99)
        {
            cdc.rightPerson.enabled = true;
            cdc.rightPerson.sprite = gameManagerAT.playerVisualPalletsList[rightVisualID].playerAliveBig;
        }
        else
        {
            cdc.rightPerson.enabled = false;
            cdc.rightPerson.sprite = null;
        }
        networkPlayer.chatroomID = id;

        if (networkPlayer.isInvestigator == true)
        {
            cdc.inputField.SetActive(false);
        }
    }

    [ClientRpc]
    public void RpcLeaveMainCanvas(int id, bool leftFree, bool rightFree, string leftName, string rightName, bool botCallsActions)
    {
        ChatDisplayContent cdc = mainChatDisplay.GetComponent<ChatDisplayContent>();
        cdc.leftPerson.gameObject.SetActive(!networkPlayer.chatroomStates[id].leftFree);
        cdc.rightPerson.gameObject.SetActive(!networkPlayer.chatroomStates[id].rightFree);
        cdc.leftName.text = networkPlayer.chatroomStates[id].leftName;
        cdc.rightName.text = networkPlayer.chatroomStates[id].rightName;
       
        if (networkPlayer.isInvestigator == false && botCallsActions == false)
        {
            cdc.investigatorVisual.SetActive(false);
        }      
    }
    [ClientRpc]
    public void RpcClearMainCanvas(int id, bool leftFree, bool rightFree, string leftName, string rightName)
    {
        ChatDisplayContent cdc = mainChatDisplay.GetComponent<ChatDisplayContent>();

        cdc.leftPerson.gameObject.SetActive(!networkPlayer.chatroomStates[id].leftFree);
        cdc.rightPerson.gameObject.SetActive(!networkPlayer.chatroomStates[id].rightFree);
        cdc.leftName.text = networkPlayer.chatroomStates[id].leftName;
        cdc.rightName.text = networkPlayer.chatroomStates[id].rightName;
        cdc.typingLeft.enabled = false;
        cdc.typingRight.enabled = false;
       
        foreach (GameObject x in mainChatDisplayContentList)
        {
            Destroy(x);
        }
    }


    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------Investigator is watching
    public void UpdateMainChatPanelInvestigatorVisual(int id, int totalNumberofInvestigatorsInRoom)
    {
        if (totalNumberofInvestigatorsInRoom > 0 && id == networkPlayer.chatroomID)
        {
            ChatDisplayContent cdc = mainChatDisplay.GetComponent<ChatDisplayContent>();
            cdc.investigatorVisual.SetActive(true);
        }
        else if (totalNumberofInvestigatorsInRoom == 0 && id == networkPlayer.chatroomID)
        {
            ChatDisplayContent cdc = mainChatDisplay.GetComponent<ChatDisplayContent>();
            cdc.investigatorVisual.SetActive(false);
        }
    }
    public void UpdateSmallPanelsInvestigatorVisualBetweenInvestigators(int id, int totalNumberofInvestigatorsInRoom)
    {
        if (totalNumberofInvestigatorsInRoom > 0)
        {
            ChatDisplayContent cdc = chatDisplayContents[id].GetComponent<ChatDisplayContent>();
            cdc.investigatorVisual.SetActive(true);
        }
        if (totalNumberofInvestigatorsInRoom == 0)
        {
            ChatDisplayContent cdc = chatDisplayContents[id].GetComponent<ChatDisplayContent>();
            cdc.investigatorVisual.SetActive(false);
        }
    }
    public void ToggleInvestigatorButton(int id, bool status)
    {
        // join button uswertig
        foreach (GameObject x in chatDisplayContents)
        {
            ChatDisplayContent newCdc = x.GetComponent<ChatDisplayContent>();
            newCdc.joinButton.interactable = status;
        }
    }

    public void ToggleDeadPlayerButtons(bool status)
    {
        // join button uswertig
        foreach (GameObject x in chatDisplayContents)
        {
            ChatDisplayContent newCdc = x.GetComponent<ChatDisplayContent>();
            newCdc.joinButton.interactable = status;
        }
    }



    public void UpdateTypingVisualOfAPlayer(int chatroomID, string playerFakeName, bool typingStatus)
    {      
        foreach ( NetworkGamePlayerAT x in gamePlayerAT.room.GamePlayers )
        {      
            if (x.chatroomID == chatroomID)
            {          
                if (x.chatBehaviour.mainChatDisplay.GetComponent<ChatDisplayContent>().leftName.text== playerFakeName && playerFakeName != x.fakeName)                   
                {
                    x.chatBehaviour.mainChatDisplay.GetComponent<ChatDisplayContent>().typingLeft.enabled = typingStatus;
                }
                if (x.chatBehaviour.mainChatDisplay.GetComponent<ChatDisplayContent>().rightName.text == playerFakeName && playerFakeName != x.fakeName)
                {
                    x.chatBehaviour.mainChatDisplay.GetComponent<ChatDisplayContent>().typingRight.enabled = typingStatus;
                }
            }
        }
    }
}


/*Stupid
 *     float width = textGen.GetPreferredWidth(text.text, textGenerationSettings);
                float height = textGen.GetPreferredHeight(text.text, textGenerationSettings);

                float spacingHeight = 0.6f;
                float spacingWidth = 0.8f;

                var extends = text.cachedTextGenerator.rectExtents.size * 0.5f;
                float hightOfOneLine = text.cachedTextGeneratorForLayout.GetPreferredHeight("A", text.GetGenerationSettings(extends));

              

                if (textGen.lineCount == 1)
                {
                    spacingHeight += textGen.lineCount * spacingHeightLine1;
                    spacingWidth += textGen.lineCount * spacingWidthLine1;
                }
                if (textGen.lineCount == 2)
                {
                    spacingHeight += textGen.lineCount * spacingHeightLine2;
                    spacingWidth += textGen.lineCount * spacingWidthLine2;
                }
                if (textGen.lineCount == 3)
                {
                    spacingHeight += textGen.lineCount * spacingHeightLine3;
                    spacingWidth += textGen.lineCount * spacingWidthLine3;
                }
                if (textGen.lineCount == 4)
                {
                    spacingHeight += textGen.lineCount * spacingHeightLine4;
                    spacingWidth += textGen.lineCount * spacingWidthLine4;
                }
                if (textGen.lineCount >= 5)
                {
                    spacingHeight += textGen.lineCount * spacingHeightLine5;
                    spacingWidth += textGen.lineCount * spacingWidthLine5;
                }
                if (textGen.lineCount >3)
                {
                    spacingHeight += textGen.lineCount * spacingHeightLine6;
                    spacingWidth += textGen.lineCount * spacingWidthLine6;
                }
                if(width1 > 417)
                {
                    width1 = 417;
                }
                else if(width1 > 50)
                {
                    width1 = width1 / 1.1f;
                }

              

                image.rectTransform.sizeDelta = new Vector2(width1, height - hightOfOneLine +1);
            
                Debug.Log("LineCout = " + textGen.lineCount);
                image.rectTransform.anchoredPosition = new Vector3(-10, -3, 0);
                //Wait a certain amount of time, then continue with the for loop
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * */
