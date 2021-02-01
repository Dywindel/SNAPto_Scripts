using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_Soil - Soil BlockScript
//	This script contains all the relevent controls specific to the Soil block
//	which is a child to the Parent_Block script

public class Sc_Block_Soil : Sc_Block {

	//##############//
	//	VARIABLES	//
	//##############//

    // Variables related to the appearance of the Soil
    [HideInInspector]
	public bool soilDry = true;
    // public Material[] soil_Materials = new Material[2];
	// Reference to sunflower child
	public Transform sunflowerChild;
	// Reference to cover grass
	public GameObject ref_CoverGrass;
	// Reference to the small grass that grows when it rains
	public GameObject ref_SmallGrass;

	// Art mode
	private GameObject ref_SimpleSoil_Dry;
	private GameObject ref_SimpleSoil_Wet;
	private int art_Mode = 0;

	//##################//
	//	INITIALISATION	//
	//##################//

    public override void StartUp ()
    {
		// soilDry = true;

        // Initialise the parent variables
		base.Initialise();

        // Add this to the UM script for reference
        gM.listOf_All_Blocks_ForReference.Add(this);

        // Set what BlockType this script type is
        self_BlockType = GlobInt.BlockType.Soil;

		// Set correct boolean inside Cloud script
        cloud_Parent.bool_ContainsSoils = true;

		// Add this puff to the levelManager, if it exists
        if (cloud_Parent.levelManager != null)
        {
            if (!cloud_Parent.levelManager.level_ListOfSoils.Contains(this))
            {
                cloud_Parent.levelManager.level_ListOfSoils.Add(this);
            }
        }

		// Randomly rotate the flower to give variation
		sunflowerChild.transform.rotation = Quaternion.Euler(-90f, 0, UnityEngine.Random.Range(0f, 360f));

        // Update the blockMesh
        // blockMesh = this.gameObject.transform.parent.GetComponent<MeshRenderer>();

		// Create the simpleMode soil block, then deactivate it
		ref_SimpleSoil_Dry = Instantiate(rRm.ref_SimpleMode_SoilDry, this.transform.position, Quaternion.identity);
		ref_SimpleSoil_Wet = Instantiate(rRm.ref_SimpleMode_SoilWatered, this.transform.position, Quaternion.identity);
		ref_SimpleSoil_Dry.transform.SetParent(this.transform);
		ref_SimpleSoil_Wet.transform.SetParent(this.transform);
		ref_SimpleSoil_Dry.SetActive(false);
		ref_SimpleSoil_Wet.SetActive(false);
    }

    //##############//
	//	FUNCTIONS	//
	//##############//

    // This functions looks for Puffs and waters itself if near to a Puff
    public void CheckFor_Puff()
    {
        // [Optimising] We're only concerned about dry soil
        if (soilDry)
        {
            // Check all cardinal directions
            for (int n = 0; n < rbV.cDVt; n++)
		    {                
                // Ideally, there should never be two soil blocks beneath one Puff, but, currently, if there is, all Soil will be watered
                foreach (Sc_Block temp_Block in listOf_Radial_Block_Check[n, (int)GlobInt.BlockType.Puff])
                {
                    // Grab the soil as its own script
                    Sc_Block_Puff temp_Puff = (Sc_Block_Puff)temp_Block;
                    // We only act on this Soil if it hasn't been watered yet
					// We check the soilDry state again, just incase there are two puff blocks is beside it
                    if (temp_Puff.puffFull && soilDry)
                    {
                        // Set the Puff to be dry and update its appearance
                        temp_Puff.puffFull = false;
                        temp_Puff.Update_Materials();
                        // Set this Soil to be watered
                        soilDry = false;
                        Update_Materials(n);

						// Play a SFX to indicate the soil has been watered
						cloud_Parent.gM.aC.Play_SFX("SFX_Soil_Watered");

                        // Because a Puff/Soil block has interacted, we need to check if this level is complete
						gM.Check_ActiveLevelComplete();

                        // The soil and the puff have changed, therefore we need to double check both of their relative Storms
                        uM.AddStorm_ToDoubleCheckList(temp_Puff.cloud_Parent.storm_Parent);
                        uM.AddStorm_ToDoubleCheckList(cloud_Parent.storm_Parent);
                    }
                }
            }
        }
    }
    public override void Record_Neighbours()
    {
        base.Record_Neighbours();

		Vector3 temp_Position = cloud_Parent.ref_Discrete_Position + ref_Discrete_Relative_Position;

        // Check the main cd and vertical
		for (int n = 0; n < rbV.vtDn; n++) {

			// Create a raycast from the soil block and see what you hit
			RaycastHit[] hitInfo = Physics.RaycastAll(temp_Position, rbV.int_To_Card[n], 1.0f);

			// As long as an object is hit
			foreach (RaycastHit item in hitInfo) {
                // If the object is a static wall, note that for this particular cardinal direction
				if (item.collider.gameObject.layer == rbV.layer_Wall)
				{
					listOf_Radial_Wall_Check[n] = true;
				}
				// For now, we also take meshes to be solid walls
				else if (item.collider.gameObject.layer == rbV.layer_Mesh)
				{
					listOf_Radial_Wall_Check[n] = true;
				}
				// There's a few objects that we can ignore for the moment
				else if (item.collider.gameObject.layer == rbV.layer_Terrain
					|| item.collider.gameObject.layer == rbV.layer_Raft)
				{
					// Do Nothing
				}
				else
				{
					// Store a temporary reference to the script, instead of calling it each time
					Sc_Block hit_Block = item.collider.gameObject.GetComponent<Sc_Block>();

					if (Accepted_Neighbours(hit_Block))
					{
						// If the object hit is a player, add its script to the list of items
						// and the same for everything else of the same type
						foreach (GlobInt.BlockType temp_BlockType in (GlobInt.BlockType[]) Enum.GetValues(typeof(GlobInt.BlockType)))
						{
							if (hit_Block.self_BlockType == temp_BlockType)
							{
								listOf_Radial_Block_Check[n, (int)temp_BlockType].Add(hit_Block);
							}

							// This is useful for later, if there is a player beside us
							if (hit_Block.self_BlockType == GlobInt.BlockType.Play)
							{
								cloud_Parent.storm_Parent.cardinal_Player_Check = true;
							}
						}

						// As long as this item isn't the player
						if (hit_Block.self_BlockType != GlobInt.BlockType.Play)
						{
							// Add the Cloud to this list
							if (!listOf_Radial_Cloud_Check[n].Contains(hit_Block.cloud_Parent))
							{
								// Store its reference to the Cloud for later collision testing
								listOf_Radial_Cloud_Check[n].Add(hit_Block.cloud_Parent);
							}

							// Add the Storm to this list
							if (!listOf_Radial_Storm_Check[n].Contains(hit_Block.cloud_Parent.storm_Parent))
							{
								// Store its reference to the Storm for later collision testing
								listOf_Radial_Storm_Check[n].Add(hit_Block.cloud_Parent.storm_Parent);
							}
						}
					}
				}
            }
        }
    }

    // This function changes the soil's material if the soil state has changed
	// Future update, I would like the flower to point in the direction of where the soil was watered OR
	// Something to show which direction the flower was watered, maybe just have the flower lean or something
	// By setting the n value to a start value, it allows passing n to be an option parameter
    public void Update_Materials(int pass_n = 0)
    {
		// If in regular art mode
		if (art_Mode == 0)
		{
			if (soilDry) {
				// Shrink the sunflower block
				sunflowerChild.localScale = new Vector3(0.5f, 0.5f, 0.5f);
				sunflowerChild.localPosition = new Vector3(0, 0, 0);
				// Change the soil block
				// blockMesh.material = soil_Materials[0];
				// Switch on or off the grass to denote its been watered
				ref_CoverGrass.SetActive(false);
			} else {
				//Shrink the sunflower block
				sunflowerChild.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
				sunflowerChild.localPosition = new Vector3(0, 0, 1.02f);
				// Change the soil block
				// blockMesh.material = soil_Materials[1];
				// Switch on or off the grass to denote its been watered
				ref_CoverGrass.SetActive(true);
			}
		}
		else if (art_Mode == 1)
		{
			if (soilDry)
			{
				ref_SimpleSoil_Dry.SetActive(true);
				ref_SimpleSoil_Wet.SetActive(false);
			}
			else if (!soilDry)
			{
				ref_SimpleSoil_Dry.SetActive(false);
				ref_SimpleSoil_Wet.SetActive(true);
			}
		}
    }

	// This is used to update the state of the grass, when its raining
	public void Update_Grass(int pass_Mode)
	{
		// No grass
		if (pass_Mode == 0)
		{
			ref_SmallGrass.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
			ref_SmallGrass.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
		else
		{
			// Randomise the rotation
			ref_SmallGrass.transform.rotation = Quaternion.Euler(-90f, 0f, 1*UnityEngine.Random.Range(0, 360));
			// Set the right scale
			ref_SmallGrass.transform.localScale = new Vector3(0.75f, 0.75f, 1.0f);
			if (pass_Mode == 1)
			{
				// Set the height
				ref_SmallGrass.transform.localPosition = new Vector3(0f, 0f, 0f);
			}
			if (pass_Mode == 2)
			{
				// Set the height
				ref_SmallGrass.transform.localPosition = new Vector3(0f, 0f, 0.1f);
			}
			if (pass_Mode == 3)
			{
				// Set the height
				ref_SmallGrass.transform.localPosition = new Vector3(0f, 0f, 0.3f);
			}
		}
	}

	// Deactivate all blocks
    void Deactivate_AllBlocks()
    {
        sunflowerChild.gameObject.SetActive(false);
        ref_CoverGrass.SetActive(false);
		this.GetComponent<MeshRenderer>().enabled = false;
        ref_SimpleSoil_Dry.SetActive(false);
        ref_SimpleSoil_Wet.SetActive(false);
    }

	public void Switch_ArtMode(int pass_Mode)
	{
		art_Mode = pass_Mode;

		Deactivate_AllBlocks();

		if (art_Mode == 0)
		{
			sunflowerChild.gameObject.SetActive(true);
			ref_CoverGrass.SetActive(true);
			this.GetComponent<MeshRenderer>().enabled = true;
		}
		else if (art_Mode == 1)
		{
			ref_SimpleSoil_Dry.SetActive(true);
        	ref_SimpleSoil_Wet.SetActive(true);
		}

		Update_Materials();
	}
}
