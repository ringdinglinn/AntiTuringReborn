using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.Events;
using System;
using UnityEngine.EventSystems;

public class ChatBehaviour : NetworkBehaviour {
    public List<TMP_Text> chatDisplays;
    public List<ChatDisplayContent> chatDisplayContents = new List<ChatDisplayContent>();
    public List<TMP_InputField> inputFields;
    public GameObject chatUI;

    private NetworkGamePlayerAT networkPlayer;

    private static event Action<String, int> OnMessage;

    public int chatroomID;
    public Dictionary<int, bool> chatbotRoomsIndex = new Dictionary<int, bool>();

    public bool check = false;

    //botname placeholder
    string botname = "???";

    public string myFakeName = "";

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void OnStartAuthority() {
        chatbotRoomsIndex[0] = true;
        chatUI.SetActive(true);
        OnMessage += HandleNewMessage;
        networkPlayer = GetComponent<NetworkGamePlayerAT>();
      
        chatbotRoomsIndex[0] = true;
        chatbotRoomsIndex[1] = false;

        chatbotRoomsIndex[2] = false;
        chatbotRoomsIndex[3] = false;

        CmdchatbotRooIndexSetup();
        CmdChatbotRoomsIndex();      
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
        RpcChatbotRoomsIndex();
    }
    [ClientRpc]
    private void RpcChatbotRoomsIndex() {
        for (int i = 1; i < inputFields.Count; i++) chatbotRoomsIndex.Add(i, false);    
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public override void OnStartClient() {
        base.OnStartClient();
        myFakeName = networkPlayer.GetDisplayName();
    }
    [ClientCallback]
    private void OnDestroy() {
        if (!hasAuthority) return;

        OnMessage -= HandleNewMessage;
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    private void HandleNewMessage(string message, int i) {         
        chatDisplays[i].text += message;     
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
        RpcHandleMessage($"[{name}]: {message}", chatroomID);
    }
    [ClientRpc]
    private void RpcHandleMessage(string message, int chatroomID) {
        OnMessage?.Invoke($"\n{message}", chatroomID);
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
        RpcHandleMessage(r, id);
    }

    public void ReceiveChatbotMessageFromPlayer(string r, int id) {
        r = $"[{botname}]: {r}";
        if (isClientOnly) {
            CmdSendOutResponseFromChatbot(r, id);
            return;
        }
        RpcHandleMessage(r, id);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void UpdateUI(int id, bool leftFree, bool rightFree, string leftName, string rightName) {
        check = true;

        // manzgöggeli, name uswertig
        ChatDisplayContent cdc = chatDisplays[id].GetComponent<ChatDisplayContent>();
        cdc.leftPerson.gameObject.SetActive(!leftFree);
        cdc.rightPerson.gameObject.SetActive(!rightFree);
        cdc.leftName.text = leftName;
        cdc.rightName.text = rightName;
        
        // join button uswertig
        foreach (ChatDisplayContent newCdc in chatDisplayContents) {
            newCdc.joinButton.interactable = true;
        }

        foreach (ChatDisplayContent newCdc in chatDisplayContents) {
            if (newCdc.rightName.text == myFakeName || newCdc.leftName.text == myFakeName) {
                foreach (ChatDisplayContent newCdc2 in chatDisplayContents) {
                    newCdc2.joinButton.interactable = false;
                }
            }
        }

        foreach (ChatDisplayContent newCdc in chatDisplayContents) {
            if (newCdc.rightName.text != "" && newCdc.leftName.text != "") {
                newCdc.joinButton.interactable = false;
            }
        }


        // later: update links chatfenster yay
    }
}
