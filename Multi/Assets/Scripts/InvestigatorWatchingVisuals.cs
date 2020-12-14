using System.Collections;
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
        if (gameManager.isLocalPlayer && !gameManager.networkGamePlayerAT.isInvestigator) networkManager.aiTheme.SetParameter("Investigator_Watching", 1);
        StartCoroutine(WaitForAnimation());
    }

    private void OnDisable() {
        if (gameManager.isLocalPlayer && !gameManager.networkGamePlayerAT.isInvestigator) networkManager.aiTheme.SetParameter("Investigator_Watching", 0);
    }

    IEnumerator WaitForAnimation() {
        yield return new WaitForSeconds(1f);
        eyeAnimator.SetTrigger("InvStartWatching");
    }
}

