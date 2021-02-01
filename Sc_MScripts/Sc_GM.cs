using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using System.Threading;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_GM - Game Manager
//	This script is an overwatch for all game objects. It also houses the restore
//	And records state list functions

public class Sc_GM : MonoBehaviour {

	// Most variables will not update in the inspecter, but this can be bypassed by using [system.nonserialized]

	// Bug Fixing
	// This boolean helps me find bugs that I want to resolve at a later date
	public bool printBugs = false;

    // We don't allow the player to move if the gM is loading the game
    // Loading is also partially performed in the sH
    [HideInInspector]
    public bool isLoading_Thread = true;
    private MasterData temp_MasterData;

	// Boolean game options
    public bool isFreeCam_Active = false;	// Whether freeCam is accessable
    [HideInInspector]
    public bool isFreeCam_On = false;	    // Whether freeCam is on
    public bool isSimpleMode = false;       // Changes the designs of the clouds to be easier to read
	public bool isFloorOn = true;			// Controls whether the player is ignoring the floor currently
	public bool isIgnoreDryPuffs = true;	// The player cannot interact with ghost puffs
	public bool isDryPuffBridge = false;	// This game mode has ghost Puffs as walkable platforms
	public bool isFallingOn = true;			// For bug checking, it's easier to turn off falling
	public bool isPlayerPaperWeight = true;	// Does the player standing on a piece stop it from moving entirely?
    public bool turnOffGameLoading = false; // Don't load any previous saves in debug mode
    public bool solveUponEnter = false;     // Solve a puzzle as soon as the player enters the arena: for debug mode

    // TEST functions
    public float TEST_completeprecentage = 0.0f;
    public float TEST_BreachPercentage = 0.0f;

    // Game completion precentage objects
    public Sc_NP_AeoMuseum_Artifact ref_Mirror;
    public Sc_NP_AeoMuseum_Artifact ref_Tulip;
    public Sc_NP_AeoMuseum_Artifact ref_Umber;
    public Sc_NP_AeoMuseum_Artifact ref_Aeo;
    public Sc_NP_AeoMuseum_Artifact ref_Astor;
    public Sc_NP_AeoMuseum_Artifact ref_DemoGuy;

    // Cloud Breach objects
    public ParticleSystem ref_Rain;
    public ParticleSystem ref_CloudEvaporate;

    // Ending state int
    // 0 - No ending, 1 - Bad Ending, 2 - Good Ending
    [HideInInspector]
    public int endingState = 0;
    [HideInInspector]
    public bool atmosphere_NormalMode = false;  // Put the lights and audio back into normal mode after the game ends
    private bool ending_WaitForLoad = false;    // Don't activate any endings until we've loaded the rest of the game first. This locks the ending from occuring for a couple of frames
    private float ending_WaitForLoad_Time = 1f;
    [HideInInspector]
    private bool badEnding_Lockout = false;     // You cannot activate the bad ending if you have returned more than 40% of cloud and are on the path of the good ending
    [HideInInspector]
    public bool badEnding_Saved = false;        // If the bad ending has been reached, or is currently reached, make sure we save this state. // When the game is loaded
                                                // Next time, we don't have to sit through the bad ending cutscene again
    [HideInInspector]
    public bool goodEnding_Saved = false;       // This is true of the good ending too (Which allows people to explore freely through the game)
    private Coroutine cr;
    // For the rain grass
    private int store_RainGrass_Mode = 0;

    // Persistance Options
    [System.NonSerialized]
    public int saveID = 0;  // This will be changed when the game is first loaded (Or will be passed to the GM)

    // Variable game options
    public float tp_bulletTime = 0.0f;      // Super slow mode for certain puzzles
    public float sample_BulletTime = 0.0f;
	
    [HideInInspector]
	public Sc_Player player;

    // Reference to the in-game canvases
    [HideInInspector]
    public Sc_ES_CanvasMessages ref_CanvasMessenger;
    public Sc_UI_ScreenTransition ref_TransitionScreen;

	// List of all recorded states
	private List<RecStateList> listOf_RecStates = new List<RecStateList>();
    private List<RecStateList_V2> listOf_RecStates_V2 = new List<RecStateList_V2>();
    private RecState_AsList listOf_RecStates_New = new RecState_AsList();
    private RecPlayerProgress current_RecPlayerProgress;

	// Reference to every object in the scene
	[HideInInspector]
	public List<Sc_Storm> listOf_All_Storms_ForReference;
	[HideInInspector]
    public List<Sc_Cloud> listOf_All_Clouds_ForReference;
	[HideInInspector]
    public List<Sc_Block> listOf_All_Blocks_ForReference;

    // The current lighting and sound
    private int current_Scene = 24;

    // World objects
    [HideInInspector]
    public Sc_SH sH;
    [HideInInspector]
    public Sc_UM uM;
    [HideInInspector]
    public Sc_PG pG;
    [HideInInspector]
    public Sc_AC aC;
    [HideInInspector]
    public Sc_LD lD;
    [HideInInspector]
    public Sc_NP_AeoMuseum nP;
    [HideInInspector]
	public Sc_RB_Values rbV;
    [HideInInspector]
    public Sc_RB_Animation rbA;
    [HideInInspector]
    public Sc_RB_Clock rbC;
    [HideInInspector]
    public Sc_RenderManager rRm;
    [HideInInspector]
	public Sc_RB_Clock wC;

    // Player progress for P - Puzzles, A - Artifacts and B - Breaches
    [HideInInspector]
    public float gM_CP_P_A_B, gM_CP_P_A, gM_CP_P, gM_CP_B = 0.0f;

    // Mute game
    private bool isMuted = false;

    // Demo Sign
    public TextMeshProUGUI ref_TextMesh;

    // Autosaving
    private float savingTimer = 5f * 60f;       // The first save occurs after 5 minutes of gameplay, when the user first loads in
    private float saviingTimeLimit = 10f * 60f;  // Then Every 10 minutes afterwards
    [HideInInspector]
    public bool bool_IsSavingReady = false;     // Is the UM ready to save the game?
    [HideInInspector]
    public bool bool_IsSavingActive = false;    // Is the game currently being saved with the binary formatter?

	///////////
	// START //
	///////////

	//Ensures we only ever have one of these managers existing at a time
	public static Sc_GM instance = null;

	void Awake()
	{
		// Grab the world references
        sH = GameObject.FindGameObjectWithTag("SH").GetComponent<Sc_SH>();
        uM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_UM>();
        pG = GameObject.FindGameObjectWithTag("PG").GetComponent<Sc_PG>();
        lD = GameObject.FindGameObjectWithTag("LD").GetComponent<Sc_LD>();
        aC = GameObject.FindGameObjectWithTag("AC").GetComponent<Sc_AC>();
        nP = GameObject.FindGameObjectWithTag("NP").GetComponent<Sc_NP_AeoMuseum>();
		GameObject rb_GameObject = GameObject.FindGameObjectWithTag("RB");
		rbV = rb_GameObject.GetComponent<Sc_RB_Values>();
        rbA = rb_GameObject.GetComponent<Sc_RB_Animation>();
        rbC = rb_GameObject.GetComponent<Sc_RB_Clock>();
        rRm = rb_GameObject.GetComponent<Sc_RenderManager>();
		wC = rb_GameObject.GetComponent<Sc_RB_Clock>();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Sc_Player>();

        ref_CanvasMessenger = GameObject.FindGameObjectWithTag("Canvas_Messages").GetComponent<Sc_ES_CanvasMessages>();
        ref_TransitionScreen = GameObject.FindGameObjectWithTag("Canvas_TransitionScreen").GetComponent<Sc_UI_ScreenTransition>();

        // Update the saveID
        saveID = sH.saveID;
	}

	void Start()
	{        
        // Initial scene
        Update_Scene(current_Scene);

        // Try loading the game, otherwise, just start the game as new
        // I've put this in a coroutine as some of the Storms weren't updating fast enough to match the GM
        StartCoroutine(GM_LoadGame());
    }

    // Elements that have to happen after the first few frames of loading
    IEnumerator GM_LoadGame()
    {
        // Wait one frame
        yield return null;

        // If we're loading the game, we'll need to start a coroutine
        if (sH.loadGame)
        {
            try
            {
                StartCoroutine(LoadGame());
            }
            catch
            {
                print ("Failed to load game");
                sH.isLoading = false;
                // Do nothing
            }
        }
        // If we're not loading anything
        else
        {
            // Turn this bool off
            sH.isLoading = false;
        }

        // Wait until the game has loaded
        while (sH.isLoading)
        {
            yield return null;
        }

        yield return null;

        // Elements that have to happen after the game has loaded
        GM_AfterLoading();

        yield return null;
    }

    // Elements that have to happen after the game has loaded
    private void GM_AfterLoading()
    {
        // Move the camera to where the player is
        StartCoroutine(GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Sc_Camera>().Start_CameraPosition_FrameDelay());

        // Check which puffs have been breeched
        foreach (Sc_Cloud temp_Cloud in listOf_All_Clouds_ForReference)
        {
            // For puffed clouds
            if (temp_Cloud.bool_ContainsPuffs)
            {
                foreach (Sc_Block_Puff temp_Puff in temp_Cloud.listOf_ChildBlocks)
                {
                    // Check if puff has been breached, on load
                    temp_Puff.Check_CloudLayerBreach_OnLoad();
                }
            }
        }

        // Wait a small amount of time before allowing the endings to occur
        StartCoroutine(Wait_ForEndings());
    }

    // Timer that pauses the endings for a little when the game first loads back in
    public IEnumerator Wait_ForEndings()
    {
        yield return new WaitForSeconds(ending_WaitForLoad_Time);

        ending_WaitForLoad = true;

        // Then, finally double check the ending conditions
        Update_TotalGame_CP();
    }

    ////////////
    // UPDATE //
    ////////////

    // I need to place these settings somewhere. I'm going to give the gM and update function for now
    // Eventually, this will go in the settings menu
    // Update the settings menu
    void Update()
    {
        rbA.tp = rbA.tp_Fixed*(rbA.tp_Factor + 10.0f*f_BulletInput(6F*tp_bulletTime - 4f));
        rbA.tr = rbA.tr_Fixed*(rbA.tp_Factor + 10.0f*f_BulletInput(6F*tp_bulletTime - 4f));
        rbA.tf = rbA.tf_Fixed*(rbA.tp_Factor + 10.0f*f_BulletInput(6F*tp_bulletTime - 4f));

        // This is too heavy at the moment, but I want to check it's working
        // Switch_ArtMode();

        // Common button presses
        // Mute Game
        if (Input.GetButtonDown("Mute"))
        {
            isMuted = !isMuted;
            if (isMuted)
                aC.Mx_Master.audioMixer.SetFloat("Mx_Mas_Volume", -80f);
            else
                aC.Mx_Master.audioMixer.SetFloat("Mx_Mas_Volume", -6f);
        }

        // I'd like to autosave the game every x minutes
        if (savingTimer <= 0)
        {
            SaveGame();
            savingTimer = saviingTimeLimit;
        }
        else
        {
            savingTimer -= Time.deltaTime;
        }
    }

    public float f_BulletInput(float x) {
        if (x == 0)
            return 0;
        else
		    return (1 / (1 + (float)Mathf.Exp(-(x*2 - 1))));
	}

	///////////////
	// TRIGGERS  //
	///////////////

    // This function occurs once during the transition between day and night
    public void DayNight_Transition_Trigger()
    {
        
    }

	// This function occurs once after the player confirmed an action
	public void Player_HasPerformedAction_BeforeAction()
	{
		
	}

	// This function occurs once after the player confirmed an action and the action has resolved
	public void Player_HasPerformedAction_AfterAction()
	{
        // Check for any message prompts
        ref_CanvasMessenger.Canvas_DisplayMessage_MessagePrompt();

        // Check for any dialogue prompts
        //ref_CanvasMessenger.Canvas_DisplayMessage_DialoguePrompt();
	}

    // This updates the lighting and music for each area scene
    public void Update_Scene(int pass_i)
    {
        current_Scene = pass_i;

        // Update the lighting
        lD.Update_LD(pass_i);

        // Update the music and sound
        aC.Play_Soundscape(pass_i);

        Update_TotalGame_CP();
    }

	// Reset the active level
	public void Reset_ActiveLevel()
	{
        pG.Reset_ActiveLevel();
	}

    // This function asks the PG if the active level has been completed
    public void Check_ActiveLevelComplete()
    {
        pG.Check_ActiveLevelComplete();
    }

    // This is the total percetange, that only effects the demo sign.
    // The game has separate values for artifacts collected, breaches made and puzzles solved
    public void Update_TotalGame_CP()
    {
        // Puzzle completion percentage on its own
        float temp_Total_P = pG.listOf_LevelManagers_ThatCountToCompletion.Count;
        float temp_Comp_P = pG.pP_Puzzle_ProgresState_ThatCountTowardsCompletion;

        gM_CP_P = (Mathf.Round((temp_Comp_P/temp_Total_P) * 1000f) / 1000f);

        // Full Total of Puzzles + Artifacts
        float temp_Total_P_A =  pG.listOf_LevelManagers_ThatCountToCompletion.Count +
                                nP.listOf_Artifacts_ThatCountTowardsCompletion;

        // Find total completed for Puzzles + Artifacts
        float temp_Comp_P_A =   pG.pP_Puzzle_ProgresState_ThatCountTowardsCompletion +
                                nP.mP_ProgressState_Total;
        
        // Calculate new complete percentage for Puzzles and Artifacts
        gM_CP_P_A = (Mathf.Round((temp_Comp_P_A/temp_Total_P_A) * 1000f) / 1000f);

        // For testing
        //gM_CP_P_A = TEST_completeprecentage;

        Update_GameWorld_CP_P();

        // Check every cloud breach
        float temp_Total_B = 0f;
        float temp_Comp_B = 0f;
        foreach (Sc_Cloud temp_Cloud in listOf_All_Clouds_ForReference)
        {
            if (temp_Cloud.bool_ContainsPuffs)
            {
                temp_Total_B += 1f;
                if (temp_Cloud.bool_ReturnToCloudLayer)
                {
                    temp_Comp_B += 1f;
                }
            }
        }

        // Complete percentage for cloud Breaches
        gM_CP_B = (Mathf.Round((temp_Comp_B/temp_Total_B) * 1000f) / 1000f);

        Update_GameWorld_CP_B();

        // The final total goes up to 200% and is used for the demo sign
        gM_CP_P_A_B = (Mathf.Round((gM_CP_P_A + gM_CP_B) * 1000f) / 1000f);

        // Update the demo complete sign
        string gM_CP_P_A_B_AsString = (Mathf.Round((gM_CP_P_A + gM_CP_B) * 1000f) / 10f).ToString() + "%";

        if (ref_TextMesh != null)
            ref_TextMesh.text = gM_CP_P_A_B_AsString;
    }

    // This updates the world objects related to game completion percentage
    public void Update_GameWorld_CP_P()
    {
        //gM_CP_P = TEST_completeprecentage;

        // What happens at each x percentage point of game progress
        if (gM_CP_P <= 0.41f)
        {
            ref_Umber.Found_Progress(0);
            if (ref_Aeo.artifact_ActionState != 4)
                ref_Aeo.Found_Progress(0); // Aeo may develope themselves if they player meets them
            ref_Astor.Found_Progress(0);
            ref_Tulip.Found_Progress(0);
            ref_DemoGuy.Found_Progress(0);
        }
        else if (gM_CP_P <= 0.6f)
        {
            ref_Umber.Found_Progress(4);
            ref_Aeo.Found_Progress(4);
            ref_Astor.Found_Progress(4);
            ref_Tulip.Found_Progress(4);
            ref_DemoGuy.Found_Progress(0);
        }
        else if (gM_CP_P <= 0.8f)
        {
            ref_Umber.Found_Progress(5);
            ref_Aeo.Found_Progress(4);
            ref_Astor.Found_Progress(5);
            ref_Tulip.Found_Progress(5);
            ref_DemoGuy.Found_Progress(4);
        }
        // Have completion total be 19/22 to get bad ending.
        else if (gM_CP_P <= 0.83f)
        {
            // Narrative game end occurs at 80% completion (Above)
            ref_Umber.Found_Progress(2);
            ref_Aeo.Found_Progress(2);
            ref_Astor.Found_Progress(2);
            ref_Tulip.Found_Progress(2);
            ref_DemoGuy.Found_Progress(2);
        }
        // Two of the 22 puzzles that count to completion are very hard or very hidden. What do?
        // Have completion total be 19/22 to get bad ending.
        else if (gM_CP_P <= 1.00f)
        {
            ref_Umber.Found_Progress(2);
            ref_Aeo.Found_Progress(2);
            ref_Astor.Found_Progress(2);
            ref_Tulip.Found_Progress(2);
            ref_DemoGuy.Found_Progress(2);

            if (endingState == 0 && !badEnding_Lockout)
            {
                if (cr != null)
                {
                    StopCoroutine(cr);
                }
                // Don't operate ending until loading is complete
                // Don't operate the bad ending if the good ending has been reached successfully
                if (ending_WaitForLoad && !goodEnding_Saved)
                {
                    if (!badEnding_Saved)
                        cr = StartCoroutine(BadEnding());
                    else
                        cr = StartCoroutine(BadEnding_Short());
                }
            }  
        }

        // Undo the bad ending if you, somehow manage to impossibly 'unsolve' some puzzles
        // 19/22 puzzles
        if (gM_CP_P <= 0.83f)
        {
            // Stop the coroutine, return to the neutral ending
            if (endingState == 1)
            {
                if (cr != null)
                {
                    StopCoroutine(cr);
                }
                cr = StartCoroutine(Undo_BadEnding());
            }
        }
    }

    // This updates the world objects based on how many cloud layers have been breached
    public void Update_GameWorld_CP_B()
    {
        //gM_CP_B = TEST_BreachPercentage;

        // At 20%, start raining
        if (gM_CP_B <= 0.2f)
        {
            // Nothing
            ref_Rain.loop = false;
            Update_RainGrass(0);
        }
        else if (gM_CP_B <= 0.4f)
        {
            ref_Rain.loop = true;
            ref_Rain.emissionRate = 25;
            ref_Rain.Play();
            Update_RainGrass(1);
        }
        else if (gM_CP_B <= 0.6f)
        {
            ref_Rain.loop = true;
            ref_Rain.emissionRate = 100;
            ref_Rain.Play();
            Update_RainGrass(2);
        }
        else if (gM_CP_B <= 1.0f)
        {
            // Call the character changes a little bit before the game ends, just so
            // The player can speak to the characters before the game closes
            ref_Aeo.Found_Progress(6);
            ref_Astor.Found_Progress(6);
            ref_Tulip.Found_Progress(6);

            ref_Rain.loop = true;
            ref_Rain.emissionRate = 300;
            ref_Rain.Play();
            Update_RainGrass(3);
        }

        // The only way to undo the bad ending is by getting above 40% in rain?
        if (gM_CP_B <= 0.4f)
        {
            // You can access/reaccess the bad ending
            badEnding_Lockout = false;
        }
        else if (gM_CP_B <= 1.0f)
        {
            // You will be locked out of the bad ending
            badEnding_Lockout = true;

            // If you accidentally triggered the bad ending
            if (endingState == 1)
            {
                if (cr != null)
                {
                    StopCoroutine(cr);
                }
                cr = StartCoroutine(Undo_BadEnding());
            }
        }

        // For the game's ending state
        // There are currently 28 clouds to breach. All of them are breachable, but it is difficult and kinda of tedious
        // 21/28 clouds (Exactly 75%), is the easiest, without presenting too difficult a challenge
        // The good ending occurs at 90% clouds fluffed
        if (gM_CP_B < 0.75f)
        {
            if (endingState == 2)
            {
                if (cr != null)
                {
                    StopCoroutine(cr);
                }
                cr = StartCoroutine(Undo_GoodEnding());
            }
        }
        else
        {
            if (endingState != 2)
            {
                if (cr != null)
                {
                    StopCoroutine(cr);
                }
                // Don't operate ending until loading has finished
                if (ending_WaitForLoad)
                {
                    // If we haven't played the good ending before, play the longer version
                    if (!goodEnding_Saved)
                        cr = StartCoroutine(GoodEnding());
                    else
                        cr = StartCoroutine(GoodEnding_Short());

                }
            }
        }        
    }

    void Update_RainGrass(int pass_RainGrass_Mode)
    {
        // We only update the grass if the mode has changed
        if (store_RainGrass_Mode != pass_RainGrass_Mode)
        {
            store_RainGrass_Mode = pass_RainGrass_Mode;
            // Update each soil block with the current rain level
            // Check every Soil
            foreach (Sc_Cloud temp_Cloud in listOf_All_Clouds_ForReference)
            {
                if (temp_Cloud.bool_ContainsSoils)
                {
                    foreach (Sc_Block temp_Block in temp_Cloud.listOf_ChildBlocks)
                    {
                        if (temp_Block.self_BlockType == GlobInt.BlockType.Soil)
                        {
                            Sc_Block_Soil temp_Soil = (Sc_Block_Soil) temp_Block;
                            temp_Soil.Update_Grass(store_RainGrass_Mode);
                        }
                    }
                }
            }
        }
    }

    /////////////////
    // PERSISTANCE //
    /////////////////

	// Record the state of all game objects in the scene
    // In this version, the vector3's have been converted into floats
	public void Record_StateList_V2()
	{
        // Grab player position
        float[] pass_PlayerPosition = new float[3];
        pass_PlayerPosition[0] = player.ref_Discrete_Position.x;
        pass_PlayerPosition[1] = player.ref_Discrete_Position.y;
        pass_PlayerPosition[2] = player.ref_Discrete_Position.z;

        // Record all puzzle solve states, using the pG
        bool[] temp_listOf_Puzzle_SolvesState = new bool[pG.listOf_LevelManagers.Count];
        for (int i = 0; i < pG.listOf_LevelManagers.Count; i++)
        {
            // Store the level solve state in the index slot that matches it's generated id value
            temp_listOf_Puzzle_SolvesState[pG.listOf_LevelManagers[i].id] = pG.listOf_LevelManagers[i].bool_levelComplete;
        }

        // Record all the artifact action states
        int[] temp_listOf_Artifact_ActionStates = new int[nP.listOf_Artifacts.Count];
        for (int i = 0; i < nP.listOf_Artifacts.Count; i++)
        {
            // Store the artifact action state
            temp_listOf_Artifact_ActionStates[nP.listOf_Artifacts[i].id] = nP.listOf_Artifacts[i].artifact_ActionState;
        }

        // Record all the Storm positions
        float[][] temp_ListOf_Storm_Positions = new float[listOf_All_Storms_ForReference.Count][];
        for (int i = 0; i < listOf_All_Storms_ForReference.Count; i++)
        {
            temp_ListOf_Storm_Positions[i] = new float[3];
        }

        for (int b = 0; b < listOf_All_Storms_ForReference.Count; b++)
        {
            // Store the positions in order of storm id
            int id = listOf_All_Storms_ForReference[b].id;

            // I've changed this to localPosition, but I'm not sure if it does anything
            // Rounded to the nearest integer just in case
            temp_ListOf_Storm_Positions[id][0] = listOf_All_Storms_ForReference[b].ref_Discrete_Position.x;
            temp_ListOf_Storm_Positions[id][1] = listOf_All_Storms_ForReference[b].ref_Discrete_Position.y;
            temp_ListOf_Storm_Positions[id][2] = listOf_All_Storms_ForReference[b].ref_Discrete_Position.z;
            // Record the Storms list of cloud children (Not working?)
            //temp_listOf_listOf_Cloud_Children.Add(listOf_All_Storms_ForReference[id].listOf_Clouds_Child);
        }

        // Record all the Cloud positions to a temporary variable, which will then be passes into the StateList
        float[][] temp_ListOf_Cloud_Positions = new float[listOf_All_Clouds_ForReference.Count][];
        for (int i = 0; i < listOf_All_Clouds_ForReference.Count; i++)
        {
            temp_ListOf_Cloud_Positions[i] = new float[3];
        }
        // Record all the cloud's parents
        for (int b = 0; b < listOf_All_Clouds_ForReference.Count; b++)
        {
            // Store the positions in order of cloud id
            int id = listOf_All_Clouds_ForReference[b].id;

            //Need to set localPosition as there's inconsitancy in Unity's setting and loading of position
            //I'm just testing regular positioning first
            temp_ListOf_Cloud_Positions[id][0] = listOf_All_Clouds_ForReference[b].ref_Discrete_Position.x;
            temp_ListOf_Cloud_Positions[id][1] = listOf_All_Clouds_ForReference[b].ref_Discrete_Position.y;
            temp_ListOf_Cloud_Positions[id][2] = listOf_All_Clouds_ForReference[b].ref_Discrete_Position.z;
        }

        // Record all the Puff states
        // And soil states
        // And raft states
        List<bool> temp_ListOf_PuffStates = new List<bool>();
        List<bool> temp_ListOF_SoilStates = new List<bool>();
        List<bool> temp_ListOf_Lift_Height = new List<bool>();
        List<bool> temp_ListOf_Lift_isTriggeredOnce = new List<bool>();
        foreach (Sc_Block block in listOf_All_Blocks_ForReference)
        {
            if (block is Sc_Block_Puff)
            {
                Sc_Block_Puff temp_Puff = (Sc_Block_Puff)block;
                temp_ListOf_PuffStates.Add(temp_Puff.puffFull);
            }
            else if (block is Sc_Block_Soil)
            {
                Sc_Block_Soil temp_Soil = (Sc_Block_Soil)block;
                temp_ListOF_SoilStates.Add(temp_Soil.soilDry);
            }
            else if (block is Sc_Block_Lift)
            {
                Sc_Block_Lift temp_Lift = (Sc_Block_Lift)block;
                temp_ListOf_Lift_Height.Add(temp_Lift.lift_isRaised);
                temp_ListOf_Lift_isTriggeredOnce.Add(temp_Lift.isTriggeredOnce);
            }
        }

        RecStateList_V2 temp_RecStateList = new RecStateList_V2(pass_PlayerPosition, player.faceInt, temp_listOf_Puzzle_SolvesState, 
                                                temp_listOf_Artifact_ActionStates, temp_ListOf_PuffStates, temp_ListOf_Cloud_Positions, 
                                                temp_ListOf_Storm_Positions, temp_ListOF_SoilStates, temp_ListOf_Lift_Height, 
                                                temp_ListOf_Lift_isTriggeredOnce);
        
        // To reduce loading times, limit record statelist to 100 moves
        listOf_RecStates_New.AddElement(temp_RecStateList);

        // For testing purposes, lets save the game
        // SaveGame();  
    }

	// Restore the previous state of all game objects in the scene
	public void Restore_StateList_V2()
	{
        // If the recstatelist only has one element, play a noise
        if (listOf_RecStates_New.listOf_RecStateList.Count <= 1)
        {
            aC.Play_SFX("SFX_UI_Click");
        }

        // First, pull out the list from the class element
        RecStateList_V2 temp_RecState_V2 = listOf_RecStates_New.CallElement();

        // Update its discrete position value
        player.ref_Discrete_Position = new Vector3(temp_RecState_V2.rec_player_Position[0], temp_RecState_V2.rec_player_Position[1], temp_RecState_V2.rec_player_Position[2]);
        // Move the player into its previous state
        player.gameObject.transform.position = player.ref_Discrete_Position;
        // Update the face int
        player.faceInt = temp_RecState_V2.rec_player_faceInt;
        //We also need to physicall set the player direction
        player.ref_TransformComponents.rotation = Quaternion.Euler(rbV.faceInt_To_Euler[player.faceInt]);

        // Solve any previously solved puzzles
        for (int p = 0; p < pG.listOf_LevelManagers.Count; p++)
        {
            // Grab the puzzle id
            int id = pG.listOf_LevelManagers[p].id;

            // Activate the level complete function, if it's complete state is true
            if (temp_RecState_V2.rec_listOf_Puzzle_SolveState[id])
                pG.listOf_LevelManagers[p].Level_Complete();
        }
        // Once all the level's have been checked, run through the level complete function inside the pG
        pG.Level_Complete();

        // Replace in the correct action state any previous artifacts
        for (int a = 0; a < nP.listOf_Artifacts.Count; a++)
        {
            // Grab the artifact id
            int id = nP.listOf_Artifacts[a].id;

            // Update the corresponding artifact to it's correct action state
            nP.listOf_Artifacts[a].artifact_ActionState = temp_RecState_V2.rec_listOf_Artifact_ActionStates[id];
            // Update the artifact to reflect this loaded value
            nP.listOf_Artifacts[a].Update_Artifact();
        }

        // Reposition all the Storms and update their reference values
        for (int s = 0; s < listOf_All_Storms_ForReference.Count; s++)
        {
            // Using their reference id
            int id = listOf_All_Storms_ForReference[s].id;

            listOf_All_Storms_ForReference[s].ref_Discrete_Position = new Vector3(temp_RecState_V2.rec_listOf_Storm_Positions[id][0], temp_RecState_V2.rec_listOf_Storm_Positions[id][1], temp_RecState_V2.rec_listOf_Storm_Positions[id][2]);
            listOf_All_Storms_ForReference[s].gameObject.transform.position = listOf_All_Storms_ForReference[s].ref_Discrete_Position;
        }

        //Reposition all the Clouds
        for (int b = 0; b < listOf_All_Clouds_ForReference.Count; b++)
        {
            // Using their reference id
            int id = listOf_All_Clouds_ForReference[b].id;

            //Need to set localPosition as there's inconsitancy in Unity's setting and loading of position
            //I'm just testing regular positioning first
            listOf_All_Clouds_ForReference[b].ref_Discrete_Position = new Vector3(temp_RecState_V2.rec_listOf_Cloud_Positions[id][0], temp_RecState_V2.rec_listOf_Cloud_Positions[id][1], temp_RecState_V2.rec_listOf_Cloud_Positions[id][2]);
            listOf_All_Clouds_ForReference[b].gameObject.transform.position = listOf_All_Clouds_ForReference[b].ref_Discrete_Position;
        }

        // Restore all the Puff states
        // And Lift Heights
        foreach (Sc_Block block in listOf_All_Blocks_ForReference)
        {
            if (block is Sc_Block_Puff)
            {
                Sc_Block_Puff temp_Puff = (Sc_Block_Puff)block;
                temp_Puff.puffFull = temp_RecState_V2.rec_Puff_Full.First();
                // We should also update the Puff materials to reflect its current state
                temp_Puff.Update_Materials();
                // Then, delete the last item in the list
                temp_RecState_V2.rec_Puff_Full.Remove(temp_RecState_V2.rec_Puff_Full.First());
            }
            else if (block is Sc_Block_Soil)
            {
                Sc_Block_Soil temp_Soil = (Sc_Block_Soil)block;
                temp_Soil.soilDry = temp_RecState_V2.rec_Soil_State.First();
                //We should also update the Soil materials to reflect its current state
                temp_Soil.Update_Materials();
                //Then, delete the last item in the list
                temp_RecState_V2.rec_Soil_State.Remove(temp_RecState_V2.rec_Soil_State.First());
            }
            else if (block is Sc_Block_Lift)
            {
                Sc_Block_Lift temp_Lift = (Sc_Block_Lift)block;
                temp_Lift.lift_isRaised = temp_RecState_V2.rec_Lift_Height.First();
                temp_Lift.isTriggeredOnce = temp_RecState_V2.rec_Lift_isTriggeredOnce.First();
                //Then, delete the last item in the list
                temp_RecState_V2.rec_Lift_Height.Remove(temp_RecState_V2.rec_Lift_Height.First());
                temp_RecState_V2.rec_Lift_isTriggeredOnce.Remove(temp_RecState_V2.rec_Lift_isTriggeredOnce.First());
            }
        }

        //Delete the element we've just set the board to. This is inefficient as we're about to record these states again
        listOf_RecStates_New.RemoveLastElement();

        // Unglue and reglue storms?
        foreach (Sc_Storm temp_Storm in listOf_All_Storms_ForReference)
        {
            uM.CheckFor_Unglue(temp_Storm);
        }

        // After restoring the statelist, a new recording of the statelist must be made
        // This should not case any clashes because all recorded statelists occur after
        // All movement resolutions have been completed (Ideally)

        //Record_StateList();
        Record_StateList_V2();

        // Update the AfterAction function
        Player_HasPerformedAction_AfterAction();
    }

    // Record the player's puzzle progress
    public void Record_PlayerProgress()
    {
        // Scene/Zone
        current_RecPlayerProgress = new RecPlayerProgress(badEnding_Saved, goodEnding_Saved, current_Scene, gM_CP_P_A_B);
    }

    // Restore the player's puzzle progress
    public void Restore_PlayerProgress()
    {
        // When updating which levels are complete, this is more complex than just return the puzzle complete percentage
        badEnding_Saved = current_RecPlayerProgress.recState_BadEnding_Seen;
        goodEnding_Saved = current_RecPlayerProgress.recState_GoodEnding_Seen;

        // Scene
        Update_Scene(current_RecPlayerProgress.recState_Scene);

        // I don't need to update the gM_CP_PAB value, as this is calculate by the game, but I might as well store it
        gM_CP_P_A_B = current_RecPlayerProgress.recState_PP_PuzzlePercentage;
    }

    // This function tells the UM that we're ready to save the game
    public void SaveGame()
    {
        // Whenver we are saving the game, we don't want the player to move unexpectedly
        bool_IsSavingReady = true;
    }

    // This function gathers all the data I want to save, put it
    // Into a class and runs the persistance functions
    // NOTE: This function has a tiny tiny bit of lag and I want to know how to deal with it
    public void Act_SaveGame()
    {
        if (!bool_IsSavingActive)
        {
            bool_IsSavingActive = true;

            // First, create the masterData file
            MasterData pass_MasterData = new MasterData();
            // Pass the positions of all the pieces
            pass_MasterData.master_RecStat_AsList = listOf_RecStates_New;
            // Pass the player's progression
            Record_PlayerProgress();
            pass_MasterData.master_RecPlayerProgress = current_RecPlayerProgress;

            // Then, run the save command
            Persistance.Save_MasterData(saveID, pass_MasterData);

            // Then, allow the player to interact again
            bool_IsSavingActive = false;
            bool_IsSavingReady = false;
        }
    }

    // This is just for saving the nonthreading functions
    public void Act_SaveGame_NonThreading()
    {
        // Start the loading icon animating
        StartCoroutine(ref_TransitionScreen.UI_LoadingIcon_Hold(2f));

        // Used for the banner icon on the start menu, showing game completion percentage
        PlayerPrefs.SetFloat("saveID_" + saveID, gM_CP_P_A_B);
    }

    // This savegame is used when the player quits the game, which requires non-threaded saving
    public void Act_SaveGame_OnQuit()
    {
        // Just perform the above functions, but without the threading
        Act_SaveGame();
        Act_SaveGame_NonThreading();
    }

    // Vice versa as above, but for loading
    public IEnumerator LoadGame()
    {
        // First, create the empty masterData file
        temp_MasterData = new MasterData();

        // Start the thread for loading the masterData
        Thread thread_LoadGame = new Thread(Load_MasterData);
        thread_LoadGame.Start();

        while (isLoading_Thread)
        {
            yield return null;
        }

        //pass_MasterData = Persistance.Load_MasterData(saveID);

        // Just in case is fails
        try
        {
            // Load the positions of all the pieces
            listOf_RecStates_New = temp_MasterData.master_RecStat_AsList;

            // For the list of RecStates, this can be achieved just by pretending the player has moved back one space?
            Restore_StateList_V2();

            // Load the player completion progress
            current_RecPlayerProgress = temp_MasterData.master_RecPlayerProgress;
            Restore_PlayerProgress();
        }
        catch
        {
            print ("Failed to loaded masterData");
        }

        // Once everything's been loaded, we can change the boolean
        sH.isLoading = false;

        yield return null;
    }

    // Threaded section
    public void Load_MasterData()
    {
        temp_MasterData = Persistance.Load_MasterData(saveID);
        // Boolean here
        isLoading_Thread = false;
    }

    // This function switches how all the puzzle elements look
    public void Switch_ArtMode()
    {
        // Go to the puzzle manager and ask to change all the art
        if (isSimpleMode)
            pG.Switch_ArtMode(1);
        else
        {
            pG.Switch_ArtMode(0);
        }
    }

    // This coroutine is used to run the bad ending.
    public IEnumerator BadEnding()
    {
        // Update the ending state
        endingState = 1;

        // Play music separately here
        aC.Play_Music("Theme_Bad");
        lD.Update_LD(17);
        // Switch on the twirl
        lD.Switch_TwirlMode(true);

        // Start the evaportation
        StartCoroutine(Increase_Evaporation());

        // After an appropriate amount of time has passed, switch of all music related to atmosphere and music
        yield return new WaitForSeconds(38f);

        // Switch off the twirl early, because it actually looks kinda interesting
        lD.Switch_TwirlMode(false);

        yield return new WaitForSeconds(24f);

        // Stop all ambient music
        aC.Stop_Music();
        aC.Stop_Ambience();

        // Go into dark mode with the lighting
        lD.testing_CL_ColourSchemes = new Color(0f, 0f, 0f, 1f);
        lD.testing_DL_ColourSchemes = new Color(0f, 0f, 0f, 1f);
        lD.isTestingColors = true;
        
        // Turn off the tulip sucking thing
        ref_Tulip.gameObject.SetActive(false);

        // Play snapping effect
        aC.Play_SFX("SFX_Snap");

        // Mark that we've seen the bad ending cutscene
        // This is always true and never returns to false for the rest of the game
        badEnding_Saved = true;

        yield return null;
    }

    // This is the short version of the bad ending cutscene
    public IEnumerator BadEnding_Short()
    {
        // Update the ending state
        endingState = 1;
        // Update lighting
        lD.Update_LD(17);
        // Stop all ambient music
        aC.Stop_Music();
        aC.Stop_Ambience();
        // Go into dark mode with the lighting
        lD.testing_CL_ColourSchemes = new Color(0f, 0f, 0f, 1f);
        lD.testing_DL_ColourSchemes = new Color(0f, 0f, 0f, 1f);
        lD.isTestingColors = true;
        // Turn off the tulip sucking thing
        ref_Tulip.gameObject.SetActive(false);
        // Play snapping effect
        aC.Play_SFX("SFX_Snap");

        yield return null;
    }

    public IEnumerator Increase_Evaporation()
    {
        ref_CloudEvaporate.loop = true;
        ref_CloudEvaporate.emissionRate = 1;
        ref_CloudEvaporate.Play();

        for (float t = 0; t < 58.8f; t += Time.deltaTime)
        {
            // Slowly increase evaporation rate
            ref_CloudEvaporate.emissionRate = (int)t*t;
            yield return null;
        }
        
        // Stop the evaporation particle effect
        ref_CloudEvaporate.loop = false;

        yield return null;
    }

    public IEnumerator Undo_BadEnding()
    {
        // Update the ending state
        endingState = 0;

        // Play reverse snapping effect
        // This has a slight delay before the actual click hits
        aC.Play_SFX("SFX_Snap_Reversed");
        yield return new WaitForSeconds(0.1f);

        // Return to regular mode
        lD.isTestingColors = false;

        // Switch off the twirl effect
        lD.Switch_TwirlMode(false);

        // Return the tulip
        ref_Tulip.gameObject.SetActive(true);

        yield return null;
    }

    // For the good ending
    public IEnumerator GoodEnding()
    {
        // Update the ending state
        endingState = 2;

        // Play music separately here
        aC.Play_Music("Theme_Good");
        lD.Update_LD(18);

        // After an appropriate amount of time has passed, Exit the game
        yield return new WaitForSeconds(125f);

        // Just before we leave. Set that the player has witnessed the good ending, then save it
        goodEnding_Saved = true;
        // In this case, when all the ending states have been factored in, we're now back to state 0
        //endingState = 0;
        SaveGame();
        print ("Saving ready");

        yield return new WaitForSeconds(10f);

        // Stop all ambient music
        aC.Stop_Music();
        aC.Stop_Ambience();

        // Play snapping effect
        aC.Play_SFX("SFX_Snap");

        // Wait just a little bit before closing the game
        yield return new WaitForSeconds(0.2f);

        // Exit the game
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif

        yield return null;
    }

    // Short version of the good ending
    public IEnumerator GoodEnding_Short()
    {
        // Update the ending state
        // In this case, when all the ending states have been factored in, we're now back to state 0
        endingState = 2;
        // Return to normal lighting mode
        atmosphere_NormalMode = true;

        // Play music separately here
        //lD.Update_LD(18);

        // Play congratualations SFX
        aC.Play_SFX("SFX_PartyHorn");

        // Make the player shoot confetti
        ParticleSystem[] player_PS = player.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem temp_PS in player_PS)
        {
            temp_PS.Play();
        }

        // However, in this version, don't exit the game, just leave it as it is
        yield return null;
    }

    public IEnumerator Undo_GoodEnding()
    {
        // Update the ending state
        endingState = 0;

        // Play reverse snapping effect
        // This has a slight delay before the actual click hits
        aC.Play_SFX("SFX_Snap_Reversed");
        yield return new WaitForSeconds(0.1f);

        // Return to regular mode
        lD.isTestingColors = false;

        yield return null;
    }
}
