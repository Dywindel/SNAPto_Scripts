using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_Puff - Puff BlockScript
//	This script contains all the relevent controls specific to the Puff block
//	which is a child to the Parent_Block script

// [Note* I can simplify the overlap scrip by finding internal blocks inside the record neighbours script]

public class Sc_Block_Puff : Sc_Block {


	//##############//
	//	VARIABLES	//
	//##############//

    // Variables related to the appearance of the Puff
    // If I create a level in which I want a Puff to start empty, I will need to add a 'Start state' function
    // Into the LvMn script and here
    public bool puffFull = true;
    [HideInInspector]
    public bool puffShare = false;

    // Self reference variables
    public GameObject puff_DummyState_Full;
    public GameObject puff_DummyState_Empty;
    public GameObject puff_FullState;
    public GameObject puff_EmptyState;

    // Style/Art Mode
    private int art_Mode = 0;

    // Particle Effect when Puff goes below the cloud layer
    public ParticleSystem ref_PS;


	//##################//
	//	INITIALISATION	//
	//##################//

    public override void StartUp ()
    {
        // Initialise the parent variables
		base.Initialise();

        // There are a series of conditions that decide whether a Block belongs inside the listOf_All_Blocks_ForReference
		gM.listOf_All_Blocks_ForReference.Add(this);

        // Set what BlockType this script type is
        self_BlockType = GlobInt.BlockType.Puff;
        // Set correct boolean inside Cloud script
        cloud_Parent.bool_ContainsPuffs = true;
        // Double check if the puff is already below the cloud layer
        Check_CloudLayerBreach_OnLoad();

        // Initialise puff materials
        Initial_Puff_Material();
    }

    // This function sets the initial looks of the puff based on its surrounding puffs
    public void Initial_Puff_Material()
    {
        // First, switch off everything
        Deactivate_AllBlocks();
        // Next, update the base materials
        Update_Materials();
        // I can write this later
    }


	//##############//
	//	FUNCTIONS	//
	//##############//    

    // Check if another Puff is occupying this space and decide what to do with it
    // [Note* I can simplify this down by checking internal blocks insid the record neighbours script]
    public void Resolve_Overlapped_Storm()
    {
        Vector3 temp_Position = cloud_Parent.ref_Discrete_Position + ref_Discrete_Relative_Position;

        // Perform and raycast on self space
        RaycastHit[] hitInfo = Physics.RaycastAll(temp_Position + rbV.int_To_Card[rbV.vtUp], rbV.int_To_Card[rbV.vtDn], 1.0f);

        // Run through each item
        foreach (RaycastHit item in hitInfo)
        {
            // Ignoring List
            if (item.collider.gameObject.layer != rbV.layer_Wall
                && item.collider.gameObject.layer != rbV.layer_EditorItems
                && item.collider.gameObject.layer != rbV.layer_Mesh)
            {
                // Ignoring this object
                if (item.collider.gameObject != this.gameObject)
                {
                    // Store a temporary reference to the script, instead of calling it each time
                    Sc_Block hit_Block = item.collider.gameObject.GetComponent<Sc_Block>();

                    // If we hit a Puff
                    if (hit_Block.self_BlockType == GlobInt.BlockType.Puff)
                    {
                        // Pass the variable
                        Sc_Block_Puff hit_Puff = (Sc_Block_Puff)hit_Block;

                        // Make sure it isn't already inside the cloud_List of this Storm
                        if (!cloud_Parent.storm_Parent.listOf_Clouds_Child.Contains(hit_Puff.cloud_Parent))
                        {
                            // Gluing only occurs it one Puff is full and the other empty
                            if ((!hit_Puff.puffFull && puffFull) || (hit_Puff.puffFull && !puffFull))
                            {
                                // Simplifying references
                                Sc_Storm hit_Storm = hit_Puff.cloud_Parent.storm_Parent;

                                // Transfer all the clouds from the hit object to the current list of clouds in the Storm
                                foreach (Sc_Cloud hit_Storm_CloudChild in hit_Storm.listOf_Clouds_Child)
                                {
                                    // Attach the Cloud
                                    cloud_Parent.storm_Parent.listOf_Clouds_Child.Add(hit_Storm_CloudChild);
                                    // And set the Cloud's new Storm Parent
                                    hit_Storm_CloudChild.storm_Parent = cloud_Parent.storm_Parent;
                                    // Also, move it to its new transform
                                    hit_Storm_CloudChild.transform.SetParent(cloud_Parent.storm_Parent.transform);
                                    // Also, set the moved Cloud's new discrete reference position
                                    // I've removed this due to a bug in which soil is checked before the cloud is able to move
                                    // This causes the cloud to be position behind it's previous move location, causing it not to move
                                    // I'm not sure why soil is updating to early and that needs to be fixed too
                                    //hit_Storm_CloudChild.ref_Discrete_Position = hit_Storm_CloudChild.transform.position;
                                }

                                // Blank the list of Clouds from the hit Storm
                                hit_Storm.listOf_Clouds_Child = new List<Sc_Cloud>();

                                // Update the glued state of this puff and the hit puff
                                puffShare = true;
                                hit_Puff.puffShare = true;

                                // I'm going to try _Not_ adding the Storms to the DoubleCheck list after glueing
                                // This makes my life a little easier when it comes to checking connections
                                // Do I only need to add the new parented Storm to the DoubleChecking list?
                                //uM.AddStorm_ToDoubleCheckList(cloud_Parent.storm_Parent);
                            }
                        }
                    }
                }
            }
        }
    }

    // This functions looks for soil and waters said soil if in contact with the Puff
    public void CheckFor_Soil()
    {
        // [Optimising] We're only concerned with unwatered Puffs
        if (puffFull)
        {
            // Check all cardinal directions
            for (int n = 0; n < rbV.cDVt; n++)
		    {                
                // Ideally, there should never be two soil blocks beneath one Puff, but, currently, if there is, all Soil will be watered
                foreach (Sc_Block temp_Block in listOf_Radial_Block_Check[n, (int)GlobInt.BlockType.Soil])
                {
                    // Grab the soil as its own script
                    Sc_Block_Soil temp_Soil = (Sc_Block_Soil)temp_Block;
                    // We only act on this Soil if it hasn't been watered yet
                    // We need to check the puffFull state again in case there are two soils beside it
                    if (temp_Soil.soilDry && puffFull)
                    {
                        // Set the soil to be dry and update its appearance
                        temp_Soil.soilDry = false;
                        temp_Soil.Update_Materials(n);
                        // Set this Puff to empty
                        puffFull = false;
                        Update_Materials();

                        // Play a SFX to indicate the soil has been watered
						cloud_Parent.gM.aC.Play_SFX("SFX_Soil_Watered");

                        // Because a Puff/Soil block has interacted, we need to check if this level is complete
						gM.Check_ActiveLevelComplete();

                        // Check if a glued set of Storms has become unglued
                        uM.CheckFor_Unglue(cloud_Parent.storm_Parent);

                        // The soil and the puff have changed, therefore we need to double check both of their relative Storms
                        uM.AddStorm_ToDoubleCheckList(temp_Soil.cloud_Parent.storm_Parent);
                        uM.AddStorm_ToDoubleCheckList(cloud_Parent.storm_Parent);
                    }
                }
            }
        }
    }

    // This function changes the puff's material if the puff state has changed
    public void Update_Materials()
    {
        if (puffFull)
        {
            // For regular design
            if (art_Mode == 0)
            {
                // Set PuffFull to active and PuffEmpty inactive
                puff_FullState.SetActive(true);
                puff_EmptyState.SetActive(false);
            }
            // For simple art mode
            else if (art_Mode == 1)
            {
                // Set Dummy PuffFull to active and Dummy PuffEmpty inactive
                puff_DummyState_Full.SetActive(true);
                puff_DummyState_Empty.SetActive(false);
            }
        }
        else
        {
            // For regular design
            if (art_Mode == 0)
            {
                // Vice versa
                puff_FullState.SetActive(false);
                puff_EmptyState.SetActive(true);
            }
            // For simple art mode
            else if (art_Mode == 1)
            {
                // Set Dummy PuffFull to active and Dummy PuffEmpty inactive
                puff_DummyState_Full.SetActive(false);
                puff_DummyState_Empty.SetActive(true);
            }
        }
    }

    // Activate Particle Effect
    public void Check_CloudLayerBreach()
    {
        // Check the height
        if ((this.transform.position.y <= 0f) && (this.transform.position.y > -1f))
        {
            // There's a few things I'd like to do at this point

            // Perform a check cloud breach function for the cloud parent
            cloud_Parent.Check_CloudLayerBreach();

            // Play your particle effect
            ref_PS.Play();

            // Create a poof noise for the cloud (Index 22)
            cloud_Parent.gM.aC.listOf_Push_SFX[22] = true;
        }
    }

    // Update breaching status, based on position (After loading), no particle effect
    public void Check_CloudLayerBreach_OnLoad()
    {
        // If we're below the cloud layer upon loading
        if (this.transform.position.y <= 0f)
        {
            // Tell the cloud layer it's been breached
            cloud_Parent.Check_CloudLayerBreach();
        }
    }

    // Change art style
    public void Switch_ArtMode(int pass_Mode)
    {
        // Change the art mode
        art_Mode = pass_Mode;

        // Switch off all blocks
        Deactivate_AllBlocks();

        // then update materials to the new mode
        Update_Materials();
    }

    // Deactivate all blocks
    void Deactivate_AllBlocks()
    {
        puff_DummyState_Full.SetActive(false);
        puff_DummyState_Empty.SetActive(false);
        puff_FullState.SetActive(false);
        puff_EmptyState.SetActive(false);
    }
}
