using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_Storm - The Block Group of Block Groups
//	This script controls two or more block groups that have become glued together

public class Sc_Storm : MonoBehaviour {
    
	//##############//
	//	VARIABLES	//
	//##############//

    // Block group settings
    public int id = 0;

    //References to World Objects
	private Sc_GM gM;					//Reference to the GM
	private Sc_UM uM;					//Reference to the UM
    private Sc_RB_Values rbV;			//Reference to the value library
	private Sc_RB_Animation rbA;		//Reference to the movement library

    // List of attached Clouds to this Storm
    [HideInInspector]
    public List<Sc_Cloud> listOf_Clouds_Child;
    // List of Clouds that were attached to this Storm at birth
    [HideInInspector]
    public List<Sc_Cloud> listOf_Clouds_BirthChild;

    // This records [n movement direciton][fr formation number][ly layer value]
    // Storms can exist inside multiple formations and lists
    public List<int>[] stormInfo_Values = new List<int>[3];
    // This records [is the Storm a lift?][Is the Storm Siddling?]
    public bool[] stormInfo_Bools = {false, false};

    // Surrounding objects
    public List<Sc_Storm>[] listOf_Radial_Storm_Check;
	public bool[] listOf_Radial_Wall_Check;
	public bool[] listOf_Radial_Player_Check;
    public bool cardinal_Player_Check;
    public bool inside_Player_Check;

    //The special case when a Storm is a lift. Sometimes it will act as a floor when it's not moving
    //It's useful also to pass a reference to the Lift's script
    [HideInInspector]
    public bool isLift = false;
    public Sc_Block_Lift ref_LiftScript;
    //Special case if the Storm is sitting on a lift
    [HideInInspector]
    public bool isSittingOnLift = false;
    // Special case if the Storm is moving through a Siddle method
    [HideInInspector]
    public bool isStormSiddling = false;

    // Reference to this blocks descrete position
	public Vector3 ref_Discrete_Position;

    // For sound, this specifies what type of object this is for the sound to be played
	// 0 - No sound, 1 - Cloud, 2 - Rock etc.
	public int intSoundType = 0;


	//##################//
	//	INITIALISATION	//
	//##################//

    //Initialise the Storm
    public void Storm_Start()
    {
        //Grab the Game Manager, dictionary, movement library...

		GameObject gM_GameObject = GameObject.FindGameObjectWithTag("GM");
		gM = gM_GameObject.GetComponent<Sc_GM>();
        uM = gM.uM;
        rbV = gM.rbV;
		rbA = gM.rbA;

        //Pass yourself to the UM
		gM.listOf_All_Storms_ForReference.Add(this);

        // Update the block's discrete position
		ref_Discrete_Position = this.transform.position;

        //Initialise Lists
        Initialise_Lists();
    }

    //Initialise any lists used in this script
	void Initialise_Lists()
	{
        // Setup stormInfo_Values
        for (int i = 0; i < stormInfo_Values.Length; i++)
        {
            stormInfo_Values[i] = new List<int>();
        }

		listOf_Radial_Storm_Check = new List<Sc_Storm>[rbV.cDVt];
		listOf_Radial_Wall_Check = new bool[rbV.cDVt];
		listOf_Radial_Player_Check = new bool[rbV.cDVt];

		Reset_Lists();
	}


	//##############//
	//	FUNCTIONS	//
	//##############//

    // Record the neighbours around the Storm by checking each child block that is part of each child Cloud
    public void Collect_Neighbours()
    {
        // Check all Cloud children
        foreach (Sc_Cloud Cloud in listOf_Clouds_Child)
        {
            // Go through each childblock
            foreach(Sc_Block Block in Cloud.listOf_ChildBlocks)
            {
                // Before reading each block, we need to perform Record Neighbours
                Block.Record_Neighbours();

                // Check all Cardinal direction
                for (int n = 0; n < rbV.cDVt; n++)
                {
                    // Go through each BlockType, apart from the player
                    for (int i = 1; i < Enum.GetNames(typeof(GlobInt.BlockType)).Length; i++)
                    {
                        //Check what the Par_BlockGroup is of each block and, as long as it's not this one, add it to our radial check
                        for (int j = 0; j < Block.listOf_Radial_Block_Check[n, i].Count; j++)
                        {
                            if (Block.listOf_Radial_Block_Check[n, i][j].cloud_Parent.storm_Parent != this)
                            {
                                if (!listOf_Radial_Storm_Check[n].Contains(Block.listOf_Radial_Block_Check[n, i][j].cloud_Parent.storm_Parent))
                                {
                                    listOf_Radial_Storm_Check[n].Add(Block.listOf_Radial_Block_Check[n, i][j].cloud_Parent.storm_Parent);
                                }
                            }
                        }
                    }

                    // Check if there is a player in one of these types
                    // The player is checked separately because it doesn't not have a BlockGroup_Parent
                    if (Block.listOf_Radial_Block_Check[n, (int)GlobInt.BlockType.Play].Count > 0)
                    {
                        listOf_Radial_Player_Check[n] = true;
                    }

                    //Check for walls
                    if (Block.listOf_Radial_Wall_Check[n])
                    {
                        listOf_Radial_Wall_Check[n] = true;
                    }
                }
			}
        }
    }

    // Reset the lists used to record neighbours
	public void Reset_Lists()
	{
        // Reset StormInfo value holder
        for (int i = 0; i < stormInfo_Values.Length; i++)
        {
            stormInfo_Values[i] = new List<int>();
        }

        //Here we check if the player is sitting beside the Storm. This is useful in some cases
        cardinal_Player_Check = false;
        //We can alos use the knowledge of whether the player is sitting inside the Storm
        inside_Player_Check = false;
        isSittingOnLift = false;
        isStormSiddling = false;

		for (int n = 0; n < rbV.cDVt; n++)
		{
			listOf_Radial_Storm_Check[n] = new List<Sc_Storm>();
			listOf_Radial_Wall_Check[n] = false;
			listOf_Radial_Player_Check[n] = false;
		}

        // Reset all the blocks that are contained within the Storm
        foreach (Sc_Cloud temp_Cloud in listOf_Clouds_Child)
        {
            foreach (Sc_Block temp_Block in temp_Cloud.listOf_ChildBlocks)
            {
                temp_Block.Reset_Lists();
            }
        }
	}

    //Reset the Storm's children
    public void Reset_Storm_Children()
    {
        listOf_Clouds_Child = new List<Sc_Cloud>();
    }

    // Update all block descrete positions
    public void Update_DiscretePositions(int pass_axisInt)
    {
        // Update the ref_Discrete_Position for this Storm
        ref_Discrete_Position = new Vector3 (	Mathf.Round(transform.position.x + rbV.int_To_Card[pass_axisInt].x),
                                                Mathf.Round(transform.position.y + rbV.int_To_Card[pass_axisInt].y),
                                                Mathf.Round(transform.position.z + rbV.int_To_Card[pass_axisInt].z));
        // Update all Cloud discrete reference positions
        foreach (Sc_Cloud temp_Cloud in listOf_Clouds_Child)
        {
            temp_Cloud.ref_Discrete_Position += rbV.int_To_Card[pass_axisInt];
        }
    
    }

    // Reverse looks up the value of layer for a specfic n and fr
    public int Reverse_StormInfo_LayerLookup(int pass_n, int pass_fr)
    {
        // Go through the length of the list (Using the size of n to dictate list
        for (int i = 0; i < stormInfo_Values[0].Count; i++)
        {
            // If any value of n matches the passed value
            if (stormInfo_Values[0][i] == pass_n)
            {
                // And, if any fr matches the passed value
                if (stormInfo_Values[1][i] == pass_fr)
                {
                    // We have the index, return the layer value
                    return stormInfo_Values[2][i];
                }
            }
        }

        // otherwise... Uhh...
        return 0;
    }

    // Reverse check if layer value is larger than passed values
    public bool Reverse_StormInfo_LayerCompare(int pass_n, int pass_fr, int pass_ly)
    {
        // Go through the length of the list (Using the size of n to dictate list
        for (int i = 0; i < stormInfo_Values[0].Count; i++)
        {
            // If any value of n matches the passed value
            if (stormInfo_Values[0][i] == pass_n)
            {
                // And, if any fr matches the passed value
                if (stormInfo_Values[1][i] == pass_fr)
                {
                    // We have the index, compare layer value
                    if (stormInfo_Values[2][i] > pass_ly)
                        return true;
                }
            }
        }

        // Otherwise, not
        return false;
    }

    public bool Reverse_StormInfo_LayerFormationCompare(Sc_Storm pass_Storm, int pass_n)
    {
        // Go through each Storm and find only those indices with matching n values
        for (int i = 0; i < stormInfo_Values[0].Count; i++)
        {
            // If any value of n matches the passed value
            if (stormInfo_Values[0][i] == pass_n)
            {
                // For each formation value, check if the layers match
                if (stormInfo_Values[2][i] == pass_Storm.Reverse_StormInfo_LayerLookup(pass_n, stormInfo_Values[1][i]))
                {
                    // If it does, the Storms are part of the same chain
                    return true;
                }
            }
        }

        return false;
    }

    // Particle Effect
    public void Check_CloudLayerBreach()
    {
        // Go through each Cloud and Puff, if there are any, check their height
        // If they're at the correct height, run the particle system

        // Check all Cloud children
        foreach (Sc_Cloud Cloud in listOf_Clouds_Child)
        {
            // Go through each childblock
            foreach(Sc_Block Block in Cloud.listOf_ChildBlocks)
            {
                // If this block is a puff
                if (Block.self_BlockType == GlobInt.BlockType.Puff)
                {
                    // Run the particle system check
                    Sc_Block_Puff temp_Puff = (Sc_Block_Puff)Block;
                    temp_Puff.Check_CloudLayerBreach();
                }
            }
        }
    }
}
