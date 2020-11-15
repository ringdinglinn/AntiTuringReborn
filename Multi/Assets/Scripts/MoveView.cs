using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveView : MonoBehaviour
{
    void Start()
    {
    }

    public void MoveViewRight() {
        LeanTween.move(GetComponent<RectTransform>(), new Vector2(700, 0), 1f).setEase(LeanTweenType.easeOutExpo);
    }

    public void MoveViewLeft() {
        LeanTween.move(GetComponent<RectTransform>(), new Vector2(0, 0), 1f).setEase(LeanTweenType.easeOutExpo);
    }
}
