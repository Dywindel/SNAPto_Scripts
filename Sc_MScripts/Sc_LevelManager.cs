using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_LevelManager - Level Manager
//	This script controls an individual level, it's state, starting positions,
//  contents, whether it has been loaded and other features

public class Sc_LevelManager : MonoBehaviour
{
    private Sc_PG pG;                   //Reference to the Puzzle Group script
    [System.NonSerialized]
    public Sc_ZoneMonitor zM;  // Reference to the zone monitor in this area
    private Sc_Player player;

    // Object values
    public int id = -1;

    public int test = 1;

    // 0 - Puzzle Index Number and 1 - Zone Monitor Index Number
    // This is sorted out through code and just allows each puzzle to have an ID
    [HideInInspector]
    public int[] levelID = {0, 0};

    // Is the level complete
    [HideInInspector]
    public bool bool_levelComplete = false;

    // Level Type - Bridge Level: Get to a position, Soil Level: Water all the plants
    public bool isBridgeLevel = false;
    // NotLevel - This are just moveable blocks that can be reset if necessary
    public bool isNotLevel = false;

    // Facing directions for player at level start
    int player_StartingDirection = 0;

    public Transform ref_LevelCamera;

    // Reference to all the clouds in this puzzle
    public GameObject Ref_CloudsFolders;
    // This contains all the clouds and soil in the level
    [HideInInspector]
    public Sc_Cloud[] level_ListOfClouds;
    [HideInInspector]
    public List<Sc_Block_Soil> level_ListOfSoils = new List<Sc_Block_Soil>();
    
    // For now, the level complete marker is just a meshRenderer
    public Sc_LevelStatusMarker_Complete refSc_LevelStatusMarker;

    // The LevelManager checks if the player has reached the Bridge Trigger marker
    // Once per move. Though the Bridge itself is a trigger, it's simpler to put the
    // Trigger inside the Check_LevelConditionsMet() function
    public Sc_ES_Trigger_LockSwitch ref_Trigger_LockSwitch;

    public string levelName = "1st of Fall";

    void Start()
    {
        // Grab world objects
        Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
        pG = gM.pG;
        player = gM.player;

        // Grab the cloud children
        level_ListOfClouds = Ref_CloudsFolders.GetComponentsInChildren<Sc_Cloud>();

        // Send this level manager to each cloud
        // Grab all the soil, if there is any
        foreach (Sc_Cloud temp_Cloud in level_ListOfClouds)
        {
            temp_Cloud.levelManager = this;
            // And the contents of each cloud
            foreach(Sc_Block temp_Block in temp_Cloud.listOf_ChildBlocks)
            {
                // Find each Soil
                if (temp_Block.self_BlockType == GlobInt.BlockType.Soil)
                {
                    // Pass the variable
                    Sc_Block_Soil temp_Soil = (Sc_Block_Soil)temp_Block;
                    if (!level_ListOfSoils.Contains(temp_Soil))
                        level_ListOfSoils.Add(temp_Soil);
                }
            }
        }

        // Add yourself to the PG's list of levels and other list if you count towards completion
        pG.listOf_LevelManagers.Add(this);
        if (!isNotLevel)
            pG.listOf_LevelManagers_ThatCountToCompletion.Add(this);

        // Calculate the starting direction
        // The value we use to set player rotation direction is based on the rotation of the Level
        // Manager mesh object
        player_StartingDirection = (int)Mathf.Round((this.gameObject.transform.localEulerAngles.y % 360f / 90f) % 4f);
    }

    // When the player enters the level
    public void Level_Activate()
    {
        // Tell the zone monitor you've been activated
        zM.Send_ActiveLevel(this);

        // Set the camera to be in levelCamera mode
        pG.LevelCamera_On(ref_LevelCamera.position, ref_LevelCamera.rotation);

        // Run the levelName coroutine - Only if the level has a name
        if (levelName != "")
            pG.Canvas_DisplayMessage_LevelName(levelName, isBridgeLevel);

        pG.gM.Update_TotalGame_CP();
    }

    // When the player leaves the level
    public void Level_Deactivate()
    {
        // Remove self from the currently active level
        zM.Send_DeactiveLevel();
        
        // Allow the camera to follow the player again
        pG.LevelCamera_Off();

        pG.gM.Update_TotalGame_CP();
    }

    // Check if the level is complete
    public bool Check_LevelConditionsMet()
    {
        // If the debug mode boolean is on
        if (pG.gM.solveUponEnter)
            return true;
        // 'Not levels' are solved as soon as the player enters the level
        else if (isNotLevel)
        {
            return true;
        }
        // For soil levels
        else if (!isBridgeLevel)
        {
            // Check which soils are watered
            foreach (Sc_Block_Soil temp_Soil in level_ListOfSoils)
            {
                if (temp_Soil.soilDry)
                {
                    return false;
                }
            }

            // Otherwise, return true
            return true;
        }
        // For Bridge levels
        else
        {
            // We just check if the lock switch has been reached by the player
            if (ref_Trigger_LockSwitch.stayActive)
            {
                return true;
            }
            else
                return false;
        }
    }

    // When the player resets a level with a button
    public void Reset_Level()
    {
        // Reset each cloud
        foreach(Sc_Cloud temp_Cloud in level_ListOfClouds)
        {
            // Reset the cloud state
            temp_Cloud.Reset_Cloud_LevelState();
        }

        // Move the player to the position of the level manager (IE, the level entrance)
        // Update it's necessary discrete variables (Stored in the blocktype script)
        // It's also a good idea face the player in the right direction to start the level
        player.Reset_PlayerPosition(this.transform.position, player_StartingDirection);

        // Then we reset the player's lists and record neighbours again?

		// Update the AfterAction function in the GM
		pG.gM.Player_HasPerformedAction_AfterAction();
    }

    // Level complete animation for the level complete marker
    // Once a level is complete, the marker will always stay in its complete state, even if the puzzle is reset
    public void Level_Complete()
    {
        // Ensure's we only do these checks once
        if (!bool_levelComplete)
        {
            bool_levelComplete = true;
            refSc_LevelStatusMarker.LevelMarker_Animate();

            // Activate the relvent petal on the zone monitor
            zM.Update_ZoneMonitorStatus(this);
        }
    }

    // Change the art style mode
    public void Switch_ArtMode(int pass_Mode)
    {
        // Grab all the clous, if there is any
        foreach (Sc_Cloud temp_Cloud in level_ListOfClouds)
        {
            // And the contents of each cloud
            foreach(Sc_Block temp_Block in temp_Cloud.listOf_ChildBlocks)
            {
                // Change their artstyle
                if (temp_Block.self_BlockType == GlobInt.BlockType.Puff)
                {
                    Sc_Block_Puff temp_Puff = (Sc_Block_Puff)temp_Block;
                    temp_Puff.Switch_ArtMode(pass_Mode);
                }
                else if (temp_Block.self_BlockType == GlobInt.BlockType.Lift)
                {
                    // Lifts might be a little tricker to pull off
                    //Sc_Block_Puff temp_Lift = (Sc_Block_Puff)temp_Lift;
                    //temp_Puff.Switch_ArtMode(pass_Mode);
                }
                else if (temp_Block.self_BlockType == GlobInt.BlockType.Flor)
                {
                    // The Floors
                    Sc_Block_Floor temp_Floor = (Sc_Block_Floor)temp_Block;
                    temp_Floor.Switch_ArtMode(pass_Mode);
                }
                if (temp_Block.self_BlockType == GlobInt.BlockType.Soil)
                {
                    // And the Soils
                    Sc_Block_Soil temp_Soil = (Sc_Block_Soil)temp_Block;
                    temp_Soil.Switch_ArtMode(pass_Mode);
                }
            }
        }
    }
}
