using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_PG - Puzzle Group
//	This oversees a portion of levels?

public class Sc_PG : MonoBehaviour
{
    // Reference to world managers
    [System.NonSerialized]
    public Sc_GM gM;					// Reference to the GM
    [System.NonSerialized]
    public Sc_UM uM;					// Reference to the uM
    private Sc_RB_Animation rbA;	        // Reference to the animation values list

    // Reference to the main camera
    private Sc_Camera ref_Camera;

    // Reference to canvas displaying text
    private Sc_ES_CanvasMessages ref_CanvasMessages;
    
    [HideInInspector]
    public List<Sc_LevelManager> listOf_LevelManagers;     // Reference to all the level managers (Individual levels)
    [HideInInspector]
    public List<Sc_LevelManager> listOf_LevelManagers_ThatCountToCompletion;     // Not all puzzles count towards completion
    [HideInInspector]
    public List<Sc_ZoneMonitor> listOf_ZoneMonitors;       // Reference to all the zone monitors for each area
    
    [System.NonSerialized]
    public Sc_LevelManager active_LevelManager;   // This variable stores the current level the player is in
    [System.NonSerialized]
    public Sc_ZoneMonitor active_ZoneMonitor;

    // Player progress
    [HideInInspector]
    public int pP_Puzzle_ProgresState = 0;
    [HideInInspector]
    public int pP_Puzzle_ProgresState_ThatCountTowardsCompletion = 0;

    void Awake()
    {
        gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
    }

    void Start()
    {
        // Grab the Game Manager, dictionary, movement library...
        uM = gM.uM;
        rbA = gM.rbA;

        // Grab the camera
        ref_Camera = GameObject.FindGameObjectWithTag("MainCamera").gameObject.GetComponent<Sc_Camera>();
        
        // Grab a reference to the message canvas script, if it exists
        if (GameObject.FindGameObjectWithTag("Canvas_Messages") != null)
            ref_CanvasMessages = GameObject.FindGameObjectWithTag("Canvas_Messages").GetComponent<Sc_ES_CanvasMessages>();

        // Grab all the LevelMaanagers, which will be children to this game object
        // Currently, the individual levels will pass themselves to the LG
    }

    // This function will check if the current level conditions have been fulfilled
    // And will also update the ZoneMonitor
    public void Check_ActiveLevelComplete()
    {
        // Run the check function in the current, active level, if there is an active level
        if (active_LevelManager != null)
        {
            // Check if the level is complete (As long as it isn't already in a completed state)
            if (active_LevelManager.Check_LevelConditionsMet() && !active_LevelManager.bool_levelComplete)
            {
                // Run the internal level manager complete function
                active_LevelManager.Level_Complete();

                // Run the PG level complete function, setting the bool to true
                Level_Complete();

                // Update the zone monitor status
                //active_ZoneMonitor.Update_ZoneMonitorStatus(active_LevelManager);

                // Save the game
                //gM.SaveGame();
            }
        }
    }

    // Pass the message onto the canvas
    public void Canvas_DisplayMessage_LevelName(string p_LevelName, bool p_LevelType)
    {
        ref_CanvasMessages.Canvas_DisplayMessage_LevelName(p_LevelName, p_LevelType);
    }

    // Resets the currently active level
    public void Reset_ActiveLevel()
    {
        // If there is a currently active level
        if (active_LevelManager != null)
        {
            // Reset that level
            active_LevelManager.Reset_Level();

            // Record a new state due to change (On second run)
            if (!uM.resetLevel_Twice)
            {
                gM.Record_StateList_V2();
            }
        }
    }

    // This is run AFTER the conditions for the level have been confirmed
    public void Level_Complete()
    {
        // Get a record of puzzle completion percentage
        float NumLevs = listOf_LevelManagers.Count;
        int temp_pP_Puzzle_ProgressState = 0;
        int temp_pP_Puzzle_ProgressState_ThatCountTowardscompletion = 0;
        for (int i = 0; i < NumLevs; i++)
        {
            if (listOf_LevelManagers[i].bool_levelComplete)
            {
                temp_pP_Puzzle_ProgressState += 1;
                if (!listOf_LevelManagers[i].isNotLevel)
                {
                    temp_pP_Puzzle_ProgressState_ThatCountTowardscompletion += 1;
                }
            }
        }
        pP_Puzzle_ProgresState = temp_pP_Puzzle_ProgressState;
        pP_Puzzle_ProgresState_ThatCountTowardsCompletion = temp_pP_Puzzle_ProgressState_ThatCountTowardscompletion;

        // Then update the total game completion percentage
        gM.Update_TotalGame_CP();
    }

    // Controls the camera movement for a level
    public void LevelCamera_On(Vector3 pass_LevelCamera_Position, Quaternion pass_LevelCamera_Rotation)
    {
        // Set the camera position
        ref_Camera.levelCamera_FocusPoint = pass_LevelCamera_Position;
        // And rotation
        ref_Camera.levelCamera_RotationPoint = pass_LevelCamera_Rotation;
        // Set the camera to move
        ref_Camera.bool_LevelCamera = true;  
    }

    public void LevelCamera_Off()
    {
        ref_Camera.bool_LevelCamera = false;
    }

    // Change art mode
    public void Switch_ArtMode(int pass_Mode)
    {
        // Go through even level manager and change the mode
        foreach (Sc_LevelManager temp_LevelManager in listOf_LevelManagers)
        {
            temp_LevelManager.Switch_ArtMode(pass_Mode);
        }
    }
}