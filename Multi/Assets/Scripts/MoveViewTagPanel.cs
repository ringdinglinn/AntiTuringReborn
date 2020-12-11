using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveViewTagPanel : MonoBehaviour
{
    public GameObject tagPanel;
    public GameObject tagPanelParent;
    float defaultX;
    float defaultY;

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
                x.transform.parent = tagPanelParent.transform;
                x.GetComponent<RectTransform>().anchoredPosition = new Vector3(77f + ( targetXPosition * 0), targetYPosition - 10, 0);
                targetYPosition -= x.GetComponent<RectTransform>().rect.height;
                x.gameObject.SetActive(true);
                x.transform.parent = tagPanel.transform.parent;
            }
            foreach (PlayerTagPanelHandler x in botsTagPanelHandlerList)
            {
                x.transform.parent = tagPanelParent.transform;
                x.GetComponent<RectTransform>().anchoredPosition = new Vector3(targetXPosition * 0, targetYPosition - 10, 0);
                targetYPosition -= x.GetComponent<RectTransform>().rect.height;
                x.gameObject.SetActive(true);
                x.transform.parent = tagPanel.transform.parent;
            }
        }
        else
        {
            int slotNr = 0;
            foreach (PlayerTagPanelHandler x in playerTagPanelHandlerList)
            {
                x.transform.parent = tagPanelParent.transform;

                if (slotNr % 2 == 0)//is Even Nr
                {
                    x.GetComponent<RectTransform>().anchoredPosition = new Vector3(77f + targetXPosition, targetYPosition , 0);
                    x.gameObject.SetActive(true);
                    x.transform.parent = tagPanel.transform.parent;
                }
                else // is not even Nr
                {
                    x.GetComponent<RectTransform>().anchoredPosition = new Vector3(77f + (targetXPosition * 0), targetYPosition , 0);
                    targetYPosition -= x.GetComponent<RectTransform>().rect.height;
                    x.gameObject.SetActive(true);
                    x.transform.parent = tagPanel.transform.parent;
                }

                slotNr++;
            }
            foreach (PlayerTagPanelHandler x in botsTagPanelHandlerList)
            {
                x.transform.parent = tagPanel.transform;

                if (slotNr % 2 == 0)//is Even Nr
                {
                    x.GetComponent<RectTransform>().anchoredPosition = new Vector3(targetXPosition, targetYPosition - 10, 0);
                    x.gameObject.SetActive(true);
                    x.transform.parent = tagPanel.transform.parent;
                }
                else // is not even Nr
                {
                    x.GetComponent<RectTransform>().anchoredPosition = new Vector3(targetXPosition * 0, targetYPosition - 10, 0);
                    targetYPosition -= x.GetComponent<RectTransform>().rect.height;
                    x.gameObject.SetActive(true);
                    x.transform.parent = tagPanel.transform.parent;
                }
                slotNr++;
            }
        }
        CloseTagPanel();
    }

    public void OpenTagPanel(float xSize, float ySize,int totalNrOfPanelSlots, List<PlayerTagPanelHandler> playerTagPanelHandlerList, List<PlayerTagPanelHandler>  botsTagPanelHandlerList )
    {
        tagPanel.SetActive(true);
        GetDefaultSize();
        StartCoroutine(OpenTagPanel(tagPanel.GetComponent<RectTransform>(), xSize, ySize, 50));

     
      
    }

  


    public void CloseTagPanel()
    {
        StartCoroutine(CloseTagPanel(tagPanel.GetComponent<RectTransform>(), 50));
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

    IEnumerator CloseTagPanel(RectTransform rect, float steps) {
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
