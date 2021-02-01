using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//////////////////////////////////////////////
//
//  Sc_Menu_ActionList_InGameMenu - Script Actions
//  This script performs specific actions for
//  the ingame menu, the pause menu

public class Sc_Menu_ActionList_InGameMenu : Sc_Menu_ActionList
{
    public void Act_CloseMenu()
    {
        // Normal process for when the pause menu is switched off
        masterMenu_Ref.isGamePaused = false;
        masterMenu_Ref.Update_Pause();
    }

    public void Act_Options()
    {
        // Deactivate Pause menu
        masterMenu_Ref.menu_Refs[0].gameObject.SetActive(false);
        // Activate options menu
        // Set the index back to zero
        masterMenu_Ref.menu_Refs[1].index = 0;
        masterMenu_Ref.menu_Refs[1].gameObject.SetActive(true);
        masterMenu_Ref.activeMenu = masterMenu_Ref.menu_Refs[1];
    }

    public void Act_Back()
    {
        // Deactivate options menu
        masterMenu_Ref.menu_Refs[1].gameObject.SetActive(false);
        // Activate pause menu
        masterMenu_Ref.menu_Refs[0].gameObject.SetActive(true);
        masterMenu_Ref.activeMenu = masterMenu_Ref.menu_Refs[0];
    }
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}
