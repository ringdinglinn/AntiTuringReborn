using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class NetworkGamePlayerAT : NetworkBehaviour {
    [SyncVar]
    private string displayName = "Loading...";

    public TagManagement tagManagement;
    public GameManagerAT gameManagerAT;

    public string fakeName;
    public string realName;
    public string newName;
    
    public Color32 color;

    public NetworkManagerAT room;
    public ChatBehaviour chatBehaviour;

    public bool isInvestigator = false;

    public GameObject investigatorText;

    public List<GameObject> joinButtonsList = new List<GameObject>();
    public List<GameObject> leaveButtonsList = new List<GameObject>();

    public MoveView moveView;
    public GameObject inputField;
    public List<ChatroomStates> chatroomStates = new List<ChatroomStates>();

    public GameObject chatroomStatePrefab;
    public int chatroomID = 99;

    public int nrOfChatrooms = 4;

    public int playerID;

    public bool left;

    public bool playerIsDead = false;

    public string pandoraBotsClientName;

    public GameObject joinChatroomPrompt;   

   
    public int playerVisualPalletID;


    public bool playerIsTyping = false;
    private float tippingCounter = 3f;
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
    IEnumerator ShortDelayChatroomStates()
    {
        yield return new WaitForSeconds(1);
        InitializeChatroomStates();
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
        CmdUpdateRoleOnServer(isInvestigator);
      

        CmdGamePlayerConnected();
       

        CmdAddToGamePlayers();
      
        GetNameAndColor(Room.GamePlayers.Count - 1);
        realName = displayName;

        StartCoroutine(ShortDelayChatroomStates());
        if (hasAuthority)
        {
            StartCoroutine(CheckIfPlayerIsTyping());
          
        }
      
        pandoraBotsClientName = Room.chatbot.clientNameList[playerID];
    }

   
    private void GetNameAndColor(int index)
    {
        fakeName = Room.randomNames[index];
        playerVisualPalletID = Room.randomPalletsInt[index];
        StartCoroutine(ShortDelayVisual());
    }



    public override void OnStartServer() {
        base.OnStartServer();
    }

    [Command]
    private void CmdAddToGamePlayers()
    {
        Room.GamePlayers.Add(this);
    }
    [Command]
    private void CmdUpdateRoleOnServer(bool status)
    {
        isInvestigator = status;
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
    public void ReceiveMessageFromChatbot(string r, int id, string chatbotName, int BotVisualId) {
        chatBehaviour.ReceiveChatbotMessageFromPlayer(r, id, chatbotName, BotVisualId);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------   
    //Start a Player Joins a Chatroom logic 
    public void CheckRequestFromJoinButton() {

        for (int x = 0; joinButtonsList.Count > x; x++) {
            if (EventSystem.current.currentSelectedGameObject == joinButtonsList[x]) {
                chatroomID = x;
                chatBehaviour.chatroomID = x;
             
             //   Debug.Log("Stage 1:" + "LeftVisualID:" + chatroomStates[chatroomID].leftVisualID + "rightVisualID:" + chatroomStates[chatroomID].rightVisualID);
                CmdRequestJoinRoom(chatroomID, fakeName, playerVisualPalletID);
                return;
            }
        }
    }


    [Command]
    public void CmdRequestJoinRoom(int roomID, string fakeName, int playerVisualPalletID)
    {
        RequestJoinRoom(roomID, fakeName, false, playerVisualPalletID);
       // Debug.Log("Stage 2:" + "LeftVisualID:" + chatroomStates[chatroomID].leftVisualID + "rightVisualID:" + chatroomStates[chatroomID].rightVisualID);
    }

    [Server]
    public void RequestJoinRoom(int roomID, string fakeName, bool isChatbot, int playerVisualPalletID)
    {
     //  Debug.Log("Stage 3:" + "LeftVisualID:" + chatroomStates[roomID].leftVisualID + "rightVisualID:" + chatroomStates[roomID].rightVisualID);
        if (isInvestigator && !isChatbot)
        {
            //Coomunication Between Investigators and other Players
            chatroomID = roomID;
            if (chatroomID == roomID)
            {
                RpcOpenChatroom(roomID, isInvestigator, playerIsDead);
                GetComponent<ChatBehaviour>().RpcClearMainCanvas(roomID, chatroomStates[roomID].leftFree, chatroomStates[roomID].rightFree, chatroomStates[roomID].leftName, chatroomStates[roomID].rightName);
                GetComponent<ChatBehaviour>().RpcFillUpMainCanvasTextAndUI(roomID, chatroomStates[roomID].leftFree, chatroomStates[roomID].rightFree, chatroomStates[roomID].leftName, chatroomStates[roomID].rightName, chatroomStates[roomID].leftVisualID, chatroomStates[roomID].rightVisualID);
                RpcDisableInvestigatorButton(roomID, false);
            }
            foreach (NetworkGamePlayerAT player in Room.GamePlayers)
            {
                player.chatroomStates[roomID].numberOfInvestigators++;
                player.RpcCommunicationBetweenInvestigatorAndOtherPlayers(roomID, player.chatroomStates[roomID].numberOfInvestigators);

                if (player.isInvestigator == true)
                {
                    player.RpcCommunicationBetweenInvestigatorAndInvestigator(roomID, player.chatroomStates[roomID].numberOfInvestigators);
                }
            }
        }

        else if (playerIsDead == true && !isChatbot)
        {
            chatroomID = roomID;

            if (chatroomID == roomID)
            {
                RpcOpenChatroom(roomID, isInvestigator, playerIsDead);
                GetComponent<ChatBehaviour>().RpcClearMainCanvas(roomID, chatroomStates[roomID].leftFree, chatroomStates[roomID].rightFree, chatroomStates[roomID].leftName, chatroomStates[roomID].rightName);
                GetComponent<ChatBehaviour>().RpcFillUpMainCanvasTextAndUI(roomID, chatroomStates[roomID].leftFree, chatroomStates[roomID].rightFree, chatroomStates[roomID].leftName, chatroomStates[roomID].rightName, chatroomStates[roomID].leftVisualID, chatroomStates[roomID].rightVisualID);
                RpcDeadPlayerToggleButtons(false);

            }
        }
        #region
        else if (chatroomStates[roomID].leftFree || chatroomStates[roomID].rightFree)
        {
            if (!isChatbot)
            {
                RpcOpenChatroom(roomID, isInvestigator, playerIsDead);
                chatroomID = roomID;
            }

            left = chatroomStates[roomID].leftFree;
            foreach (NetworkGamePlayerAT player in Room.GamePlayers)
            {
                if (player.chatroomStates[roomID].leftFree || player.chatroomStates[roomID].rightFree)
                {
                    if (player.chatroomStates[roomID].leftFree)
                    {
                        chatroomStates[roomID].leftVisualID = playerVisualPalletID;
                        player.chatroomStates[roomID].leftFree = false;
                        player.chatroomStates[roomID].leftName = fakeName;
                        player.RpcUpdateChatroomStates(roomID, false, player.chatroomStates[roomID].rightFree, fakeName, player.chatroomStates[roomID].rightName, left, playerVisualPalletID, player.chatroomStates[roomID].rightVisualID);

                        if (player.chatroomID == roomID)
                        {
                            player.GetComponent<ChatBehaviour>().RpcFillUpMainCanvasOnlyUI(roomID, false, player.chatroomStates[roomID].rightFree, fakeName, player.chatroomStates[roomID].rightName,  playerVisualPalletID, player.chatroomStates[roomID].rightVisualID);
                        }
                    }
                    else if (player.chatroomStates[roomID].rightFree)
                    {
                        chatroomStates[roomID].rightVisualID = playerVisualPalletID;
                        player.chatroomStates[roomID].rightFree = false;
                        player.chatroomStates[roomID].rightName = fakeName;
                        player.RpcUpdateChatroomStates(roomID, player.chatroomStates[roomID].leftFree, false, player.chatroomStates[roomID].leftName, fakeName, left, player.chatroomStates[roomID].leftVisualID, playerVisualPalletID);

                        if (player.chatroomID == roomID)
                        {
                            player.GetComponent<ChatBehaviour>().RpcFillUpMainCanvasOnlyUI(roomID, player.chatroomStates[roomID].leftFree, false, player.chatroomStates[roomID].leftName, fakeName, player.chatroomStates[roomID].leftVisualID, playerVisualPalletID);
                        }
                    }
                }
                else
                {
                    //room is full message
                    Debug.Log("this should not happen");
                }
            }
            if (!isChatbot) GetComponent<ChatBehaviour>().RpcFillUpMainCanvasTextAndUI(roomID, chatroomStates[roomID].leftFree, chatroomStates[roomID].rightFree, chatroomStates[roomID].leftName, chatroomStates[roomID].rightName, chatroomStates[roomID].leftVisualID, chatroomStates[roomID].rightVisualID);
        }

    }

    [ClientRpc]
    private void RpcOpenChatroom(int chatroomID, bool playerWhoCallsIsInvestigator, bool playerWhoCallsIsDead ) {
        joinChatroomPrompt.SetActive(false);
        if (!isInvestigator && isLocalPlayer && playerIsDead == false) StartCoroutine(ToggleInputField(true));
        moveView.MoveViewRight();

        if (playerWhoCallsIsDead == false)
        {
            if (playerWhoCallsIsInvestigator == false)
            {
                if (!isLocalPlayer)
                {
                    foreach (NetworkGamePlayerAT player in Room.GamePlayers)
                    {
                        if (playerIsDead == false || player.isInvestigator == false)
                        {

                            if (player.isLocalPlayer)
                            {
                                if (player.chatroomID == chatroomID)
                                {
                                    Room.otherPersonJoinsRoom.Play();
                                }
                                else
                                {
                                    Room.othersJoinRooms.Play();
                                }
                            }
                        }
                    }
                }
            }
        }
        this.chatroomID = chatroomID;
        chatBehaviour.chatDisplayContents[chatroomID].GetComponent<ChatDisplayContent>().joinButton.GetComponent<Image>().sprite = chatBehaviour.chatDisplayContents[chatroomID].GetComponent<ChatDisplayContent>().selectedChatroomSprite;
    }

    IEnumerator ToggleInputField(bool state) {
        inputField.SetActive(!state);
        yield return new WaitForSeconds(0.8f);
        inputField.SetActive(state);
        if (state) {
            Room.activateInputFieldSound.Play();
        }
    }

    #endregion
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------   
    //Start a Player Leaves a Chatroom logic 
    public void CheckRequestFromLeaveButton() {
        CmdUpdateYourTypingVisualInYouChatroom(chatroomID, fakeName, false, false);
        CmdLeaveChatroom(chatroomID, fakeName);
        chatBehaviour.mainInputField.text = "";
        
    }

    [Command]
    public void CmdLeaveChatroom(int ID, string fakeName)
    {
        LeaveChatroom(ID, fakeName, false);
    }

    [Server]
    public void LeaveChatroom(int ID, string fakeName, bool isChatbot) {
        if (ID != 99) // When ID = 99 it woul mean that the player is in no Chatroom when he died.
        {
            if (!isChatbot) {
                RpcCloseChatroom(ID, isInvestigator, playerIsDead);
                
            }
        }

        if (isInvestigator && !isChatbot) {
            chatroomID = 99;
            RpcDisableInvestigatorButton(ID, true);
            GetComponent<ChatBehaviour>().RpcClearMainCanvas(ID, chatroomStates[ID].leftFree, chatroomStates[ID].rightFree, chatroomStates[ID].leftName, chatroomStates[ID].rightName);

            foreach (NetworkGamePlayerAT player in Room.GamePlayers) {
                player.chatroomStates[ID].numberOfInvestigators--;
                player.RpcCommunicationBetweenInvestigatorAndOtherPlayers(ID, player.chatroomStates[ID].numberOfInvestigators);

                if (player.isInvestigator == true) {
                    player.RpcCommunicationBetweenInvestigatorAndInvestigator(ID, player.chatroomStates[ID].numberOfInvestigators);
                }
            }
        }
        else if (playerIsDead && !isChatbot) {
            RpcDeadPlayerToggleButtons(true);
            GetComponent<ChatBehaviour>().RpcLeaveMainCanvas(ID, chatroomStates[ID].leftFree, chatroomStates[ID].rightFree, chatroomStates[ID].leftName, chatroomStates[ID].rightName);
        } else if (isChatbot) {
            bool chatbotIsLeft = true;
            if (Room.chatbot.chatroomBotIndex[ID][0] == -1) {
                chatbotIsLeft = false;
            } else if (Room.chatbot.chatbotAIs[Room.chatbot.chatroomBotIndex[ID][0]].fakeName != fakeName) {
                chatbotIsLeft = false;
            }
            if (chatbotIsLeft) {
                foreach (NetworkGamePlayerAT player in room.GamePlayers) {
                    player.UpdateChatroomStatesEvent(ID, true, chatroomStates[ID].rightFree, "", chatroomStates[ID].rightName, Room.chatbot.chatbotAIs[Room.chatbot.chatroomBotIndex[ID][0]].left, 99, player.chatroomStates[ID].rightVisualID);
                    player.RpcUpdateChatroomStates(ID, true, player.chatroomStates[ID].rightFree, "", player.chatroomStates[ID].rightName, Room.chatbot.chatbotAIs[Room.chatbot.chatroomBotIndex[ID][0]].left, 99, player.chatroomStates[ID].rightVisualID);
                    if (player.chatroomID == ID) {
                        player.GetComponent<ChatBehaviour>().RpcLeaveMainCanvas(ID, true, player.chatroomStates[ID].rightFree, "", player.chatroomStates[ID].rightName);
                    }
                }
            } else {
                foreach (NetworkGamePlayerAT player in room.GamePlayers) {
                    player.UpdateChatroomStatesEvent(ID, chatroomStates[ID].leftFree, true, chatroomStates[ID].leftName, "", Room.chatbot.chatbotAIs[Room.chatbot.chatroomBotIndex[ID][1]].left, player.chatroomStates[ID].leftVisualID, 99);
                    player.RpcUpdateChatroomStates(ID, player.chatroomStates[ID].leftFree, true, player.chatroomStates[ID].leftName, "", Room.chatbot.chatbotAIs[Room.chatbot.chatroomBotIndex[ID][1]].left, player.chatroomStates[ID].leftVisualID, 99);
                    if (player.chatroomID == ID) {
                        player.GetComponent<ChatBehaviour>().RpcLeaveMainCanvas(ID, player.chatroomStates[ID].leftFree, true, player.chatroomStates[ID].leftName, "");
                    }
                }
            }
        }
        else if (ID != 99 && chatroomStates[ID].leftName == fakeName) {
            foreach (NetworkGamePlayerAT player in room.GamePlayers) {
                player.UpdateChatroomStatesEvent(ID, true, chatroomStates[ID].rightFree, "", chatroomStates[ID].rightName, player.left, 99, player.chatroomStates[ID].rightVisualID);
                player.RpcUpdateChatroomStates(ID, true, player.chatroomStates[ID].rightFree, "", player.chatroomStates[ID].rightName, player.left, 99, player.chatroomStates[ID].rightVisualID);
                if (player.chatroomID == ID) {
                    player.GetComponent<ChatBehaviour>().RpcLeaveMainCanvas(ID, true, player.chatroomStates[ID].rightFree, "", player.chatroomStates[ID].rightName);
                }
            }
            chatroomID = 99;
            GetComponent<ChatBehaviour>().RpcClearMainCanvas(ID, true, chatroomStates[ID].rightFree, "", chatroomStates[ID].rightName);
        }
        else if (ID != 99 && chatroomStates[ID].rightName == fakeName) {
            foreach (NetworkGamePlayerAT player in room.GamePlayers) {
                player.UpdateChatroomStatesEvent(ID, chatroomStates[ID].leftFree, true, chatroomStates[ID].leftName, "", player.left, player.chatroomStates[ID].leftVisualID, 99);
                player.RpcUpdateChatroomStates(ID, player.chatroomStates[ID].leftFree, true, player.chatroomStates[ID].leftName, "", player.left, player.chatroomStates[ID].leftVisualID, 99);
                if (player.chatroomID == ID) {
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
    private void RpcCloseChatroom(int roomID, bool playerWhoCallsIsInvestigator, bool playerWhoCallsIsDead)
    {
        moveView.MoveViewLeft();
        if (playerWhoCallsIsDead == false)
        {
            if (playerWhoCallsIsInvestigator == false)
            {
                if (!isInvestigator && isLocalPlayer && !playerIsDead) StartCoroutine(ToggleInputField(false));
                if (!isLocalPlayer)
                {
                    foreach (NetworkGamePlayerAT player in Room.GamePlayers)
                    {
                        if (player.isLocalPlayer)
                        {
                            if (player.chatroomID == roomID)
                            {
                                Room.otherPersonLeavesRoom.Play();
                            }
                            else
                            {
                                Room.othersLeaveRooms.Play();
                            }
                        }
                    }
                }
            }
        }
        chatBehaviour.chatDisplayContents[roomID].GetComponent<ChatDisplayContent>().joinButton.GetComponent<Image>().sprite = chatBehaviour.chatDisplayContents[roomID].GetComponent<ChatDisplayContent>().baseChatroomSprite;
        this.chatroomID = 99;
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------   
    [ClientRpc]
    private void RpcUpdateChatroomStates(int id, bool leftFree, bool rightFree, string leftName, string rightName, bool left , int leftVisualID, int rightVisualID)
    {
        CmdReUpdateToServer(id, leftFree, rightFree, leftName, rightName, left, leftVisualID, rightVisualID);
        UpdateChatroomStatesEvent(id, leftFree, rightFree, leftName, rightName, left, leftVisualID, rightVisualID);
        this.left = left;
    }

    [Command]
   private void CmdReUpdateToServer(int id, bool leftFree, bool rightFree, string leftName, string rightName, bool left, int leftVisualID, int rightVisualID)
    {
        chatroomStates[id].leftFree = leftFree;
        chatroomStates[id].rightFree = rightFree;
        chatroomStates[id].leftName = leftName;
        chatroomStates[id].rightName = rightName;
        chatroomStates[id].leftVisualID = leftVisualID;
        chatroomStates[id].rightVisualID = rightVisualID;
    } 
   private void UpdateChatroomStatesEvent(int id, bool leftFree, bool rightFree, string leftName, string rightName, bool left, int leftVisualID, int rightVisualID)
    {
        chatroomStates[id].leftFree = leftFree;
        chatroomStates[id].rightFree = rightFree;
        chatroomStates[id].leftName = leftName;
        chatroomStates[id].rightName = rightName;
        chatroomStates[id].leftVisualID = leftVisualID;
        chatroomStates[id].rightVisualID = rightVisualID;
        this.left = left;
        GetComponent<ChatBehaviour>().UpdateUI(id, leftFree, rightFree, leftName, rightName,  leftVisualID,  rightVisualID);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Random name and color
    [Command]
    private void CmdGamePlayerConnected() {
        Room.GamePlayerConnected();
    }



   
    IEnumerator ShortDelayVisual()
    {
        yield return new WaitForSeconds(3);
        CmdSyncPlayerVisualPalletID(playerVisualPalletID);
    }
    [Command]
    private void CmdSyncPlayerVisualPalletID(int newVisualID)
    {
        playerVisualPalletID = newVisualID;
        RpcSyncPlayerVisualPalletID(newVisualID);
    }
    [ClientRpc]
    private void RpcSyncPlayerVisualPalletID(int newVisualID)
    {
        playerVisualPalletID = newVisualID;
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------Investigator is watching
    [ClientRpc]
    private void RpcCommunicationBetweenInvestigatorAndOtherPlayers(int id, int newTotalNumberOfInvestigators)
    {
        CommunicationBetweenInvestigatorAndOtherPlayers(id, newTotalNumberOfInvestigators);
    }
    private void CommunicationBetweenInvestigatorAndOtherPlayers(int id, int newTotalNumberOfInvestigators)
    {
        chatroomStates[id].numberOfInvestigators = newTotalNumberOfInvestigators;
        GetComponent<ChatBehaviour>().UpdateMainChatPanelInvestigatorVisual(id, chatroomStates[id].numberOfInvestigators);
    }

    [ClientRpc]
    private void RpcCommunicationBetweenInvestigatorAndInvestigator(int id, int newTotalNumberOfInvestigators)
    {
        CommunicationBetweenInvestigatorAndInvestigator(id, newTotalNumberOfInvestigators);
    }
    private void CommunicationBetweenInvestigatorAndInvestigator(int id, int newTotalNumberOfInvestigators)
    {
        chatroomStates[id].numberOfInvestigators = newTotalNumberOfInvestigators;
        GetComponent<ChatBehaviour>().UpdateSmallPanelsInvestigatorVisualBetweenInvestigators(id, chatroomStates[id].numberOfInvestigators);
    }

    [ClientRpc]
    private void RpcDisableInvestigatorButton(int id, bool status)
    {
        DisableInvestigatorButton(id, status);
    }
    private void DisableInvestigatorButton(int id, bool status)
    {
        GetComponent<ChatBehaviour>().ToggleInvestigatorButton(id, status);
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------when Player dies

    [Command]
    public void CmdPlayerDied()
    {
        RpcPlayerDied();
    }
    [ClientRpc]
    public void RpcPlayerDied()
    {
        PlayerDied();
    }

    public void PlayerDied()
    {
        playerIsDead = true;
    }


    [ClientRpc]
    private void RpcDeadPlayerToggleButtons(bool status)
    {
        DeadPlayerToggleButtons(status);
    }
    private void DeadPlayerToggleButtons( bool status)
    {
         GetComponent<ChatBehaviour>().ToggleDeadPlayerButtons(status);
    }


    #region Set IsDead On Client And Server
    public void SetIsDead(bool status)
    {
        playerIsDead = status;
        if(hasAuthority)
        {
            CheckRequestFromLeaveButton(); //Make the player Leave Room if the player is in a Room while getting killed
            DeadPlayerToggleButtons(true);
            CmdSetIsDead(playerIsDead);          
        }
    }
    [Command]
    public void CmdSetIsDead(bool status)
    {
        playerIsDead = status;
    }
    #endregion


    #region Typing Visual Sync
   private IEnumerator CheckIfPlayerIsTyping()
   {
        Debug.Log("CheckIfPlayerIsTyping Running");
        yield return new WaitForSeconds(0.1f);
        playerIsTyping = room.keySoundEffectHandler.isTyping;

        if (playerIsTyping == true)
        {
            tippingCounter = 2f;
            CheckIfPlayerIsCurrentlyInAChatroom();
         
        }
        else
        {
            tippingCounter -= Random.Range(0.1f,0.2f);
            if(tippingCounter < 0)
            {
                CheckIfPlayerIsCurrentlyInAChatroom();
            }
         
        }

          StartCoroutine(CheckIfPlayerIsTyping());
        
   }   
   public void CheckIfPlayerIsCurrentlyInAChatroom()
    {
        if(chatroomID != 99)
        {        
           CmdUpdateYourTypingVisualInYouChatroom(chatroomID, fakeName,  playerIsTyping, false);          
        }
        else
        {
            
            //Player Is Not in A Chatroom
        }
    }
    [Command]
    public void CmdUpdateYourTypingVisualInYouChatroom(int chatroomID, string fakeName, bool isTypingStatus, bool isBotMessage)
    {
        RpcUpdateYourTypingVisualInYouChatroom(chatroomID, fakeName, isTypingStatus, isBotMessage);
    }
    [ClientRpc]
    public void RpcUpdateYourTypingVisualInYouChatroom(int chatroomID, string fakeName, bool isTypingStatus, bool isBotMessage)
    {
        if (isBotMessage == false)
        {
            this.playerIsTyping = isTypingStatus;

            chatBehaviour.UpdateTypingVisualOfAPlayer(chatroomID, fakeName, isTypingStatus);
        }
    }
    #endregion
}