using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveViewTagPanel : MonoBehaviour
{
    public GameObject tagPanel;
    float defaultX;
    float defaultY;

    private void GetDefaultSize() {
        defaultX = tagPanel.GetComponent<RectTransform>().sizeDelta.x;
        defaultY = tagPanel.GetComponent<RectTransform>().sizeDelta.y;
    }

    public void OpenTagPanel(float xSize, float ySize)
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
