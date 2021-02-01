using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

////////////////////////////////////////////
//
//  Sc_MenuSettings - Settings Menu Script
//  Here I've stored the functions that are
//  used to alter the game's settings

public class Sc_MenuSettings : MonoBehaviour
{
    //When it comes to resolution, Unity will grab the right values
    //For us, for the screen being used.
    Resolution[] listof_Resolutions;
    public Dropdown ui_ResolutionDropdown;
    void Start()
    {
        //Grab all the resolutions picked up from the screen
        listof_Resolutions = Screen.resolutions;

        //Clear the current dropdown list
        ui_ResolutionDropdown.ClearOptions();

        //Set the current resolution of the game window before we start playing
        int currentResolutionIndex = 0;

        //Add the options that we've picked up
        //We'll have to turn our array into a list
        List<string> ui_ResolutionOptions = new List<string>();
        for (int i = 0; i < listof_Resolutions.Length; i++)
        {
            string option = listof_Resolutions[i].width + " x " + listof_Resolutions[i].height + " : " + listof_Resolutions[i].refreshRate;
            ui_ResolutionOptions.Add(option);

            //If our current screen resolution is equal to one of these values
            //We set that as the starting value for the game
            //Unity won't let us compare resolutions. We have to compare w&h
            if (listof_Resolutions[i].width == Screen.currentResolution.width
                && listof_Resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        ui_ResolutionDropdown.AddOptions(ui_ResolutionOptions);
        ui_ResolutionDropdown.value = currentResolutionIndex;
        ui_ResolutionDropdown.RefreshShownValue();


    }

    public void SetVolume(float volume)
    {
        print (volume);
    }

    //I don't know if I want to add this atm
    public void SetQuality(int qualityIndex)
    {

    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution pass_Resolution = listof_Resolutions[resolutionIndex];
        Screen.SetResolution(listof_Resolutions[resolutionIndex].width, listof_Resolutions[resolutionIndex].height, Screen.fullScreen);
    }

}
