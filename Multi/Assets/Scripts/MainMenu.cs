using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour {
    [SerializeField] private NetworkManagerAT networkManager;

    [Header("UI")]
    [SerializeField] private GameObject nameInput;
    [SerializeField] private GameObject landingPagePanel;

    [Header("Sound")]
    [SerializeField] private GameObject soundFolder;


    public void HostLobby() {
        networkManager.StartHost();
        landingPagePanel.SetActive(false);
    }

    public void StartNewServer() {
        nameInput.SetActive(false);
        networkManager.StartServer();
        landingPagePanel.SetActive(false);
        networkManager.isSeverOnly = true;
    }

    private void Start() {
        DontDestroyOnLoad(soundFolder);
    }

    private void Update() {
        if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.LeftShift) && (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl))){
            StartNewServer();
        }
    }
}
