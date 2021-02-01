using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////////
//  Sc_Menu_Action_Carousel_Bool
//  This is a subset of the carousel function that
//  Works specifically for boolean options

public class Sc_Menu_Action_Carousel_Fullscreen : Sc_Menu_Action_Carousel
{
    public override void Pass_Start()
    {
        // Check the current screen resolution and update the status of the menu
        if (!Screen.fullScreen)
        {
            index = 0;
        }
        else
        {
            index = 1;
        }

        storeIndex = index;

        ref_Text.SetText(listOf_Carousel_Items[index]);
    }

    public override void TakeAction()
    {
        // Send true of false depending on index selection
        if (index == 0)
        {
            Screen.fullScreen = false;
        }
        else
        {
            Screen.fullScreen = true;
        }

        anim.SetTrigger("Pressed");
    }
}
