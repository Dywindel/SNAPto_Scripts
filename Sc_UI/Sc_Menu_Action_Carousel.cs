using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

//////////////////////////////////////////////////
//  Sc_Menu_Action_Carousel
//  This menu action is a carousel. Players use the
//  Directional pad to slide through a list of
//  Options

public class Sc_Menu_Action_Carousel : Sc_Menu_Action_Par
{
    //Horizontal or vertical carousel selection
    public bool isVertical = false;

    //Reference to textPro item
    public TextMeshProUGUI ref_Text;

    //List of items the player will pick from
    public string[] listOf_Carousel_Items;

    //Which number to start on
    public int start_Index;

    //Which number we're currently on
    protected int index;
    //To check if the index has changed
    protected int storeIndex;

    bool keyDown;

    public UnityEvent takeAction;

    public override void Pass_Start()
    {
        // Here is where we'd normally get the list of items, but for the base
        // Carousel, you write in items manually

        // Then, we set the first start item
        index = start_Index;
        storeIndex = start_Index;
    }

    public override void Pass_Input()
    {
        // Using the left and right values of the D-Pad (Or up and down for vertical menu)
        // The player can cycle through a list of items
        if (isVertical)
        {
            if (Input.GetAxis ("Vertical") != 0) {
                // We fire the menu selection once, then set keyDown to true so the menu doesn't change any more
                if (!keyDown) {
                    if (Input.GetAxis ("Vertical") < 0) {
                        if (index < listOf_Carousel_Items.Length - 1)
                        {
                            index ++;
                        }
                    } else if (Input.GetAxis ("Vertical") > 0){
                        if (index > 0)
                        {
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
                // We fire the menu selection once, then set keyDown to true so the menu doesn't change any more
                if (!keyDown) {
                    if (Input.GetAxis ("Horizontal") > 0) {
                        if (index < listOf_Carousel_Items.Length - 1)
                        {
                            index ++;
                        }
                    } else if (Input.GetAxis ("Horizontal") < 0) {
                        if (index > 0)
                        {
                            index --;
                        }
                    }
                    keyDown = true;
                }
            } else {
                keyDown = false;
            }
        }

        //If the index has changed, we can update what appears on screen and TakeAction
        if (index != storeIndex)
        {
            // Play SFX
            aC.Play_SFX("SFX_UI_Click");

            // Once the index has been updated, we can changed what appears on screen
            ref_Text.SetText(listOf_Carousel_Items[index]);
            // We can take action too
            TakeAction();
        }

        //Update the stored index
        storeIndex = index;
    }

    //For the setting of the resolution, take Action has been written manually
    //In the relevent script
    public virtual void TakeAction()
    {
        //Play sound here

        //Perform button press
        if (takeAction != null)
        {
            takeAction.Invoke();
        }

        anim.SetBool ("Pressed", true);
        //anim.SetBool ("Selected", false);
    }
}
