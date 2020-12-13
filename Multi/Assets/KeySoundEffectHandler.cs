using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using FMODUnity;

public class KeySoundEffectHandler : NetworkBehaviour
{
    [Header("Key Sounds")]
    protected List<KeyCode> m_activeInputs = new List<KeyCode>();

    NetworkManagerAT networkManagerAT;
    NetworkGamePlayerAT NetworkGamePlayerAT;


    public bool isTyping = false;

    public StudioEventEmitter normalKeyDownSound;
    public StudioEventEmitter normalKeyUpSound;
    public StudioEventEmitter spaceDownSound;
    public StudioEventEmitter spaceUpSound;
    public StudioEventEmitter enterDownSound;
    public StudioEventEmitter enterUpSound;
    public StudioEventEmitter escapeDownSound;
    public StudioEventEmitter escapeUpSound;

    
    private void Update()
    {
        List<KeyCode> pressedInput = new List<KeyCode>();

        //Sound bei Down Spielen
        if (Input.anyKeyDown)
        {
            foreach (KeyCode code in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(code))
                {
                    if (code == KeyCode.Space)
                    {
                        isTyping = true;
                        //Play Space Key Down
                        // Debug.Log("Space down ");
                        spaceDownSound.Play();
                        isTyping = true;
                    }
                    else if (code == KeyCode.Return)
                    {
                        //Play Enter Key Down
                     //   Debug.Log("Enter down ");
                        enterDownSound.Play();
                       
                    }
                    else if (code == KeyCode.Mouse0 || code == KeyCode.Mouse1 || code == KeyCode.Mouse2)
                    {
                        //Play Enter Key Down
                    //    Debug.Log("Mouse down ");
                    }
                    else if (code == KeyCode.Escape)
                    {
                        escapeDownSound.Play();
                        isTyping = true;
                    }
                    else if (Input.GetKey(code))
                    {
                        //Play Normal Key Down
                    //    Debug.Log(code + " Key Down");
                        normalKeyDownSound.Play();
                        isTyping = true;
                    }
                }
            }
        }


        //Um Daten für reales zu Speichern
        if (Input.anyKey)
        {
            foreach (KeyCode code in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(code))
                {

                    m_activeInputs.Remove(code);
                    m_activeInputs.Add(code);
                    pressedInput.Add(code);

                    //Debug.Log(code + " was pressed");
                    break;
                }
            }
        }


        //Sound bei Up Spielen

        List<KeyCode> releasedInput = new List<KeyCode>();

        foreach (KeyCode code in m_activeInputs)
        {
            releasedInput.Add(code);

            if (!pressedInput.Contains(code))
            {
                releasedInput.Remove(code);
              
                    if (code == KeyCode.Space)
                    {
                        //Play Space Key Down
                      //  Debug.Log("Space Up ");
                        spaceUpSound.Play();
                    }
                    else if (code == KeyCode.Return)
                    {
                        //Play Enter Key Down
                      //  Debug.Log("Enter UP ");
                        enterUpSound.Play();
                    }
                    else if (code == KeyCode.Mouse0 || code == KeyCode.Mouse1 || code == KeyCode.Mouse2)
                    {
                        //Play Enter Key Down
                     //   Debug.Log("Mouse UP ");
                    }
                    else if (code == KeyCode.Escape)
                    {
                        escapeUpSound.Play();
                    }
                    else
                    {
                        //Play Normal Key Down
                   //     Debug.Log(code + " Key UP");
                        normalKeyUpSound.Play();
                    }
                
                //Debug.Log(code + " was released");
            }
        }

        m_activeInputs = releasedInput;

        if(isTyping == true)
        {
            StartCoroutine(TypingCoolDown(1.5f));
        }
    }


    IEnumerator TypingCoolDown(float coolDown)
    {
       
        yield return new WaitForSeconds(coolDown);
        isTyping = false;
    }



}
