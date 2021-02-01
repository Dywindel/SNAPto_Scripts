using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This script controls the messages displayed by this canvas and uses a simple coroutine
// To make message appear and dissapear.

public class Sc_ES_CanvasMessages : MonoBehaviour
{
    // World Variables
    private Sc_RB_Animation rbA;
    private Sc_AC aC;

    /////////////////////
    // MESSAGE PROMPTS //
    /////////////////////

    // Keep track of which message prompt script is currently active.
    // The active script will check for changes once per player movement action
    [System.NonSerialized]
    public Sc_ES_MessagePrompt ref_Active_MessagePrompt;
    // This bool checks if the message prompts is being displayed
    private bool is_MessagePrompt_Displaying = false;

    ////////////////
    // LEVEL NAME //
    ////////////////

    private bool is_LevelName_Displaying = false;
    // Is another LevelName lined up to appear?
    private bool isStored_LevelName = false;
    // Store its name and type
    private string stored_LevelName = "";
    private bool stored_LevelType = false;

    ///////////////
    //  DIALOGUE //
    ///////////////

    // Keep track of which dialogue prompt script is currently active.
    // The active script will check for changes once per player movement action
    [System.NonSerialized]
    public Sc_ES_DialogueMessages ref_Active_DialogueMessages;
    // This bool checks if the dialogue prompts is being displayed
    private bool is_DialoguePrompt_Displaying = false;
    // This bool helps control when we cycle dialogue
    private bool bool_IgnoreOnce = false;

    // Reference to the main camera
    private Sc_Camera ref_Camera;

    // Player reference
    private Sc_Player player;

    // Reference both text boxes and their respective animators
    public TextMeshProUGUI ref_Text_Message, ref_Text_LevelName, ref_Text_Dialogue, ref_Text_Letters;
    public GameObject ref_GameObject_Letters;
    public Image ref_Image_LevelName_TextBacking, ref_Image_MessagePrompt_TextBacking, ref_Image_DialoguePrompt_TextBacking,
    ref_Image_MessagePrompt_Flourette, ref_Image_LevelType_Soil, ref_Image_LevelType_Bridge, ref_Image_DialoguePrompt_Flourette,
    ref_Image_Letters;

    Animator ref_Anim_Message, ref_Anim_LevelName, ref_Anim_Dialogue, ref_Anim_Letters,
    ref_AnimImage_LevelName_TextBacking, ref_AnimImage_MessagePrompt_TextBacking, ref_AnimImage_MessagePrompt_Flourette, ref_AnimImage_DialoguePrompt_TextBacking, ref_AnimImage_DialoguePrompt_Flourette;
    Animator ref_AnimImage_LevelType_Soil, ref_AnimImage_LevelType_Bridge;

    void Start()
    {
        // Grab world references
        Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
        rbA = gM.rbA;
        aC = gM.aC;
        player = gM.player;

        // For Message Prompts
        ref_Anim_Message = ref_Text_Message.GetComponent<Animator>();
        ref_AnimImage_MessagePrompt_Flourette = ref_Image_MessagePrompt_Flourette.GetComponent<Animator>();
        ref_AnimImage_MessagePrompt_TextBacking = ref_Image_MessagePrompt_TextBacking.GetComponent<Animator>();

        // For Level Names
        ref_Anim_LevelName = ref_Text_LevelName.GetComponent<Animator>();
        ref_AnimImage_LevelName_TextBacking = ref_Image_LevelName_TextBacking.GetComponent<Animator>();

        // For Dialogues
        ref_Anim_Dialogue = ref_Text_Dialogue.GetComponent<Animator>();
        ref_AnimImage_DialoguePrompt_Flourette = ref_Image_DialoguePrompt_Flourette.GetComponent<Animator>();
        ref_AnimImage_DialoguePrompt_TextBacking = ref_Image_DialoguePrompt_TextBacking.GetComponent<Animator>();

        // For Letters
        ref_Anim_Letters = ref_GameObject_Letters.GetComponent<Animator>();

        ref_Camera = GameObject.FindGameObjectWithTag("MainCamera").gameObject.GetComponent<Sc_Camera>();

        // Grab the flouretters too (Title as images)
        ref_AnimImage_LevelType_Soil = ref_Image_LevelType_Soil.GetComponent<Animator>();
        ref_AnimImage_LevelType_Bridge = ref_Image_LevelType_Bridge.GetComponent<Animator>();
    }

    ////////////////////
    // MESSAGE PROMPT //
    ////////////////////

    // This function will now be run through the UpdateManager, whenever the player makes a move
    public void Canvas_DisplayMessage_MessagePrompt()
    {
        // Is there a message prompter that is currently active?
        // We want to check if the player is about to face the direction of the message prompt
        // Or is currently facing that direction
        // Or the object is current active in the scene
        if ((ref_Active_MessagePrompt != null) && (ref_Active_MessagePrompt.obj_FaceDir == player.faceInt)
                                                && (ref_Active_MessagePrompt.gameObject.activeInHierarchy == true))
        {
            if (!is_MessagePrompt_Displaying)
            {
                Canvas_DisplayMessage_MessagePrompt_FadeIn();
            }
        }
        // If the player isn't in the right place, we don't need to do anything... Except
        else
        {
            // If there is currently a messaging being displayed, we need to turn it off
            if (is_MessagePrompt_Displaying)
            {
                Canvas_DisplayMessage_MessagePrompt_FadeOut();
            }
        }
    }

    public void Canvas_DisplayMessage_MessagePrompt_FadeIn()
    {
        // The message is being displayed
        is_MessagePrompt_Displaying = true;

        // Update what the text should display
        ref_Text_Message.text = ref_Active_MessagePrompt.message;

        // Animate the text items to fade in
        ref_Anim_Message.SetBool("FadeIn", true);
        ref_AnimImage_MessagePrompt_TextBacking.SetBool("FadeIn", true);
        ref_AnimImage_MessagePrompt_Flourette.SetBool("FadeIn", true);
    }

    public void Canvas_DisplayMessage_MessagePrompt_FadeOut()
    {
        // The message is no longer being displayed
        is_MessagePrompt_Displaying = false;

        // Animate the textitems to fade out
        ref_Anim_Message.SetBool("FadeIn", false);
        ref_AnimImage_MessagePrompt_TextBacking.SetBool("FadeIn", false);
        ref_AnimImage_MessagePrompt_Flourette.SetBool("FadeIn", false);
    }

    ////////////////
    // LEVEL NAME //
    ////////////////

    // Display text function for the level name text
    public void Canvas_DisplayMessage_LevelName(string p_Message, bool p_LevelType)
    {
        if (!is_LevelName_Displaying)
        {
            StartCoroutine(Canvas_DisplayMessage_LevelName_Coroutine(p_Message, p_LevelType));
        }
        else
        {
            // If another levelname needs to be displayed
            // We store it and come back later
            isStored_LevelName = true;
            stored_LevelName = p_Message;
            stored_LevelType = p_LevelType;
        }
    }

    // Display LevelName Coroutine
    public IEnumerator Canvas_DisplayMessage_LevelName_Coroutine(string p_Message, bool p_LevelType)
    {
        // We are displaying a level name
        is_LevelName_Displaying = true;

        // Update what the text should display
        ref_Text_LevelName.text = p_Message;

        // Animate the texts to fade in
        ref_Anim_LevelName.SetBool("FadeIn", true);
        // And the level type image
        Set_LevelTypeImage_Bool(p_LevelType, true);

        // Wait for fade to end
        yield return new WaitForSeconds(1);

        // Hold while displaying level
        yield return new WaitForSeconds(2);

        // Let the text fade out
        ref_Anim_LevelName.SetBool("FadeIn", false);
        // And the level type image
        Set_LevelTypeImage_Bool(p_LevelType, false);

        // Wait for a moment
        yield return new WaitForSeconds(1);

        // We are no longer displaying a levelname
        is_LevelName_Displaying = false;

        // If we have another level name, lined up, run a second coroutine here
        if (isStored_LevelName)
        {
            StartCoroutine(Canvas_DisplayMessage_LevelName_Coroutine(stored_LevelName, stored_LevelType));
            // Then we reset everything
            stored_LevelName = "";
            stored_LevelType = false;
            isStored_LevelName = false;
        }

        yield return null;
    }

    void Set_LevelTypeImage_Bool(bool p_LevelType, bool pass_Bool)
    {
        // The backing for the text
        ref_AnimImage_LevelName_TextBacking.SetBool("FadeIn", pass_Bool);

        // For soil
        if (!p_LevelType)
        {
            // Set the booleans
            ref_AnimImage_LevelType_Soil.SetBool("FadeIn", pass_Bool);
        }
        // For Bridges
        else
        {
            // Set the booleans
            ref_AnimImage_LevelType_Bridge.SetBool("FadeIn", pass_Bool);
        }
    }

    //////////////
    // DIALOGUE //
    //////////////

    public void Canvas_DisplayMessage_DialoguePrompt()
    {
        // Is dialogue currently active?
        // THIS HAS CHANGED MAYBE??? -- We want to check if the player is about to face the direction of a dialogue prompt
        // Or is currently facing that direction
        // Or is the messenger active in the hierarchy
        if ((ref_Active_DialogueMessages != null) && (ref_Active_DialogueMessages.listOf_DialogueTriggers[ref_Active_DialogueMessages.dialogueTrigger_Active].obj_FaceDir == player.moveInt)
                                                    && (ref_Active_DialogueMessages.gameObject.activeInHierarchy == true))
        // MAYBE -- The player just presses the action button to go to the next dialogue, once it's finished writing out?
        {
            // Filter out the music here?
            aC.FadeMusicTo_LowPassFilter(true);

            if (!is_DialoguePrompt_Displaying)
            {
                Canvas_DisplayMessage_DialoguePrompt_FadeIn();

                // Set the camera position and rotation
                ref_Camera.dialogueCamera_FocusPoint = ref_Active_DialogueMessages.listOf_DialogueTriggers[ref_Active_DialogueMessages.dialogueTrigger_Active].ref_Camera.position;
                // We will always send the reference camera's angle 
                ref_Camera.dialogueCamera_RotationPoint = ref_Active_DialogueMessages.listOf_DialogueTriggers[ref_Active_DialogueMessages.dialogueTrigger_Active].ref_Camera.rotation;
                // Set the camera to move towards the dialoguepoint
                ref_Camera.bool_DialogueCamera = true;
            }
            // If the dialogue is currently being displayed, cycle to the next prompt
            else
            {
                Canvas_DisplayMessage_DialoguePrompt_CycleToNextPrompt();
            }
        }
        // If the player isn't in the right place, we don't need to do anything... Except
        else
        {
            // If there is currently a dialogue being displayed, we need to turn it off
            if (is_DialoguePrompt_Displaying)
            {
                Canvas_DisplayMessage_DialoguePrompt_FadeOut();
            }
        }
    }

    public void Canvas_DisplayMessage_DialoguePrompt_FadeIn()
    {
        // Because we cycle messages at a bad time interval
        bool_IgnoreOnce = true;

        // The message is being displayed
        is_DialoguePrompt_Displaying = true;

        // Update what the text should display
        Scroll_Dialogue_Check();
        //ref_Text_Dialogue.text = ref_Active_DialogueMessages.messageStore[ref_Active_DialogueMessages.paragraphInt][ref_Active_DialogueMessages.messageInt];
        // Update the integer to the next value, cycle the text basically
        //ref_Active_DialogueMessages.CycleToNext_Diagloue();

        // Animate the text items to fade in
        ref_Anim_Dialogue.SetBool("FadeIn", true);
        ref_AnimImage_DialoguePrompt_TextBacking.SetBool("FadeIn", true);
        ref_AnimImage_DialoguePrompt_Flourette.SetBool("FadeIn", true);
    }

    // What occurs when the dialogue message prompt fades out
    public void Canvas_DisplayMessage_DialoguePrompt_FadeOut()
    {
        // Undo the LowPassFilter
        aC.FadeMusicTo_LowPassFilter(false);

        // The message is no longer being displayed
        is_DialoguePrompt_Displaying = false;

        // Animate the text items to fade out
        ref_Anim_Dialogue.SetBool("FadeIn", false);
        ref_AnimImage_DialoguePrompt_TextBacking.SetBool("FadeIn", false);
        ref_AnimImage_DialoguePrompt_Flourette.SetBool("FadeIn", false);

        // Turn off level camera
        ref_Camera.bool_DialogueCamera = false;

        // As the player left the dialogue by themselves, we need to reset it to 0
        ref_Active_DialogueMessages.messageInt = 0;
    }

    // Move to next piece of dialogue
    public void Canvas_DisplayMessage_DialoguePrompt_CycleToNextPrompt()
    {
        // If no text is currently scrolling and the coroutine is empty
        if (!rbA.bool_isScrolling)
        {
            // Show new message, as long as the message iteration is above zero and isn't close to its limit
            if (ref_Active_DialogueMessages.messageInt >= 0 && ref_Active_DialogueMessages.messageInt < ref_Active_DialogueMessages.messageStore[ref_Active_DialogueMessages.paragraphInt].Length - 1)
            {
                Scroll_Dialogue_Check();
                //ref_Text_Dialogue.text = ref_Active_DialogueMessages.messageStore[ref_Active_DialogueMessages.paragraphInt][ref_Active_DialogueMessages.messageInt];
                // Update integer
                //ref_Active_DialogueMessages.CycleToNext_Diagloue();
            }
            else
            {
                // If we're on the last message, cycle the massage
                // THIS WORKS BUT ONLY BECAUSE ITS BEING HELD TOGETHER BY DUCK TAPE
                // THAT LITTLE <= IS DOING SO MUCH WORK RIGHT NOW, ITS NOT A CODING ERROR, DO NOT DELETE
                if (ref_Active_DialogueMessages.messageInt <= ref_Active_DialogueMessages.messageStore[ref_Active_DialogueMessages.paragraphInt].Length - 1)
                    ref_Active_DialogueMessages.CycleToNext_Diagloue();
                // FadeOut the dialogue
                Canvas_DisplayMessage_DialoguePrompt_FadeOut();
            }
        }
        // However, if the text is currently scrolling, then we stop the coroutine and display the text
        else
        {
            Scroll_Dialogue_Check();
        }
    }

    // Scroll Dialogue Check
    public void Scroll_Dialogue_Check()
    {
        // If no text is currently scrolling and the coroutine is empty
        if (!rbA.bool_isScrolling)
        {
            // As long as this isn't the first, or last piece of dialogue
            if (!bool_IgnoreOnce)
                ref_Active_DialogueMessages.CycleToNext_Diagloue();
            else
                bool_IgnoreOnce = false;
            // Run the coroutine and store it
            rbA.ref_Coroutine = StartCoroutine(rbA.Text_TypeIn(ref_Text_Dialogue, ref_Active_DialogueMessages.messageStore[ref_Active_DialogueMessages.paragraphInt][ref_Active_DialogueMessages.messageInt]));
        }
        else
        {
            // Stop the coroutine;
            StopCoroutine(rbA.ref_Coroutine);
            // Set the isScrolling boolean to false
            rbA.bool_isScrolling = false;
            // Update the message box with the current display text
            ref_Text_Dialogue.text = ref_Active_DialogueMessages.messageStore[ref_Active_DialogueMessages.paragraphInt][ref_Active_DialogueMessages.messageInt];
        }
    }

    // Open and closing letters
    public void Opening_Letters(string pass_Text, Sprite pass_Sprite)
    {
        // Update the background image
        ref_Image_Letters.sprite = pass_Sprite;
        // Update the text
        ref_Text_Letters.text = pass_Text;

        // Play SFX
        aC.Play_SFX("SFX_Letter_Open");

        // Run animation
        ref_Anim_Letters.SetBool("isReading", true);

    }

    public void Closing_Letters()
    {
        // Play SFX
        aC.Play_SFX("SFX_Letter_Close");

        // End animation
        ref_Anim_Letters.SetBool("isReading", false);

    }
}
