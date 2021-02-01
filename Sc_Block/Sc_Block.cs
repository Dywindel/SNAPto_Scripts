using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_Par_Block - Parent Block Script
//	This script is the parent block script all children block scripts will be 
//	derrived from. Including: Soil, Floor and Puff

public class Sc_Block : MonoBehaviour {
	

	//##############//
	//	VARIABLES	//
	//##############//

	//References to World Objects
	protected Sc_GM gM;					// Reference to the GM
	protected Sc_UM uM;					// Reference to the update manager
	protected Sc_RB_Values rbV;			// Reference to the general common values
	protected Sc_RB_Animation rbA;		// Reference to animation values
	protected Sc_RenderManager rRm;		// Reference to the render manager, that stores meshes
	protected AudioSource aU;			// Reference to the AudioSource in the gM

	// References to game objects
	[HideInInspector]
	public Sc_Cloud cloud_Parent;
	[HideInInspector]
	public GlobInt.BlockType self_BlockType;
	[HideInInspector]
	public List<Sc_Storm>[] listOf_Radial_Storm_Check;
	[HideInInspector]
	public List<Sc_Block>[,] listOf_Radial_Block_Check;
	[HideInInspector]
	public List<Sc_Cloud>[] listOf_Radial_Cloud_Check;
	[HideInInspector]
	public bool[] listOf_Radial_Wall_Check;
	public List<Sc_Rotor>[] listOf_Radial_Rotor_Check;

	// Protect from Record Neighbours if data has been collected
	[HideInInspector]
	public bool hasRecordedNeighbours = false;

	// This variable is currently only used by the player, but I'm putting it in here because
	// I'm not farbAliar enough with overrides yet
	[HideInInspector]
	public bool[] listOf_Radial_CloseFloors_Check;
	// For one very specific scenario when the player steps onto a Storm that they are also pushing
	public List<Sc_Storm>[] listOf_Radial_CloseFloors_Storm_Check;
	// Same thing for the player, checking if the player is standing on a ghost bridge
	[HideInInspector]
	public List<Sc_Storm> player_StandingOnGhostBridge;
	// Check if the player is floating above the group
	[HideInInspector]
	public bool player_isPlayerFloating;

	// References to this object's appearance
	protected MeshRenderer blockMesh;

	// Reference to this blocks descrete position, relative to its cloud (As the block will never move from their cloud)
	public Vector3 ref_Discrete_Relative_Position;
	// And for the player
	public Vector3 ref_Discrete_Position;


	//##################//
	//	INITIALISATION	//
	//##################//

	// This will called by the Cloud and each block type will have its own version which I will create
	public virtual void StartUp()
	{
		//Will be create in child block
	}

	// This will be called by the child scripts in start to establish some of the basic, shared variables
	protected void Initialise ()
	{
		// Grab the Game Manager, dictionary, movement library...
		GameObject gM_GameObject = GameObject.FindGameObjectWithTag("GM");
		gM = gM_GameObject.GetComponent<Sc_GM>();
		uM = gM.uM;
		rbV = gM.rbV;
		rbA = gM.rbA;
		rRm = gM.rRm;
		aU = gM.aC.GetComponent<AudioSource>();

		// Update the block's relative position to its cloud
		ref_Discrete_Relative_Position = this.transform.localPosition;

		// Reference to the object's appearance
		blockMesh = GetComponent<MeshRenderer>();

		// Initialise lists
		Initialise_Lists();
	}

	// Initialise any lists used in this script
	protected void Initialise_Lists()
	{
		// Define list lengths, the first term is the cardinal direction, the second term is a number dictating each block child type
		listOf_Radial_Storm_Check = new List<Sc_Storm>[rbV.cDVt];
		listOf_Radial_Block_Check = new List<Sc_Block>[rbV.cDVt, Enum.GetNames(typeof(GlobInt.BlockType)).Length];
		listOf_Radial_Cloud_Check = new List<Sc_Cloud>[rbV.cDVt];
		listOf_Radial_Wall_Check = new bool[rbV.cDVt];
		listOf_Radial_Rotor_Check = new List<Sc_Rotor>[rbV.cDVt];
		listOf_Radial_CloseFloors_Check = new bool[rbV.cDVt];
		listOf_Radial_CloseFloors_Storm_Check = new List<Sc_Storm>[rbV.cDVt];

		Reset_Lists();
	}
	

	//##############//
	//	FUNCTIONS	//
	//##############//

	// Record the neighbours around this block
	public virtual void Record_Neighbours()
	{
		if (!hasRecordedNeighbours)
		{
			hasRecordedNeighbours = true;

			Vector3 temp_Position;
			if (self_BlockType == GlobInt.BlockType.Play)
				temp_Position = ref_Discrete_Position;
			else
				temp_Position = cloud_Parent.ref_Discrete_Position + ref_Discrete_Relative_Position;

			// First, if the Block's position is lower than or equal to the gM.BaseLevel, we automatically say there is a floor beneath it
			if (temp_Position.y <= rbV.baseLevel) {
				listOf_Radial_Wall_Check[rbV.vtDn] = true;
			}
			
			// Check all Cardinal direction
			for (int n = 0; n < rbV.cDVt; n++)
			{
				// Create a raycast and see what objects you hit, if any
				// Use the relative discrete plus cloud position to cast the ray
				RaycastHit[] hitInfo = Physics.RaycastAll(temp_Position, rbV.int_To_Card[n], 1.0f);

				// As long as an object is hit
				foreach (RaycastHit item in hitInfo)
				{
					// Some blocks can be ignored completely
					if (item.collider.gameObject.layer == rbV.layer_Ignore)
					{
						// Do Nothing
					}
					// If the object is a static wall, note that for this particular cardinal direction
					else if (item.collider.gameObject.layer == rbV.layer_Wall)
					{
						listOf_Radial_Wall_Check[n] = true;
					}
					// If the block is a mesh object, all blocks perceive this as a wall, except Puffs
					else if (item.collider.gameObject.layer == rbV.layer_Mesh)
					{
						// Meshes act like a wall, unless we are a puff block
						if (self_BlockType != GlobInt.BlockType.Puff)
						{
							listOf_Radial_Wall_Check[n] = true;
						}
					}
					// There's a few objects that we can ignore for the moment
					else if (item.collider.gameObject.layer == rbV.layer_Terrain)
					{
						// Do Nothing
					}
					else if (item.collider.gameObject.layer == rbV.layer_Raft)
					{
						// Do Nothing
					}
					else if (item.collider.gameObject.layer == rbV.layer_EditorItems)
					{
						// Do Nothing
					}
					else if (item.collider.gameObject.layer == rbV.layer_Rotor)
					{
						// Add the rotor to our radial list
						listOf_Radial_Rotor_Check[n].Add(item.collider.gameObject.GetComponent<Sc_Rotor>());
						// Also, these count as walls
						listOf_Radial_Wall_Check[n] = true;
					}
					else
					{
						// Store a temporary reference to the script, instead of calling it each time
						Sc_Block hit_Block = item.collider.gameObject.GetComponent<Sc_Block>();

						if (Accepted_Neighbours(hit_Block))
						{
							// If the object looking is a player, add its script to the list of items
							// and the same for everything else of the same type
							foreach (GlobInt.BlockType temp_BlockType in (GlobInt.BlockType[]) Enum.GetValues(typeof(GlobInt.BlockType)))
							{
								if (hit_Block.self_BlockType == temp_BlockType)
								{
									listOf_Radial_Block_Check[n, (int)temp_BlockType].Add(hit_Block);
								}

								// This is useful for later, if there is a player beside us
								// (As long as we aren't the player)
								if ((hit_Block.self_BlockType == GlobInt.BlockType.Play) && (self_BlockType != GlobInt.BlockType.Play))
								{
									cloud_Parent.storm_Parent.cardinal_Player_Check = true;
								}
							}
							
							// Special case if there is a lift below the player
							/*
							if (self_BlockType != GlobInt.BlockType.Play)
							{
								if (hit_Block.self_BlockType == GlobInt.BlockType.Lift && n >= gM.cD)
								{
									if (gM.printBugs) {print ("Downward Storms look at lifts as a solid wall, which won't work for animating downwards");};
									// The problem here is that, if I make this a wall, then I can't do the downward
									// Falling animation later on for a lowering lift (Because the cloud will always read
									// A wall being in the way)
									listOf_Radial_Wall_Check[n] = true;
									// cloud_Parent.storm_Parent.isSittingOnLift = true;
								}
							}*/

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

						// Special case for ghost blocks and players
						// Special case for lifts and players
						if (self_BlockType == GlobInt.BlockType.Play)
						{
							// There's a specific case for when we're the player and a ghost Puff is beside us
							// This is outside the 'Accepted neighbours' section, because we want to pick up ghosts
							if (hit_Block.self_BlockType == GlobInt.BlockType.Puff)
							{
								Sc_Block_Puff temp_Puff = (Sc_Block_Puff)hit_Block;
								//If this puff is a ghost, we can store that a floor exists here
								if (!temp_Puff.puffFull && gM.isDryPuffBridge)
								{
									listOf_Radial_CloseFloors_Check[n] = true;
								}
							}
						}
					}
				}
			}

			// Specifically for the player only
			if (self_BlockType == GlobInt.BlockType.Play)
			{
				for (int n = 0; n < rbV.cD; n++)
				{
					// We only do this if the floor variable is on
					if (gM.isFloorOn)
					{
						// Create a raycast to one side and down, but only for the four cardinal directions
						RaycastHit[] hitInfo_CloseFloors = 
							Physics.RaycastAll(temp_Position + rbV.int_To_Card[n], rbV.int_To_Card[rbV.vtDn], 1.0f);

						foreach (RaycastHit item in hitInfo_CloseFloors)
						{
							// Some blocks can be ignored completely
							if (item.collider.gameObject.layer == rbV.layer_Ignore)
							{
								// Do Nothing
							}
							// If the object is a static wall, note that for this particular cardinal direction
							// We also allow Rafts to act as floors
							// We also allow Meshes to act as floors
							else if (item.collider.gameObject.layer == rbV.layer_Wall
								|| item.collider.gameObject.layer == rbV.layer_Raft
								|| item.collider.gameObject.layer == rbV.layer_Mesh)
							{
								listOf_Radial_CloseFloors_Check[n] = true;
							}
							// There's a few objects that we can ignore for the moment
							else if (item.collider.gameObject.layer == rbV.layer_Terrain
									|| item.collider.gameObject.layer == rbV.layer_EditorItems)
							{
								// Do Nothing
							}
							else
							{
								// Store a temporary reference to the script, instead of calling it each time
								Sc_Block hit_Block = item.collider.gameObject.GetComponent<Sc_Block>();

								if (Accepted_Neighbours(hit_Block))
								{
									Sc_Storm temp_Storm = hit_Block.cloud_Parent.storm_BirthParent;
									// For now, I'll just set all objects to true.
									// If there is _any_ block in this spot, the player can walk on it
									listOf_Radial_CloseFloors_Check[n] = true;
									// I'm recording what storm makes up the closefloor
									if (!listOf_Radial_CloseFloors_Storm_Check[n].Contains(temp_Storm))
									{
										listOf_Radial_CloseFloors_Storm_Check[n].Add(temp_Storm);
									}
								}
							}
						}
					}
				}

				// This is a specific case to check if the player is standing on a ghost bridge
				if (gM.isDryPuffBridge)
				{
					RaycastHit[] hitInfo_ghostbridge = Physics.RaycastAll(temp_Position + rbV.int_To_Card[1], rbV.int_To_Card[3], 1.0f);

					foreach (RaycastHit item in hitInfo_ghostbridge)
					{
						if (item.collider.gameObject.layer == rbV.layer_Puff)
						{
							// We can cast it
							Sc_Block_Puff temp_Puff = (Sc_Block_Puff)item.collider.gameObject.GetComponent<Sc_Block>();

							// If its a ghost
							if (!temp_Puff.puffFull)
							{
								// We record the Storm inside the player's list
								player_StandingOnGhostBridge.Add(temp_Puff.cloud_Parent.storm_Parent);
							}
						}
					}
				}

				// Finally, for the player only, we check if there are any object below the player that can be stood on
				if (!listOf_Radial_Wall_Check[rbV.vtDn] && listOf_Radial_Storm_Check[rbV.vtDn].Count == 0)
				{
					player_isPlayerFloating = true;
				}
			}
		}
		else
		{
			// Do Nothing
		}
	}

	// This function takes the specified neighbour that's being checked and confirms if it follows the checking rules
	protected bool Accepted_Neighbours(Sc_Block temp_Block)
	{
		// If the neighbour is a Puff
		if (temp_Block.self_BlockType == GlobInt.BlockType.Puff)
		{
			Sc_Block_Puff temp_Puff = (Sc_Block_Puff)temp_Block;
			// We ignore them if they're in ghost form if we are a player or a puff block
			if (self_BlockType == GlobInt.BlockType.Play && !temp_Puff.puffFull ||
				self_BlockType == GlobInt.BlockType.Puff && !temp_Puff.puffFull)
				return false;
			// Or, if I'm in ghost form
			if (self_BlockType == GlobInt.BlockType.Puff)
			{
				Sc_Block_Puff self_Puff = (Sc_Block_Puff)this;
				if (!self_Puff.puffFull)
					return false;
			}
		}

		// If the neighbour is a player
		if (temp_Block.self_BlockType == GlobInt.BlockType.Play)
		{
			// And I am a ghost
			if (self_BlockType == GlobInt.BlockType.Puff)
			{
				Sc_Block_Puff self_Puff = (Sc_Block_Puff)this;
				if (!self_Puff.puffFull)
					return false;
			}
		}
		// We ignore ghost puffs and that's about it for now
		return true;
	}

	// Reset the lists (Including record neighbour lists)
	public void Reset_Lists()
	{
		
		// Reset the Record Neighbours bool
		hasRecordedNeighbours = false;

		for (int n = 0; n < rbV.cDVt; n++)
		{
			for (int j = 0; j < Enum.GetNames(typeof(GlobInt.BlockType)).Length; j++)
			{
				listOf_Radial_Block_Check[n, j] = new List<Sc_Block>();
			}

			listOf_Radial_Storm_Check[n] = new List<Sc_Storm>();
			listOf_Radial_Cloud_Check[n] = new List<Sc_Cloud>();
			listOf_Radial_Wall_Check[n] = false;
			listOf_Radial_Rotor_Check[n] = new List<Sc_Rotor>();
			listOf_Radial_CloseFloors_Check[n] = false;
			listOf_Radial_CloseFloors_Storm_Check[n] = new List<Sc_Storm>();
			player_StandingOnGhostBridge = new List<Sc_Storm>();
			player_isPlayerFloating = false;
		}
	}
}
