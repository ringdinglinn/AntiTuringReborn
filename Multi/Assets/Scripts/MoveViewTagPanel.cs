using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveViewTagPanel : MonoBehaviour
{
    public void OpenTagPanel()
    {
        LeanTween.move(GetComponent<RectTransform>(), new Vector2(-700, 0), 1f).setEase(LeanTweenType.easeOutExpo);
    }

    public void CloseTagPanel()
    {
        LeanTween.move(GetComponent<RectTransform>(), new Vector2(0, 0), 1f).setEase(LeanTweenType.easeOutExpo);
    }
}
