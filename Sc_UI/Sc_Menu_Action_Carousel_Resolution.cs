using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////////
//  Sc_Menu_Action_Carousel_Resolution
//  This is a subset of the carousel function that
//  Works specifically for selecting screen resolution

public class Sc_Menu_Action_Carousel_Resolution : Sc_Menu_Action_Carousel
{
    Resolution[] listof_Resolutions;

    public override void Pass_Start()
    {
        // In this version of the script, the items in the carousel
        // Are collect from the player's screen settings
        Collect_Resolutions();

        // Update the text in the menu
        storeIndex = index;
        ref_Text.SetText(listOf_Carousel_Items[index]);
    }

    void Collect_Resolutions()
    {
        // Grab all the resolutions picked up from the screen
        listof_Resolutions = Screen.resolutions;
        listOf_Carousel_Items = new string[listof_Resolutions.Length];

        for (int i = 0; i < listof_Resolutions.Length; i++)
        {
            string option = "???";
            //Send each resolution as a string into the carousel items array
            if (i == 0)
            {
                option = listof_Resolutions[i].width + " x " + listof_Resolutions[i].height 
                                                    + " : " + listof_Resolutions[i].refreshRate + " >";
            }
            else if (i == listof_Resolutions.Length - 1)
            {
                option = "< " + listof_Resolutions[i].width + " x " + listof_Resolutions[i].height 
                                                    + " : " + listof_Resolutions[i].refreshRate;
            }
            else
            {
                option = "< " + listof_Resolutions[i].width + " x " + listof_Resolutions[i].height 
                                                    + " : " + listof_Resolutions[i].refreshRate + " >";
            }

            listOf_Carousel_Items[i] = option;

            // If our current screen resolution is equal to one of these values
            // We set that as the starting value for the game
            // Unity won't let us compare resolutions. We have to compare w&h
            if (listof_Resolutions[i].width == Screen.currentResolution.width
                && listof_Resolutions[i].height == Screen.currentResolution.height)
            {
                index = i;
            }
        }
    }

    // Setting the resolution is a little fiddlier than I'd like, so I'm manually
    // Writing the take action here
    public override void TakeAction()
    {
        // Play sound here

        // Perform button press
        Screen.SetResolution(listof_Resolutions[index].width, listof_Resolutions[index].height, Screen.fullScreen);

        anim.SetTrigger("Pressed");
    }
}
