using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

//////////////////////////////////////////////////
//  Sc_Menu_Action_SaveLoad
//  This menu action is a simple button that also
//  Highlights player progress when selecting a save

public class Sc_Menu_Action_SaveLoad : Sc_Menu_Action_Par
{
    public bool isVertical = true;
    private bool isNewGame = true;

    public UnityEvent takeAction;

    public Animator anim2;

    public TextMeshProUGUI ref_MainText;

    public TextMeshProUGUI ref_Delete;
    public GameObject ref_DeleteButton;
    public GameObject ref_DeteleText;

    // For indexing options
    private int start_Index = 0;
    private int index;
    private int maxIndex = 1;
    private int storeIndex = 0;

    bool keyDown;

    // This section is for the AreYouSure command when deleting a save
    // This will be just a timed button hold
    private bool areYouSure_Timer_Active = false;
    private float areYouSure_Timer_End = 4.0f;
    private float areYouSure_Timer = 0.0f;

    // Check if the save file exists
    public override void Pass_Start()
    {
        Update_SaveLoad_Banner();
    }

    public void Update_SaveLoad_Banner()
    {
        isNewGame = !Persistance.CheckIf_SaveExists(thisIndex);

        // Check if a save file exists
        if (!isNewGame)
        {
            ref_MainText.text = Persistance.Return_PlayerPrefs(thisIndex);
            // Activate the delete button
            ref_DeleteButton.SetActive(true);
            ref_DeteleText.SetActive(true);
        }
        else
        {
            // Just show 'New Game'
            ref_MainText.text = "New Game";
            // Disable the delete button
            ref_DeleteButton.SetActive(false);
            ref_DeteleText.SetActive(false);
        }
    }

    // Reset the initial index value
    public override void Reset_State()
    {
        index = start_Index;
        storeIndex = index;
    }

    // We're overriding the parent button as this script behaves a little differenly
    public override void pass_Update()
    {
        if (menuController.index == thisIndex) {
            // The animation we set to selected actually depends on which part of the banner is selected
            if (index == 0)
            {
                anim.SetBool ("Selected", true);
                anim2.SetBool ("Selected", false);
            }
            else
            {
                anim.SetBool ("Selected", false);
                anim2.SetBool ("Selected", true);
            }
            
            Pass_Input();
        } 
        else
        {
            // Turn off all animators
            anim.SetBool ("Selected", false);
            anim2.SetBool ("Selected", false);
        }
    }

    public override void Pass_Input() 
    {
        // This selection is only possible if we're not starting a New Game
        if (!isNewGame)
        {
            // Using the left and right values of the D-Pad (Or up and down for vertical menu)
            // The player can cycle through a list of items
            if (isVertical)
            {
                if (Input.GetAxis ("Vertical") != 0) {
                    //We fire the menu selection once, then set keyDown to true so the menu doesn't change any more
                    if (!keyDown) {
                        if (Input.GetAxis ("Vertical") < 0) {
                            if (index < maxIndex) {
                                index++;
                            }
                        } else if (Input.GetAxis ("Vertical") > 0) {
                            if (index > 0) {
                                index --;
                            }
                        }
                        keyDown = true;
                    }
                } else {
                    keyDown = false;
                }
            }
            else
            {
                if (Input.GetAxis ("Horizontal") != 0) {
                    //We fire the menu selection once, then set keyDown to true so the menu doesn't change any more
                    if (!keyDown) {
                        if (Input.GetAxis ("Horizontal") > 0) {
                            if (index < maxIndex) {
                                index++;
                            }
                        } else if (Input.GetAxis ("Horizontal") < 0) {
                            if (index > 0) {
                                index --;
                            }
                        }
                        keyDown = true;
                    }
                } else {
                    keyDown = false;
                }
            }
        }
        
        //Update the stored index
        storeIndex = index;

        // For the save file selection
        if (index == 0)
        {
            if (Input.GetButtonDown("Submit"))
            {
                // If we're on the banner selection, take action one
                if (index == 0)
                {
                    anim.SetBool ("Pressed", true);
                    TakeAction();
                }
                // If we're on the second index
                else
                {
                    anim2.SetBool("Pressed", true);
                    TakeAction_2();
                }
            }
            else if (anim.GetBool ("Pressed"))
            {
                anim.SetBool ("Pressed", false);
            }
        }
        // For the delete button
        else if (index == 1)
        {
            // If the button is held for the timer length, delete the save
            if (Input.GetButton("Submit"))
            {
                // Check if this is the first time we're pressing this button
                if (!areYouSure_Timer_Active)
                {
                    areYouSure_Timer = 0.0f;
                    areYouSure_Timer_Active = true;

                    // Start the hold animation
                    anim2.SetBool("Held", true);
                }
                // Otherwise, increment the time
                else
                {
                    areYouSure_Timer += Time.deltaTime;
                    //anim2.SetFloat("CycleOffset", areYouSure_Timer/areYouSure_Timer_End);

                    // If the timer reaches its limit, delete the save
                    if (areYouSure_Timer >= areYouSure_Timer_End)
                    {
                        TakeAction_2();
                    }
                }
            }
            // If we stop holding the button down, we reset the timer and animation
            else
            {   
                // Update the animation cycle offset
                //AnimatorStateInfo animationState = anim2.GetCurrentAnimatorStateInfo(0);
                //float myTime = animationState.normalizedTime;
                //anim2.SetFloat("CycleOffset", myTime);

                areYouSure_Timer_Active = false;
                areYouSure_Timer = 0.0f;

                // Reset the animation
                anim2.SetBool("Held", false);
            }
        }
        else if ((index == 1) && (anim2.GetBool ("Pressed")))
        {
            anim2.SetBool ("Pressed", false);
        }
    }

    void TakeAction()
    {
        // Play sound here

        // Perform button press
        if (takeAction != null)
        {
            takeAction.Invoke();
        }

        anim.SetBool ("Pressed", false);
        anim.SetBool ("Selected", false);
    }

    void TakeAction_2()
    {
        // Play sound here

        // Delete this save
        Persistance.Delete_Save(thisIndex);

        // Update the banner and reset it

        Update_SaveLoad_Banner();
        Reset_State();

        anim2.SetBool ("Pressed", false);
        anim2.SetBool ("Selected", false);
    }
}
