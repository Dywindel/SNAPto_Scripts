using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_Cloud - Cloud Script
//	This script controls a group of blocks that are connected together.

public class Sc_Cloud : MonoBehaviour 
{

	//##############//
	//	VARIABLES	//
	//##############//

	// BlockGroup Settings
	public bool isStatic = false;		// Can this BlockGroup move?
	private Vector3 startPosition;		// Cloud Starting position in each level
	// Unique ID
	public int id = -1;

	// References to World Objects
	[HideInInspector]
	public Sc_GM gM;					// Reference to the GM
	private Sc_UM uM;					// Reference to the UM

	// References to game objects
	// Attached level manager(If there is one)
	[HideInInspector]
	public Sc_LevelManager levelManager;
	// Storm Parent
	[HideInInspector]
	public Sc_Storm storm_Parent;
	// The original parent of the cloud
	[HideInInspector]
	public Sc_Storm storm_BirthParent;
	public GameObject storm_Prefab;
	[HideInInspector]
	// Block Children
	public Sc_Block[] listOf_ChildBlocks;

	// Reference to this blocks descrete position
	public Vector3 ref_Discrete_Position;

	// For sound, this specifies what type of object this is for the sound to be played
	// 0 - No sound, 1 - Cloud, 2 - Rock etc.
	[Range(0, 5)]
	public int intSoundType = 0;

	// If the cloud has 'Returned to the cloud layer'
    [HideInInspector]
    public bool bool_ReturnToCloudLayer = false;
	// If this cloud formation contains puffs
	[HideInInspector]
	public bool bool_ContainsPuffs = false;
	// Or soil
	[HideInInspector]
	public bool bool_ContainsSoils = false;

	//##################//
	//	INITIALISATION	//
	//##################//

	// I've used Awake as a quick fix ATM. I shouldn't allow any Blocks to 'Start' until after their parent Clouds have been setup
	// I could achieve this by calling the Start command from the Cloud itself
	void Start() 
	{
		// Grab the Game Manager, dictionary, movement library...
		GameObject gM_GameObject = GameObject.FindGameObjectWithTag("GM");
		gM = gM_GameObject.GetComponent<Sc_GM>();
		uM = gM.uM;

		// Pass yourself to the UM
		gM.listOf_All_Clouds_ForReference.Add(this);

		// Disable the block icon that indicates a blockGroup
		MeshRenderer mR = GetComponent<MeshRenderer>();
		mR.enabled = false;

		// Note your start position
		startPosition = this.transform.position;
		// And reference marker
		ref_Discrete_Position = this.transform.position;

		// Create the parent Storm
		Create_Storm();

		// Collect all the blocks attached to this group
		Collect_Blocks();
	}

	// Create and assign the Storm
	void Create_Storm()
	{
		// Create a Storm and have this Cloud reference it
		storm_BirthParent = Instantiate(storm_Prefab, transform.position, Quaternion.identity).GetComponent<Sc_Storm>();
		storm_Parent = storm_BirthParent;

		// I want the Storms to appear inside the level folders when the game runs.
		// So, I will make the Storm's children of this clouds parent, if it has one
		if (this.transform.parent.transform != null)
		{
			storm_BirthParent.transform.SetParent(this.gameObject.transform.parent.transform);
		}

		// Startup the Storm
		storm_Parent.Storm_Start();
		// Set this Cloud as the storm's child and position it inside its folder
		storm_Parent.listOf_Clouds_Child.Add(this);
		// Also, set that Storm to be the Cloud's birth parent
		storm_Parent.listOf_Clouds_BirthChild.Add(this);
		transform.SetParent(storm_Parent.gameObject.transform);
		// Pass the same id
		storm_Parent.id = id;

		// Pass the sound type
		storm_Parent.intSoundType = intSoundType;
	}

	// Resets the parent and transform of the Cloud
	public void Reset_Parent()
	{
		// Reset the parent
		storm_Parent = storm_BirthParent;
		// Add to the cloud list
		storm_Parent.listOf_Clouds_Child.Add(this);
		// Move the transform
		transform.SetParent(storm_Parent.gameObject.transform);
		// And update the discrete reference position
		// I've removed this due to a bug in which soil is checked before the cloud is able to move
		// This causes the cloud to be position behind it's previous move location, causing it not to move
		// I'm not sure why soil is updating to early and that needs to be fixed too
		//ref_Discrete_Position = this.transform.position;
	}

	// First, we should gather all the blocks attached to this gameObject and store them as members of this block group
	void Collect_Blocks()
	{
		listOf_ChildBlocks = GetComponentsInChildren<Sc_Block>();

		// We need to setup each of the blocks within the group
		foreach (Sc_Block block in listOf_ChildBlocks)
		{
			// Pass a reference to the child block of this script
			block.cloud_Parent = this;
			block.StartUp();
		}
	}

	// This is run when a level is reset
	public void Reset_Cloud_LevelState()
	{
		// Reset the Storm parent
		storm_Parent.Reset_Storm_Children();
		// Reset the Cloud parent
		Reset_Parent();
		// Reset the Cloud's position and discrete position value
		this.gameObject.transform.position = startPosition;
		ref_Discrete_Position = startPosition;
		// And the cloud's cloud layer state
		bool_ReturnToCloudLayer = false;
		// Reset all the Puffs to their initial state
		foreach (Sc_Block temp_Block in listOf_ChildBlocks)
		{
			// For Puffs, this will require rewatering
			if (temp_Block.self_BlockType == GlobInt.BlockType.Puff)
			{
				Sc_Block_Puff temp_Puff = (Sc_Block_Puff)temp_Block;
				// I haven't stored the start position of the Puffs. If I create any levels in which the Puff starts
				// Empty, I will have to change this
				temp_Puff.puffFull = true;
				temp_Puff.puffShare = false;
				temp_Puff.Update_Materials();
			}

			// For Soil, this will require resetting the Sprout
			if (temp_Block.self_BlockType == GlobInt.BlockType.Soil)
			{
				Sc_Block_Soil temp_Soil = (Sc_Block_Soil)temp_Block;
				temp_Soil.soilDry = true;
				temp_Soil.Update_Materials();
			}

			// For lifts, there's a bunch I'll add here

			// Floors are fine
			
		}
	}

	public void Check_CloudLayerBreach()
	{
		// If the breach boolean is currently false, set it to true
		if (!bool_ReturnToCloudLayer)
		{
			bool_ReturnToCloudLayer = true;
			// Do something with the GM
			gM.Update_TotalGame_CP();
		}
	}
}
