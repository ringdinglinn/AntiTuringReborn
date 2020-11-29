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
    public int totalHumanBots;
    public int currentHumanBotsAlive;
    public int minHumanBotsNeededAliveToWin;
    public int currentNrOfMadeConncetions;
    public int investigatorsFailedConnnections;

    [Header("You Died Window")]
    [SerializeField] private GameObject youDiedWindow;
    [SerializeField] private GameObject youAreDeadLobbyText;

    public List<TagsBetweenPlayersHolder> playerTagsOverviewList = new List<TagsBetweenPlayersHolder>();
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
        yield return new WaitForSeconds(0.2f);
     //   Debug.Log("Message kommt an 1");
        CmdStartSetup();
       // ConnectionsOverviewSetup();
       
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

    #region Handle All Connnations
    private void ConnectionsOverviewSetup()
    { //Kreiere eine Liste mit allen Möglichen Connections
       // Debug.Log("Message kommt an 4");
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
    [Command]
    private void CmdHandleNewConnections(string tagedPlayerRealName, string playerWhoTagedRealNamen)
    {
        RpcHandleNewConnections(tagedPlayerRealName, playerWhoTagedRealNamen);
    }

    [ClientRpc]
    private void RpcHandleNewConnections(string tagedPlayerRealName, string playerWhoTagedRealNamen)
    {
      //  Debug.Log("tagedPlayerRealName " + tagedPlayerRealName);
      //  Debug.Log("playerWhoTagedRealNamen " + playerWhoTagedRealNamen);
        foreach (NetworkGamePlayerAT player in networkManagerAT.GamePlayers)
        {
            foreach (TagsBetweenPlayersHolder alreadyExsitingHolder in player.gameManagerAT. playerTagsOverviewList)
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
    private void HandleConnactionWhenPlayerDies()
    {

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
        }

        tagManagement.ChangePlayerTagToFound(foundPlayerRealName);
      
    }
    #endregion
}
