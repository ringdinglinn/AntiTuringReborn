using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.Events;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatBehaviour : NetworkBehaviour {
    
 
    public List<GameObject> chatDisplayContents = new List<GameObject>();
    public GameObject mainChatDisplay;
    public List<TMP_InputField> inputFields;
    public GameObject chatUI;

    public NetworkGamePlayerAT networkPlayer;

    private static event Action<String, String, int> OnMessage;

    public int chatroomID;
    public Dictionary<int, bool> chatbotRoomsIndex = new Dictionary<int, bool>();

    public bool check = false;

    //botname placeholder
    string botname = "???";

    public string myFakeName = "";

    public GameObject textPrefab;
    public List<List<GameObject>> listOfChatroomLists = new List<List<GameObject>>();
    public List<GameObject> mainChatDisplayContentList = new List<GameObject>();

    public NetworkManagerAT networkManagerAT;
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



        //CmdServerAddNetworkPlayerAndShit();
        StartCoroutine(ShortDelay());

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
    private void CmdChatbotRoomsIndex() {
        for (int i = 1; i < inputFields.Count; i++) chatbotRoomsIndex.Add(i, false);       
    }
    [Command]
    private void CmdSetup(int indexOfPlayer)
    {
        for (int x = 0; networkManagerAT.GamePlayers[indexOfPlayer].nrOfChatrooms > x; x++)
        {
            listOfChatroomLists.Add(new List<GameObject>());

        }
        Debug.Log("Nr of Lists in listOfChatroomLists =" + listOfChatroomLists.Count);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void OnStartClient() {
        base.OnStartClient();
        myFakeName = networkPlayer.GetDisplayName();
        networkPlayer = GetComponent<NetworkGamePlayerAT>();
        for (int x = 0; 4 > x; x++)
        {
            listOfChatroomLists.Add(new List<GameObject>());
            Debug.Log(listOfChatroomLists.Count);

        }
    }

    [Command]
    private void CmdServerAddNetworkPlayerAndShit()
    {
        networkPlayer = GetComponent<NetworkGamePlayerAT>();
        for (int x = 0; 4 > x; x++)
        {
            listOfChatroomLists.Add(new List<GameObject>());
            Debug.Log(listOfChatroomLists.Count);

        }
        RpcClientsDoTheFuckingSameAddNetworkPlayerAndShit();
    }

    private IEnumerator ShortDelay()
    {
        yield return new WaitForSeconds(5);
        CmdServerAddNetworkPlayerAndShit();
    }



    [ClientRpc]
    private void  RpcClientsDoTheFuckingSameAddNetworkPlayerAndShit()
    {
        networkPlayer = GetComponent<NetworkGamePlayerAT>();

        if(listOfChatroomLists.Count <4)
        { 
        for (int x = 0; 4 > x; x++)
        {
            listOfChatroomLists.Add(new List<GameObject>());
            Debug.Log(listOfChatroomLists.Count);

        }
        }
    }


    [ClientCallback]
    private void OnDestroy() {
        if (!hasAuthority) return;

        OnMessage -= HandleNewMessage;
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    private void HandleNewMessage(string message, string name, int chatroomID) {         
     
        //Create and add new Message
        GameObject newMessage = Instantiate(textPrefab, chatDisplayContents[chatroomID].GetComponent<ChatDisplayContent>().scrollPanelContent.transform);
        newMessage.GetComponent<Text>().text = name + "" + message;

        //Set Left or right Bound
        if(networkPlayer.chatroomStates[chatroomID].rightName == name)
        {
            newMessage.GetComponent<Text>().alignment = TextAnchor.LowerRight;
        }
        if (networkPlayer.chatroomStates[chatroomID].leftName == name)
        {
            newMessage.GetComponent<Text>().alignment = TextAnchor.LowerLeft;
        }

        listOfChatroomLists[chatroomID].Add(newMessage);


        if (chatroomID == networkPlayer.chatroomID)
        {
            GameObject newMessage1 = Instantiate(textPrefab, mainChatDisplay.GetComponent<ChatDisplayContent>().scrollPanelContent.transform);
            newMessage1.GetComponent<Text>().text = name + "" + message;

            //Set Left or right Bound
            if (networkPlayer.chatroomStates[chatroomID].rightName == name)
            {
                newMessage1.GetComponent<Text>().alignment = TextAnchor.LowerRight;
            }
            if (networkPlayer.chatroomStates[chatroomID].leftName == name)
            {
                newMessage1.GetComponent<Text>().alignment = TextAnchor.LowerLeft;
            }

            mainChatDisplayContentList.Add(newMessage1);
        }

        //Here we can Update Main Canvas if we are in there
    }
    [Client]
    public void Send(string message) {
        if (string.IsNullOrWhiteSpace(message)) return;


        CmdSendMessage(message, chatroomID, networkPlayer.GetDisplayName());


        if (chatbotRoomsIndex[chatroomID]) CmdSendMessageToChatbot(message);
        inputFields[chatroomID].text = string.Empty;
    }
    [Command]
    private void CmdSendMessage(string message, int chatroomID, string name) {
        //Add new Messege on Sever Entity
        GameObject newMessage = Instantiate(textPrefab, chatDisplayContents[chatroomID].GetComponent<ChatDisplayContent>().scrollPanelContent.transform);
        newMessage.GetComponent<Text>().text = name + "" + message;

      

        listOfChatroomLists[chatroomID].Add(newMessage);


        RpcHandleMessage(message, name, chatroomID);
    }
    [ClientRpc]
    private void RpcHandleMessage(string message, string name,  int chatroomID) {
        OnMessage?.Invoke($"\n{message}", name, chatroomID);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [Client]
    public void SelectChatroom(string s) {
        chatroomID = int.Parse(EventSystem.current.currentSelectedGameObject.name);
    }


    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    [Command]
    public void CmdSendMessageToChatbot(string text) {
        networkPlayer = GetComponent<NetworkGamePlayerAT>();
       
        networkPlayer.Room.chatbot.SendTextToChatbot(text, chatroomID);      
    }

    [Command]
    public void CmdSendOutResponseFromChatbot(string r, int id) {
        RpcHandleMessage(r,"nameOFBot", id);
    }

    public void ReceiveChatbotMessageFromPlayer(string r, int id) {
        r = $"[{botname}]: {r}";
        if (isClientOnly) {
            CmdSendOutResponseFromChatbot(r, id);
            return;
        }
        RpcHandleMessage(r, "nameOFBot", id);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void UpdateUI(int id, bool leftFree, bool rightFree, string leftName, string rightName) {
        check = true;
        // manzgöggeli, name uswertig
        ChatDisplayContent cdc = chatDisplayContents[id].GetComponent<ChatDisplayContent>();
        cdc.leftPerson.gameObject.SetActive(!leftFree);
        cdc.rightPerson.gameObject.SetActive(!rightFree);
        cdc.leftName.text = leftName;
        cdc.rightName.text = rightName;

        // join button uswertig
        foreach (GameObject x in chatDisplayContents)
        {
            ChatDisplayContent newCdc = x.GetComponent<ChatDisplayContent>();
            newCdc.joinButton.interactable = true;
        }

        foreach (GameObject x in chatDisplayContents) {

            ChatDisplayContent newCdc = x.GetComponent<ChatDisplayContent>();
            if (newCdc.rightName.text == myFakeName || newCdc.leftName.text == myFakeName) {
                foreach (GameObject y in chatDisplayContents) {
                    ChatDisplayContent newCdc2 = y.GetComponent<ChatDisplayContent>();
                    newCdc2.joinButton.interactable = false;
                }
               // FillUmMainCanvas(id);
            }
        }

        foreach (GameObject x in chatDisplayContents)
        {
            ChatDisplayContent newCdc = x.GetComponent<ChatDisplayContent>();
            if (newCdc.rightName.text != "" && newCdc.leftName.text != "") {
                newCdc.joinButton.interactable = false;
                
            }
        }

       
           
        
        
        // later: update links chatfenster yay
        if(id == networkPlayer.chatroomID)
        {

        }
    }

    [ClientRpc]
    public void RpcFillUpMainCanvasTextAndUI(int id, bool leftFree, bool rightFree, string leftName, string rightName)
    {
        ChatDisplayContent cdc = mainChatDisplay.GetComponent<ChatDisplayContent>();
       
        cdc.leftPerson.gameObject.SetActive(!leftFree);
        cdc.rightPerson.gameObject.SetActive(!rightFree);
        cdc.leftName.text = networkPlayer.chatroomStates[id].leftName;
        cdc.rightName.text = networkPlayer.chatroomStates[id].rightName;      
        for (int x = 0; listOfChatroomLists[id].Count > x; x++)
        {
                GameObject newMessage = Instantiate(textPrefab, mainChatDisplay.GetComponent<ChatDisplayContent>().scrollPanelContent.transform);          
                newMessage.GetComponent<Text>().text = listOfChatroomLists[id][x].GetComponent<Text>().text;
                newMessage.GetComponent<Text>().alignment = listOfChatroomLists[id][x].GetComponent<Text>().alignment;    
                mainChatDisplayContentList.Add(newMessage);
        }
        networkPlayer.chatroomID = id;
    }

    [ClientRpc]
    public void RpcFillUpMainCanvasOnlyUI(int id, bool leftFree, bool rightFree, string leftName, string rightName)
    {
        ChatDisplayContent cdc = mainChatDisplay.GetComponent<ChatDisplayContent>();

        cdc.leftPerson.gameObject.SetActive(!leftFree);
        cdc.rightPerson.gameObject.SetActive(!rightFree);
        cdc.leftName.text = networkPlayer.chatroomStates[id].leftName;
        cdc.rightName.text = networkPlayer.chatroomStates[id].rightName;
        
        networkPlayer.chatroomID = id;
    }




    public void  ClearMainCanvas()
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
}
