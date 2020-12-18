using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameMenu : MonoBehaviour
{

    public GameObject inGameMenuObj;
    private bool isOpen = false;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(isOpen == false)
            {
                OpenInGameMenu();
            }
            else
            {
                CloseInGameMenu();
            }
        }
    }




    public void OpenInGameMenu()
    {
        isOpen = true;
        inGameMenuObj.SetActive(true);
    }

    public void CloseInGameMenu()
    {
        isOpen = false;
        inGameMenuObj.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
