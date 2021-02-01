using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Threading;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_UM - Update Manager Script - V4
//	This script organises all block movement and input control in the game.
//	It contains the main game loop, but this is mainly controlled by the player input

// [Note*] in Check_Stack_CanMove
// [Note*] can't put faceInt update in animations script for some ordering reason I don't understand
// [Note*] Cheating with lifts for now
// [Note*] Operate_Lift_Once, I haven't written the CheckFor_Lift_Fall script yet

public class Sc_UM : MonoBehaviour {

	///////////////
	// VARIABLES //
	///////////////

	private Sc_GM gM;
	private Sc_SH sH;
	private Sc_RB_Values rbV;
	private Sc_RB_Animation rbA;
	private Sc_PG pG;
	private Sc_AC aC;

	// This integer controls what phase within the update cycle we're in
	int updatePhase = 1;

	// Stop taking inputs if the game is paused
	[System.NonSerialized]
	public bool boolCheck_Paused = false;

	/////////////////////////////
	// REFERENCE SCENE OBJECTS //
	/////////////////////////////

	private Sc_Player player;
	// We need to keep a list of Storms that have been checked
	public List<Sc_Storm> listOf_AllStorm_Info;
	// This check occurs after movement, if the Storm has moved or experienced an action
	// We need to check it again to see if it needs to update
	public List<Sc_Storm> listOf_AllStorm_DoubleCheck;
	// Storms[n movement direction][f formation number][ly layer number]
	// A list to mark which Storms need to be checked and which storms are good to move next turn
	public List<List<List<Sc_Storm>>>[] listOf_Storms_Movement_Check;
	public List<List<List<Sc_Storm>>>[] listOf_Storms_Movement_Confirm;

	// TEST - This is for a rotor mechanic. It's in it's very simple stages atm
	public List<Sc_Rotor>[] listOf_Rotors;
	
	////////////////////////
	// MOVEMENT VARIABLES //
	////////////////////////

	// These booleans stop any input from being received after an input has been confirmed
	// But before an action has taken place
	[System.NonSerialized]
	public bool actBool_Input = false;

	// While this window is true, we can gather information about where the player wants to move next
	// This window becomes closed for a percentage at the beginning of player movement
	[System.NonSerialized]
	public bool actBool_GatherWindow = true;

	// These booleans stop any input from being received while an object is moving
	[System.NonSerialized]
	public bool moveBool_Lift = false;
	[System.NonSerialized]
	public bool moveBool_Rotor = false;
	[System.NonSerialized]
	public bool moveBool_Storm = false;
	[System.NonSerialized]
	public bool moveBool_Player = false;

	[System.NonSerialized]
	public bool rotStage_Two = false;
	[System.NonSerialized]
	public bool resetLevel_Twice = false;	// Hotfix for gluing/ungluing bug

	// To check if any chain of Storms can move
	List<List<bool>>[] listOf_BoolCheck_Chain_CanMove;

	// Stop player movement earlier if the player decides to move again


	///////////
	// START //
	///////////

	void Start()
	{
		// Grab the world variables
		gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
		sH = gM.sH;
		pG = gM.pG;
		aC = gM.aC;
        rbV = gM.rbV;
        rbA = gM.rbA;

        player = gM.player;

		Initialise_Lists();
	}

	///////////////
	// GAME LOOP //
	///////////////
	void Update()
	{
		//--//--//
		// PHASE ONE - Process Player

		// Grab player input, process it and move the player or any relevent Storms
		// Don't do this while the game is saving
		if (actBool_GatherWindow && !boolCheck_Paused && !sH.isLoading && updatePhase == 1)
		{
			///////////////
			// SAVE GAME //
			///////////////

			// If the GM has allowed us to save the game, we do that here using a separate thread
			if (gM.bool_IsSavingReady)
			{
				// This method has lag
				//gM.Act_SaveGame();

				// Save the game information using threading
				Thread thread_SaveGame = new Thread(gM.Act_SaveGame);
        		thread_SaveGame.Start();

				// Perform saving methods that can't be called through threading
				// Playerprefs saves + loading icon animation
				gM.Act_SaveGame_NonThreading();			
			}

			///////////////////////////
			// GATHERING INFORMATION //
			///////////////////////////

			// If there are any movements queued up, we should perform those first
			if ((rbA.listOf_QueueMovement_Translate.Count + rbA.listOf_QueueMovement_Rotate.Count + rbA.listOf_QueueMovement_Lift.Count) > 0)
			{
				updatePhase = 2;
			}
			// Otherwise, we collect player input
			else
			{
				// Don't process player movement if we're in freeCam mode
				if (!gM.isFreeCam_On)
				{
					// Find out what action the player wants to take
					// Ignore this if we're processing the second stage of rotation
					if (!rotStage_Two)
					{
						Process_Input();
					}
				}

				// If the player's action is movement
				if ((player.stored_Movement != GlobInt.MoveOption.none) || (actBool_Input))
				{
					// Find out which Storms are sitting beside the player
					player.Record_Neighbours();

					// Collect all Storms near the player
					Gather_Storms();

					////////////////////
					// CHECK MOVEMENT //
					////////////////////

					if (player.stored_Movement != GlobInt.MoveOption.none)
						// Take player input and process the movement of the player and the storms
						Process_Movement();
				
					///////////
					// RESET //
					///////////

					// Reset the player and Storm scripts
					// This must be separate from the regular reset_Lists as UM runs before thes scripts have time to initialise
					Reset_Block_Lists();

					Reset_Lists();

					if (actBool_Input)
					{
						updatePhase = 2;
						// Reset the input bool state
						actBool_Input = false;
					}
				}
			}
		}

		//--//--//
		// PHASE TWO - Perform movements

		if (!AreObjectsMoving() && !boolCheck_Paused && updatePhase == 2)
		{
			// Move all objects, then go to the next step
			rbA.Run_QueueuedMovements();
			// Play all relevent sounds?
			aC.Play_SFXList();

			updatePhase = 3;
		}

		//--//--//
		// PHASE THREE - Check until empty

		// After the player's input has been process, we run through and check preceeding Storms until our
		// DoubleCheck list is empty
		if (!AreObjectsMoving() & !boolCheck_Paused & updatePhase == 3)
		{
			///////////////////////
			// CHECK UNTIL EMPTY //
			///////////////////////

			// If, after resolving all movement, there are no elements left in the DoubleCheck list
			// We can move onto the next phase
			if (listOf_AllStorm_DoubleCheck.Count <= 0)
				updatePhase = 4;
			// Otherwise, we need to move anything that was found to have a second movement stage
			else
				updatePhase = 2;

			CheckUntil_Empty();
		}

		//--//--//
		// PHASE FOUR - Record all current states of objects, once
		
		// Record all the current states of each game object
		if (!boolCheck_Paused && updatePhase == 4)
		{
			// Record all object states once, only after the second rotation stage has finished
			if (!rotStage_Two)
			{
				// We record the movement if the player has actually moved
				if (player.stored_Movement != GlobInt.MoveOption.none)
        			gM.Record_StateList_V2();
			}

			// First, we should reset the Storms within the Infor list, as all movement has finished occuring
			//Reset_Block_Lists();
			// After the individual Storms have finished moving, we need to reset the Info lists itself
			// So that this list doesn't build up with every single Storm after a while
			listOf_AllStorm_Info = new List<Sc_Storm>();

			// Return to waiting for the player's input
			updatePhase = 1;
		}
	}

	/////////////////////
	// BASIC FUNCTIONS //
	/////////////////////

	// This list initialisation only happens upon startup
	void Initialise_Lists()
	{
		// These lists will always be the size of the fixed cardinal directions
		listOf_Storms_Movement_Check = new List<List<List<Sc_Storm>>>[rbV.cDVt];
		listOf_Storms_Movement_Confirm = new List<List<List<Sc_Storm>>>[rbV.cDVt];

		// TEST - For rotors
		listOf_Rotors = new List<Sc_Rotor>[rbV.cDVt];

		listOf_BoolCheck_Chain_CanMove = new List<List<bool>>[rbV.cDVt];

		listOf_AllStorm_Info = new List<Sc_Storm>();

		// Then perform the reset of the list setup
		Reset_Lists();
	}

	// This list reset occurs after all movement has finished occuring
	void Reset_Lists()
	{
		// Setup new lists for each cardinal direction
		for (int n = 0; n < rbV.cDVt; n++)
        {
			// Setup zero case for direction
			listOf_Storms_Movement_Check[n] = new List<List<List<Sc_Storm>>>();
			listOf_Storms_Movement_Confirm[n] = new List<List<List<Sc_Storm>>>();

			// TEST - For rotors
			listOf_Rotors[n] = new List<Sc_Rotor>();

			listOf_BoolCheck_Chain_CanMove[n] = new List<List<bool>>();
		}
	}

	// This list resets any blocks that need resetting
	void Reset_Block_Lists()
	{
		// Reset the player lists
		player.Reset_Lists();
		// Reset the Storms inside the Info list
		foreach (Sc_Storm temp_Storm in listOf_AllStorm_Info)
			temp_Storm.Reset_Lists();
	}

	// Stops the update function if anything is moving
	bool AreObjectsMoving()
	{
		if (!moveBool_Player && !moveBool_Storm && !moveBool_Lift && !moveBool_Rotor)
			return false;
		return true;
	}

	// This is a generic function that checks if a Storm is already inside a list
	public void CheckAndAdd_StormToList(List<Sc_Storm> Pass_List, Sc_Storm pass_Storm)
	{
		if (!Pass_List.Contains(pass_Storm))
			Pass_List.Add(pass_Storm);
	}

	// To make my life easier, this simple script add any pass Storm into the DoubleCheck list
	// If it isn't already there
	public void AddStorm_ToDoubleCheckList(Sc_Storm pass_Storm)
	{
		CheckAndAdd_StormToList(listOf_AllStorm_DoubleCheck, pass_Storm);
	}

	/////////////////////
	// LARGE FUNCTIONS //
	/////////////////////

	// Process and perform movement operations
	void Process_Movement()
	{
		// Check if there are no basic obstructions in the player movement direction
		// This also takes into account rotational movement
		if (player.CheckIf_Player_NoObstructions())
		{
			// Now, we check if the Storms can move

			//////////////////
			// For Translation
			if (!player.isPlayerRotating)
			{
				// For the movement direction the player plans to move in
				// Perform the chain cycle on each formation
				for (int fr = 0; fr < listOf_Storms_Movement_Check[player.moveInt].Count; fr++)
				{
					Chain_Creation_Cycle(player.moveInt, fr);
				}		

				// Next, for each confirmed fomration, check if each layer can move
				for (int fr = 0; fr < listOf_Storms_Movement_Confirm[player.moveInt].Count; fr++)
				{
					Check_Chain_CanMove(player.moveInt, fr);
				}

				// Finally, for each formation, we check if the stack can move based on the layer below it
				for (int fr = 0; fr < listOf_Storms_Movement_Confirm[player.moveInt].Count; fr++)
				{
					Check_Stack_CanMove(player.moveInt, fr);
				}

				// Before we move anything, we just need to check that the player's path is clear
				// As Storms sitting on top the player do not obstruct movement, but Storms in the player's path, do
				// (The Storms in front of the player's movement)
				bool checkBool_StormIsBlockingPlayer = false;
				Sc_Storm temp_Storm = player.Grab_ObstructingStorm();
				// If there is a Storm in the way
				if (temp_Storm != null)
				{
					// Check if Storm can move in all formations and layers
					for (int i = 0; i < temp_Storm.stormInfo_Values[0].Count; i++)
					{
						// For this direction
						if (temp_Storm.stormInfo_Values[0][i] == player.moveInt)
						{
							// All formation and layer values must be able to move
							if (!listOf_BoolCheck_Chain_CanMove[player.moveInt][temp_Storm.stormInfo_Values[1][i]][temp_Storm.stormInfo_Values[2][i]])
							{
								checkBool_StormIsBlockingPlayer = true;
							}
						}
					}
				}

				// As long as no Storms are blocking player movement, the player can move freely
				if (!checkBool_StormIsBlockingPlayer)
				{
					// Go through each confirmed formation
					for (int fr = 0; fr < listOf_Storms_Movement_Confirm[player.moveInt].Count; fr++)
					{
						// Go through each layer that has content
						for (int ly = 0; ly < listOf_Storms_Movement_Confirm[player.moveInt][fr].Count; ly++)
						{
							// And each Storm
							foreach (Sc_Storm part_Storm in listOf_Storms_Movement_Confirm[player.moveInt][fr][ly])
							{
								// Queue up all these Storms to be moved
								rbA.listOf_QueueMovement_Translate.Add(new QueueMovement_Translate(0, part_Storm.transform, player.moveInt));
							}
						}
					}

					// Then add the player to the queue
					rbA.listOf_QueueMovement_Translate.Add(new QueueMovement_Translate(1, player.transform, player.moveInt));

					// The place has performed movement, allow passage to next phase
					updatePhase = 2;
				}
			}
			
			///////////////
			// For Rotation
			else
			{
				// For a rotational cases, we will need to check two directions together
				foreach (int pass_MoveInt in player.rot_moveInt)
				{
					// Perform this check on each formation
					for (int fr = 0; fr < listOf_Storms_Movement_Check[pass_MoveInt].Count; fr++)
					{
						Chain_Creation_Cycle(pass_MoveInt, fr);
					}
				}

				foreach (int pass_MoveInt in player.rot_moveInt)
				{
					for (int fr = 0; fr < listOf_Storms_Movement_Confirm[pass_MoveInt].Count; fr++)
					{
						Check_Chain_CanMove(pass_MoveInt, fr);
					}
				}

				// Is the storm accidently added to the wrong layer here?
				//print (_TF_SizeOf_CheckingList(listOf_Storms_Movement_Check, 1));

				foreach (int pass_MoveInt in player.rot_moveInt)
				{
					for (int fr = 0; fr < listOf_Storms_Movement_Confirm[pass_MoveInt].Count; fr++)
					{
						Check_Stack_CanMove(pass_MoveInt, fr);
					}
				}

				// So, the problem so far is that the check_stack_canMove is deleting the list of moving shapes?

				// Then, move all the Storms in the confirmed list for both direction cases
				foreach (int pass_MoveInt in player.rot_moveInt)
				{
					// Go through each confirmed formation
					for (int fr = 0; fr < listOf_Storms_Movement_Confirm[pass_MoveInt].Count; fr++)
					{
						// Go through each layer that has content
						for (int ly = 0; ly < listOf_Storms_Movement_Confirm[pass_MoveInt][fr].Count; ly++)
						{
							// And each Storm
							foreach (Sc_Storm part_Storm in listOf_Storms_Movement_Confirm[pass_MoveInt][fr][ly])
							{
								// Queue up all these Storms to be moved
								rbA.listOf_QueueMovement_Translate.Add(new QueueMovement_Translate(0, part_Storm.transform, pass_MoveInt));
							}
						}
					}
				}

				// [Note*] Issue here with the faceInt stored inside the player script. It needs to update just before anything moves, but there's
				// Something wrong with my code such that I can't update the faceInt inside the ref_Animations script
				if (rotStage_Two)
				{
					//Update the player faceInt
					if (player.stored_Movement == GlobInt.MoveOption.clockwise)
						player.faceInt = (player.faceInt + 1) % rbV.cD;
					else
						player.faceInt = (player.faceInt + rbV.cD - 1) % rbV.cD;
				}

				// TEST - Animated the rotors
				foreach (int pass_MoveInt in player.rot_DirInt)
				{
					foreach (Sc_Rotor temp_Rotor in listOf_Rotors[pass_MoveInt])
					{
						rbA.listOf_QueueMovement_Rotate.Add(new QueueMovement_Rotate(1, player.stored_Movement, temp_Rotor.transform, rotStage_Two));
					}
				}

				// Finally, Add the player's rotational movement to the queue
				rbA.listOf_QueueMovement_Rotate.Add(new QueueMovement_Rotate(0, player.stored_Movement, player.transform, rotStage_Two));

				// Enter rotation stage two
				rotStage_Two = !rotStage_Two;

				// The place has performed movement, allow passage to next phase
				updatePhase = 2;
			}
		}
	}

	// Go through any Storms that moved or changed previously and check if they still require action
	void CheckUntil_Empty()
	{	
		// I could do lifts first, as a priority
		// This would also require performing record neighbours for the player
		// THIS IS VERY SLAPDASH FOR THE SAKE OF VISUALS
		player.Record_Neighbours();
		CheckFor_Lifts_Player();

		// We need to make sure we check each Storm once per passing
		List<Sc_Storm> listOf_AllStorm_DoubleCheck_Once = new List<Sc_Storm>();
		// If a Storm is found again, we pass it to the next iteration cycle
		List<Sc_Storm> listOf_AllStorm_DoubleCheck_Pass = new List<Sc_Storm>();

		// Grab the Storm_Info that we've been collecting this cycle. It's worth checking them all again.
		// Just in case there are stack Storms ontop of translating Storms
		foreach (Sc_Storm temp_Storm in listOf_AllStorm_Info)
			AddStorm_ToDoubleCheckList(temp_Storm);

		int failSafe_dc = 0;

		// This will be a while loop that will keep perform actions on Storms until they stop changing
		while (listOf_AllStorm_DoubleCheck.Count > 0 && failSafe_dc < 100)
		{
			// Grab the Storm first, then remove it from the list (In case we need to re-add it later)
			Sc_Storm temp_Storm = listOf_AllStorm_DoubleCheck[0];
			listOf_AllStorm_DoubleCheck.Remove(temp_Storm);
			// Put this Storm into a store for reference
			// We will check it again, but on a second passing
			if (!listOf_AllStorm_DoubleCheck_Once.Contains(temp_Storm))
			{
				listOf_AllStorm_DoubleCheck_Once.Add(temp_Storm);

				// First, re-record the neighbours around the Storm
				temp_Storm.Collect_Neighbours();

				// Check if Puff/Soil are beside a Soil/Puff and if watering can occur
				CheckFor_Watering(temp_Storm);

				// Check if Storms can fall
				CheckFor_Fall(temp_Storm);

				// Check if moved Storm has landed on a lift (Or moved Storm is a lift)
				CheckFor_Lifts(temp_Storm);

				// Check if glued
				CheckFor_Glue(temp_Storm);

				// For now, just reset and the Storm
				temp_Storm.Reset_Lists();
			}
			else
				listOf_AllStorm_DoubleCheck_Pass.Add(temp_Storm);
		}

		if (failSafe_dc >= 100)
			print ("Failsage DC while loop has failed!");

		// After movement has occured, check if any active levels have been completed
		gM.Check_ActiveLevelComplete();

		// Move everything from the Store into the DoubleCheck
		foreach (Sc_Storm temp_Storm in listOf_AllStorm_DoubleCheck_Pass)
			listOf_AllStorm_DoubleCheck.Add(temp_Storm);

		// Reset the Storms, so they can record neighbours again in the next pass
		Reset_Block_Lists();

		// Let everything move before repeating the DoubleCheckList
		// We don't change phases until the DoubleCheckList is empty
	}

	///////////////////////////
	// GATHERING INFORMATION //
	///////////////////////////

	// Find out what action the player wants to take
	void Process_Input()
	{
		// This is handled in the player script
		player.Sort_Movement();
	}

	// Collect all Storms nearby the player
	void Gather_Storms()
	{
		// This is handled in the player script
		player.Pass_Neighbours();
	}

	// Add Storm to checking list and update stormInfo
	// Also, make sure other similar lists also have the same space available for Storm information
	public void AddStorm_ToCheckList(Sc_Storm pass_Storm, int pass_n, int pass_fr = -1, int pass_ly = -1)
	{
		// First, we add this Storm into the list of Storms we've checked (no duplicates)
		CheckAndAdd_StormToList(listOf_AllStorm_Info, pass_Storm);

		// Check if storm is unwatered? - TEST
		CheckFor_Unglue(pass_Storm);

		// First, we need to perform Record Neighbours/Collect Neighbours for this Storm
		pass_Storm.Collect_Neighbours();

		// If passed a non-movement int of -1, we just skip everything
		if (pass_n == -1)
		{
			// Do Nothing
		}
		// If passed only a direction, we pass the Storm to a new formation number and layer number
		else if (pass_fr == -1 && pass_ly == -1)
		{
			bool boolCheck_FoundEmptyFormation = false;

			// Find the first empty list for formation number
			for (int fr = 0; fr < listOf_Storms_Movement_Check[pass_n].Count; fr++)
        	{
				if (listOf_Storms_Movement_Check[pass_n][fr].Count == 0)
				{
					// We found an empty formation
					boolCheck_FoundEmptyFormation = true;

					// Creat the new list for this layer number at zero
					listOf_Storms_Movement_Check[pass_n][fr].Add(new List<Sc_Storm>());
					// Add the Storm to this list
					listOf_Storms_Movement_Check[pass_n][fr][0].Add(pass_Storm);

					// Update stormInfo
					Pass_StormInfo(pass_Storm, pass_n, fr, 0);
				}

				// If we found an empty formation, we break out of the for loop
				if (boolCheck_FoundEmptyFormation)
					break;
			}

			// If we didn't find an empty formation, we create a new one
			if (!boolCheck_FoundEmptyFormation)
			{
				// Create a new formation list
				listOf_Storms_Movement_Check[pass_n].Add(new List<List<Sc_Storm>>());
				// And a new layer list
				listOf_Storms_Movement_Check[pass_n][0].Add(new List<Sc_Storm>());
				// Add the passed Storm
				listOf_Storms_Movement_Check[pass_n][0][0].Add(pass_Storm);

				// Update stormInfo
				Pass_StormInfo(pass_Storm, pass_n, 0, 0);
			}
		}
		// If passed a direction and a formation number, we pass the Storm to a new layer number
		else if (pass_ly == -1)
		{

			// First, make sure the storm isn't already in this formation
			if (!CheckIf_Storm_AlreadyInsideFormation(pass_Storm, pass_n, pass_fr))
			{
				// Find either an empty layer, or create a new empty layer
				bool boolCheck_FoundEmptyLayer = false;

				// Find the first empty layer number
				for (int ly = 0; ly < listOf_Storms_Movement_Check[pass_n][pass_fr].Count; ly++)
				{
					if (listOf_Storms_Movement_Check[pass_n][pass_fr][ly].Count == 0)
					{
						// We found an empty layer
						boolCheck_FoundEmptyLayer = true;

						// Add the storm to that layer
						listOf_Storms_Movement_Check[pass_n][pass_fr][ly].Add(pass_Storm);

						// Update stormInfo
						Pass_StormInfo(pass_Storm, pass_n, pass_fr, ly);
					}

					// If we found an empty layer, we break out of the for loop
					if (boolCheck_FoundEmptyLayer)
						break;
				}

				// If we didn't find an empty layer, we create a new one
				if (!boolCheck_FoundEmptyLayer)
				{
					// And a new layer list
					listOf_Storms_Movement_Check[pass_n][pass_fr].Add(new List<Sc_Storm>());
					// Add the passed Storm to the new layer (Using .Last())
					listOf_Storms_Movement_Check[pass_n][pass_fr].Last().Add(pass_Storm);

					// Update stormInfo
					Pass_StormInfo(pass_Storm, pass_n, pass_fr, 0);
				}
			}
		}

		// If passed all 3 ints, we add it to that specific chain
		else
		{
			// First, make sure the storm isn't already in this formation
			if (!CheckIf_Storm_AlreadyInsideFormation(pass_Storm, pass_n, pass_fr))
			{
				// Add the passed Storm
				listOf_Storms_Movement_Check[pass_n][pass_fr][pass_ly].Add(pass_Storm);

				// Update stormInfo
				Pass_StormInfo(pass_Storm, pass_n, pass_fr, pass_ly);
			}
		}
	}
	
	// Updates the storm values when being addded to the checking list
	void Pass_StormInfo(Sc_Storm pass_Storm, int pass_n, int pass_fr, int pass_ly)
	{
		// Add each of these values to the stormInfo_Values, such that their indexes are the same
		pass_Storm.stormInfo_Values[0].Add(pass_n);
		pass_Storm.stormInfo_Values[1].Add(pass_fr);
		pass_Storm.stormInfo_Values[2].Add(pass_ly);
	}

	// Make sure we don't add Storms repetitively to the formation they're already in
	bool CheckIf_Storm_AlreadyInsideFormation(Sc_Storm pass_Storm, int pass_n, int pass_fr)
	{
		bool boolCheck_Storm_AlreadyInFormation = false;

		// Cases where:
			// Storm we're checking already has a formation number?
			// Storm we're checking already has a layer number?

		// This may be the first Storm we've come across
		// First check the passed formation list exists
		if (listOf_Storms_Movement_Check[pass_n].Count > pass_fr)
		{
			// Then, check if this Storm exists in that formation
			for (int ly = 0; ly < listOf_Storms_Movement_Check[pass_n][pass_fr].Count; ly++)
			{
				if (listOf_Storms_Movement_Check[pass_n][pass_fr][ly].Contains(pass_Storm))
				{
					boolCheck_Storm_AlreadyInFormation = true;
				}
			}
		}

		if (boolCheck_Storm_AlreadyInFormation)
			return true;
		else
			return false;
	}

	///////////////////
	// CREATE CHAINS //
	///////////////////

	// This function creates the chains of Storms that are moving together for each Formation
	void Chain_Creation_Cycle(int n, int fr)
	{
		// Check at each layer
		int ly = 0;
		// Failsafe
		int failSafe_ly = 0;

		while ((ly < listOf_Storms_Movement_Check[n][fr].Count) && (failSafe_ly < 100))
		{
			failSafe_ly += 1;
			int failSage_st = 0;

			// Go through each Storm in this list and, if it can move, place it in the confirmed list
			while ((listOf_Storms_Movement_Check[n][fr][ly].Count > 0) && (failSage_st < 100))
			{

				failSage_st += 1;

				// Slighty easier to storm the Storm as a variable
				Sc_Storm temp_Storm = listOf_Storms_Movement_Check[n][fr][ly][0];

				// If a Storm is moving in a cardinal direction, it will slide other stacked Storms
				// Check which Storms are stacked on this one
				if (n < rbV.cD)
				{
					Stack_Creation_Cycle(temp_Storm, n, fr);
				}

				// 1 - Check there are no walls
				if (!temp_Storm.listOf_Radial_Wall_Check[n])
				{
					// 2 - Check the player isn't in the way or is moving in the same direction
					if ((!temp_Storm.listOf_Radial_Player_Check[n]) ||
						(temp_Storm.listOf_Radial_Player_Check[n] &&
									!player.isPlayerRotating &&	(player.moveInt == n)))
					{
						// Go through every Storm that's in the direction the pass_Storm wants to move, which I call 'close_Storms'
						foreach (Sc_Storm close_Storm in temp_Storm.listOf_Radial_Storm_Check[n])
						{
							// If the close_Storm is a lift, but stationary, it acts as a regular Storm
							if (!close_Storm.isLift || (close_Storm.isLift && close_Storm.ref_LiftScript.lift_isStationary))
							{
								// Add this new storm to the checking list
								AddStorm_ToCheckList(close_Storm, n, fr, ly);
							}
						}
					}
				}	

				// Once we've checked this Storm, move it from the checking list to the confirmed list
				// Create any part of the checking list that doesn't already exist
				// (+1 because fr is an index)
				// Create the n direction list
				if (listOf_Storms_Movement_Confirm[n] == null)
				{
					listOf_Storms_Movement_Confirm[n].Add(new List<List<Sc_Storm>>());
				}
				// Check if its full
				for (int i = 0; i < ((fr + 1) - listOf_Storms_Movement_Confirm[n].Count); i++)
				{
					listOf_Storms_Movement_Confirm[n].Add(new List<List<Sc_Storm>>());
					listOf_Storms_Movement_Confirm[n][fr].Add(new List<Sc_Storm>());
				}
				// Create the formation list
				if (listOf_Storms_Movement_Confirm[n][fr] == null)
				{
					listOf_Storms_Movement_Confirm[n][fr].Add(new List<Sc_Storm>());
				}
				for (int i = 0; i < ((ly + 1) - listOf_Storms_Movement_Confirm[n][fr].Count); i++)
				{
					listOf_Storms_Movement_Confirm[n][fr].Add(new List<Sc_Storm>());
				}
				
				listOf_Storms_Movement_Confirm[n][fr][ly].Add(temp_Storm);
				listOf_Storms_Movement_Check[n][fr][ly].Remove(temp_Storm);
			}

			//print (listOf_Storms_Movement_Confirm[n][fr][ly].Count);

			if (failSage_st >= 100)
				print ("Failsafe ST has failed");

			// Increment ly, to check any new layers that have been added
			ly += 1;
		}

		if (failSafe_ly >= 100)
		{
			print ("Failsafe LY has failed");
		}
	}

	// This functions grabs any Storms that might be sitting on top the current Storm
	void Stack_Creation_Cycle(Sc_Storm pass_Storm, int n, int fr)
	{
		// Are there Storms above the one we're checking?
		if (pass_Storm.listOf_Radial_Storm_Check[rbV.vtUp].Count != 0)
		{
			// Go through each of these
			foreach (Sc_Storm temp_Storm in pass_Storm.listOf_Radial_Storm_Check[rbV.vtUp])
			{
				// Add them to the checking list on their own new layer (Don't pass the layer value)
				AddStorm_ToCheckList(temp_Storm, n, fr);
			}
		}
	}

	////////////////////
	// CHECK MOVEMENT //
	////////////////////

	// Check if each layer can move
	void Check_Chain_CanMove(int pass_n, int pass_fr)
	{
		// Go through each layer
		for (int ly = 0; ly < listOf_Storms_Movement_Confirm[pass_n][pass_fr].Count; ly++)
        {
			// We need to expand the listOf_BoolCheck_Chain_CanMove
			// Create any part of the checking list that doesn't already exist
			// (+1 because fr is an index)
			// We set them to true, initially suggesting all layered chains can move
			// Create the n direction list
			if (listOf_BoolCheck_Chain_CanMove[pass_n].Count == 0)
				listOf_BoolCheck_Chain_CanMove[pass_n].Add(new List<bool>());
			// Check if the list is full
			for (int i = 0; i < ((pass_fr + 1) - listOf_BoolCheck_Chain_CanMove[pass_n].Count); i++)
			{
				listOf_BoolCheck_Chain_CanMove[pass_n].Add(new List<bool>());
				listOf_BoolCheck_Chain_CanMove[pass_n][pass_fr].Add(true);
			}
			// Create the formation list
			if (listOf_BoolCheck_Chain_CanMove[pass_n][pass_fr].Count == 0)
				listOf_BoolCheck_Chain_CanMove[pass_n][pass_fr].Add(true);
			// Check if the list is full
			for (int i = 0; i < ((ly + 1) - listOf_BoolCheck_Chain_CanMove[pass_n][pass_fr].Count); i++)
				listOf_BoolCheck_Chain_CanMove[pass_n][pass_fr].Add(true);

			// If any Storms in the chain can't move, no Storm in the chain can move
			bool boolCheck_CanMove = true;

			// Check each element in the chain can move
			foreach (Sc_Storm temp_Storm in listOf_Storms_Movement_Confirm[pass_n][pass_fr][ly])
			{
				if (!Check_CanMove_Translation(temp_Storm, pass_n))
					boolCheck_CanMove = false;
				if (!Check_CanMove_RestingOrder(temp_Storm, pass_n))
					boolCheck_CanMove = false;
			}

			// If any Storm in the chain can't move, record that the whole chain layer can't move
			if (!boolCheck_CanMove)
				listOf_BoolCheck_Chain_CanMove[pass_n][pass_fr][ly] = false;
		}
	}

	// Translation Rule - For Storms being pushed
	bool Check_CanMove_Translation(Sc_Storm pass_Storm, int pass_n)
	{
		// If this local boolean fails, return false
		bool boolCheck_CanMove = true;

		// Translation Rules

		// 1 - If a wall is in the way
		if (pass_Storm.listOf_Radial_Wall_Check[pass_n])
		{
			boolCheck_CanMove = false;
		}

		// 2 - If there's a player in the way
		if (pass_Storm.listOf_Radial_Player_Check[pass_n])
		{
			// 2a - And is not moving in the same direction as this Storm
			if (player.isPlayerRotating || player.moveInt != pass_n)
				boolCheck_CanMove = false;

		}

		// 3 - If the player is sitting on the Storm (And paperweight mode is on)
		if (pass_Storm.listOf_Radial_Player_Check[rbV.vtUp] && gM.isPlayerPaperWeight)
			boolCheck_CanMove = false;

		// 4 - For ghost block bridges; if the player rotates whilst inside a ghost block that belongs
		// To the same Storm. The Shape won't move only when the player is floating on a ghost block
		// (IE, standing only on the ghost block)
		if (gM.isDryPuffBridge && player.isPlayerRotating &&
					player.player_StandingOnGhostBridge.Contains(pass_Storm) && player.player_isPlayerFloating)
			boolCheck_CanMove = false;
		
		// 5 - If the Storm the player is about to walk on also happens to be the Storm the player is currently pushing
		if (player.listOf_Radial_CloseFloors_Storm_Check[pass_n].Contains(pass_Storm))
			boolCheck_CanMove = false;

		// TESTING
		// 6 - If there's a Storm in the way, which a player is sitting on
		foreach (Sc_Storm temp_Storm in pass_Storm.listOf_Radial_Storm_Check[pass_n])
		{
			// Is the player sitting on this?
			if (temp_Storm.listOf_Radial_Player_Check[rbV.vtUp])
			{
				// In that case, we can't move
				boolCheck_CanMove = false;
			}
		}

		if (!boolCheck_CanMove)
			return false;
		return true;
	}

	// Resting Order Rules - When Storms are dragged from beneath
	bool Check_CanMove_RestingOrder(Sc_Storm pass_Storm, int pass_n)
	{
		// For now, ignore these rules if the cardinal direction is up
		if (pass_n == rbV.vtUp)
			return true;

		// Resting Order Rules

		// 1 - For the cases when a Storm is sitting on a wall
		if (pass_Storm.listOf_Radial_Wall_Check[rbV.vtDn])
		{
			// 1a - If there's a Storm opposite to where we're moving, which is also moving in the same
			// Direction. This means our Storm is being pushed and it can move
            bool boolCheck_CanMove_Local = false;
            foreach (Sc_Storm temp_Storm in pass_Storm.listOf_Radial_Storm_Check[((pass_n + (rbV.cD / 2)) % rbV.cD)])
            {
				// If the Storm we're checking behind us has the same layer number (And formation number)
                if (pass_Storm.Reverse_StormInfo_LayerFormationCompare(temp_Storm, pass_n))
                {
                    boolCheck_CanMove_Local = true;
                }
            }
            if (boolCheck_CanMove_Local)
            {
                // We are being pushed and, thus, can move
                // Continue
            }
            // 1b - If there's a player beside the Storm whilst the player's rotating, it will always move
			// Because the Storm movement is always perpendicular to where the player is sitting
            else if (player.isPlayerRotating && pass_Storm.cardinal_Player_Check)
            {
                //Continue
            }
            // 1c - If the player isn't rotation, but it's next to the Storm and moving in the same direction
            else if (!player.isPlayerRotating && (player.moveInt == pass_n)
                    && (pass_Storm.listOf_Radial_Player_Check[((player.moveInt + (rbV.cD / 2)) % rbV.cD)]))
            {
                //Continue
            }
            // 1d - What if the player is 'inside' the Storm, like under an archway, and wants to rotate? This is allowed
            else if (player.isPlayerRotating && pass_Storm.inside_Player_Check)
            {
                //Continue
            }
            // 1e - What if the player is 'inside' the Storm, say for example, an archway, and wants to translate? We can allow this
            else if (!player.isPlayerRotating && (player.moveInt == pass_n)
                    && (pass_Storm.inside_Player_Check))
            {
                //Continue
            }
			// In all other cases, the Storm can't move
            else
			{
                return false;
			}
		}

		// 2 - For the case whe a Storm is sitting on a Storm
		else if (pass_Storm.listOf_Radial_Storm_Check[rbV.vtDn].Count > 0)
        {
			foreach (Sc_Storm temp_Storm in pass_Storm.listOf_Radial_Storm_Check[rbV.vtDn])
            {
                // 2a - When sitting on a Lift (n == vtDn)
                //	When a Storm is created, it will have an 'IsLift' boolean to mark it
                if (temp_Storm.isLift && pass_n == rbV.vtDn)
                {
                    // There are two scenarios here.
                    // 1) The lift is about to move/is moving, in which case the Storm can 'ignore' the lift, allowing it to fall
                    if (!temp_Storm.ref_LiftScript.lift_isStationary)
                    {
                        // We can ignore it
                        // continue
                    }
                    // 2) The lift is stationary, in which case it acts as a wall
                    else
                    {
                        // Acts as a wall
                        return false;
                    }
                }

                // 2b - When the player is directly below the storm
                // And the player is not pushing the Storm
                // And the Storm sitting on another Storm
                // Don't move?
                if (temp_Storm.listOf_Radial_Player_Check[rbV.vtDn] &&
                    temp_Storm.listOf_Radial_Player_Check[(pass_n + 2) % rbV.cD] &&
                    temp_Storm.listOf_Radial_Storm_Check[rbV.vtDn].Count > 0)
                {
                    // This Storm shouldn't move
                    return false;
                }
				// 2c - When the player is below the Storm
				// And the player is not pushing the Storm
				// Or, the player is not rotating rotating
				// And the Storm is sitting on another Storm
				else if (pass_Storm.listOf_Radial_Player_Check[rbV.vtDn] &&
                    (!pass_Storm.listOf_Radial_Player_Check[(pass_n + 2) % rbV.cD] && !player.isPlayerRotating) &&
                	pass_Storm.listOf_Radial_Storm_Check[rbV.vtDn].Count > 0)
				{
					return false;
				}
            }
		}

		// We return true if nothering else is caught
		return true;
	}

	// Check if Storms are stacked on other Storms that can't move
	void Check_Stack_CanMove(int pass_n, int pass_fr)
	{
		// We need to empty any confirmed lists that are stacked on Storms that can't move
		// We collect all the ly values for stacks and grab any other ly values that might be on top of the previous ly

		List<int> temp_lyDelete_Checking = new List<int>();
        List<int> temp_lyDelete_Confirmed = new List<int>();

		// First, we need to grab all the ly's that can't move
        for (int ly = 0; ly < listOf_Storms_Movement_Confirm[pass_n][pass_fr].Count; ly++)
        {
			if (!listOf_BoolCheck_Chain_CanMove[pass_n][pass_fr][ly])
			{
				// We add this ly value to our list, if we haven't already checked it yet
				if (!temp_lyDelete_Checking.Contains(ly) && !temp_lyDelete_Confirmed.Contains(ly))
				{
					temp_lyDelete_Checking.Add(ly);
				}
			}
		}

		// Next, we stack check to see which Storms are sitting on these ly's

		int failSafe_ly = 0;

		while ((temp_lyDelete_Checking.Count != 0) && (failSafe_ly < 100))
		{
			// Hold the layer value
			int temp_ly = temp_lyDelete_Checking[0];

			// We don't perform this check for vertical chains
			if (pass_n < rbV.cD)
			{
				// Go through each storm and find anything on top it
				foreach (Sc_Storm temp_Storm in listOf_Storms_Movement_Confirm[pass_n][pass_fr][temp_ly])
				{
					if (temp_Storm.listOf_Radial_Storm_Check[rbV.vtUp].Count > 0)
					{
						// If there is, go through each of those
						foreach (Sc_Storm temp_Storm_Stacked in temp_Storm.listOf_Radial_Storm_Check[rbV.vtUp])
						{
							// We can ignore ly's that are lower than or the same as the ly we're checking
							// I'm not certain that this is accurate
							if (temp_Storm_Stacked.Reverse_StormInfo_LayerLookup(pass_n, pass_fr) > temp_Storm.Reverse_StormInfo_LayerLookup(pass_n, pass_fr))
							{
								// Then, as long as we don't already have that ly
								if (!temp_lyDelete_Checking.Contains(temp_Storm_Stacked.Reverse_StormInfo_LayerLookup(pass_n, pass_fr)) &&
									!temp_lyDelete_Confirmed.Contains(temp_Storm_Stacked.Reverse_StormInfo_LayerLookup(pass_n, pass_fr)))
								{
									temp_lyDelete_Checking.Add(temp_Storm_Stacked.Reverse_StormInfo_LayerLookup(pass_n, pass_fr));
								}
							}
						}
					}
				}
			}

			// Once we've finished checking a layer, shift it into the confirmed list and delete it
			temp_lyDelete_Confirmed.Add(temp_ly);
			temp_lyDelete_Checking.Remove(temp_ly);

			failSafe_ly += 1;
		}

		if (failSafe_ly >= 100)
			print ("Failsafe LY has failed, inside Check_Stack_CanMove");

		// Finally, delete all the layer that we've collected in the confirmed list
		foreach(int ly in temp_lyDelete_Confirmed)
		{
			listOf_Storms_Movement_Confirm[pass_n][pass_fr][ly] = new List<Sc_Storm>();
		}
	}
	
	// Check Storm can fall
	void CheckFor_Fall(Sc_Storm temp_Storm)
	{
		// We only do this if a Storm contains clouds
		if (/* !Storm.isLift &&*/ (temp_Storm.listOf_Clouds_Child.Count != 0))
		{
			// If, after checking neighbours, the Storm has no ground beneath it
			if (!temp_Storm.listOf_Radial_Wall_Check[rbV.vtDn])
			{
				// Add this Storm to the checking list and perform and chain check downwards
				AddStorm_ToCheckList(temp_Storm, rbV.vtDn);
				
				// Perform the chain cycle on each formation
				for (int fr = 0; fr < listOf_Storms_Movement_Check[rbV.vtDn].Count; fr++)
				{
					Chain_Creation_Cycle(rbV.vtDn, fr);
				}

				// Next, for each confirmed fomration, check if each layer can move
				for (int fr = 0; fr < listOf_Storms_Movement_Confirm[rbV.vtDn].Count; fr++)
				{
					Check_Chain_CanMove(rbV.vtDn, fr);
				}

				// Finally, for each formation, we check if the stack can move based on the layer below it
				for (int fr = 0; fr < listOf_Storms_Movement_Confirm[rbV.vtDn].Count; fr++)
				{
					Check_Stack_CanMove(rbV.vtDn, fr);
				}

				// Go through each confirmed formation
				for (int fr = 0; fr < listOf_Storms_Movement_Confirm[rbV.vtDn].Count; fr++)
				{
					// Go through each layer that has content
					for (int ly = 0; ly < listOf_Storms_Movement_Confirm[rbV.vtDn][fr].Count; ly++)
					{
						// And each Storm
						foreach (Sc_Storm part_Storm in listOf_Storms_Movement_Confirm[rbV.vtDn][fr][ly])
						{
							// Queue up all these Storms to be moved
							rbA.listOf_QueueMovement_Translate.Add(new QueueMovement_Translate(2, part_Storm.transform, rbV.vtDn));
						}
					}
				}

				//We reset the lists before the next Storm is checked
				Reset_Lists();
			}
		}
	}

	/*
	//This Fall script is used for checking when a Lift is going downwards. It behaves slightly differently from
    //The regular CheckFor_Fall script
    public void CheckFor_Fall_Lift()
    {
        //We only care about the first layer, because stacking is replaced by the chain during VtDw and VtUp movement
        int ly = 0;

        //Go through every Storm
        foreach (Sc_Storm Storm in listOf_All_Storms_ForReference)
        {
            //We ignore lifts
            //We only do this if a Storm contains clouds
            if (!Storm.isLift && (Storm.listOf_Clouds_Child.Count != 0))
            {
                //If, after checking neighbours, the Storm has no ground beneath it
                if (!Storm.listOf_Radial_Wall_Check[rbV.vtDn])
                {
                    //Add this Storm to the checking list and perform and chain check downwards
                    AddStormToCheckingList(rbV.vtDn, Storm);
                    //If the Storm finds a lift while checking itself, it should ignore it
                    Chain_Creation_Cycle(rbV.vtDn);
                    //Then check they Storms can move
                    Check_Chain_CanMove(rbV.vtDn);
                    //Then we delete any ly that can't move
                    Check_Stack_CanMove(rbV.vtDn);

                    //Now we have a list of confirmed Storms that the Lift can move
                }
            }
        }
    } */

	//////////////////
	// CHECK ACTION //
	//////////////////

	// Check if a Puff or Soil block can be watered by passing a Storm
	void CheckFor_Watering(Sc_Storm temp_Storm)
	{
		foreach (Sc_Cloud temp_Cloud in temp_Storm.listOf_Clouds_Child)
		{
			foreach(Sc_Block temp_Block in temp_Cloud.listOf_ChildBlocks)
			{
				// Check if a puff block is beside soil
				if (temp_Block is Sc_Block_Puff)
				{
					Sc_Block_Puff temp_Puff = (Sc_Block_Puff)temp_Block;
					temp_Puff.CheckFor_Soil();
				}

				// Check is a soil block is beside puff
				if (temp_Block is Sc_Block_Soil)
				{
					Sc_Block_Soil temp_Soil = (Sc_Block_Soil)temp_Block;
					temp_Soil.CheckFor_Puff();
				}
			}
		}
	}

	// Check if two Puffs should be glued
	public void CheckFor_Glue(Sc_Storm temp_Storm)
	{
		// Grab all the blocks that make up this Storm
		// Must do this first, otherwise the list may change size when checking for overlapping
		List<Sc_Block_Puff> temp_ListOf_PuffBlocks = new List<Sc_Block_Puff>();
		foreach (Sc_Cloud temp_Cloud in temp_Storm.listOf_Clouds_Child)
		{
			foreach(Sc_Block temp_Block in temp_Cloud.listOf_ChildBlocks)
			{
				if (temp_Block is Sc_Block_Puff)
				{
					Sc_Block_Puff temp_Puff = (Sc_Block_Puff)temp_Block;
					temp_ListOf_PuffBlocks.Add(temp_Puff);
				}
			}
		}

		// Then, enact changes to Puffs, if there are any
		foreach(Sc_Block_Puff temp_Puff in temp_ListOf_PuffBlocks)
		{
			temp_Puff.Resolve_Overlapped_Storm();
		}
	}

	// Check all the Puff's inside the current list of Cloud children
	// And see if any of those children are no longer linked to separate cloud parents
	public void CheckFor_Unglue(Sc_Storm temp_Storm)
	{
		// Simpler idea, just don't let recently glued Storms into the DoubleCheck List
		// And here, unglue all Storms, then reglue them

		// First, I need to grab all the Storms that are connected together
		// I do this by collecting each Cloud's birthparent
		List<Sc_Storm> temp_ListOf_Storms = new List<Sc_Storm>();
		foreach (Sc_Cloud temp_Cloud in temp_Storm.listOf_Clouds_Child)
		{
			CheckAndAdd_StormToList(temp_ListOf_Storms, temp_Cloud.storm_BirthParent);
		}

        // Reset the Storm cloudlists
		foreach(Sc_Storm proxy_Storm in temp_ListOf_Storms)
		{
        	proxy_Storm.Reset_Storm_Children();

			// Reset the Clouds in each Storm
			foreach (Sc_Cloud temp_Cloud in proxy_Storm.listOf_Clouds_BirthChild)
			{
				temp_Cloud.Reset_Parent();
			}
		}

		foreach(Sc_Storm proxy_Storm in temp_ListOf_Storms)
		{
			// Then reglue everything
			CheckFor_Glue(temp_Storm);
		}

		/*
		// First, I need to grab all the Storms that are connected together
		List<Sc_Storm> temp_ListOf_Storms = new List<Sc_Storm>();
		foreach (Sc_Cloud temp_Cloud in temp_Storm.listOf_Clouds_Child)
		{
			CheckAndAdd_StormToList(temp_ListOf_Storms, temp_Cloud.storm_Parent);
		}
		
		// This should give me a list of about two or three Storms

		// Next, go through each Storm, checking the Storm's actual birth children and ask
		// "Are you overlapping with a Storm that isn't your birth parent?"
		// If the answer is "No" for _All_ Puffs... Then, that Storm becomes unglued
		// And reverts back to its original state
		foreach(Sc_Storm proxy_Storm in temp_ListOf_Storms)
		{
			foreach (Sc_Cloud temp_Cloud in temp_Storm.listOf_Clouds_BirthChild)
			{

			}
		}*/

	}

	// Check if a Storm has moved onto a Lift or if the Storm is a lift
	void CheckFor_Lifts(Sc_Storm pass_Storm)
	{
		// Assuming there is only one lift (Otherwise I'd go crazy)
		Sc_Block_Lift ref_Lift = null;

		// Either, this Storm is a lift
		if (pass_Storm.isLift)
		{
			// Grab the lift block
			foreach (Sc_Cloud temp_Cloud in pass_Storm.listOf_Clouds_Child)
			{
				foreach (Sc_Block temp_Block in temp_Cloud.listOf_ChildBlocks)
				{
					if (temp_Block is Sc_Block_Lift)
					{
						Sc_Block_Lift temp_Lift = (Sc_Block_Lift)temp_Block;
						ref_Lift = temp_Lift;
					}
				}
			}
		}
		// Or, it's sitting on a lift
		else
		{
			// Go through each Storm below this one and find if any of them are lifts
			foreach (Sc_Storm temp_Storm in pass_Storm.listOf_Radial_Storm_Check[rbV.vtDn])
			{
				// Find the lifts
				if (temp_Storm.isLift)
				{
					// Grab the lift block
					foreach (Sc_Cloud temp_Cloud in temp_Storm.listOf_Clouds_Child)
					{
						foreach (Sc_Block temp_Block in temp_Cloud.listOf_ChildBlocks)
						{
							if (temp_Block is Sc_Block_Lift)
							{
								Sc_Block_Lift temp_Lift = (Sc_Block_Lift)temp_Block;
								ref_Lift = temp_Lift;
							}
						}
					}
				}
			}	
		}

		// Now we've found a reference lift, we can check if the lift can move
		if (ref_Lift != null)
		{
			ref_Lift.Check_PreOperate_Lift();
		}
	}

	void CheckFor_Lifts_Player()
	{
		// Assuming there is only one lift (Otherwise I'd go crazy)
		Sc_Block_Lift ref_Lift = null;

		foreach (Sc_Storm temp_Storm in player.listOf_Radial_Storm_Check[rbV.vtDn])
		{
			// Find the lifts
			if (temp_Storm.isLift)
			{
				// Grab the lift block
				foreach (Sc_Cloud temp_Cloud in temp_Storm.listOf_Clouds_Child)
				{
					foreach (Sc_Block temp_Block in temp_Cloud.listOf_ChildBlocks)
					{
						if (temp_Block is Sc_Block_Lift)
						{
							Sc_Block_Lift temp_Lift = (Sc_Block_Lift)temp_Block;
							ref_Lift = temp_Lift;
						}
					}
				}
			}
		}

		// Now we've found a reference lift, we can check if the lift can move
		if (ref_Lift != null)
		{
			ref_Lift.Check_PreOperate_Lift();
		}
	}

	//////////////////////
	// PERFORM MOVEMENT //
	//////////////////////

	// This functions prepares the Lift for movement

	public void Operate_Lift_Once(Sc_Block_Lift pass_Lift, bool pass_isRising)
	{
		// During this function, the lift will have its stationary status removed, temporarily
		pass_Lift.lift_isStationary = false;

		// Find the movement direction of the lift
		int vec_State = rbV.vtUp;
		if (!pass_isRising)
			vec_State = rbV.vtDn;

		// Next, collect all the data on what objects are sitting on the lift
		// Also, whether the player is present on the lift
		bool player_isPresentOnLift = false;
		foreach (GlobInt.BlockType temp_BlockType in (GlobInt.BlockType[])Enum.GetValues(typeof(GlobInt.BlockType)))
        {
			// Check each blocktype
            foreach (Sc_Block temp_Block in pass_Lift.listOf_Radial_Block_Check[rbV.vtUp, (int)temp_BlockType])
            {
				// For the player, specifically
				if (temp_Block.self_BlockType == GlobInt.BlockType.Play)
				{
					player_isPresentOnLift = true;
					// And collect any Storms that may be sitting on the player (Moving direction, checking direction)
					player.CollectStorms_Vertical(vec_State, rbV.vtUp);
				}
				// For all other blocks, just collect their Storms
				else
				{
					AddStorm_ToCheckList(temp_Block.cloud_Parent.storm_Parent, vec_State);
				}
			}
		}

		// There's a section I skipped here, which might be a problem later, but oh well

		// This is only a concern for upwards movement
		if (vec_State == rbV.vtUp)
		{
			// We perform the check, receiving a list of Storms that are ready to move upwards, in this case
			for (int fr = 0; fr < listOf_Storms_Movement_Check[vec_State].Count; fr++)
			{
				Chain_Creation_Cycle(vec_State, fr);
			}		

			// Next, for each confirmed fomration, check if each layer can move
			for (int fr = 0; fr < listOf_Storms_Movement_Confirm[vec_State].Count; fr++)
			{
				Check_Chain_CanMove(vec_State, fr);
			}

			// Finally, for each formation, we check if the stack can move based on the layer below it
			for (int fr = 0; fr < listOf_Storms_Movement_Confirm[vec_State].Count; fr++)
			{
				Check_Stack_CanMove(vec_State, fr);
			}
		}
		else
		{
			// For the downwards case, we run a slightly different version of the regular CheckFor_Fall() script
			// [Note*] I will add this later
		}

		// We're only concerened about the Storm blockstack if there are any Storms there at all
		bool can_BlockStack_Move = true;
		if (listOf_Storms_Movement_Confirm[vec_State].Count > 0)
		{
			can_BlockStack_Move = false;
			// Next, we check if the Storms that are moving due to the lift are blocked in any way
			if (vec_State == rbV.vtUp)
			{
				for (int fr = 0; fr < listOf_Storms_Movement_Confirm[vec_State].Count; fr++)
				{
					for (int ly = 0; ly < listOf_Storms_Movement_Confirm[vec_State][fr].Count; ly++)
					{
						// If any Storms can move, it means the lift can move too
						if (listOf_Storms_Movement_Confirm[vec_State][fr][ly].Count > 0)
						{
							can_BlockStack_Move = true;
						}
					}
				}
			}
			// If the vec_State is down, I'm just gonna allow movement for now
			else
			{
				can_BlockStack_Move = true;
			}
		}

		// If anything can move, we're good to go
		if (can_BlockStack_Move || (listOf_Storms_Movement_Confirm[vec_State].Count == 0))
		{
			// We next need to calculate the switchcase for how the lift moves
			// If the lift has one floor, we move in a sigmoid fashion, case 0
			if (pass_Lift.topFloor == 1)
				rbA.t_Case = 0;
			// For larger floors
			else if (pass_Lift.topFloor >= 2)
			{
				// If we're currently moving down, at the top floor, or moving up from the bottom floor, case 1
                if ((pass_Lift.currentFloor == 0 && pass_isRising)
					|| (pass_Lift.currentFloor == pass_Lift.topFloor && !pass_isRising))
                {
                    rbA.t_Case = 1;
                }
                // Vice versa
                else if ((pass_Lift.currentFloor == 1 && !pass_isRising) || 
						(pass_Lift.currentFloor == (pass_Lift.topFloor - 1) && pass_isRising))
                {
                    rbA.t_Case = 2;
                }
                // For everything else, linear movement
                else
                {
                    rbA.t_Case = 3;
                }
			}

			// Next, we queue the movement up for every confirmed Storm
			for (int fr = 0; fr < listOf_Storms_Movement_Confirm[vec_State].Count; fr++)
			{
				// Go through each layer that has content
				for (int ly = 0; ly < listOf_Storms_Movement_Confirm[vec_State][fr].Count; ly++)
				{
					// And each Storm
					foreach (Sc_Storm part_Storm in listOf_Storms_Movement_Confirm[vec_State][fr][ly])
					{
						print ("Is the lift being added to the queue list?");
						rbA.listOf_QueueMovement_Lift.Add(new QueueMovement_Lift(1, pass_isRising, part_Storm.transform, rbA.t_Case));
					}
				}
			}

			// And queue the movement for each player, if there is one
			if (player_isPresentOnLift)
			{
				rbA.listOf_QueueMovement_Lift.Add(new QueueMovement_Lift(1, pass_isRising, player.transform, rbA.t_Case));
			}

			// And also queue the movement for the lift, oh lordy
			rbA.listOf_QueueMovement_Lift.Add(new QueueMovement_Lift(0, pass_isRising, pass_Lift.transform, rbA.t_Case));
		}
		// In the alternate case where the lift can't move
		else
		{
			// If the lift isn't currently moving, don't do anything else
			if (pass_Lift.currentFloor == 0 || pass_Lift.currentFloor == pass_Lift.topFloor)
            {

            }
			// Otherwise, we reverse the movement process
			else
			{
				pass_Lift.lift_isRising = !pass_Lift.lift_isRising;
				// We immediately check the lift again, until we find some movement or not
				pass_Lift.Check_PreOperate_Lift();
			}
		}

		// Once finished checking the lift, we can return the Lift back into it's stationary mode
		pass_Lift.lift_isStationary = true;
	}


	////////
	// TF //
	////////

	public int _TF_SizeOf_CheckingList(List<List<List<Sc_Storm>>>[] pass_List, int pass_n = -1)
    {
		int total = 0;

		if (pass_n == -1)
		{
			for (int n = 0; n < pass_List.Length; n++)
			{
				for (int fr = 0; fr < pass_List[n].Count; fr++)
				{
					for (int ly = 0; ly < pass_List[n][fr].Count; ly++)
					{
						total += pass_List[n][fr][ly].Count;
					}
				}
			}
		}
		else
		{
			for (int fr = 0; fr < pass_List[pass_n].Count; fr++)
			{
				for (int ly = 0; ly < pass_List[pass_n][fr].Count; ly++)
				{
					print ("Layer: " + ly + " has " + pass_List[pass_n][fr][ly].Count + " total Storms");
					total += pass_List[pass_n][fr][ly].Count;
				}
			}
		}

        return total;
    }

	// Just prints a reference marker
	public void _TF_PrintMarker(int pass_Int = -1)
	{
		if (pass_Int == -1)
			print ("Marker");
		else
			print ("Marker and int: " + pass_Int);
	}

	public bool _TF_StormAndBlockType(Sc_Storm pass_Storm, GlobInt.BlockType pass_BlockType)
	{
		foreach (Sc_Cloud temp_Cloud in pass_Storm.listOf_Clouds_Child)
		{
			foreach (Sc_Block temp_Block in temp_Cloud.listOf_ChildBlocks)
			{
				if (temp_Block.self_BlockType == pass_BlockType)
				{
					return true;
				}
			}
		}

		return false;
	}
}
