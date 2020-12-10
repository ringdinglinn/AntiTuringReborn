using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FMODUnity;

public class KeySoundEffectHandler : MonoBehaviour
{
    [Header("Key Sounds")]
    protected List<KeyCode> m_activeInputs = new List<KeyCode>();



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
                        //Play Space Key Down
                       // Debug.Log("Space down ");
                        spaceDownSound.Play();
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
                    }
                    else if (Input.GetKey(code))
                    {
                        //Play Normal Key Down
                    //    Debug.Log(code + " Key Down");
                        normalKeyDownSound.Play();
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
    }
}
