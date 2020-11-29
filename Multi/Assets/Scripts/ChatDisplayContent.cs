using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChatDisplayContent : MonoBehaviour
{
    public int numberOfInvestigatorsInRoom = 0;

    public TMP_Text leftName;
    public TMP_Text rightName;
    public Image leftPerson;
    public Image rightPerson;
    public Button joinButton;
    public GameObject scrollPanelContent;
    public GameObject inputField;
    public GameObject investigatorVisual;
}
