using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/////////////////////////////////////////////////////
//
//  Sc_MenuController_Master - Master Menu Controller
//  This script controlls whether other menu scripts
//  are active and when certain functions can be
//  performed

public class Sc_MenuController_Master : MonoBehaviour
{
    //##############//
	//	VARIABLES	//
	//##############//

	//References to World Objects
	private Sc_GM gM;
	private Sc_UM uM;
    private Sc_AC aC;

    // Some menus don't want a pause button
    public bool hasPauseButton = false;

    // Pause bool
    [System.NonSerialized]
    public bool isGamePaused = false;

    // Reference to the action list
    [HideInInspector]
    private Sc_Menu_ActionList actionList_Ref;
    // What the back button does
    public UnityEvent backButton_Action;

    // This is a reference to the active menu we're currently using
    // When active, it will be run through the update loop
    public Sc_MenuController_Basic activeMenu;

    // Referemce to other menus 0 - Main, 1 - Options
    public Sc_MenuController_Basic[] menu_Refs;

    // Ensure buttons are only pressed one at a time
    private bool buttonDown;

    // Start is called before the first frame update
    void Start()
    {
        //Grab the World Scripts, for the pausemenu
        if (hasPauseButton)
        {
            GameObject gM_GameObject = GameObject.FindGameObjectWithTag("GM");
            gM = gM_GameObject.GetComponent<Sc_GM>();
            uM = gM.uM;
            aC = gM.aC;
        }

        actionList_Ref = this.gameObject.GetComponent<Sc_Menu_ActionList>();

        // We want to make sure the pause menu gameobject is not active
        // On start up
        if (hasPauseButton)
        {
            // Make sure, upon startup, the game is not paused
            isGamePaused = false;
            Update_Pause();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // If the pause button is pressed, update the approriate systems
        if (Input.GetButtonDown("Pause") && hasPauseButton)
        {
            isGamePaused = !isGamePaused;
            Update_Pause();
        }

        if (isGamePaused || !hasPauseButton)
        {
            // This is main menu loop update
            activeMenu.pass_Update();

            // If the back button is pressed, when we're not at the main menu
            if (activeMenu != menu_Refs[0])
            {
                if (Input.GetButton("Back"))
                {
                    backButton_Action.Invoke();
                }
            }
        }
    }

    // This function responds to the pause button being pressed on or off
    public void Update_Pause()
    {
        // Pause ON
        if (isGamePaused)
        {
            // Switch on all the appropraite variables
            uM.boolCheck_Paused = true;
            // Activate only the first menu
            menu_Refs[0].gameObject.SetActive(true);
            // Set the index back to 0
            menu_Refs[0].index = 0;

            // Mute all music and atmosphere
            aC.Mx_NonSFX.audioMixer.SetFloat("Mx_NonSFX_Volume", -80f);
        }
        // Pause OFF
        else
        {
            // Switch off all the appropraite variables
            uM.boolCheck_Paused = false;

            // Make sure the activate menu is reset to the main menu (Just in case we're still in the options menu)
            activeMenu = menu_Refs[0];

            // Deactivate all menus
            foreach (Sc_MenuController_Basic item_Menu in menu_Refs)
            {
                item_Menu.gameObject.SetActive(false);
            }

            // Unmute all music and atmosphere
            aC.Mx_NonSFX.audioMixer.SetFloat("Mx_NonSFX_Volume", 0f);
        }
    }
}
