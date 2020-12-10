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
        //LeanTween.move(GetComponent<RectTransform>(), new Vector2(-700, 0), 1f).setEase(LeanTweenType.easeOutExpo);
        tagPanel.SetActive(true);
        GetDefaultSize();
        //LeanTween.scale(tagPanel.GetComponent<RectTransform>(), new Vector3(xSize, ySize, 1), 1f);
        StartCoroutine(OpenTagPanel(tagPanel.GetComponent<RectTransform>(), xSize, ySize, 50));
    }

    public void CloseTagPanel()
    {
        //LeanTween.move(GetComponent<RectTransform>(), new Vector2(0, 0), 1f).setEase(LeanTweenType.easeOutExpo);
        //LeanTween.scale(tagPanel.GetComponent<RectTransform>(), new Vector3(defaultX, defaultY, 1), 1f);
        //tagPanel.SetActive(false);
        StartCoroutine(CloseTagPanel(tagPanel.GetComponent<RectTransform>(), 50));
    }

    IEnumerator OpenTagPanel(RectTransform rect, float xSize, float ySize, float steps) {
        float deltaX = xSize - defaultX;
        deltaX /= steps;
        float deltaY = ySize - defaultY;
        deltaY /= steps;
        for (int i = 0; i < steps; i++) {
            rect.sizeDelta = new Vector2(rect.sizeDelta.x + deltaX, rect.sizeDelta.y + deltaY);
            yield return null;
        }
    }

    IEnumerator CloseTagPanel(RectTransform rect, float steps) {
        float deltaX = rect.sizeDelta.x - defaultX;
        deltaX /= steps;
        float deltaY = rect.sizeDelta.y - defaultY;
        deltaY /= steps;
        for (int i = 0; i < steps; i++) {
            rect.sizeDelta = new Vector2(rect.sizeDelta.x - deltaX, rect.sizeDelta.y - deltaY);
            yield return null;
        }
        rect.gameObject.SetActive(false);
    }
}
