using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ConnectionDiagramLineHandler : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private Image arrowTowardsPlayerAlive;
    [SerializeField] private Image arrowTowardsPlayerDead;
   
    [Header("Visible For Debugging")]
    [SerializeField] private string playerArrowTowardsRealName;
    [SerializeField] private string playerArrowFromRealName;


    private void Start()
    {
        arrowTowardsPlayerAlive.enabled = false;
    }

    public void SetPlayerArrowTowardsRealName(string newPlayerArrowTowardsRealName)
    {
        playerArrowTowardsRealName = newPlayerArrowTowardsRealName;
    }
    public void SetPlayerArrowFromRealName(string newPlayerArrowFromRealName)
    {
        playerArrowFromRealName = newPlayerArrowFromRealName;
    }

    public string GetPlayerArrowTowardsRealName()
    {
        return playerArrowTowardsRealName;
    }
    public string GetPlayerArrowFromRealName()
    {
        return playerArrowFromRealName;
    }

    public void ActivateVisualArrow()
    {       
       arrowTowardsPlayerAlive.enabled = true;             
    }

    public void PlayerDied()
    {
        if(arrowTowardsPlayerAlive.enabled == true)
        {
            arrowTowardsPlayerAlive.enabled = false;
            arrowTowardsPlayerDead.enabled = true;
        }      
    } 
}
