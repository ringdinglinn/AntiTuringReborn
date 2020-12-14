using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YouAreDeadScreen : MonoBehaviour
{
    public NetworkGamePlayerAT networkPlayer;
    NetworkManagerAT networkManager;

    private void OnEnable() {
        if (networkManager == null) networkManager = networkPlayer.Room;
        networkManager.youDied.Play();
    }
}
