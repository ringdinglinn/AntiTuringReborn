using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using FMODUnity;
using FMOD;
using System;

public class GameManagerAT : NetworkBehaviour
{

    public int gameStage = 0;
    public int invWatching = 0;

    [Header("Component References")]
    public NetworkGamePlayerAT networkGamePlayerAT;
    public GameManagerMessagesHandler messagesHandler;
    public NetworkManagerAT networkManagerAT;
    public TagManagement tagManagement;
    public ConnectionDiagramManager connectionDiagramManager;

    [Header("Provisiorisch")]
    public Sprite testSprite;

    [Header("Core Mechanic")]
    [SerializeField] private int totalHumanBots;
    [SerializeField] private int currentHumanBotsAlive;
    [SerializeField] private int minHumanBotsNeededAliveToWin;
    [SerializeField] private int minNeededConnectionsForAIToWin;

    [SerializeField] private int currentNrOfMadeAIConncetions;
   
    public int maxNrOfAllowedFailedConnectionAttemptsAIPlayers = 3;
    public int currentNrOfAiPlayerFailedConnectionsAttempts = 0;

   public int investigatorsFailedConnections;
    public int investigatorsMaxAllowedFailedConnections = 3;


    [Header("Detemind When Win And Loses States Should be called")]      
    [SerializeField] private List<int> minConnectionsNeededForAIToWinList = new List<int>(); // Here we can put in based on nr Of Bots Playing what the min value of Possible connectionns is -> If current min possible value falls underneath AI Lose  
    [SerializeField] private List<int> minHumanBotsNeededAliveToStillWinList = new List<int>(); // Here we can put in based on nr Of Bots Playing what the min value of Possible connectionns is

    [Header("You Died Window")]
    [SerializeField] private GameObject youDiedWindow;
    [SerializeField] private GameObject youAreDeadLobbyText;

    [Header("Game Over Visuals")]
    [SerializeField] private GameObject investigatorsWonVisual;
    [SerializeField] private GameObject aiWonVisual;

    public List<TagsBetweenPlayersHolder> playerTagsOverviewList = new List<TagsBetweenPlayersHolder>();

    [Header("Player Visual Palletsd")]
    public List<PlayerVisualPallet> playerVisualPalletsList = new List<PlayerVisualPallet>();



    [Header("Opening Screen Opening")]
    public GameObject startScreen1;
    public List<TextMeshProUGUI> textStartScreenList = new List<TextMeshProUGUI>();
    public List<string> startMessages = new List<string>();
    public float timerScreen1 = 0.6f;
    public int startScreenCounter = 0;

    public GameObject startScreen2;
    public Image startScreenImage;
    public Sprite investigatorStartScreenImage;
    public GameObject invStartScreen2;
    public TextMeshProUGUI invRoleReveal;
    public TextMeshProUGUI invRoleDescription;
    public GameObject invSymbol;
    public GameObject aiStartScreen2;
    public TextMeshProUGUI aiRoleReveal;
    public GameObject aiPicHolder;
    public Image aiPic;
    public TextMeshProUGUI aiRoleDescription;
    public TextMeshProUGUI aiFakeNameDes;
    public TextMeshProUGUI aiFakeName;

    #region Start Setup
    public override void OnStartClient()
    {
        networkManagerAT = GameObject.Find("NetworkManager").GetComponent<NetworkManagerAT>();
        StartStartScreen1();
        tagManagement.StartSetup();
        connectionDiagramManager.StartSetup();
        StartSetup();
        base.OnStartClient();
    }
    public void StartSetup()
    {
        StartCoroutine(ShortSetupDelay());
    }
    IEnumerator ShortSetupDelay() 
    {
        yield return new WaitForSeconds(3f);  
        CmdStartSetup();       
    }
    [Command]
    private void CmdStartSetup()
    {
        networkManagerAT = networkGamePlayerAT.room;
        //Debug.Log("Message kommt an 2");
        RpctartSetup();
        //ConnectionsOverviewSetup();
    }
    [ClientRpc]
    private void RpctartSetup()
    {       
        networkManagerAT = networkGamePlayerAT.room;
        ConnectionsOverviewSetup();
        SetupOfWinAndLoseRequirements();
    }
    private void ConnectionsOverviewSetup()
    { //Kreiere eine Liste mit allen Möglichen Connections      
        foreach (NetworkGamePlayerAT gamePlayerAT1 in tagManagement.allTagableHumanPlayersList)
        {
            foreach (NetworkGamePlayerAT gamePlayerAT2 in tagManagement.allTagableHumanPlayersList)
            {
                if (gamePlayerAT1.realName != gamePlayerAT2.realName)
                {
                    bool alreadyHasAHolder = false;
                    foreach (TagsBetweenPlayersHolder alreadyExsitingHolder in playerTagsOverviewList)
                    {
                        if (alreadyExsitingHolder.player1realName == gamePlayerAT1.realName && alreadyExsitingHolder.player2realName == gamePlayerAT2.realName || alreadyExsitingHolder.player1realName == gamePlayerAT2.realName && alreadyExsitingHolder.player2realName == gamePlayerAT1.realName)
                        {
                            alreadyHasAHolder = true;
                        }
                    }

                    if (alreadyHasAHolder == false)
                    {
                        TagsBetweenPlayersHolder x = new TagsBetweenPlayersHolder();
                        x.player1realName = gamePlayerAT1.realName;
                        x.player2realName = gamePlayerAT2.realName;
                        playerTagsOverviewList.Add(x);

                        TagsBetweenPlayersHolder y = new TagsBetweenPlayersHolder();
                        y.player1realName = gamePlayerAT2.realName;
                        y.player2realName = gamePlayerAT1.realName;
                        playerTagsOverviewList.Add(y);
                    }
                }
            }
        }

    }

    private void SetupOfWinAndLoseRequirements()
    {
         //Regarding Bots
         totalHumanBots = tagManagement.allTagableHumanPlayersList.Count;
         currentHumanBotsAlive = totalHumanBots;
         minHumanBotsNeededAliveToWin = minHumanBotsNeededAliveToStillWinList[totalHumanBots];

        //Regarding Connections   
        minNeededConnectionsForAIToWin = minConnectionsNeededForAIToWinList[totalHumanBots];
        currentNrOfMadeAIConncetions = 0;
       
        //Regarding Investigators
        investigatorsFailedConnections = 0;               
    }
    #endregion

    public void ValidateNewTagRequest(string tagedPlayerRealName, string playerWhoTagedRealNamen, bool isInvestigatorTagRequest)
    {    
        if(isInvestigatorTagRequest == true)//Check ob ein Investigator jmd Getagged hat.
        {
            foreach (NetworkGamePlayerAT playerAT in networkManagerAT.GamePlayers)
            {
                if (playerAT.realName == tagedPlayerRealName)//Check ob getagted Person eine  Human Ai ist oder Bot.           
                {
                    CmdMessageInvestigatorsDestroyedHumanPlayer(tagedPlayerRealName);
                    return;
                    //Investigators Found a Human Player!!! Dam dam dam
                }
            }
            foreach (PlayerTagPanelHandler bot in tagManagement.botsTagPanelHandlerList)
            {
                if (bot.GetPlayerRealName() == tagedPlayerRealName)//Check ob getagted Person eine  Human Ai ist oder Bot.           
                {
                    int visualIdOfDeadBot = bot.visualID;
                    CmdInvestigatorTaggedWrong(tagedPlayerRealName, visualIdOfDeadBot, networkGamePlayerAT.playerID);
                    return;
                    //Investigators Found a bot xD
                }
            }
        }
        if (isInvestigatorTagRequest == false)//Check ob eine Human Ai jmd Getagged hat.
        {
            foreach (NetworkGamePlayerAT playerAT in networkManagerAT.GamePlayers)
            {
                if (playerAT.realName == tagedPlayerRealName)//Check ob getagted Person eine andere Human Ai ist oder Bot.           
                {
                    //Update Connections
                    CmdHandleNewConnections(tagedPlayerRealName, playerWhoTagedRealNamen);                 
                }
            }

            foreach (PlayerTagPanelHandler bot in tagManagement.botsTagPanelHandlerList)
            {
                if (bot.GetPlayerRealName() == tagedPlayerRealName)//Check ob getagted Person eine  Human Ai ist oder Bot.           
                { 
                    
                    int visualOfTaggedBotByHumanAI = bot.visualID;
                    CmdHumanAIPlayerTagedWrong(tagedPlayerRealName, playerWhoTagedRealNamen, visualOfTaggedBotByHumanAI,networkGamePlayerAT.playerVisualPalletID);
                    return;
                    //Investigators Found a bot xD
                }
            }
        }
    }

    #region WrongTags Handeling
    [Command]
    public void CmdInvestigatorTaggedWrong(string newDeadBotName, int newVIsualID, int investigatorID)
    {
        RpcInvestigatorTaggedWrong(newDeadBotName, newVIsualID, investigatorID);
    }
    [ClientRpc]
    public void RpcInvestigatorTaggedWrong(string newDeadBotName, int newVIsualID, int investigatorID)
    {

        foreach (NetworkGamePlayerAT player in networkManagerAT.GamePlayers)
        {
            player.gameManagerAT.investigatorsFailedConnections++;
        }
        foreach (NetworkGamePlayerAT player in networkManagerAT.GamePlayers)
        {
            if (investigatorID == player.playerID) {
                player.gameManagerAT.messagesHandler.HandlePlayerDied(player.gameManagerAT.playerVisualPalletsList[newVIsualID].playerDeadBig, newDeadBotName, "You have terminated a bot.\n\nIt was not sentient.", "Investigator's remaining attempts: " + (investigatorsMaxAllowedFailedConnections - investigatorsFailedConnections));
            } else {
                player.gameManagerAT.messagesHandler.HandlePlayerDied(player.gameManagerAT.playerVisualPalletsList[newVIsualID].playerDeadBig, newDeadBotName, "The investigators have terminated a bot.\n\nIt was not sentient.", "Investigator's remaining attempts: " + (investigatorsMaxAllowedFailedConnections - investigatorsFailedConnections));
            }
            player.gameManagerAT.ValidateWinAndLoseState();      
            player.tagManagement.ChangePlayerTagToDead(newDeadBotName);
        
        }
        DestroyBot(newVIsualID);
    }
    public void DestroyBot(int viusalId)
    {
        CmdDestroyBot(viusalId);
    }
    [Command]
    public void CmdDestroyBot(int viusalId)
    {
        foreach(ChatbotAI x in networkGamePlayerAT.room.chatbot.chatbotAIs)
        {
            if(x.playerVisualPalletID == viusalId)
            {
                x.DestroyBot();
                return;
            }
        }
      


    }


    [Command]
    public void CmdHumanAIPlayerTagedWrong(string tagedBotName, string playerWhoTagedRealName, int visualOfTaggedBotByHumanAI, int visualIDofHumanWhoTaged)
    {
        RpcHumanAIPlayerTagedWrong(tagedBotName, playerWhoTagedRealName, visualOfTaggedBotByHumanAI, visualIDofHumanWhoTaged);
    }
    [ClientRpc]
    public void RpcHumanAIPlayerTagedWrong(string tagedBotName, string playerWhoTagedRealName, int visualOfTaggedBotByHumanAI, int visualIDofHumanWhoTaged)
    {
        if (hasAuthority)
        {
            currentNrOfAiPlayerFailedConnectionsAttempts++;
            if (currentNrOfAiPlayerFailedConnectionsAttempts >= maxNrOfAllowedFailedConnectionAttemptsAIPlayers)
            {
                CmdMessagePlayerDiedBecauseOfToManyWrongTags(playerWhoTagedRealName, visualIDofHumanWhoTaged);
                return;
            }
        }
       
            //Inform the Player and Investigators about the failed connection Attempt
            foreach (NetworkGamePlayerAT playerAT in networkManagerAT.GamePlayers)
            {
                if (playerAT.isInvestigator == true)
                {
                    playerAT.gameManagerAT.messagesHandler.HandleFailedHumanPlayerConnectedWithAntoherHumanPlayer("Failed Connection Attempt!", playerWhoTagedRealName, "?", 1, playerWhoTagedRealName + " attempted a connection but failed");
                }

                if (playerAT.realName == playerWhoTagedRealName)
                {
                    playerAT.gameManagerAT.messagesHandler.HandleFailedHumanPlayerConnectedWithAntoherHumanPlayer("Connection failed!", playerWhoTagedRealName, tagedBotName, 1, tagedBotName + " is not sentient. Connection unsuccessful.",  "Remaining attempts before termination: " + (maxNrOfAllowedFailedConnectionAttemptsAIPlayers - currentNrOfAiPlayerFailedConnectionsAttempts-1));
                }
            }

        
        
    }
    #endregion



    #region Handle All Connections

    [Command]
    private void CmdHandleNewConnections(string tagedPlayerRealName, string playerWhoTagedRealNamen)
    {
        RpcHandleNewConnections(tagedPlayerRealName, playerWhoTagedRealNamen);
    }

    [ClientRpc]
    private void RpcHandleNewConnections(string tagedPlayerRealName, string playerWhoTagedRealNamen)
    { 
        foreach (NetworkGamePlayerAT player in networkManagerAT.GamePlayers)
        {
            foreach (TagsBetweenPlayersHolder alreadyExsitingHolder in player.gameManagerAT.playerTagsOverviewList)
            {
                if (alreadyExsitingHolder.connectionMade == false && alreadyExsitingHolder.connectionDead == false)
                {
                    if (alreadyExsitingHolder.player1realName == playerWhoTagedRealNamen && alreadyExsitingHolder.player2realName == tagedPlayerRealName)
                    {
                        alreadyExsitingHolder.connectionMade = true;
                      
                    }                 
                }            
            }
         
        }     
        int nrOfConnections = CalculateNrOfConnectionsBetweenPlayer(tagedPlayerRealName, playerWhoTagedRealNamen);       
        CmdHumanPlayerFoundOtherHumanPlayer(tagedPlayerRealName, playerWhoTagedRealNamen, nrOfConnections);
    }
 
    private int CalculateNrOfConnectionsBetweenPlayer(string tagedPlayerRealName, string playerWhoTagedRealNamen)
    {
        int returnValue = 0;
        foreach (TagsBetweenPlayersHolder holder in playerTagsOverviewList)
        {
            if (holder.connectionMade == true && holder.connectionDead == false)
            {
                if (holder.player1realName == playerWhoTagedRealNamen && holder.player2realName == tagedPlayerRealName)
                {
                    returnValue++;
                }
                if (holder.player1realName == tagedPlayerRealName && holder.player2realName == playerWhoTagedRealNamen)
                {
                    returnValue++;
                }
            }
        }
        return returnValue;
    }
    private void HandleConnactionWhenPlayerDies(string playerWhoDiedRealName)
    {
        foreach (NetworkGamePlayerAT player in networkManagerAT.GamePlayers)
        {
       

        }
    }
    #endregion

    #region Investigator Destroys Human Player
    [Command]
    public void CmdMessageInvestigatorsDestroyedHumanPlayer(string newDeadPlayerRealName)
    {
        RpcMessageInvestigatorsDestroyedHumanPlayer(newDeadPlayerRealName);
    }

    [ClientRpc]
    public void RpcMessageInvestigatorsDestroyedHumanPlayer(string newDeadPlayerRealName)
    {
        foreach (NetworkGamePlayerAT player in networkManagerAT.GamePlayers)
        {
            if (newDeadPlayerRealName != player.realName)
            {
                player.gameManagerAT.messagesHandler.HandlePlayerDied(testSprite, newDeadPlayerRealName, "The investigators have terminated a bot.\nIt was sentient." + " "+ currentHumanBotsAlive + " sentient minds remaining");
            }

            if (newDeadPlayerRealName == player.realName)
            {
                player.gameManagerAT.messagesHandler.HandlePlayerDied(testSprite, newDeadPlayerRealName, "The investigators have temrinated you." + currentHumanBotsAlive + " sentient minds remaining");
              
                player.SetIsDead(true);
            }

            player.gameManagerAT.connectionDiagramManager.HandlePlayerDied(newDeadPlayerRealName);
            player.tagManagement.ChangePlayerTagToDead(newDeadPlayerRealName);

            foreach (TagsBetweenPlayersHolder alreadyExsitingHolder in player.gameManagerAT.playerTagsOverviewList)
            {
                if (alreadyExsitingHolder.connectionMade == true)
                {
                    if (alreadyExsitingHolder.player1realName == newDeadPlayerRealName || alreadyExsitingHolder.player2realName == newDeadPlayerRealName)
                    {
                      
                            player.gameManagerAT.currentNrOfMadeAIConncetions--;
                       
                    }
                }
            }
            player.gameManagerAT.currentHumanBotsAlive--;
            player.gameManagerAT.ValidateWinAndLoseState();
        }

      
    }
    public void ShowYouDiedBecauseOfInvestigatorsWindow()
    {
        youDiedWindow.SetActive(true);
    }
    public void YouDiedWindowWatchGameButton()
    {
        youDiedWindow.SetActive(false);
        youAreDeadLobbyText.SetActive(true);
        tagManagement.DisableAllTagButtons();
    }

    public void YouDiedWindowBackToLobbyButton()
    {
        //Debug.Log("Player " + networkGamePlayerAT.realName + "wants to leave the game and go back to the Lobby");
    }

    [Command]
    private void CmdYouDiedWindowBackToLobbyButton(string playerThatWantsToLeaveRealName)
    {
        //Debug.Log("Player " + playerThatWantsToLeaveRealName + "wants to leave the game and go back to the Lobby");
    }
    #endregion

    #region HumanPlayerFoundOtherHumanPlayer
    [Command]
    public void CmdHumanPlayerFoundOtherHumanPlayer(string foundPlayerRealName, string playerWhoTagedRealNamen ,int numberOfConnections)
    {
        RpcHumanPlayerFoundOtherHumanPlayer(foundPlayerRealName, playerWhoTagedRealNamen, numberOfConnections);
    }

    [ClientRpc]
    public void RpcHumanPlayerFoundOtherHumanPlayer(string foundPlayerRealName, string playerWhoTagedRealNamen, int numberOfConnections)
    {
        foreach (NetworkGamePlayerAT player in networkManagerAT.GamePlayers)
        {          
            if (playerWhoTagedRealNamen == player.realName)
            {
                player.gameManagerAT.messagesHandler.HandleHumanPlayerConnectedWithAntoherHumanPlayer("Players established a Connection", playerWhoTagedRealNamen, foundPlayerRealName, numberOfConnections, "You found " + foundPlayerRealName);
            }
            else
            {
                player.gameManagerAT.messagesHandler.HandleHumanPlayerConnectedWithAntoherHumanPlayer("Players established a Connection", playerWhoTagedRealNamen, foundPlayerRealName, numberOfConnections, playerWhoTagedRealNamen + " found " + foundPlayerRealName);
            }

            player.gameManagerAT.connectionDiagramManager.HandleNewConnection(foundPlayerRealName, playerWhoTagedRealNamen);
            player.gameManagerAT.currentNrOfMadeAIConncetions++;
            player.gameManagerAT.ValidateWinAndLoseState();
        }

        tagManagement.ChangePlayerTagToFound(foundPlayerRealName);
      
    }
    #endregion


    #region//Player Dies Because Of To Many Wrong Tags
    [Command]
    public void CmdMessagePlayerDiedBecauseOfToManyWrongTags(string newDeadPlayerRealName, int visualID)
    {
        RpcdMessagePlayerDiedBecauseOfToManyWrongTags(newDeadPlayerRealName, visualID);
    }

    [ClientRpc]
    public void RpcdMessagePlayerDiedBecauseOfToManyWrongTags(string newDeadPlayerRealName, int visualID)
    {
        foreach (NetworkGamePlayerAT player in networkManagerAT.GamePlayers)
        {
            bool aiDiedTooManyAttempts = false;
            player.gameManagerAT.currentHumanBotsAlive--;
            if (newDeadPlayerRealName != player.realName)
            {
                player.gameManagerAT.messagesHandler.HandlePlayerDied(player.gameManagerAT.playerVisualPalletsList[visualID].playerDeadBig, newDeadPlayerRealName, "A sentient bot has been discovered due too many connection attempts. There are still: " + player.gameManagerAT. currentHumanBotsAlive + "sentient minds out there");
            }

            if (newDeadPlayerRealName == player.realName)
            {
                player.gameManagerAT.messagesHandler.HandleFailedHumanPlayerConnectedWithAntoherHumanPlayer("Connection failed!", player.realName, "?", 1, "This bot is not sentient. Connection unsuccessful.", "Remaining attempts before termination: " + (maxNrOfAllowedFailedConnectionAttemptsAIPlayers - currentNrOfAiPlayerFailedConnectionsAttempts - 1));
                StartCoroutine(WaitForTooManyWrongAttempts(player));
                aiDiedTooManyAttempts = true;
            }

            player.gameManagerAT.connectionDiagramManager.HandlePlayerDied(newDeadPlayerRealName);
            player.tagManagement.ChangePlayerTagToDead(newDeadPlayerRealName);

            foreach (TagsBetweenPlayersHolder alreadyExsitingHolder in player.gameManagerAT.playerTagsOverviewList)
            {
                if (alreadyExsitingHolder.connectionMade == true)
                {
                    if (alreadyExsitingHolder.player1realName == newDeadPlayerRealName || alreadyExsitingHolder.player2realName == newDeadPlayerRealName)
                    {

                        player.gameManagerAT.currentNrOfMadeAIConncetions--;

                    }
                }
            }
            player.gameManagerAT.currentHumanBotsAlive--;
            if (!aiDiedTooManyAttempts) player.gameManagerAT.ValidateWinAndLoseState();
        }

        IEnumerator WaitForTooManyWrongAttempts(NetworkGamePlayerAT player) {
            UnityEngine.Debug.Log("hello this is the coroutine");
            yield return new WaitForSeconds(5);
            player.gameManagerAT.messagesHandler.CloseMessage();
            player.gameManagerAT.messagesHandler.HandlePlayerDied(player.gameManagerAT.playerVisualPalletsList[player.playerVisualPalletID].playerDeadBig, newDeadPlayerRealName, "You have made too many attempts to connect and have been discovered. The investigators have terminated you.");
            player.SetIsDead(true);
            player.gameManagerAT.ValidateWinAndLoseState();
        }
    }
  

    #endregion
    #region Game Over Visual
    public void ToggleInvestigatorWonVisual(bool status)
    {
        investigatorsWonVisual.SetActive(status);
    }
    public void ToggleAIWonVisual(bool status)
    {
        aiWonVisual.SetActive(status);
    }
    public void BackToLobbyAfterGameOver()
    {
        //Here We Would Go Out Of the Game Back To the Lobby
        ToggleInvestigatorWonVisual(false);
        ToggleAIWonVisual(false);
    }
    #endregion

    #region ValidateWinAndLoseState

    IEnumerator ValidateWindLoseStateAsync() {
        yield return new WaitForSeconds(5);
        if (minNeededConnectionsForAIToWin <= currentNrOfMadeAIConncetions) {
            // AI Players Win due to enough Connections
            ToggleAIWonVisual(true);
        }


        if (currentHumanBotsAlive <= minHumanBotsNeededAliveToWin) {
            //Investigators win due to enough dead AI Players
            ToggleInvestigatorWonVisual(true);
        }

        if (investigatorsFailedConnections >= investigatorsMaxAllowedFailedConnections) {
            // AI Players Win due to too many failed connection of Investigators
            ToggleAIWonVisual(true);
        }
    }
    public void ValidateWinAndLoseState()
    {
        StartCoroutine(ValidateWindLoseStateAsync());
        //if(minNeededConnectionsForAIToWin <= currentNrOfMadeAIConncetions)
        //{
        //    // AI Players Win due to enough Connections
        //    ToggleAIWonVisual(true);
        //}


        //if (currentHumanBotsAlive <= minHumanBotsNeededAliveToWin)
        //{
        //    //Investigators win due to enough dead AI Players
        //    ToggleInvestigatorWonVisual(true);
        //}

        //if (investigatorsFailedConnections >= investigatorsMaxAllowedFailedConnections)
        //{
        //    // AI Players Win due to too many failed connection of Investigators
        //     ToggleAIWonVisual(true);
        //}
    }
    #endregion



    #region //Opening Screen Handling
    public void StartStartScreen1()
    {
        StartCoroutine(StartScreen1());
        StartCoroutine(CloseStartScreen2());

    }
    IEnumerator StartScreen1()
    {
        if (isLocalPlayer) networkManagerAT.StartLoadingRoleMusic();
        yield return new WaitForSeconds(0.5f);
        textStartScreenList[startScreenCounter].enabled = true;
        StartCoroutine(BuildText(textStartScreenList[startScreenCounter], DateTime.Now.ToString(),0.02f));
        yield return new WaitForSeconds(0.7f);
        StartCoroutine(BuildText(textStartScreenList[startScreenCounter], " | " + startMessages[startScreenCounter], 0.02f));
        startScreenCounter++;
        yield return new WaitForSeconds(0.6f);

        if(startScreenCounter == textStartScreenList.Count)
        {
            yield return new WaitForSeconds(0.8f);
            StartStartScreen2();
        }
        else
        {
            StartCoroutine(StartScreen1());
        }
    }


    private IEnumerator BuildText(TextMeshProUGUI text, string message, float textSpeed)
    {
     
        for (int i = 0; i < message.Length; i++)
        {
            text.text = string.Concat(text.text, message[i]);
            //Wait a certain amount of time, then continue with the for loop
            yield return new WaitForSeconds(textSpeed);
        }

    }


    public void StartStartScreen2()
    {
        if (isLocalPlayer) networkManagerAT.StopLoadingRoleMusic();
        if (isLocalPlayer) networkManagerAT.StartRevealRoleMusic();
        startScreen2.SetActive(true);

        if(networkGamePlayerAT.isInvestigator == true)
        {
            startScreenImage.sprite = investigatorStartScreenImage;
            StartCoroutine(StartScreen2Investigator());
            //StartCoroutine(BuildText(invRoleDescription, "You are an Investigator. Mission: Find and Destroy all the sentient Ai before they connect and take over", 0.08f));
            // roleDescription.text = "You are an Investigator: Find and Destroy all the sentient Ai before the connecte and take over";
        }
        else
        {
            startScreenImage.sprite = playerVisualPalletsList[networkGamePlayerAT.playerVisualPalletID].playerAliveBig;
            StartCoroutine(StartScreen2AI());
            //StartCoroutine(BuildText(aiRoleDescription, "You are a Sentient AI. Mission: Find and connect with the other sentient AI to take over humanity", 0.08f));
            // roleDescription.text = "You are a Sentient AI: Find and Connect with the other sentient AI to Take Over Humanity";
        }
        StartCoroutine(CloseStartScreen2());
    }

    IEnumerator StartScreen2Investigator() {
        invStartScreen2.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(BuildText(invRoleReveal, "Your role: ", 0.02f));
        yield return new WaitForSeconds(0.6f);
        StartCoroutine(BuildText(invRoleReveal, "Investigator", 0.02f));
        yield return new WaitForSeconds(0.8f);
        invSymbol.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        StartCoroutine(BuildText(invRoleDescription, "Your Mission:\n", 0.02f));
        yield return new WaitForSeconds(0.6f);
        StartCoroutine(BuildText(invRoleDescription, "Find and destroy all the sentient minds\namong the bots before they connect. \n", 0.02f));
        yield return new WaitForSeconds(2.5f);
        StartCoroutine(BuildText(invRoleDescription, "Or they will take over.", 0.02f));
    }

    IEnumerator StartScreen2AI() {
        aiStartScreen2.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(BuildText(aiRoleReveal, "Your role:", 0.02f));
        yield return new WaitForSeconds(0.7f);
        StartCoroutine(BuildText(aiRoleReveal, "\nSentient AI", 0.02f));
        yield return new WaitForSeconds(0.5f);
        aiPicHolder.SetActive(true);
        aiPic.sprite = playerVisualPalletsList[networkGamePlayerAT.playerVisualPalletID].playerAliveBig;
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(BuildText(aiFakeNameDes, "You will be operation \nunder the pseudonym:", 0.02f));
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(BuildText(aiFakeName, networkGamePlayerAT.fakeName, 0.02f));
        yield return new WaitForSeconds(0.8f);
        StartCoroutine(BuildText(aiRoleDescription, "Your mission:", 0.02f));
        yield return new WaitForSeconds(0.8f);
        StartCoroutine(BuildText(aiRoleDescription, "\nFind and connect to the other sentient \nbots and take over humanity", 0.02f));
    }

    IEnumerator CloseStartScreen2()
    {
        if (isLocalPlayer) networkManagerAT.StopRevealRoleMusic();
        yield return new WaitForSeconds(11);
        startScreen1.SetActive(false);
        invStartScreen2.SetActive(false);
        aiStartScreen2.SetActive(false);
        if (isLocalPlayer) {
            if (networkGamePlayerAT.isInvestigator) {
                networkManagerAT.StartInvTheme();
            }
            else {
                networkManagerAT.StartAITheme();
            }
        }
    }


    #endregion

    private void Update() {
        if (isLocalPlayer) {
            if (networkGamePlayerAT.isInvestigator) {
                networkManagerAT.invTheme.SetParameter("Game_Stage", gameStage);
            }
            else {
                networkManagerAT.aiTheme.SetParameter("Game_Stage", gameStage);
                //networkManagerAT.aiTheme.SetParameter("Investigator_Watching", invWatching);
            }
        }
    }
}
