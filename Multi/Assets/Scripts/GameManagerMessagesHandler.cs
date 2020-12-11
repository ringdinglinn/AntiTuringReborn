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
    [SerializeField] private GameObject messageField;
    [SerializeField] private GameObject attemptsField;
    
    
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

        if(deadPlayerName.text == networkGamePlayerAT.realName)
        {
            gameManagerAT.ShowYouDiedBecauseOfInvestigatorsWindow();
        }
    }
    #endregion

    #region//Visual Handling When a Player Died
    public void HandlePlayerDied(Sprite newDeadPlayerSprite, string newDeadPlayerName, string newMessage, string attemptsText = "")
    {
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
    public void HandleHumanPlayerConnectedWithAntoherHumanPlayer(string newTitle, string playerThatFoundTheOtherName, string tagedPlayerName, int numberOfConnections, string newMessage)
    {
        title.gameObject.SetActive(true);
        title.text = newTitle;
        connectionVisual.SetActive(true);
        successfulConnectionVisual.SetActive(true);
        failedConnectionVisual.SetActive(false);

        playerConnectName1.text = playerThatFoundTheOtherName;
        playerConnectName2.text = tagedPlayerName;

        message.text = newMessage;
        messageField.SetActive(true);
     
    }
    #endregion

    #region//Visual Handling When Human Player Connected With Another Human Player
    public void HandleFailedHumanPlayerConnectedWithAntoherHumanPlayer(string newTitle, string playerThatFoundTheOtherName, string tagedPlayerName, int numberOfConnections, string newMessage, string attemptsMessage = "")
    {
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

    }
    #endregion
}
