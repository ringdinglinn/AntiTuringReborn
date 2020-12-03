﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.Events;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ChatBehaviour : NetworkBehaviour
{


    public List<GameObject> chatDisplayContents = new List<GameObject>();
    public GameObject mainChatDisplay;
    public List<TMP_InputField> inputFields;
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

    public NetworkManagerAT networkManagerAT;

    public TMP_InputField mainInputField;

    public GameObject speechBubPrefab;
    public override void OnStartAuthority()
    {
        chatbotRoomsIndex[0] = true;
        chatUI.SetActive(true);
        OnMessage += HandleNewMessage;


        chatbotRoomsIndex[0] = true;
        chatbotRoomsIndex[1] = false;

        chatbotRoomsIndex[2] = false;
        chatbotRoomsIndex[3] = false;

        CmdchatbotRooIndexSetup();
        CmdChatbotRoomsIndex();



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
        for (int i = 1; i < inputFields.Count; i++) chatbotRoomsIndex.Add(i, false);
    }
    [Command]
    private void CmdSetup(int indexOfPlayer)
    {
        for (int x = 0; networkManagerAT.GamePlayers[indexOfPlayer].nrOfChatrooms > x; x++)
        {
            listOfChatroomLists.Add(new List<GameObject>());

        }
        // Debug.Log("Nr of Lists in listOfChatroomLists =" + listOfChatroomLists.Count);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void OnStartClient()
    {
        base.OnStartClient();
        networkPlayer = GetComponent<NetworkGamePlayerAT>();
        myFakeName = networkPlayer.fakeName;

        for (int x = 0; 4 > x; x++)
        {
            listOfChatroomLists.Add(new List<GameObject>());
           // Debug.Log(listOfChatroomLists.Count);

        }
    }

    [Command]
    private void CmdServerAddNetworkPlayerAndShit()
    {
        networkPlayer = GetComponent<NetworkGamePlayerAT>();
        for (int x = 0; 4 > x; x++)
        {
            listOfChatroomLists.Add(new List<GameObject>());
            //Debug.Log(listOfChatroomLists.Count);

        }
        RpcClientsDoTheFuckingSameAddNetworkPlayerAndShit();
    }

    private IEnumerator ShortDelay()
    {
        yield return new WaitForSeconds(5);
        CmdServerAddNetworkPlayerAndShit();
    }



    [ClientRpc]
    private void RpcClientsDoTheFuckingSameAddNetworkPlayerAndShit()
    {
        networkPlayer = GetComponent<NetworkGamePlayerAT>();

        if (listOfChatroomLists.Count < 4)
        {
            for (int x = 0; 4 > x; x++)
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

        GameObject speechBubPrefab1 = Instantiate(speechBubPrefab);
        speechBubPrefab1.transform.SetParent(newMessage1.transform);
        //Set Left or right Bound
        if (networkPlayer.chatroomStates[chatroomID].rightName == name)
        {
            newMessage1.GetComponent<Text>().alignment = TextAnchor.LowerRight;
            newMessage1.GetComponent<Text>().rectTransform.anchorMin = new Vector2(1, 0);
            newMessage1.GetComponent<Text>().rectTransform.anchorMax = new Vector2(1, 0);
            newMessage1.GetComponent<Text>().rectTransform.pivot = new Vector2(1, 0);
            newMessage1.GetComponent<Text>().rectTransform.anchoredPosition = new Vector3(0, 0, 0);
            speechBubPrefab1.GetComponent<RawImage>().rectTransform.anchorMin = new Vector2(1, 0);
            speechBubPrefab1.GetComponent<RawImage>().rectTransform.anchorMax = new Vector2(1, 0);
            speechBubPrefab1.GetComponent<RawImage>().rectTransform.pivot = new Vector2(1, 0);
            newMessage1.GetComponent<Text>().fontSize = 12;
            newMessage1.GetComponent<Text>().GetComponent<RectTransform>().sizeDelta = new Vector2(230, 60);
            listOfChatroomLists[chatroomID].Add(newMessage1);
            StartCoroutine(BuildText(newMessage1.GetComponent<Text>(), name + "" + message, 0.02f, speechBubPrefab1.GetComponent<RawImage>(), true));
        }
    
        if (networkPlayer.chatroomStates[chatroomID].leftName == name)
        {          
            newMessage1.GetComponent<Text>().alignment = TextAnchor.LowerLeft;
            newMessage1.GetComponent<Text>().fontSize = 12;
            newMessage1.GetComponent<Text>().GetComponent<RectTransform>().sizeDelta = new Vector2(230, 60);
            listOfChatroomLists[chatroomID].Add(newMessage1);
            StartCoroutine(BuildText(newMessage1.GetComponent<Text>(), name + "" + message, 0.02f, speechBubPrefab1.GetComponent<RawImage>(), true));       
        }
       // StartCoroutine(BuildText(newMessage.GetComponent<Text>(), name + "" + message, 0.02f));
        if (chatroomID == networkPlayer.chatroomID)
        {
            GameObject newMessage2 = Instantiate(textPrefab);
             newMessage2.transform.SetParent(mainChatDisplay.GetComponent<ChatDisplayContent>().scrollPanelContent.transform);
            // newMessage1.GetComponent<Text>().text = name + "" + message;
            newMessage2.transform.localScale = new Vector3(1, 1, 1);
            GameObject speechBubPrefab2 = Instantiate(speechBubPrefab);
            speechBubPrefab2.transform.SetParent(newMessage2.transform);
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
                speechBubPrefab2.GetComponent<RawImage>().rectTransform.anchorMin = new Vector2(1, 0);
                speechBubPrefab2.GetComponent<RawImage>().rectTransform.anchorMax = new Vector2(1, 0);
                speechBubPrefab2.GetComponent<RawImage>().rectTransform.pivot = new Vector2(1, 0);
              //  listOfChatroomLists[chatroomID].Add(newMessage2);
                StartCoroutine(BuildText(newMessage2.GetComponent<Text>(), name + "" + message, 0.02f, speechBubPrefab2.GetComponent<RawImage>(), false));

            }
            if (networkPlayer.chatroomStates[chatroomID].leftName == name)
            {
                newMessage2.GetComponent<Text>().alignment = TextAnchor.LowerLeft;
                newMessage2.GetComponent<Text>().fontSize = 20;
                newMessage2.GetComponent<Text>().GetComponent<RectTransform>().sizeDelta = new Vector2(230, 90);
             //   listOfChatroomLists[chatroomID].Add(newMessage2);
                StartCoroutine(BuildText(newMessage2.GetComponent<Text>(), name + "" + message, 0.02f, speechBubPrefab2.GetComponent<RawImage>(), false));
            }
            mainChatDisplayContentList.Add(newMessage2);


       
        }

        //Here we can Update Main Canvas if we are in there
    }


    private IEnumerator BuildText(Text text, string message, float textSpeed, RawImage image, bool smallUpdate)
    {
        if (smallUpdate == true)
        {
            for (int i = 0; i < message.Length; i++)
            {
                text.text = string.Concat(text.text, message[i]);

                TextGenerator textGen = new TextGenerator();
                TextGenerationSettings textGenerationSettings = text.GetComponent<Text>().GetGenerationSettings(text.rectTransform.rect.size);
                float width = textGen.GetPreferredWidth(text.text, textGenerationSettings);
                float height = textGen.GetPreferredHeight(text.text, textGenerationSettings);
                image.rectTransform.sizeDelta = new Vector2(width * 0.9f, height * 0.6f);
              //  image.transform.position = newMessage.transform.position;

                image.rectTransform.anchoredPosition = new Vector3(0, 0, 0);
                //Wait a certain amount of time, then continue with the for loop
                yield return new WaitForSeconds(textSpeed);
            }
        }
        else
        {
          
            for (int i = 0; i < message.Length; i++)
            {
              
                text.text = string.Concat(text.text, message[i]);

                TextGenerator textGen = new TextGenerator();
                TextGenerationSettings textGenerationSettings = text.GetComponent<Text>().GetGenerationSettings(text.rectTransform.rect.size);
                float width = textGen.GetPreferredWidth(text.text, textGenerationSettings);
                float height = textGen.GetPreferredHeight(text.text, textGenerationSettings);
                image.rectTransform.sizeDelta = new Vector2(width * 0.8f, height *0.9f);
                //  testImage.transform.position = newMessage.transform.position;

                image.rectTransform.anchoredPosition = new Vector3(0, 0, 0);
                //Wait a certain amount of time, then continue with the for loop
                yield return new WaitForSeconds(textSpeed);
            }
        }
    }

    [Client]
    public void Send(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        Clear();
        CmdSendMessage(message, chatroomID, networkPlayer.fakeName, networkPlayer.playerVisualPalletID);
        mainInputField.text = "";
        inputFields[chatroomID].text = string.Empty;
        
    }
    public void Clear()
    {
        mainInputField.text = "";
    }
    [Command]
    private void CmdSendMessage(string message, int chatroomID, string name, int visualIDOfPlayerWhoSendMessage)
    {
        //Add new Messege on Sever Entity
        GameObject newMessage = Instantiate(textPrefab, chatDisplayContents[chatroomID].GetComponent<ChatDisplayContent>().scrollPanelContent.transform);
        newMessage.GetComponent<Text>().text = name + "" + message;

        listOfChatroomLists[chatroomID].Add(newMessage);

         
        RpcHandleMessage(message, name, chatroomID, visualIDOfPlayerWhoSendMessage);

        var chatroomBotIndex = networkPlayer.Room.chatbot.chatroomBotIndex;
        if (chatroomBotIndex[chatroomID][0] != -1 || chatroomBotIndex[chatroomID][1] != -1)
        {
            int chatbotID;
            if (!networkPlayer.left)
            {
                chatbotID = chatroomBotIndex[chatroomID][0];
            }
            else
            {
                chatbotID = chatroomBotIndex[chatroomID][1];
            }
            SendMessageToChatbot(message, chatbotID, chatroomID);
        }
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
    public void SendMessageToChatbot(string text, int chatbotID, int chatroomID)
    {
        networkPlayer = GetComponent<NetworkGamePlayerAT>();

        networkPlayer.Room.chatbot.SendTextToChatbot(text, chatroomID, chatbotID);
        //Debug.Log("ChatBehaviour, ReceiveChatbotMessageFromPlayer, chatroom id = " + chatroomID);

    }

    [Command]
    public void CmdSendOutResponseFromChatbot(string r, int id, string chatbotName)
    {
        RpcHandleMessage(r, chatbotName, id);
    }

    public void ReceiveChatbotMessageFromPlayer(string r, int id, string chatbotName)
    {
        // Debug.Log("ChatBehaviour, ReceiveChatbotMessageFromPlayer, chatroom id = " + id);
        r.Remove(0, 1);
        r.Remove(r.Length - 1, 1);
        if (isClientOnly)
        {
            CmdSendOutResponseFromChatbot(r, id, chatbotName);
            return;
        }
        RpcHandleMessage(r, chatbotName, id);
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
        for (int x = 0; listOfChatroomLists[id].Count > x; x++)
        {
            GameObject newMessage = Instantiate(textPrefab, mainChatDisplay.GetComponent<ChatDisplayContent>().scrollPanelContent.transform);
            newMessage.GetComponent<Text>().text = listOfChatroomLists[id][x].GetComponent<Text>().text;
            newMessage.GetComponent<Text>().alignment = listOfChatroomLists[id][x].GetComponent<Text>().alignment;
            mainChatDisplayContentList.Add(newMessage);
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

    public void ClearMainCanvas()
    {
        foreach (GameObject x in mainChatDisplayContentList)
        {
            Destroy(x);
        }
    }


    [ClientRpc]
    public void RpcLeaveMainCanvas(int id, bool leftFree, bool rightFree, string leftName, string rightName)
    {
        ChatDisplayContent cdc = mainChatDisplay.GetComponent<ChatDisplayContent>();

        cdc.leftPerson.gameObject.SetActive(!networkPlayer.chatroomStates[id].leftFree);
        cdc.rightPerson.gameObject.SetActive(!networkPlayer.chatroomStates[id].rightFree);
        cdc.leftName.text = networkPlayer.chatroomStates[id].leftName;
        cdc.rightName.text = networkPlayer.chatroomStates[id].rightName;




        if (networkPlayer.isInvestigator == false)
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
}
