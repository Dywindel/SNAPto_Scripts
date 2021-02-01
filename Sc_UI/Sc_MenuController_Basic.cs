using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_MenuController_Basic : MonoBehaviour
{
    // World References
    private Sc_AC aC;

    // Boolean for horizontal or vertical selection
    public bool isVertical = true;
    
    // Current/Initial selected button
    public int index;
    private int storeIndex;
    bool keyDown;

    //Number of menu buttons
    public int maxIndex;

    //List of child button
    [HideInInspector]
    public List<Sc_Menu_Action_Par> listOf_ChildActions;

    public void Start()
    {
        aC = GameObject.FindGameObjectWithTag("AC").GetComponent<Sc_AC>();

        storeIndex = index;
    }

    public void pass_Update()
    {
        if (isVertical)
        {
            if (Input.GetAxis ("Vertical") != 0) {
                // We fire the menu selection once, then set keyDown to true so the menu doesn't change any more
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
                // We fire the menu selection once, then set keyDown to true so the menu doesn't change any more
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

        // If the index value has changed, reset the "new selection2 - Ok, this is kinda dumb but whatever
        if (storeIndex != index)
        {
            // Play the menu SFX
            aC.Play_SFX("SFX_UI_Click");
            
            foreach(Sc_Menu_Action_Par actor in listOf_ChildActions)
            {
                actor.Reset_State();
            }
            
            storeIndex = index;
        }

        //Run through each child that is part of this button
        foreach(Sc_Menu_Action_Par actor in listOf_ChildActions)
        {
            actor.pass_Update();
        }
    }
}
