﻿using System.Collections;
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

    public int visualID;
    private TagManagement tagManagement;
    private string playerRealName;
   public void StartSetup(string newPlayerFakeName, string newPlayerRealName, Sprite newPlayerSprite, TagManagement myManagement, int visualID)
   {
        playerFakeName.text = newPlayerFakeName;
        playerRealName = newPlayerRealName;
        playerVisual.sprite = newPlayerSprite;
        tagManagement = myManagement;
        this.visualID = visualID;
   }

    public void ButtonClick()
    {
        tagManagement.PlayerInTagPanelHasBeenClickd(playerRealName, playerFakeName.text, visualID);
    }

   public void ActivateHiglightVisual()
   {
        playerVisual.sprite = tagManagement.gameManagerAT.playerVisualPalletsList[visualID].playerSmallHover;

   }
    public void DeactivateHiglightVisual()
    {
        playerVisual.sprite = tagManagement.gameManagerAT.playerVisualPalletsList[visualID].playerSmall;
    }

    public void DisableButton()
    {
        button.interactable =  false;
    }

    public void SwitchSpriteToDead() {
        playerVisual.sprite = deadSprite;
    }

    public void SetButtonDisabledColor(Color newColor)
    {
        var newColorBlock = button.colors;
        newColorBlock.disabledColor = newColor;
        button.colors = newColorBlock;
    }
    public void EnableButton()
    {
        button.interactable = true;
    }

    public string GetPlayerRealName()
    {
        return playerRealName;
    }
}
