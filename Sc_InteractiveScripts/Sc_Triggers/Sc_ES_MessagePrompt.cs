using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

////////////////////////////////////////////////
//
//  Sc_ES_Cutscene_DiplayText
//  Triggers some text to appear after the player
//  is inside the bounding box and facing the 
//  right direction

public class Sc_ES_MessagePrompt : MonoBehaviour
{
    //References to World Objects
	protected Sc_RB_Animation rbA;		//Reference to the animation list
    private Coroutine ref_Coroutine;

    // Canvas of message
    private Sc_ES_CanvasMessages ref_CanvasMessages;

    private bool isDoOnce = false;

    //The text string to be read
    [TextArea]
    public string message;
    //Reference to the canvas and text box
    GameObject ref_NarrativeText;
    TextMeshProUGUI ref_Text;

    //A reference to the player object
    Sc_Player temp_Play;

    //Boolean to confirm the player is here
    private bool playerHere = false;

    //Which direction should the player be facing?
    public int obj_FaceDir = 0;
    
    //Mesh Renderer reference to switch off the image
    private MeshRenderer ref_Mesh;

    //Booleans to indicate when the text is off and on
    [System.NonSerialized]
    private bool bool_isBeingRead = false;
    [System.NonSerialized]
    private bool bool_isFacingDirection = false;

    void Start()
    {
        //Grab the Game Manager, dictionary, movement library...
		Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
		rbA = gM.rbA;

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
                ref_CanvasMessages.ref_Active_MessagePrompt = this;

                // There is a situation, where, when undoing certain moves, text will not be displayed
                // I can resolve this by running the function once, here
                ref_CanvasMessages.Canvas_DisplayMessage_MessagePrompt();
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
                if (ref_CanvasMessages.ref_Active_MessagePrompt == this)
                {
                    ref_CanvasMessages.ref_Active_MessagePrompt = null;
                }

                // There is a situation, where, when undoing certain moves, text will not fade away
                // I can resolve this by running the function once, here
                ref_CanvasMessages.Canvas_DisplayMessage_MessagePrompt();
            }
        }
    }
}
