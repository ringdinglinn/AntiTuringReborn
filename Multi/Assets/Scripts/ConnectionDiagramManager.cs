﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ConnectionDiagramManager : NetworkBehaviour
{
    [Header ("References")]
    [SerializeField] private List<GameObject> connectionDiagramsList = new List<GameObject>();


    public TagManagement tagManagement;
    public GameManagerAT gameManagerAT;

    public List<ConnectionDiagramPlayerRepresentation> allPlayerRepresentationList = new List<ConnectionDiagramPlayerRepresentation>();
    public List<ConnectionDiagramLineHandler> allArrownInDiagramList = new List<ConnectionDiagramLineHandler>();

    #region Start Setup
    public void StartSetup( )
    {
        StartCoroutine(ShortSetupDelay());
    }
    IEnumerator ShortSetupDelay()
    {
        yield return new WaitForSeconds(3f);        
        CmdStartSetup();      
    }

    [Command]
    private void CmdStartSetup( )
    {         
        RpcStartSetup();
    }
    [ClientRpc]
    private void RpcStartSetup()
    {
        ActivateConnectionDiagramBasedOnNrOfPlayer();
    }
    public void ActivateConnectionDiagramBasedOnNrOfPlayer()
    {
        connectionDiagramsList[tagManagement.allTagableHumanPlayersList.Count-1].SetActive(true);
        allPlayerRepresentationList = connectionDiagramsList[tagManagement.allTagableHumanPlayersList.Count - 1].GetComponent<ConnectionDiagramInformationHolder>().allPlayerRepresentationList;
        allArrownInDiagramList = connectionDiagramsList[tagManagement.allTagableHumanPlayersList.Count - 1].GetComponent<ConnectionDiagramInformationHolder>().allArrownInDiagramList;

        AssignPlayerToDiagramRepresentation();
    }

    private void  AssignPlayerToDiagramRepresentation()
    {      
        List<NetworkGamePlayerAT> randomlyTagableHumanPlayersList = new List<NetworkGamePlayerAT>();     
        randomlyTagableHumanPlayersList.AddRange(tagManagement.allTagableHumanPlayersList);    
        foreach  (ConnectionDiagramPlayerRepresentation x in allPlayerRepresentationList)
        {         
            x.SetRealPlayerName(randomlyTagableHumanPlayersList[0].realName);
            randomlyTagableHumanPlayersList.RemoveAt(0);
        }
    }
    #endregion


    #region Handle Game Manager Requests
    public void HandleNewConnection(string foundPlayerRealName, string playerWhoTagedRealNamen)
    {
        Debug.Log("Arrow Request Came In");

        Debug.Log("foundPlayerRealName " + foundPlayerRealName);
        Debug.Log("playerWhoTagedRealNamen " + playerWhoTagedRealNamen);
        foreach (ConnectionDiagramLineHandler x in allArrownInDiagramList)
        {
            if(x.GetPlayerArrowFromRealName()== playerWhoTagedRealNamen &&  x.GetPlayerArrowTowardsRealName()== foundPlayerRealName)
            {
                Debug.Log("Activate Arrow Alive");
                x.ActivateVisualArrow();
            }
        }
    }
    public void HandlePlayerDied(string deadPlayer)
    {
        foreach (ConnectionDiagramLineHandler x in allArrownInDiagramList)
        {
            if (x.GetPlayerArrowFromRealName() == deadPlayer || x.GetPlayerArrowTowardsRealName() == deadPlayer)
            {
                Debug.Log("Activate Arrow Dead");
                x.PlayerDied();
            }
        }
        foreach (ConnectionDiagramPlayerRepresentation x in allPlayerRepresentationList)
        {
            if (x.GetRealPlayerName() == deadPlayer)
            {
                x.ChangeVisualRepresentationToDead();
            }
        }
    }

    #endregion
}

