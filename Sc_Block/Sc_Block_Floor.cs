using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_Block_Floor : Sc_Block {

	//##############//
	//	VARIABLES	//
	//##############//

	//For empty floors, we want to disable the floor mesh renderer, basically
	//This is acting as an invisible wall
	public bool isInvisibleWall = false;

	//For lifts, we want to know if the shape is part of a lift, which will turn on and off regularly
	public bool isPartLift = false;
	[HideInInspector]
	bool isVisible = false;

	// Art mode
	private GameObject ref_SimpleFloor;
	private int art_Mode = 0;


	//##################//
	//	INITIALISATION	//
	//##################//

	// Use this for initialization
	public override void StartUp ()
	{
		//Initialise the parent variables
		base.Initialise();

		//Add this to the UM script
		gM.listOf_All_Blocks_ForReference.Add(this);

		//Set what BlockType this script type is
        self_BlockType = GlobInt.BlockType.Flor;

		//If this is acting as an invisible wall
		if (isInvisibleWall)
		{
			this.GetComponent<MeshRenderer>().enabled = false;
		}

		// Create the simpleMode floor block, then deactivate it
		ref_SimpleFloor = Instantiate(rRm.ref_SimpleMode_Floor, this.transform.position, Quaternion.identity);
		ref_SimpleFloor.transform.SetParent(this.transform);
		ref_SimpleFloor.SetActive(false);
	}

	//////////////////////////////////
	//	RECORD NEIGHBOURS - FLOORs	//
	//////////////////////////////////

	//Specifically for the floor, there will be cases where the player can exist inside the shape, like an archway.
	//We need to add this check to the record neighbours function
	public override void Record_Neighbours()
    {	
		if (!isPartLift || (isPartLift && isVisible))
		{
			base.Record_Neighbours();

			Vector3 temp_Position = cloud_Parent.ref_Discrete_Position + ref_Discrete_Relative_Position;

			//Create a raycast going to the inside of the shape
			RaycastHit[] hitInfo = Physics.RaycastAll(temp_Position + rbV.int_To_Card[rbV.vtUp], rbV.int_To_Card[rbV.vtDn], 1.0f);
			//As long as an object is hit
			foreach (RaycastHit item in hitInfo)
			{
				//If the hit item of the object is the player
				if (item.collider.gameObject.layer == rbV.layer_Player)
				{
					//Then, we confirm that this Storm has a player inside it
					cloud_Parent.storm_Parent.inside_Player_Check = true;
				}
			}
		}
		else
		{
			//The shape ignores neighbours and is inactive
		}
	}

	//##############//
	//	FUNCTIONS	//
	//##############//

	//This function not only reveal/hides the floor, but also disables the box collider to allow other objects to pass through it
	public void RevealHide_InvisibleFloor(bool pass_IsVisible)
	{
		//Update block's state
		isVisible = pass_IsVisible;

		if (isVisible)
		{
			this.gameObject.SetActive(true);
			//this.GetComponent<BoxCollider>().isTrigger = true;
		}
		else
		{
			//this.GetComponent<BoxCollider>().isTrigger = false;
			this.gameObject.SetActive(false);
		}
	}

	// Change how the floor looks
	public void Update_Materials()
	{
		// If this item is not an invisible floor
		if (!isInvisibleWall)
		{
			if (art_Mode == 0)
			{
				// Disable the simple floor and enable the main mesh renderer
				ref_SimpleFloor.SetActive(false);
				this.GetComponent<MeshRenderer>().enabled = true;
			}
			else if (art_Mode == 1)
			{
				// Vice versa
				ref_SimpleFloor.SetActive(true);
				this.GetComponent<MeshRenderer>().enabled = false;
			}
		}
	}

	// Change art style
    public void Switch_ArtMode(int pass_Mode)
    {
        // Change the art mode
        art_Mode = pass_Mode;

        // then update materials to the new mode
        Update_Materials();
    }
}
