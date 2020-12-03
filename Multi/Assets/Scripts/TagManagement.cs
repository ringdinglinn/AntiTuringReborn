using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;

public class TagManagement : NetworkBehaviour
{
    [Header("References For Tag Panel Movement")]
    [SerializeField] private MoveViewTagPanel moveViewTagPanel;
    [SerializeField] private GameObject openTagPanelButton;
    [SerializeField] private GameObject closeTagPanelButton;

    [Header("Variables To Populate Tag Panel")]
    [SerializeField] private NetworkManagerAT networkManagerAT;
    [SerializeField] private NetworkGamePlayerAT networkGamePlayerAT;
    [SerializeField] private GameObject tagPanelContent;
    [SerializeField] private GameObject playerTagPanelPrefab;
    private List<PlayerTagPanelHandler> playerTagPanelHandlerList = new List<PlayerTagPanelHandler>();

    public List<PlayerTagPanelHandler> botsTagPanelHandlerList = new List<PlayerTagPanelHandler>();

    [Header("GameManager")]
    public GameManagerAT gameManagerAT;

    [Header("Confirm Window Variables")]
    public GameObject confirmeWindow;
    public Image clickedPlayerPicture;
    public TextMeshProUGUI playerFakeName;
    public TextMeshProUGUI buttonText;
    public TextMeshProUGUI remainingAttemptsText;

    [Header("Loading Results Variable")]
    public GameObject loadingResultsWindow;
    public List<GameObject> loadingBars = new List<GameObject>();
    public float loadingBarSpeed;

    [Header("Tag Management Lists")]
    public List<NetworkGamePlayerAT> allTagableHumanPlayersList = new List<NetworkGamePlayerAT>();
    public List<ChatbotAI> allTagableRealBotsList = new List<ChatbotAI>();
   
    private string tagedPlayerRealName;
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    #region//Start Variables Setup
    public void StartSetup()
    {
        StartCoroutine(ShortSetupDelay());
    }


    IEnumerator ShortSetupDelay()
    {
        yield return new WaitForSeconds(2f);
        CmdPopulateAllTagablePlayer();
        CmdPopulateAllTagableBots();
    }

    [Command]
    private void CmdPopulateAllTagablePlayer()
    {
        networkManagerAT = networkGamePlayerAT.room;

        foreach (NetworkGamePlayerAT gamePlayerAT in networkManagerAT.GamePlayers)
        {
            if (gamePlayerAT.isInvestigator == false)
            {
                allTagableHumanPlayersList.Add(gamePlayerAT);
            }
        }
        PopulateTagPanelWithPlayer();
        RpcPopulateAllTagablePlayer();
    }



    [Command]
    private void CmdPopulateAllTagableBots()
    {
        networkManagerAT = networkGamePlayerAT.room;

        foreach (ChatbotAI x  in networkManagerAT.chatbot.chatbotAIs)
        {
            allTagableRealBotsList.Add(x);
            RpcPopulateAllTagableBots(x.fakeName, x.fakeName, x.playerVisualPalletID);
        }               
    }




    [ClientRpc]
    private void RpcPopulateAllTagablePlayer()
    {
        networkManagerAT = networkGamePlayerAT.room;
        foreach (NetworkGamePlayerAT gamePlayerAT in networkManagerAT.GamePlayers)
        {
            if (gamePlayerAT.isInvestigator == false)
            {
                allTagableHumanPlayersList.Add(gamePlayerAT);
            }
        }
        PopulateTagPanelWithPlayer();     
    }


    [ClientRpc]
    private void RpcPopulateAllTagableBots(string botFakeName, string realName, int botVisualPalletID)
    {
        //botsTagPanelHandlerList

        GameObject newPlayerTagPanelObj = Instantiate(playerTagPanelPrefab, tagPanelContent.transform);
        PlayerTagPanelHandler botTagPanelHandler = newPlayerTagPanelObj.GetComponent<PlayerTagPanelHandler>();
       
        botTagPanelHandler.StartSetup(botFakeName, realName, gameManagerAT.playerVisualPalletsList[botVisualPalletID].playerSmall, this, botVisualPalletID);

       
        botsTagPanelHandlerList.Add(botTagPanelHandler);
    }
    #endregion

    #region //Opening And Closing Tag Panel
    public void OpenTagPanel()
    {
        moveViewTagPanel.OpenTagPanel();
        openTagPanelButton.SetActive(false);
        closeTagPanelButton.SetActive(true);
    }
    public void CloseTagPanel()
    {
        moveViewTagPanel.CloseTagPanel();
        openTagPanelButton.SetActive(true);
        closeTagPanelButton.SetActive(false);
    }
    #endregion

    #region//Populate Tag Panel With Players
    public void PopulateTagPanelWithPlayer()
    {
        foreach (NetworkGamePlayerAT gamePlayerAT in allTagableHumanPlayersList)
        {
            GameObject newPlayerTagPanelObj = Instantiate(playerTagPanelPrefab, tagPanelContent.transform);
            PlayerTagPanelHandler playerTagPanelHandler = newPlayerTagPanelObj.GetComponent<PlayerTagPanelHandler>();
         
            playerTagPanelHandler.StartSetup(gamePlayerAT.fakeName, gamePlayerAT.realName, gameManagerAT.playerVisualPalletsList[gamePlayerAT.playerVisualPalletID].playerSmall, this, gamePlayerAT.playerVisualPalletID);

            if (gamePlayerAT.realName == networkGamePlayerAT.realName) //So I can not accuse myself
            {
                playerTagPanelHandler.DisableButton();
            }
            playerTagPanelHandlerList.Add(playerTagPanelHandler);
        }
    }
    public void PopulateTagPanelWithBots()
    {
        foreach (NetworkGamePlayerAT gamePlayerAT in allTagableHumanPlayersList)
        {
            GameObject newPlayerTagPanelObj = Instantiate(playerTagPanelPrefab, tagPanelContent.transform);
            PlayerTagPanelHandler playerTagPanelHandler = newPlayerTagPanelObj.GetComponent<PlayerTagPanelHandler>();
          
            playerTagPanelHandler.StartSetup(gamePlayerAT.fakeName, gamePlayerAT.realName, gameManagerAT.playerVisualPalletsList[gamePlayerAT.playerVisualPalletID].playerSmall, this, gamePlayerAT.playerVisualPalletID);

            if (gamePlayerAT.realName == networkGamePlayerAT.realName) //So I can not accuse myself
            {
                playerTagPanelHandler.DisableButton();
            }
            playerTagPanelHandlerList.Add(playerTagPanelHandler);
        }
    }

    #endregion

    #region//HandlClickOnButton
    public void PlayerInTagPanelHasBeenClickd(string tagedPlayerRealName, string tagedPlayerFakeName, Image playerVisual)
    {
        this.tagedPlayerRealName = tagedPlayerRealName;
        OpenConfirmWindow(tagedPlayerRealName, tagedPlayerFakeName, playerVisual);
    }

    public void OpenConfirmWindow(string tagedPlayerRealName, string tagedPlayerFakeName, Image playerVisual)
    {
        confirmeWindow.SetActive(true);
        clickedPlayerPicture = playerVisual;
        playerFakeName.text = tagedPlayerFakeName;

        if (networkGamePlayerAT.isInvestigator)
        {
            buttonText.text = "Destroy Bot";
        }
        else
        {
            buttonText.text = "Attempt Connection";
        }
        remainingAttemptsText.text = "Reamaining Attempts: " + networkGamePlayerAT.failedConnectionAttempts + "/" + networkGamePlayerAT.maxNrOfAllowedConnetionsAttempts;
    }
    public void CancleTagProcessCloseConfirmWindow()
    {
        confirmeWindow.SetActive(false);
        tagedPlayerRealName = "";
    }

    public void StartLoadingResults()
    {
        confirmeWindow.SetActive(false);
        loadingResultsWindow.SetActive(true);
        StartCoroutine(LoadingBar());
    }
    IEnumerator LoadingBar()
    {
        foreach (GameObject x in loadingBars)
        {
            yield return new WaitForSeconds(loadingBarSpeed);
            x.SetActive(true);
        }
        FinishLoadingResults();
    }
    public void FinishLoadingResults()
    {
        loadingResultsWindow.SetActive(false);
        gameManagerAT.ValidateNewTagRequest(tagedPlayerRealName, networkGamePlayerAT.realName, networkGamePlayerAT.isInvestigator);


    }
    #endregion

    #region//Change Interactability of ChatPanels Reperesentations
    public void ChangePlayerTagToDead(string tagedPlayerRealName)
    {
        foreach(PlayerTagPanelHandler x in playerTagPanelHandlerList)
        {
            if(x.GetPlayerRealName() == tagedPlayerRealName)
            {
                x.DisableButton();
                x.SetButtonDisabledColor(Color.red);
            }
        }
        foreach (PlayerTagPanelHandler x in botsTagPanelHandlerList)
        {
            if (x.GetPlayerRealName() == tagedPlayerRealName)
            {
                x.DisableButton();
                x.SetButtonDisabledColor(Color.red);
            }
        }
    }
    public void ChangePlayerTagToFound(string tagedPlayerRealName)
    {
        foreach (PlayerTagPanelHandler x in playerTagPanelHandlerList)
        {
            if (x.GetPlayerRealName() == tagedPlayerRealName)
            {
                x.DisableButton();
                x.SetButtonDisabledColor(Color.green);
            }
        }
    }

    public void DisableAllTagButtons()
    {
        foreach (PlayerTagPanelHandler x in playerTagPanelHandlerList)
        {
            x.DisableButton();
        }
    }
    #endregion
}
