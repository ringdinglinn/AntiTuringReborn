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
    
    
    [Header("Component References")]
    public TextMeshProUGUI title;
    public TextMeshProUGUI message;

    [Header("Player Dead References")]
    [SerializeField] private GameObject deadPlayerVisual;
    public Image deadPlayerImage;
    public TextMeshProUGUI deadPlayerName;
   

    [Header("Player Connected References")]
    [SerializeField] private GameObject connectionVisual;
    public Image connectionVisualImage1;
    public Image connectionVisualImage2;
    public TextMeshProUGUI playerConnectName1;
    public TextMeshProUGUI playerConnectName2;



    #region//Open And Close The Message  
    public void CloseMessage()
    {
        messageField.SetActive(false);
        deadPlayerVisual.SetActive(false);
        connectionVisual.SetActive(false);
        connectionVisualImage1.enabled = false;
        connectionVisualImage2.enabled = false;

        if(deadPlayerName.text == networkGamePlayerAT.realName)
        {
            gameManagerAT.ShowYouDiedWindow();
        }
    }
    #endregion

    #region//Visual Handling When a Player Died
    public void HandlePlayerDied(string newTitle, Sprite newDeadPlayerSprite, string newDeadPlayerName, string newMessage)
    {
        title.text = newTitle;
        deadPlayerVisual.SetActive(true);
        deadPlayerImage.sprite = newDeadPlayerSprite;
        deadPlayerName.text = newDeadPlayerName;
        message.text = newMessage;
        messageField.SetActive(true);
        deadPlayerVisual.SetActive(true);
    }
    #endregion

    #region//Visual Handling When Human Player Connected With Another Human Player
    public void HandleHumanPlayerConnectedWithAntoherHumanPlayer(string newTitle, string playerThatFoundTheOtherName, string tagedPlayerName, int numberOfConnections, string newMessage)
    {
        title.text = newTitle;
        connectionVisual.SetActive(true);

        playerConnectName1.text = playerThatFoundTheOtherName;
        playerConnectName2.text = tagedPlayerName;

        if(numberOfConnections == 1 )
        {
            connectionVisualImage1.enabled = true;
        }
        else if(numberOfConnections == 2)
        {
            connectionVisualImage1.enabled = true;
            connectionVisualImage2.enabled = true;
        }

            message.text = newMessage;
            messageField.SetActive(true);
     
    }
    #endregion
}
