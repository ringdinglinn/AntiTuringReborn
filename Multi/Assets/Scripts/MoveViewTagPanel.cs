using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class MoveViewTagPanel : MonoBehaviour
{
    public GameObject tagPanel;
    public GameObject tagPanelParent;
    float defaultX;
    float defaultY;


    private List<PlayerTagPanelHandler> tagHandlerListRightSide = new List<PlayerTagPanelHandler>();
    private List<PlayerTagPanelHandler> tagHandlerListLeftSid1 = new List<PlayerTagPanelHandler>();

    public StudioEventEmitter toggleSound;

    private void GetDefaultSize() {
        defaultX = tagPanel.GetComponent<RectTransform>().sizeDelta.x;
        defaultY = tagPanel.GetComponent<RectTransform>().sizeDelta.y;
    }


    public void SetTagPanelPositions(int totalNrOfPanelSlots, List<PlayerTagPanelHandler> playerTagPanelHandlerList, List<PlayerTagPanelHandler> botsTagPanelHandlerList)
    {
        float targetXPosition = playerTagPanelHandlerList[0].GetComponent<RectTransform>().rect.width;
        float targetYPosition = -101;
    
        if (totalNrOfPanelSlots <= 11)
        {
            foreach (PlayerTagPanelHandler x in playerTagPanelHandlerList)
            {             
                x.GetComponent<RectTransform>().anchoredPosition = new Vector3(374 + ( targetXPosition * 0), targetYPosition - 5, 0);
                targetYPosition -= x.GetComponent<RectTransform>().rect.height;
              
                tagHandlerListLeftSid1.Add(x);
            }
            foreach (PlayerTagPanelHandler x in botsTagPanelHandlerList)
            {
               
                x.GetComponent<RectTransform>().anchoredPosition = new Vector3(374 + (targetXPosition * 0), targetYPosition - 5, 0);
                targetYPosition -= x.GetComponent<RectTransform>().rect.height;
            
                tagHandlerListLeftSid1.Add(x);
            }
        }
        else
        {
            int slotNr = 0;
            foreach (PlayerTagPanelHandler x in playerTagPanelHandlerList)
            {               
                if (slotNr % 2 == 0)//is Even Nr
                {
                    x.GetComponent<RectTransform>().anchoredPosition = new Vector3(77f + targetXPosition, targetYPosition , 0);                 
                    tagHandlerListRightSide.Add(x);
                }
                else // is not even Nr
                {
                    x.GetComponent<RectTransform>().anchoredPosition = new Vector3(77f + (targetXPosition * 0), targetYPosition , 0);
                    targetYPosition -= x.GetComponent<RectTransform>().rect.height;              
                    tagHandlerListLeftSid1.Add(x);
                }

                slotNr++;
            }
            foreach (PlayerTagPanelHandler x in botsTagPanelHandlerList)
            {      
                if (slotNr % 2 == 0)//is Even Nr
                {
                    x.GetComponent<RectTransform>().anchoredPosition = new Vector3(77f + targetXPosition, targetYPosition, 0);                 
                    tagHandlerListRightSide.Add(x);
                }
                else // is not even Nr
                {
                    x.GetComponent<RectTransform>().anchoredPosition = new Vector3(77f + (targetXPosition * 0), targetYPosition, 0);
                    targetYPosition -= x.GetComponent<RectTransform>().rect.height;                  
                    tagHandlerListLeftSid1.Add(x);

                }
                slotNr++;
            }
        }
      
    }

    public void OpenTagPanel(float xSize, float ySize,int totalNrOfPanelSlots, List<PlayerTagPanelHandler> playerTagPanelHandlerList, List<PlayerTagPanelHandler>  botsTagPanelHandlerList )
    {
        tagPanel.SetActive(true);
        GetDefaultSize();
        StartCoroutine(OpenTagPanel(tagPanel.GetComponent<RectTransform>(), xSize, ySize, 50));


        StartCoroutine(ToggleTagHandlerVisability(0.2f, 0.05f,tagHandlerListLeftSid1, true));
        StartCoroutine(ToggleTagHandlerVisability(0.2f, 0.05f, tagHandlerListRightSide, true));
    }

  


    public void CloseTagPanel()
    {
        StartCoroutine(ToggleTagHandlerVisability(0.001f, 0.05f, tagHandlerListLeftSid1, false));
        StartCoroutine(ToggleTagHandlerVisability(0.002f, 0.05f, tagHandlerListRightSide, false));
        StartCoroutine(CloseTagPanel(tagPanel.GetComponent<RectTransform>(), 50,0.4f));
    
    }

    IEnumerator OpenTagPanel(RectTransform rect, float xSize, float ySize, float steps) {
      
        float deltaX = xSize - defaultX;
        float deltaY = ySize - defaultY;
        float factorX = deltaX / Mathf.Log(steps);
        float factorY = deltaY / Mathf.Log(steps);
        for (int i = 0; i < steps; i++) {
            rect.sizeDelta = new Vector2(defaultX + Mathf.Log(i) * factorX, defaultY + Mathf.Log(i) * factorY);
            yield return null;
        }
    }

    IEnumerator ToggleTagHandlerVisability(float waitTime, float toggleSpeed, List<PlayerTagPanelHandler> toggleList, bool toggleStatus)
    {
        yield return new WaitForSeconds(waitTime);

        if (toggleStatus == true) //turn the handlers on
        {


            if (toggleList.Count > 0)
            {
                foreach (PlayerTagPanelHandler x in toggleList)
                {
                    toggleSound.Play();
                    x.transform.gameObject.SetActive(true);
                    yield return new WaitForSeconds(toggleSpeed);
                }

            }
        }
        else
        {
            if (toggleList.Count > 0)
            {
                int x = toggleList.Count;

                while (x != 0)
                {
                    x--;
                    toggleSound.Play();
                    toggleList[x].transform.gameObject.SetActive(false);
                    yield return new WaitForSeconds(toggleSpeed);

                }
            }

        }





    }
    


    IEnumerator CloseTagPanel(RectTransform rect, float steps, float delay) {
        yield return new WaitForSeconds(delay);
        float initialX = rect.sizeDelta.x;
        float initialY = rect.sizeDelta.y;
        float deltaX = initialX - defaultX;
        float deltaY = initialY - defaultY;
        float factorX = deltaX / Mathf.Log(steps);
        float factorY = deltaY / Mathf.Log(steps);
        for (int i = 0; i < steps; i++) {
            rect.sizeDelta = new Vector2(initialX - Mathf.Log(i) * factorX, initialY - Mathf.Log(i) * factorY);
            yield return null;
        }
        rect.gameObject.SetActive(false);
    }
}
