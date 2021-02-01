using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;

//////////////////////////////////////////////
//
//  Sc_Menu_ActionList - Script Actions
//  This script performs specific actions for
//  the 3D main menu I created

public class Sc_Menu_ActionList : MonoBehaviour
{
    // World references
    [HideInInspector]
    public Sc_SH sH;
    [HideInInspector]
    private Sc_GM gM;
    private Sc_AC aC;

    // References to the cameras
    public Camera camera_TitleMenu;
    public Camera camera_OptionsMenu;
    public Camera camera_SaveLoadMenu;
    public Camera camera_Credits;

    // Reference to menu objects
    public GameObject objects_TitleMenu;
    public GameObject objects_OptionsMenu;
    public GameObject objects_SaveLoadMenu;
    public GameObject objects_Credits;

    // Reference to the Master Menu Controller
    protected Sc_MenuController_Master masterMenu_Ref;

    // Reference to the screen transition script object
    private Sc_UI_ScreenTransition scr_ScreenTransition;

    // Scene name to load when pressing play/start
    public string playScene;
    public string titleScene;

    void Start()
    {
        // Grab the Save Handler
        sH = GameObject.FindGameObjectWithTag("SH").GetComponent<Sc_SH>();
        // As long as the gM exists
        if (GameObject.FindGameObjectWithTag("GM") != null)
        {
            gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
            aC = gM.aC;
        }

        // Grab the screen transition
        scr_ScreenTransition = GameObject.FindGameObjectWithTag("Canvas_TransitionScreen").GetComponent<Sc_UI_ScreenTransition>();

        // camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Sc_Camera_MainMenu>();
        masterMenu_Ref = this.gameObject.GetComponent<Sc_MenuController_Master>();
    }


    ////////////////////////////
    // Title Screen Functions //
    ////////////////////////////

    public void Act_TitleScreen_Play()
    {
        // Here we activate the SaveLoad menu
        scr_ScreenTransition.activate_MenuTransitions(Act_TitleScreen_Activate_SaveLoadMenu);
        //scr_ScreenTransition.activate_SceneTransitions(playScene);
    }

    public void Act_TitleScreen_OptionsMenu()
    {
        scr_ScreenTransition.activate_MenuTransitions(Act_TitleScreen_Activate_OptionsMenu);
    }

    public virtual void Act_TitleScreen_TitleMenu()
    {
        scr_ScreenTransition.activate_MenuTransitions(Act_TitleScreen_Activate_TitleMenu);
    }

    public virtual void Act_TitleScreen_Credits()
    {
        scr_ScreenTransition.activate_MenuTransitions(Act_TitleScreen_Activate_Credits);
    }

    ///////////////////
    // SaveLoad Menu //
    ///////////////////

    public void Act_SaveLoadMenu_LoadGame(int pass_SaveID)
    {
        // Pass the integer into the Sc_SH component
        // Load the scene, and the GM should do all the work?
        sH.saveID = pass_SaveID;

        scr_ScreenTransition.activate_SceneTransitions(playScene, true);
    }

    void Act_TitleScreen_Activate_OptionsMenu()
    {
        // Lots of things will happen when this button is activated
        // We need to transition to the options menu using fading and we need to switch a bunch of things on and off

        DeactivateAll_ApartFrom(2);
    }

    void Act_TitleScreen_Activate_TitleMenu()
    {
        DeactivateAll_ApartFrom(0);
    }

    // This function needs to be a little more complex than before.
    // It needs to check if a current save file exists for this element
    // And load some information about that save file
    void Act_TitleScreen_Activate_SaveLoadMenu()
    {
        // Load whether a save file exists
        // What i could do is create two save files for each game
        // One is smaller and just records player progress (Maybe in player Prefs feature)
        // And one is huge, recording everything else

        DeactivateAll_ApartFrom(1);
    }

    public void Act_TitleScreen_Activate_Credits()
    {
        DeactivateAll_ApartFrom(3);
    }

    public void Act_TitleScreen_Exit()
    {
        // If I wanted an animation or fade out or something,
        // I've put that in the screen coroutine because... It's just faster to write, I guess
        scr_ScreenTransition.activate_QuitGame();
    }

    public void DeactivateAll_ApartFrom(int passInt)
    {
        // Disable all cameras
        camera_TitleMenu.gameObject.SetActive(false);
        camera_SaveLoadMenu.gameObject.SetActive(false);
        camera_OptionsMenu.gameObject.SetActive(false);
        camera_Credits.gameObject.SetActive(false);

        // Disable all objects
        objects_TitleMenu.SetActive(false);
        objects_SaveLoadMenu.SetActive(false);
        objects_OptionsMenu.SetActive(false);
        objects_Credits.SetActive(false);

        // Switch off the active menu
        masterMenu_Ref.activeMenu.gameObject.SetActive(false);

        switch (passInt)
        {
            // Title Menu
            case 0:
                camera_TitleMenu.gameObject.SetActive(true);
                objects_TitleMenu.SetActive(true);
                masterMenu_Ref.activeMenu = masterMenu_Ref.menu_Refs[0];
                break;
            // SaveLoad
            case 1:
                camera_SaveLoadMenu.gameObject.SetActive(true);
                objects_SaveLoadMenu.SetActive(true);
                masterMenu_Ref.activeMenu = masterMenu_Ref.menu_Refs[2];
                break;
            // Options
            case 2:
                camera_OptionsMenu.gameObject.SetActive(true);
                objects_OptionsMenu.SetActive(true);
                masterMenu_Ref.activeMenu = masterMenu_Ref.menu_Refs[1];
                break;
            // Credits
            case 3:
                camera_Credits.gameObject.SetActive(true);
                objects_Credits.SetActive(true);
                masterMenu_Ref.activeMenu = masterMenu_Ref.menu_Refs[3];
                break;
        }

        // Switch on the active menu
        masterMenu_Ref.activeMenu.gameObject.SetActive(true);
    }

    //////////////////////////////////////
    // In-Game Options Screen Functions //
    //////////////////////////////////////

    public void Act_InGameMenu_Resume()
    {
        // Normal process for when the pause menu is switched off
        masterMenu_Ref.isGamePaused = false;
        masterMenu_Ref.Update_Pause();
    }

    public void Act_InGameMenu_OptionsMenu()
    {
        // Deactivate Pause menu
        masterMenu_Ref.menu_Refs[0].gameObject.SetActive(false);
        // Activate options menu
        // Set the index back to zero
        masterMenu_Ref.menu_Refs[1].index = 0;
        masterMenu_Ref.menu_Refs[1].gameObject.SetActive(true);
        masterMenu_Ref.activeMenu = masterMenu_Ref.menu_Refs[1];
    }

    public void Act_InGameMenu_Back()
    {
        // Deactivate options menu
        masterMenu_Ref.menu_Refs[1].gameObject.SetActive(false);
        // Activate pause menu
        masterMenu_Ref.menu_Refs[0].gameObject.SetActive(true);
        masterMenu_Ref.activeMenu = masterMenu_Ref.menu_Refs[0];
    }

    // Return to the game's title scene
    public void Act_InGameMenu_ReturnToTitle()
    {
        print ("Return to title");
        // Save the game state - This is run through the gM
        //gM.Act_SaveGame_OnQuit();

        Thread thread_SaveGame = new Thread(gM.Act_SaveGame);
        thread_SaveGame.Start();
        // Perform saving methods that can't be called through threading
        // Playerprefs saves + loading icon animation
        gM.Act_SaveGame_NonThreading();

        // Make sure the non SFX volume is back to its regular value
        // THIS IS NOT A GREAT PLACEMENT
        if (aC != null)
            aC.Mx_NonSFX.audioMixer.SetFloat("Mx_NonSFX_Volume", 0f);

        // Load the TitleScene, but also wait for saving to finished
        scr_ScreenTransition.activate_SceneTransitions(titleScene);
    }
}
