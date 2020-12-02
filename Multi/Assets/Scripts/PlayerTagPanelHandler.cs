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

    private TagManagement tagManagement;
    private string playerRealName;
   public void StartSetup(string newPlayerFakeName, string newPlayerRealName, Sprite newPlayerSprite, TagManagement myManagement)
   {
        playerFakeName.text = newPlayerFakeName;
        playerRealName = newPlayerRealName;
        playerVisual.sprite = newPlayerSprite;
        tagManagement = myManagement;
   }

    public void ButtonClick()
    {
        tagManagement.PlayerInTagPanelHasBeenClickd(playerRealName, playerFakeName.text, playerVisual);
    }


    public void DisableButton()
    {
        button.interactable =  false;
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
