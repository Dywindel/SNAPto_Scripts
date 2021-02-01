using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

////////////////////////////////////////////////////////
//
//  Sc_Block_Lift - Lift Script
//  This lift raises and lowers when an object sets
//  pressure on it

public class Sc_Block_Lift : Sc_Block {


    //##############//
	//	VARIABLES	//
	//##############//

	//References to World Objects
    private Sc_Player player;

    //References to game objects
    [HideInInspector]
    public bool isTriggeredOnce = false;            //Ensure we only do this once after an item has first sat on the lift
    [HideInInspector]
    public bool isBlockPresent = false;            //Ensure we only do this once while a block is sitting on the raft
    private Sc_Block listOf_Blocks_Resting;                 //A list of references to the blocks sitting on the Raft

    //Self Variables
    //I'll need to add a start value if I want to use this with the reset LvMn
    public bool lift_isRaised = true;

    //This game object is an invisible wall that we turn on when the lift is raised and vice versa
    public GameObject obj_InvsFloor;
    public Sc_Block_Floor[] obj_InvsFloor_Ref;

    //We need some special functions to check what state the lift is in. Whether it's falling, rising, at its peak or at its base
    //Floor 0 is the base floor
    [HideInInspector]
    public int currentFloor = 0;
    [HideInInspector]
    public bool lift_isRising = false;
    [HideInInspector]
    public bool lift_isStationary = true;   // When the lift is stationary, it acts as a regular Storm

    // The lift's base floor is 0 and topFloor is defined below. A topFloor of 1
    // Means the lift technically has 2 floors in total
    [Range (1, 6)]
    public int topFloor;

	//##################//
	//	INITIALISATION	//
	//##################//

    // Start is called before the first frame update
    public override void StartUp ()
    {
        //Initialise the parent variables
		base.Initialise();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Sc_Player>();

        //Pass yourself to your UM
        gM.listOf_All_Blocks_ForReference.Add(this);

        //Set what BlockType this script type is
        self_BlockType = GlobInt.BlockType.Lift;

        //Tell the attached Storm that it is a lift
        //And pass it a reference to this script
        cloud_Parent.storm_Parent.isLift = true;
        cloud_Parent.storm_Parent.ref_LiftScript = this;

        //If the lift is as its peak, we set the currentFloor to max
        if (lift_isRaised)
            currentFloor = topFloor;

        //I need to create all the objects that are going to act as the lift
        obj_InvsFloor_Ref = new Sc_Block_Floor[topFloor];
        Place_LiftComponents();
    }

    ///////////////
	// FUNCTIONS //
	///////////////

    // Figure out if the lift is going to move and in which direction
    public void Check_PreOperate_Lift()
    {
        // We need to update the Storm Neighbours of this lift
        cloud_Parent.storm_Parent.Collect_Neighbours();

        // We're not yet concerned about what's sitting on the Lift, we want to begin by asking what state the lift is in
        // Regular operation occurs when the lift is at its peak or at its base
        if ((currentFloor == 0) ||
            (currentFloor == topFloor))
        {
            // We can update the isRising boolean here, before the lift begins movement
            if (currentFloor == 0)
                lift_isRising = true;
            else
                lift_isRising = false;

            //Quick check through every blocktype
            bool quickCheckForEachBlockType = false;

            // Because this is a child of the block script, I have a variable which tells me if the player is sitting on the lift
            // However, we will, in future, also need to note any Clouds sitting on the lift
            foreach (GlobInt.BlockType temp_BlockType in (GlobInt.BlockType[]) Enum.GetValues(typeof(GlobInt.BlockType)))
            {
                // If there are any blocks within the list of blocks, then set the BlockPresent boolean to true
                if (listOf_Radial_Block_Check[rbV.vtUp, (int)temp_BlockType].Count > 0)
                {
                    // We indicate that a block is present
                    quickCheckForEachBlockType = true;
                }
            }

            if (quickCheckForEachBlockType)
                isBlockPresent = true;
            else
                isBlockPresent = false;

            // If there is a block here
            if (isBlockPresent)
            {
                // And the trigger is off
                if (!isTriggeredOnce)
                {
                    // We set the state to have been triggered
                    isTriggeredOnce = true;
                    // Then we move the lift
                    uM.Operate_Lift_Once(this, lift_isRising);
                }
            }
            else
            {
                //As soon as any block has gone, we can switch off the doOnce trigger.
                isTriggeredOnce = false;
            }
        }
        //In any other case, we have a situation where the lift is, (if this code works), already moving
        else
        {
            //In this case, we bypass those worrisome check and go straight to operating the lift as before
            uM.Operate_Lift_Once(this, lift_isRising);
        }
    }

    //This function places all the invisible walls that make up the lift
    void Place_LiftComponents()
    {
        // Place the screw components
        for (int i = 0; i < topFloor; i++)
        {
            // The initial position of the invisible block should be the same, using the Storm's diamond as a base for all lifts (Regardless of up or down starting state)
            // I've partially missaligned the screws to be just below the starting point so that They don't clip into other blocks

            obj_InvsFloor_Ref[i] = Instantiate(obj_InvsFloor, new Vector3(0, i - 0.05f, 0) + this.transform.position,  Quaternion.identity).GetComponent<Sc_Block_Floor>();
            
            // Rescale the components slightly to avoid clipping
            obj_InvsFloor_Ref[i].gameObject.transform.localScale = new Vector3(0.9f, 1f, 0.9f);

            // Make the top part of the lift a little bit short and move it down a little
            if (i + 1 == topFloor)
            {
                obj_InvsFloor_Ref[i].gameObject.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                obj_InvsFloor_Ref[i].gameObject.transform.position = new Vector3(0, i - 0.05f - 0.05f, 0) + this.transform.position;
            }

            //Set the parent to this screw block
            obj_InvsFloor_Ref[i].gameObject.transform.SetParent(cloud_Parent.transform);
        }

        // Add to the list of child blocks
        if (gM.printBugs) {print ("the cloud's liftOf_ChildBlocks is an array, which is a problem");};
        // This is currently a bit a pickle as the GetComponentsInChildren creates an array not a list
        // My workaround is to just reset the cloud's listOf_ChildBlocks for now
        cloud_Parent.listOf_ChildBlocks = cloud_Parent.GetComponentsInChildren<Sc_Block>();

        for (int i = 0; i < topFloor; i++)
        {
            //Pass the cloud parent
            obj_InvsFloor_Ref[i].cloud_Parent = cloud_Parent;
            //Set the Floor script as part of a lift
            obj_InvsFloor_Ref[i].isPartLift = true;
            //Start the flamin thing
            obj_InvsFloor_Ref[i].StartUp();
        }

        //We then operate on them depending on whether we're current at the top or base of the lift
        RevealHide_LiftComponents(currentFloor == topFloor);
    }

    //This script activates and deactivates all the invisble floors that make up the lift
    void RevealHide_LiftComponents(bool pass_Activate)
    {   
        foreach (Sc_Block_Floor obj in obj_InvsFloor_Ref)
        {
            obj.RevealHide_InvisibleFloor(pass_Activate);
        }
    }

    //We dictate whether a lift component should be revealed or hidden based
    //On the lifts current floor and whether it's moving up or down

    //REMEMBER: This is for the specific case that the blocks are static meshes
    //This entire setup changes a little it the blocks are animated
    public void Check_RevealHide_LiftComponents(bool pass_isRising, int pass_CurrentFloor)
    {
        //We're about to perform the lift movement and we've just updated
        //The current floor value

        //If the lift is rising
        if (pass_isRising)
        {
            //We can activate the block just below the moving lift
            obj_InvsFloor_Ref[currentFloor - 1].RevealHide_InvisibleFloor(true);
        }
        //If the lift is falling
        else if (!pass_isRising)
        {
            //We don't do anything if the current floor being passed is topfloor - 1
            //For every other case, which switch of the corresponding block
            if (pass_CurrentFloor < (topFloor - 1))
            {
                //We now disable the block just above where we are about to move
                obj_InvsFloor_Ref[currentFloor + 1].RevealHide_InvisibleFloor(false);
            }
        }
    }


    //////////////////////
    //  ACTIONS         //
    //////////////////////

    //This action performs the lift movement
    public void Action_Lift(bool pass_isRising)
    {
        //Set the movement boolean to true
        uM.moveBool_Lift = true;

        //Here is where I update all the variables that control how the lift operates
        //We first update the currentFloor
        if (lift_isRising)
            currentFloor += 1;
        else
            currentFloor -= 1;
        
        //Here I can figure out when the floor blocks should and should not activate
        //RevealHide_LiftComponents(currentFloor == topFloor);
        Check_RevealHide_LiftComponents(lift_isRising, currentFloor);

        //Sound, if needed        

        //The animation for this is a little more complex now
        //I have to animat certain objects in one direction depending on
        //Their stacking position in the lift
        StartCoroutine(rbA.Move_Lift(transform, pass_isRising, rbA.t_Case));

        // Would be nice to have each lift block component rotate as the lift goes up and down
        foreach (Sc_Block_Floor temp_LiftScrew in obj_InvsFloor_Ref)
        {
            // Make the lift screw rotate
            // WARNING: This may cause some collision issues because the gameobject is rotating (It may not point in the right direction
            // When performing collision detection any more
            StartCoroutine(rbA.Lift_RotateScrew(temp_LiftScrew.gameObject.transform, pass_isRising, rbA.t_Case));
        }
    }
}
