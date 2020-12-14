﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestigatorWatchingVisuals : MonoBehaviour {
    public Animator eyeAnimator;
    public Animator borderAnimator;

    public GameManagerAT gameManager;
    private NetworkManagerAT networkManager;

    private void Awake() {
        networkManager = gameManager.networkManagerAT;
    }

    private void OnEnable() {
        eyeAnimator.SetTrigger("InvStartWatching");
        if (gameManager.isLocalPlayer && !gameManager.networkGamePlayerAT.isInvestigator) networkManager.aiTheme.SetParameter("Investigator_Watching", 1);
    }

    private void OnDisable() {
        if (gameManager.isLocalPlayer && !gameManager.networkGamePlayerAT.isInvestigator) networkManager.aiTheme.SetParameter("Investigator_Watching", 0);
    }
}

