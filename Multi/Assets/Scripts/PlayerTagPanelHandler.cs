using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerTagPanelHandler : MonoBehaviour
{
    [Header("Component references")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI playerFakeName;
    [SerializeField] private Image playerVisual;
    [SerializeField] private Sprite deadSprite;

    private Sprite baseSprite;
    private Sprite hoverSprite;

    private bool dead = false;

    public int visualID;
    private TagManagement tagManagement;
    private string playerRealName;
   public void StartSetup(string newPlayerFakeName, string newPlayerRealName, Sprite newPlayerSprite, TagManagement myManagement, int visualID, bool isLocalPlayer)
   {
        if (isLocalPlayer)
        {
            playerFakeName.text = "(" + newPlayerFakeName + ")";
        }
        else
        {
            playerFakeName.text =  newPlayerFakeName ;
        }
        playerRealName = newPlayerRealName;
        playerVisual.sprite = newPlayerSprite;
        baseSprite = playerVisual.sprite;
        tagManagement = myManagement;
        this.visualID = visualID;
   }

    public void ButtonClick()
    {
        tagManagement.PlayerInTagPanelHasBeenClickd(playerRealName, playerFakeName.text, visualID);
    }
    private void Update()
    {
      
    }
    public void ActivateHiglightVisual()
   {
        if (button.interactable == true)
        {
            playerVisual.sprite = tagManagement.gameManagerAT.playerVisualPalletsList[visualID].playerSmallHover;
        }
        if (dead) playerVisual.sprite = deadSprite;

    }
    public void DeactivateHiglightVisual()
    {
        playerVisual.sprite = tagManagement.gameManagerAT.playerVisualPalletsList[visualID].playerSmall;
        if (dead) playerVisual.sprite = deadSprite;
    }

    public void DisableButton()
    {
        button.interactable =  false;
        Debug.Log("DisableButton");
    }

    public void SwitchSpriteToDead() {
        playerVisual.sprite = deadSprite;
        button.interactable = false;
        baseSprite = deadSprite;
        hoverSprite = baseSprite;
        dead = true;
    }


   

    public string GetPlayerRealName()
    {
        return playerRealName;
    }
}
