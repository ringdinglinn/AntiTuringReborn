using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatroomStates : MonoBehaviour
{
  
        public int id;
        public bool leftFree;
        public bool rightFree;
        public string leftName;
        public string rightName;
    
    public ChatroomStates(int id, bool leftFree, bool rightFree, string leftName, string rightName)
    {     
        this.id = id;
        this.leftFree = leftFree;
        this.rightFree = rightFree;
        this.leftName = leftName;
        this.rightName = rightName;
    }
}
