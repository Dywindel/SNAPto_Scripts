using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_Player - Player Script
//	This script is attached to the player and controls player actions. It's a child
//	of the parent block script, making it a block too.

public class Sc_Player : Sc_Block {


	//##############//
	//	VARIABLES	//
	//##############//

	//References to World Objects
	private Sc_Player_Input inp;			// Reference to the Input script
	public Transform ref_TransformComponents;	// These components will rotate

	//Player self-descriptive variables	
	[HideInInspector]
	public int faceInt = 0;					// The current direction the player is facing

	//Movement variables
	public bool isPlayerRotating = false;	//Describes if the player is rotating or translating
	public int moveInt = 0;					//The integer the player intends to move in (Passed from input)
	[HideInInspector]
	public bool inp_AxisMove = false;		//Boolean check for whether the player is moving
	[HideInInspector]
	public bool inp_Back = false;			//Boolean check for whether the back button has been pressed
	[HideInInspector]
	public bool inp_Reset = false;			// Boolean to reset a level
	[HideInInspector]
	public bool inp_Dir_Held = false;		// Boolean, check if the same input direction is being held down
	[HideInInspector]
	public GlobInt.MoveOption stored_Movement = GlobInt.MoveOption.none;
	[HideInInspector]
	public int[] rot_moveInt = new int[2];	//It's easier to pass these variables to the UM, then recalculate them
	[HideInInspector]
	public int[] rot_DirInt = new int[2]; 	// The directiond we are checking in (Facing and behind), when rotating // This value changes depending on which rotation stage we're in

	//This variable stores which direction the camera is facing
	//For consistency, 0 is facing north (Negative z value), 1 is east, (Negative x value), 2 is South...
	//LB rotates by -90, which translates to +1 to this value. RB is +90 and thus -1 to the value
	[HideInInspector]
	public int fezStyle_CameraFacing_Int = 0;

	//If I reference the movememnt function, I can cancel it out later
	[System.NonSerialized]
	public Coroutine ref_Coroutine;

	// Solid gold easter egg
	public Material ref_SolidGold;


	//##################//
	//	INITIALISATION	//
	//##################//

	void Start()
	{
		//Initialise the parent variables
		base.Initialise();

		//Set what BlockType this script type is
    	self_BlockType = GlobInt.BlockType.Play;

		//Grab a reference to relevent scripts
		inp = gameObject.GetComponent<Sc_Player_Input>();

		// Set discrete position
		Update_DiscretePositions(6); // Six means add no direction
	}


	//##############//
	//	FUNCTIONS	//
	//##############//

	public void Sort_Movement()
	{
		// We don't want to read the player input if we're at the second stage of the set of rotation movement
		if (!uM.rotStage_Two)
		{
			// Update the input
			inp.pass_CollectInput();

			// Interpret the player's intended movement
			Extract_Movement();
		}
	}

	// This function takes the input variables and decides what the player wants to do
	void Extract_Movement()
	{
		// The back button acts as a priority, so this can go first
		// Unless the player is resetting a level
		if (inp_Back && !uM.resetLevel_Twice)
		{
			// An action has been performed
			uM.actBool_Input = true;
			// Restore previous recorded state
			//gM.Restore_StateList();
			gM.Restore_StateList_V2();
			stored_Movement =  GlobInt.MoveOption.none;
		}
		// If the player is reseting a level
		else if (inp_Reset || uM.resetLevel_Twice)
		{
			// We want to do this twice
			if (!uM.resetLevel_Twice)
			{
				uM.resetLevel_Twice = true;
			}
			else
			{
				uM.resetLevel_Twice = false;
			}

			// An action has been performed
			uM.actBool_Input = true;
			// Reset the level, if a level is currently active
			gM.Reset_ActiveLevel();
			stored_Movement = GlobInt.MoveOption.none;
		}
		//Next, if an axis direction has been specified
		else if (inp_AxisMove)
		{
			// For Dialogue, here - If the axis value is the same as the interaction value, update the
			// Dialogue stage index?
			// We only cycle through the messages if the input direction is _Not_ being held down
			// And that same direction is also the direction the dialogue is being spoke in
			if (gM.ref_CanvasMessenger != null && gM.ref_CanvasMessenger.ref_Active_DialogueMessages != null && !inp_Dir_Held)
			{
				gM.ref_CanvasMessenger.Canvas_DisplayMessage_DialoguePrompt();
			}

			//If the current facing direction and the input direction, when added, are odd values, then the player wants to rotate
			if ((moveInt + faceInt) % 2 != 0)
			{
				isPlayerRotating = true;

				//This equation holds for clockwise movement and vice versa
				if ((faceInt + 1) % rbV.cD == moveInt)
					stored_Movement = GlobInt.MoveOption.clockwise;
				else if ((moveInt + 1) % rbV.cD == faceInt)
					stored_Movement = GlobInt.MoveOption.anticlockwise;
			}
			//Otherwise, the player wants to perform a translation movement
			else
			{
				isPlayerRotating = false;

				//When movement is the same as the facing direction, move forwards and vice versa
				if (faceInt == moveInt)
				{
					stored_Movement =  GlobInt.MoveOption.forwards;
				}
				else if ((faceInt + (rbV.cD/2)) % rbV.cD == moveInt)
					stored_Movement =  GlobInt.MoveOption.backwards;
			}
		}
		//If no axis movement has been registered, we don't move
		else
		{
			stored_Movement =  GlobInt.MoveOption.none;
		}
	}


	// Check if there are no basic obstructions in the player movement direction
	public bool CheckIf_Player_NoObstructions()
	{
		switch (stored_Movement)
		{
			///////////////////////
			// Case for Translation
			case GlobInt.MoveOption.forwards:
			case GlobInt.MoveOption.backwards:
			{
				// Check for a wall
				if (!listOf_Radial_Wall_Check[moveInt])
				{
					// Check there is a floor to move to
					if (!gM.isFloorOn || (gM.isFloorOn && listOf_Radial_CloseFloors_Check[moveInt]))
                    {
						return true;
					}
				}

				return false;
			}

			////////////////////
			// Case for Rotation
			case GlobInt.MoveOption.clockwise:
			case GlobInt.MoveOption.anticlockwise:
			{
				// The player will always be able to rotate (For now)
				return true;
			}

			///////////////////////
			// Case for no movement
			case GlobInt.MoveOption.none:
			{
				return false;
			}
		}

		return false;
	}

	// Grab the surrounding neighbours that may be moved during this process
	public void Pass_Neighbours()
	{
		switch (stored_Movement)
		{
			///////////////////////
			// Case for Translation
			case GlobInt.MoveOption.forwards:
			case GlobInt.MoveOption.backwards:
			{
				// First, grab the Storms above the player, using the moveInt as direction
				CollectStorms_Vertical(moveInt, rbV.vtUp);

				// Go through each BlockType, apart from the player
				for (int i = 1; i < Enum.GetNames(typeof(GlobInt.BlockType)).Length; i++)
				{
					// If there is a BlockGroup in this axisInt direction
					if (listOf_Radial_Block_Check[moveInt, i].Count > 0)
					{
						foreach (Sc_Block block in listOf_Radial_Block_Check[moveInt, i])
						{
							// First, we check the ruleset to see which blocks we can ignore
							if (_Pass_Neighbours_BlockCheck(i, moveInt, moveInt, block))
							{
								// Pass the storm into the uM's checking list
								uM.AddStorm_ToCheckList(block.cloud_Parent.storm_Parent, moveInt);
							}
						}
					}
				}

				break;
			}
			///////////////////
			// Case for Rotation
			case GlobInt.MoveOption.clockwise:
			case GlobInt.MoveOption.anticlockwise:
			{
				// On the first rotation phase, we pass the BlockGroups facing front and behind the player
				// Go through each BlockType, apart from the player
				for (int i = 1; i < Enum.GetNames(typeof(GlobInt.BlockType)).Length; i++)
				{
					// First we check in front of the player
					rot_DirInt[0] = faceInt;

					// The items we check will change depending on which stage of rotation we're in
					if (uM.rotStage_Two)
					{
						// This additional step of putting facing blocks ahead of behind blocks doesn't add anything at the moment
						// But, it may be useful later on
						if (stored_Movement == GlobInt.MoveOption.clockwise)
						{
							rot_DirInt[0] = (rot_DirInt[0] + 1) % rbV.cD;
						}
						else
						{
							rot_DirInt[0] = (rot_DirInt[0] + (rbV.cD - 1)) % rbV.cD;
						}
						
					}
					
					// This equation will tell the checked block which direction it will be moving in, based on the player's rotation direction
					if (stored_Movement == GlobInt.MoveOption.clockwise)
						rot_moveInt[0] = (rot_DirInt[0] + 1) % rbV.cD;
					else
						rot_moveInt[0] = (rot_DirInt[0] + (rbV.cD - 1)) % rbV.cD;

					// If there is a BlockGroup in this axisInt direction
					if (listOf_Radial_Block_Check[rot_DirInt[0], i].Count > 0 )
					{
						foreach (Sc_Block block in listOf_Radial_Block_Check[rot_DirInt[0], i])
						{
							//First, we check the ruleset to see which blocks we can ignore
							if (_Pass_Neighbours_BlockCheck(i, rot_moveInt[0], rot_DirInt[0], block))
							{
								// Pass the storm into the uM's checking list
								uM.AddStorm_ToCheckList(block.cloud_Parent.storm_Parent, rot_moveInt[0]);
							}
						}
					}

					// TEST - Pass rotors
					foreach (Sc_Rotor temp_Rotor in listOf_Radial_Rotor_Check[rot_DirInt[0]])
					{
						if (!uM.listOf_Rotors[rot_DirInt[0]].Contains(temp_Rotor))
						{
							uM.listOf_Rotors[rot_DirInt[0]].Add(temp_Rotor);
						}
					}

					// Then we check behind
					rot_DirInt[1] = ((rot_DirInt[0] + rbV.cD/2) % rbV.cD);
					if (stored_Movement == GlobInt.MoveOption.clockwise)
						rot_moveInt[1] = (rot_DirInt[1] + 1) % rbV.cD;
					else
						rot_moveInt[1] = (rot_DirInt[1] + (rbV.cD - 1)) % rbV.cD;

					// If there is a BlockGroup in this axisInt direction
					if (listOf_Radial_Block_Check[rot_DirInt[1], i].Count > 0 )
					{
						foreach (Sc_Block block in listOf_Radial_Block_Check[rot_DirInt[1], i])
						{
							//First, we check the ruleset to see which blocks we can ignore
							if (_Pass_Neighbours_BlockCheck(i, rot_moveInt[1], rot_DirInt[1], block))
							{
								// Pass the storm into the uM's checking list
								uM.AddStorm_ToCheckList(block.cloud_Parent.storm_Parent, rot_moveInt[1]);
							}
						}
					}

					// TEST - Pass rotors
					foreach (Sc_Rotor temp_Rotor in listOf_Radial_Rotor_Check[rot_DirInt[1]])
					{
						if (!uM.listOf_Rotors[rot_DirInt[1]].Contains(temp_Rotor))
						{
							uM.listOf_Rotors[rot_DirInt[1]].Add(temp_Rotor);
						}
					}
				}
				
				//On the second rotation phase, we pass the side BlockGroups
				break;
			}
			///////////////////////
			// Case for no movement
			// The moveInt should be -1
			case GlobInt.MoveOption.none:

				// First, grab the Storms above the player, using the moveInt as direction
				CollectStorms_Vertical(moveInt, rbV.vtUp);

				// Go through each BlockType, apart from the player
				for (int i = 1; i < Enum.GetNames(typeof(GlobInt.BlockType)).Length; i++)
				{
					// Go through each cardinal direction
					for (int j = 0; j < rbV.cD; j++)
					{
						// If there is a BlockGroup in this axisInt direction
						if (listOf_Radial_Block_Check[j, i].Count > 0)
						{
							foreach (Sc_Block block in listOf_Radial_Block_Check[j, i])
							{
								// First, we check the ruleset to see which blocks we can ignore
								if (_Pass_Neighbours_BlockCheck(i, moveInt, j, block))
								{
									// Pass the storm into the uM's checking list
									uM.AddStorm_ToCheckList(block.cloud_Parent.storm_Parent, moveInt);
								}
							}
						}
					}
				}

				break;
		}
	}

	bool _PN_GhostCheck(int pass_i, Sc_Block pass_block)
	{
		// 1/ we ignore ghost puffs
		if (pass_i == (int)GlobInt.BlockType.Puff)
		{
			Sc_Block_Puff temp_Puff = (Sc_Block_Puff)pass_block;
			// We accept regular puffs
			if (!temp_Puff.puffFull)
				return false;
		}
		return true;
	}

	// This function sorts through any particular Puffs we don't want the player to accept
	bool _Pass_Neighbours_BlockCheck(int pass_BlockType, int pass_MoveDir, int pass_CheckDir, Sc_Block pass_block)
	{
		//Remains true unless something indicates a falsehood
		bool temp_Check = true;

		if (!_PN_GhostCheck(pass_BlockType, pass_block))
		{
			temp_Check = false;
		}

		//This check is only performed for the case where the player is rotating
		if (isPlayerRotating)
		{
			// 2/ We consider the case where the block being rotated on opposite sides of the player is part of the same blockGroup
			
			//[PROBLEM] - I haven't included the case for glued groups. I'm not sure what to do here

			//Go through each BlockType, apart from the player
			for (int j = 1; j < Enum.GetNames(typeof(GlobInt.BlockType)).Length; j++)
			{
				//Check the blocks on the opposite side of the player
				foreach (Sc_Block block in listOf_Radial_Block_Check[(pass_CheckDir + rbV.cD/2) % rbV.cD, j])
				{
					//We also ignore ghostPuffs in this situation
					if (j == (int)GlobInt.BlockType.Puff)
					{
						Sc_Block_Puff temp_Puff = (Sc_Block_Puff)block;
						//We accept regular puffs
						if (temp_Puff.puffFull)
						{
							//If any of these items has the same Storm parent as the current pass_Block, then that pass_Block doesn't get to move
							if (block.cloud_Parent.storm_Parent == pass_block.cloud_Parent.storm_Parent)
							{
								temp_Check = false;
							}
						}
					}
					//If any of these items has the same Storm parent as the current pass_Block, then that pass_Block doesn't get to move
					else if (block.cloud_Parent.storm_Parent == pass_block.cloud_Parent.storm_Parent)
					{
						temp_Check = false;
					}
				}
			}
		}

		if (temp_Check)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	//Checks for blocks above or below the player
	public void CollectStorms_Vertical(int pass_MoveInt, int pass_vt)
	{
		//Go through each BlockType, apart from the player
		for (int i = 1; i < Enum.GetNames(typeof(GlobInt.BlockType)).Length; i++)
		{
			//Find any blocks sitting on top of the player
			if (listOf_Radial_Block_Check[pass_vt, i].Count > 0)
			{
				foreach (Sc_Block block in listOf_Radial_Block_Check[pass_vt, i])
				{
					//First, we check which blocks we can ignore
					if (_Pass_Neighbours_BlockCheck(i, pass_MoveInt, pass_vt, block))
					{
						// Pass the storm into the uM's checking list
						uM.AddStorm_ToCheckList(block.cloud_Parent.storm_Parent, pass_MoveInt);
					}
				}
			}
		}
	}

	// This checks and collects any Storms that may be obstructing the player's movement
	public Sc_Storm Grab_ObstructingStorm()
	{
		//Go through each BlockType, apart from the player
		for (int i = 1; i < Enum.GetNames(typeof(GlobInt.BlockType)).Length; i++)
		{
			//We go through each block and check a couple of details
			foreach (Sc_Block temp_Block in listOf_Radial_Block_Check[moveInt, i])
			{
				//If the block is a puff, we can ignore it when it's empty
				if (i == (int)GlobInt.BlockType.Puff)
				{
					Sc_Block_Puff temp_Puff = (Sc_Block_Puff)temp_Block;
					if (temp_Puff.puffFull)
					{
						// Pass back this Storm
						return temp_Puff.cloud_Parent.storm_Parent;
					}
				}
				//For every other block, we pass back their storm
				else
				{
					return temp_Block.cloud_Parent.storm_Parent;
				}
			}
		}

		return null;
	}

	// Update the discrete position of the player
	public void Update_DiscretePositions(int pass_MoveInt)
	{
		ref_Discrete_Position = new Vector3 (	Mathf.Round(transform.position.x + rbV.int_To_Card[pass_MoveInt].x),
												Mathf.Round(transform.position.y + rbV.int_To_Card[pass_MoveInt].y),
												Mathf.Round(transform.position.z + rbV.int_To_Card[pass_MoveInt].z));
	}

	// Reset player position and facing direction
	// This resets the physical position of the player and updates it's discrete block value
	public void Reset_PlayerPosition(Vector3 p_Position, int p_n)
	{
		transform.position = p_Position;
        ref_Discrete_Position = p_Position;

		// Update the faceInt
		faceInt = p_n;

		// Physically rotate the player to face that direction using the RBV
		ref_TransformComponents.rotation = Quaternion.Euler(rbV.faceInt_To_Euler[p_n]);
	}

	// Solid gold mode
	// If you speak to the secret tree, your character can turn into solid gold. A warning of things to come
	// 'Goldmode' as a play on words of 'Godmode'
	public void Activate_SolidGold()
	{
		// Get a list of all the game object materials
		MeshRenderer[] ref_MR = ref_TransformComponents.gameObject.GetComponentsInChildren<MeshRenderer>();
		// Go through each one and change the material
		foreach (MeshRenderer mr in ref_MR)
		{
			mr.material = ref_SolidGold;
		}
	}
}
