using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManagerMessagesHandler : MonoBehaviour
{
    [Header("Manager References")]
    [SerializeField] private GameManagerAT gameManagerAT;
    [SerializeField] private NetworkGamePlayerAT networkGamePlayerAT;


    [Header("Object Referenc")]
    [SerializeField] private GameObject mainBroadcastHolder;
    [SerializeField] private GameObject messageField;
    [SerializeField] private GameObject attemptsField;
    [SerializeField] private GameObject invFailed;
    [SerializeField] private GameObject closeScreenButtonObj;

    [Header("Component References")]
    public TextMeshProUGUI title;
    public TextMeshProUGUI message;
    public TextMeshProUGUI attempts;

    [Header("Player Dead References")]
    [SerializeField] private GameObject deadPlayerVisual;
    public Image deadPlayerImage;
    public TextMeshProUGUI deadPlayerName;
   

    [Header("Player Connected References")]
    [SerializeField] private GameObject connectionVisual;
    [SerializeField] private GameObject successfulConnectionVisual;
    [SerializeField] private GameObject failedConnectionVisual;
    public TextMeshProUGUI playerConnectName1;
    public TextMeshProUGUI playerConnectName2;



    #region//Open And Close The Message  
    public void CloseMessage()
    {

        if (deadPlayerName.text == networkGamePlayerAT.realName)
        {
            gameManagerAT.ShowYouDiedWindow();
        }

        title.gameObject.SetActive(false);
        messageField.SetActive(false);
        attemptsField.SetActive(false);
        deadPlayerVisual.SetActive(false);
        connectionVisual.SetActive(false);
        successfulConnectionVisual.SetActive(false);
        failedConnectionVisual.SetActive(false);
        playerConnectName1.text = "";
        playerConnectName2.text = "";
        deadPlayerName.text = "";
        invFailed.SetActive(false);





        mainBroadcastHolder.SetActive(false);
    }
    #endregion

    #region//Visual Handling When a Player Died
    public void HandlePlayerDied(Sprite newDeadPlayerSprite, string newDeadPlayerName, string newMessage, string attemptsText, bool localPlayer)
    {


        CloseMessage();

        if(localPlayer == true && newDeadPlayerName == networkGamePlayerAT.realName)
        {
            gameManagerAT.tagManagement.DisableAllTagButtons();
        }
  
        mainBroadcastHolder.SetActive(true);
        deadPlayerVisual.SetActive(true);
        deadPlayerImage.sprite = newDeadPlayerSprite;
        deadPlayerName.text = newDeadPlayerName;
        message.text = newMessage;
        messageField.SetActive(true);
        attempts.text = attemptsText;
        attemptsField.SetActive(true);
        deadPlayerVisual.SetActive(true);
        failedConnectionVisual.SetActive(false);
    }

 
    #endregion

    #region//Visual Handling When Human Player Connected With Another Human Player
    public void HandleHumanPlayerConnectedWithAntoherHumanPlayer(string newTitle, string playerThatFoundTheOtherName, string tagedPlayerName, int numberOfConnections, string newMessage, bool localPlayer)
    {
        CloseMessage();
        mainBroadcastHolder.SetActive(true);
        title.gameObject.SetActive(true);
        title.text = newTitle;
        connectionVisual.SetActive(true);
        successfulConnectionVisual.SetActive(true);
        failedConnectionVisual.SetActive(false);

        playerConnectName1.text = playerThatFoundTheOtherName;
        playerConnectName2.text = tagedPlayerName;

        message.text = newMessage;
        messageField.SetActive(true);

        if (localPlayer) {
            if (networkGamePlayerAT.isInvestigator) {
                gameManagerAT.networkManagerAT.inv_AIConnectionMade.Play();
            }
            else {
                gameManagerAT.networkManagerAT.ai_AIConnectionMade.Play();
            }
        }
    }
    #endregion

    #region//Visual Handling When Human Player Failed to connect to Another Human Player
    public void HandleFailedHumanPlayerConnectedWithAntoherHumanPlayer(string newTitle, string playerThatFoundTheOtherName, string tagedPlayerName, int numberOfConnections, string newMessage, string attemptsMessage, bool localPlayer)
    {
      
        CloseMessage();
        mainBroadcastHolder.SetActive(true);
        title.text = newTitle;
        title.gameObject.SetActive(true);
        connectionVisual.SetActive(true);
        failedConnectionVisual.SetActive(true);
        successfulConnectionVisual.SetActive(false);
        attemptsField.SetActive(true);
        attempts.text = attemptsMessage;
        playerConnectName1.text = playerThatFoundTheOtherName;
        playerConnectName2.text = tagedPlayerName;

        message.text = newMessage;
        messageField.SetActive(true);

        if (networkGamePlayerAT.isInvestigator && localPlayer) {
            gameManagerAT.networkManagerAT.inv_AIConnectionFailed.Play();
        }
    }
    #endregion

    #region Broadcast for investigators when they made too many wrong attempts
    public void HandleInvestigatorsMadeTooManyWrongAttempts(string message) {
        CloseMessage();
        gameManagerAT.networkManagerAT.botTerminated.Play();
        mainBroadcastHolder.SetActive(true);
        messageField.SetActive(true);
        invFailed.SetActive(true);
        this.message.text = message;
    }

    #endregion
}
