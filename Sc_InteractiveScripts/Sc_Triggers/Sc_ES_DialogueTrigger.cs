using System.Collections;
using System.Collections.Generic;
using UnityEngine;

////////////////////////////////////////////////
//
//  Sc_ES_DialogueTrigger
//  Triggers dialogue loop when player meets
//  Correct facing conditions

public class Sc_ES_DialogueTrigger : MonoBehaviour
{

    // Canvas of message
    private Sc_ES_CanvasMessages ref_CanvasMessages;

    // Parent DialoguePrompts script
    [HideInInspector]
    public Sc_ES_DialogueMessages ref_Sc_DialogueMessages;
    // Trigger identity number
    [HideInInspector]
    public int dialgoueTrigger_Identity = 0;

    // Player face direction
    public int obj_FaceDir = 0;
    // Reference camera
    public Transform ref_Camera;

    void Start()
    {
        // Grab a reference to the message canvas script, if it exists
        if (GameObject.FindGameObjectWithTag("Canvas_Messages") != null)
            ref_CanvasMessages = GameObject.FindGameObjectWithTag("Canvas_Messages").GetComponent<Sc_ES_CanvasMessages>();
    }

    // When the player enters the trigger box, we send this script to the active
    // Message prompt in the canvas script
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (ref_CanvasMessages != null)
            {
                // Also, send the parent dialogueMessages your identity number?
                ref_Sc_DialogueMessages.dialogueTrigger_Active = dialgoueTrigger_Identity;

                ref_CanvasMessages.ref_Active_DialogueMessages = ref_Sc_DialogueMessages;

                // There is a situation, where, when undoing certain moves, text will not be displayed
                // I can resolve this by running the function once, here
                ref_CanvasMessages.Canvas_DisplayMessage_DialoguePrompt();
            }
        }
    }

    // Likeways, when the player leaves, we remove the active message prompt
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            if (ref_CanvasMessages != null)
            {
                // Make sure, when deactivating the script, the one we deactivate is this one and nothing else
                if (ref_CanvasMessages.ref_Active_DialogueMessages == ref_Sc_DialogueMessages)
                {
                    ref_CanvasMessages.ref_Active_DialogueMessages = null;
                }

                // There is a situation, where, when undoing certain moves, text will not fade away
                // I can resolve this by running the function once, here
                ref_CanvasMessages.Canvas_DisplayMessage_DialoguePrompt();
            }
        }
    }
}
