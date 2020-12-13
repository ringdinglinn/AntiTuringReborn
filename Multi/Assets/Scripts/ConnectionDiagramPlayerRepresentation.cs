using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ConnectionDiagramPlayerRepresentation : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private Image visualRepresentationAlive;
    [SerializeField] private Image visualRepresentationDead;
    [SerializeField] private TextMeshProUGUI playerRealName;


    [Header("ListOfArrownPointingTowardsThePlayer")]
    public List<ConnectionDiagramLineHandler> arrowsTowardsThePlayer = new List<ConnectionDiagramLineHandler>();
    public List<ConnectionDiagramLineHandler> arrowsAwayFromThePlayer = new List<ConnectionDiagramLineHandler>();

    public void ChangeVisualRepresentationToDead()
    {
        visualRepresentationAlive.enabled = false;
        visualRepresentationDead.enabled = true;

      
    }
  
    public string GetRealPlayerName()
    {
        return playerRealName.text;

       
    }
    public void SetRealPlayerName(string newRealPlayerName)
    {
        playerRealName.text = newRealPlayerName;

        foreach (ConnectionDiagramLineHandler x in arrowsTowardsThePlayer)
        {
            x.SetPlayerArrowTowardsRealName(newRealPlayerName);
        }
        foreach (ConnectionDiagramLineHandler x in arrowsAwayFromThePlayer)
        {
            x.SetPlayerArrowFromRealName(newRealPlayerName);
        }
        
    }
}
