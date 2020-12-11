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
    public List<PlayerTagPanelHandler> playerTagPanelHandlerList = new List<PlayerTagPanelHandler>();

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

    public GameObject tagPanelParent;
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    #region//Start Variables Setup
    public void StartSetup()
    {
        StartCoroutine(ShortSetupDelay());
    }


    IEnumerator ShortSetupDelay()
    {
        yield return new WaitForSeconds(3f);
        CmdPopulateAllTagablePlayer();
        CmdPopulateAllTagableBots();
        StartCoroutine(ShortSetupDelay2());
    }
    IEnumerator ShortSetupDelay2()
    {
        yield return new WaitForSeconds(2f);
        int totalNrOfPanelSlots = playerTagPanelHandlerList.Count + botsTagPanelHandlerList.Count;
        moveViewTagPanel.SetTagPanelPositions(totalNrOfPanelSlots, playerTagPanelHandlerList, botsTagPanelHandlerList);
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

        GameObject newBotTagPanelObj = Instantiate(playerTagPanelPrefab);
        newBotTagPanelObj.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        PlayerTagPanelHandler botTagPanelHandler = newBotTagPanelObj.GetComponent<PlayerTagPanelHandler>();
       
        botTagPanelHandler.StartSetup(botFakeName, realName, gameManagerAT.playerVisualPalletsList[botVisualPalletID].playerSmall, this, botVisualPalletID);

       
        botsTagPanelHandlerList.Add(botTagPanelHandler);

    
    }
    #endregion

    #region //Opening And Closing Tag Panel
    public void OpenTagPanel()
    {
        // playerTagPanelHandlerList[0].transform.localPosition = 
        //1.Wir brauchen eine Auswertung wieviele Tags Müssen angezeigt werden
        int totalNrOfPanelSlots = playerTagPanelHandlerList.Count + botsTagPanelHandlerList.Count;
        //2. Anhange dieser Zahl festlegen wie die Konfiguration ist -> Spricht 4 Unter einander, oder 3Links 4 Rechtes, Diese Konfiguartion speichern
        float neededXSize = 0;
        float neededYSize = 30; //10 für offset
        Debug.Log("totalNrOfPanelSlots " + totalNrOfPanelSlots);
        if (totalNrOfPanelSlots < 12)
        {
            neededXSize += playerTagPanelHandlerList[0].GetComponent<RectTransform>().rect.width;
            neededYSize += playerTagPanelHandlerList[0].GetComponent<RectTransform>().rect.height * totalNrOfPanelSlots;
        }
        else
        {
            neededXSize += playerTagPanelHandlerList[0].GetComponent<RectTransform>().rect.width * 2;
            neededYSize += playerTagPanelHandlerList[0].GetComponent<RectTransform>().rect.height * (totalNrOfPanelSlots /2);
        }
            Debug.Log("in");
        
        //3. Durch die Liste aller chatPanels Cyclen und ihnen eihne position geben. 
      
        //4. Sie alle anmachen sobald Tag Panel Offen
        //5. Schauen, dass Verknüpfungen stimmen


        Debug.Log("neededXSize " + neededXSize);

        Debug.Log("neededYSize " + neededYSize);

        moveViewTagPanel.OpenTagPanel(neededXSize, neededYSize, totalNrOfPanelSlots, playerTagPanelHandlerList, botsTagPanelHandlerList);
        openTagPanelButton.SetActive(false);
        closeTagPanelButton.SetActive(true);
    }
    public void CloseTagPanel()
    {

        foreach (PlayerTagPanelHandler x in playerTagPanelHandlerList)
        {
            x.gameObject.SetActive(false);
        }

        foreach (PlayerTagPanelHandler x in botsTagPanelHandlerList)
        {
            x.gameObject.SetActive(true);
        }



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
            GameObject newPlayerTagPanelObj = Instantiate(playerTagPanelPrefab);
            newPlayerTagPanelObj.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
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
        clickedPlayerPicture.sprite = playerVisual.sprite;
        playerFakeName.text = tagedPlayerFakeName;

        if (networkGamePlayerAT.isInvestigator)
        {
            buttonText.text = "Destroy Bot";
            remainingAttemptsText.text = "Reamaining Attempts: " + gameManagerAT.investigatorsFailedConnections + "/" + gameManagerAT.investigatorsMaxAllowedFailedConnections;
        }
        else
        {
            buttonText.text = "Attempt Connection";
            remainingAttemptsText.text = "Reamaining Attempts: " + gameManagerAT.currentNrOfAiPlayerFailedConnectionsAttempts + "/" + gameManagerAT.maxNrOfAllowedFailedConnectionAttemptsAIPlayers;
        }
    
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
                x.SwitchSpriteToDead();
                //x.SetButtonDisabledColor(Color.red);
            }
        }
        foreach (PlayerTagPanelHandler x in botsTagPanelHandlerList)
        {
            if (x.GetPlayerRealName() == tagedPlayerRealName)
            {
                x.DisableButton();
                x.SwitchSpriteToDead();
                //x.SetButtonDisabledColor(Color.red);
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
                x.SwitchSpriteToDead();
                //x.SetButtonDisabledColor(Color.green);
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
