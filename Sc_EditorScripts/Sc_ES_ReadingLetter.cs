using System.Collections;
using System.Collections.Generic;
using UnityEngine;

////////////////////////////////////////////////////////////////////
//
//  Sc_ES_ReadingLetter - Letter Reading Narrative script
//  Makes a letter appear on screen while the player is inside the
//  box collider that gives some narrative drive to the story

public class Sc_ES_ReadingLetter : MonoBehaviour
{
    [TextArea(20, 40)]
    public string ref_Message;
    public Sprite ref_Sprite;
    private Sc_ES_CanvasMessages ref_CanvasMessages;

    void Start()
    {
        ref_CanvasMessages = GameObject.FindGameObjectWithTag("Canvas_Messages").GetComponent<Sc_ES_CanvasMessages>();
    }

    // Read the letter
    public void Open_Letter()
    {
        ref_CanvasMessages.Opening_Letters(ref_Message, ref_Sprite);
    }

    // Stop reading the letter
    public void Close_Letter()
    {
        ref_CanvasMessages.Closing_Letters();
    }
}
