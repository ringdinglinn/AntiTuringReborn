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
    private Dictionary<int, bool> chatbotRoomsIndex = new Dictionary<int, bool>();

    public bool check = false;

    //botname placeholder
    string botname = "???";

    public string myFakeName = "";

    public override void OnStartAuthority() {
        chatbotRoomsIndex[0] = true;
        chatUI.SetActive(true);
        OnMessage += HandleNewMessage;
        networkPlayer = GetComponent<NetworkGamePlayerAT>();
        chatbotRoomsIndex[0] = true;
    }

    [Command]
    private void CmdPopulateChatDisplayContentsList() {
        RpcPopulateChatDisplayContentsList();
    }

    [ClientRpc]
    private void RpcPopulateChatDisplayContentsList() {
        for (int i = 1; i < inputFields.Count; i++) chatbotRoomsIndex.Add(i, false);
        foreach (TMP_Text display in chatDisplays) {
            chatDisplayContents.Add(display.GetComponent<ChatDisplayContent>());
        }
    }

    public override void OnStartClient() {
        base.OnStartClient();
        myFakeName = networkPlayer.GetDisplayName();
    }

    public override void OnStartLocalPlayer() {
        base.OnStartLocalPlayer();
        StartCoroutine(WaitForStuff());
    }

    IEnumerator WaitForStuff() {
        yield return new WaitForEndOfFrame();
        CmdPopulateChatDisplayContentsList();
    }

    [ClientCallback]
    private void OnDestroy() {
        if (!hasAuthority) return;

        OnMessage -= HandleNewMessage;
    }

    private void HandleNewMessage(string message, int i) {
        Debug.Log("handle new message event");
        Debug.Log(chatDisplays[i].text.Replace("\n", "  "));
        chatDisplays[i].text += message;
        Debug.Log(chatDisplays[i].text.Replace("\n", "  "));
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

    [Client]
    public void SelectChatroom(string s) {
        chatroomID = int.Parse(EventSystem.current.currentSelectedGameObject.name);
    }

    [Command]
    public void CmdSendMessageToChatbot(string text) {
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

    public void UpdateUI(int id, bool leftFree, bool rightFree, string leftName, string rightName) {
        check = true;
        // manzgöggeli, name uswertig
        ChatDisplayContent cdc = chatDisplays[id].GetComponent<ChatDisplayContent>();
        cdc.leftPerson.gameObject.SetActive(!leftFree);
        cdc.rightPerson.gameObject.SetActive(!rightFree);
        cdc.leftName.text = leftName;
        cdc.rightName.text = rightName;
        Debug.Log(cdc.leftPerson.gameObject.activeInHierarchy + ", " + cdc.rightPerson.gameObject.activeInHierarchy + ", " + cdc.leftName.text + " , " + cdc.rightName.text);

        // join button uswertig


        //foreach (ChatDisplayContent newCdc in chatDisplayContents) {
        //    newCdc.joinButton.interactable = true;
        //    if (!leftFree && !rightFree) newCdc.joinButton.interactable = false;
        //}

        //// angezeigt wenn spieler nicht drin
        //if (!(leftName == myFakeName || rightName == myFakeName) && id == chatroomID) {
        //    cdc.leaveButton.gameObject.SetActive(false);

        //}

        //// angezeigt wenn voll, nicht angezeigt wenn nicht voll
        ////cdc.joinButton.interactable = !(leftFree && rightFree);


        //// nicht angezeigt wenn spieler drin
        //if (leftName == myFakeName || rightName == myFakeName) {
        //    cdc.leaveButton.gameObject.SetActive(true);
        //    foreach (ChatDisplayContent newCdc in chatDisplayContents) {
        //        newCdc.joinButton.interactable = false;
        //    }
        //}

        foreach (ChatDisplayContent newCdc in chatDisplayContents) {
            newCdc.joinButton.interactable = true;
            newCdc.leaveButton.gameObject.SetActive(false);
        }

        foreach (ChatDisplayContent newCdc in chatDisplayContents) {
            if (newCdc.rightName.text == myFakeName || newCdc.leftName.text == myFakeName) {
                newCdc.leaveButton.gameObject.SetActive(true);
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
