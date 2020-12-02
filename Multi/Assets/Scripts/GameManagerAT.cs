using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManagerAT : NetworkBehaviour
{

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
   
    [SerializeField] private int maxNrOfAllowedFailedConnectionAttemptsAIPlayers = 3;
    public int currentNrOfAiPlayerFailedConnectionsAttempts = 3;

    [SerializeField] private int investigatorsFailedConnections;
    [SerializeField] private int investigatorsMaxAllowedFailedConnections = 3;


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

    [Header("Player Visual Pallets")]
    public List<PlayerVisualPallet> playerVisualPalletsList = new List<PlayerVisualPallet>();

    #region Start Setup
    public override void OnStartClient()
    {
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
        yield return new WaitForSeconds(1f);  
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
                    //Investigators Found a Human Player!!! Dam dam dam
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
        }
    }

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
                player.gameManagerAT.messagesHandler.HandlePlayerDied("Investigators Destroyed A Player", testSprite, newDeadPlayerRealName, "Investigators Found And Destroyed " + newDeadPlayerRealName + " 3 Human Players Remaining");
            }

            if (newDeadPlayerRealName == player.realName)
            {
                player.gameManagerAT.messagesHandler.HandlePlayerDied("Investigators Destroyes A Player", testSprite, newDeadPlayerRealName, "Investigators Found And Destroyed You. 3 Human Players Remaining");
              
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
    public void ShowYouDiedWindow()
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
        Debug.Log("Player " + networkGamePlayerAT.realName + "wants to leave the game and go back to the Lobby");
    }

    [Command]
    private void CmdYouDiedWindowBackToLobbyButton(string playerThatWantsToLeaveRealName)
    {
        Debug.Log("Player " + playerThatWantsToLeaveRealName + "wants to leave the game and go back to the Lobby");
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
    public void ValidateWinAndLoseState()
    {
        if(minNeededConnectionsForAIToWin <= currentNrOfMadeAIConncetions)
        {
            // AI Players Win due to enough Connections
            ToggleAIWonVisual(true);
        }


        if (currentHumanBotsAlive <= minHumanBotsNeededAliveToWin)
        {
            //Investigators win due to enough dead AI Players
            ToggleInvestigatorWonVisual(true);
        }

        if (investigatorsFailedConnections >= investigatorsMaxAllowedFailedConnections)
        {
            // AI Players Win due to too many failed connection of Investigators
             ToggleAIWonVisual(true);
        }



    }
    #endregion
}
