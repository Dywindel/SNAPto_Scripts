using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_Dictionaries - Dictionaries
//	This script stores the dictionary items that are referenced through the games
//	mechanics

public class Sc_RB_Values : MonoBehaviour {

	[HideInInspector]
	public Dictionary<int, Vector3> faceInt_To_Euler;			//This converts the Facing Direction Int into Euler Angles
	[HideInInspector]
	public Dictionary <int, Vector3> int_To_Card;				//This converts the Facing Direction Int into a Vector3 Direction
	[HideInInspector]
	public Dictionary <Vector3, int> card_To_Int;				//This converts a Vector3 Direction into the Facing Direction Int
	//[HideInInspector]
	//public enum BlockType : int {Play, Flor, Puff, Soil};		//Useful references to the types of blocks available

	//Fixed Cardinal Directions
	[System.NonSerialized]
	public int cDVt = 4 + 2;			// Cardinal directions + Vertical
	[System.NonSerialized]
    public int cD = 4;					// Cardinal directions
	[System.NonSerialized]
	public int vtDn = 4;				// Down
	[System.NonSerialized]
	public int vtUp = 5;				// Up

	//Mesh cardinal directions
	[System.NonSerialized]
	public int mD = 8;

	[System.NonSerialized]
	public float baseLevel = -3.5f;		// Lowest floor in the game. // The Storms will stop falling when they reach this Y-value
										// Cloud layer height is based on this value

	//Draw height
	//This stops the drawing of grass cover below a certain height
	[System.NonSerialized]
	public int cover_DrawHeight = -1;

	//Layer references
	[System.NonSerialized]
	public int layer_Ignore = 20;		//Sometimes I want to ignore a box entirely for a separate purpose
	[System.NonSerialized]
	public int layer_Player = 8;
	[System.NonSerialized]
	public int layer_EditorItems = 17;
	[System.NonSerialized]
	public int layer_Wall = 9;			//Reference to the wall layer value
	[System.NonSerialized]
	public int layer_Raft = 13;
	[System.NonSerialized]
	public int layer_Rotor = 11;
	[System.NonSerialized]
	public int layer_Terrain = 15;
	[System.NonSerialized]
	public int layer_Puff = 10;
	[HideInInspector]
	public int layer_Mesh = 22;

	// Reference to a marker that is used to help move blocks
	public GameObject ref_Marker;

	//Button mapping
	[System.NonSerialized]
	public float[] jen_TimeDelay = {0.15f, 1.0f, 1.0f, 1.0f}; 	//Time delay between the action of a button when held down
	// For 0 - Back button, 1 - Swivel camera, 3 - Reset level button
	//public float dir_TimeDelay = 5.0f;		//Time delay between movement steps if a direction is held down
	//I don't think I want a delay between movement

	
	//public Color Temp_Color = new Color (0.0f, 1.0f, 0.67f, 1.0f);

	//I may set this to static later, or have it as a function that's called first inside the game manager. For now, to make things easier, I'm just going to call it on awake
	void Awake ()
	{
		//This converts the Facing Direction Int into Euler Angles
		faceInt_To_Euler = new Dictionary<int, Vector3>();
		faceInt_To_Euler.Add(0, new Vector3(0, 0, 0));
		faceInt_To_Euler.Add(1, new Vector3(0, 90, 0));
		faceInt_To_Euler.Add(2, new Vector3(0, 180, 0));
		faceInt_To_Euler.Add(3, new Vector3(0, 270, 0));

		//This converts the Facing Direction Int into a Vector3 Direction
		int_To_Card = new Dictionary <int, Vector3>();
		int_To_Card.Add(0, new Vector3(0, 0, 1));
		int_To_Card.Add(1, new Vector3(1, 0, 0));
		int_To_Card.Add(2, new Vector3(0, 0, -1));
		int_To_Card.Add(3, new Vector3(-1, 0, 0));
		int_To_Card.Add(4, new Vector3(0, -1, 0));
		int_To_Card.Add(5, new Vector3(0, 1, 0));
		int_To_Card.Add(6, new Vector3(0, 0, 0));

		//This converts a Vector3 Direction into the Facing Direction Int
		card_To_Int = new Dictionary <Vector3, int>();
		card_To_Int.Add(new Vector3(0, 0, 1), 0);
		card_To_Int.Add(new Vector3(1, 0, 0), 1);
		card_To_Int.Add(new Vector3(0, 0, -1), 2);
		card_To_Int.Add(new Vector3(-1, 0, 0), 3);
		card_To_Int.Add(new Vector3(0, -1, 0), 4);
		card_To_Int.Add(new Vector3(0, 1, 0), 5);
		card_To_Int.Add(new Vector3(0, 0, 0), 6);
	}

	//Lazy solution
	//I'm just going to run the dictionary for now
	public void AwakeInEditor()
	{
		//This converts the Facing Direction Int into Euler Angles
		faceInt_To_Euler = new Dictionary<int, Vector3>();
		faceInt_To_Euler.Add(0, new Vector3(0, 0, 0));
		faceInt_To_Euler.Add(1, new Vector3(0, 90, 0));
		faceInt_To_Euler.Add(2, new Vector3(0, 180, 0));
		faceInt_To_Euler.Add(3, new Vector3(0, 270, 0));

		//This converts the Facing Direction Int into a Vector3 Direction
		int_To_Card = new Dictionary <int, Vector3>();
		int_To_Card.Add(0, new Vector3(0, 0, 1));
		int_To_Card.Add(1, new Vector3(1, 0, 0));
		int_To_Card.Add(2, new Vector3(0, 0, -1));
		int_To_Card.Add(3, new Vector3(-1, 0, 0));
		int_To_Card.Add(4, new Vector3(0, -1, 0));
		int_To_Card.Add(5, new Vector3(0, 1, 0));
		int_To_Card.Add(6, new Vector3(0, 0, 0));

		//This converts a Vector3 Direction into the Facing Direction Int
		card_To_Int = new Dictionary <Vector3, int>();
		card_To_Int.Add(new Vector3(0, 0, 1), 0);
		card_To_Int.Add(new Vector3(1, 0, 0), 1);
		card_To_Int.Add(new Vector3(0, 0, -1), 2);
		card_To_Int.Add(new Vector3(-1, 0, 0), 3);
		card_To_Int.Add(new Vector3(0, -1, 0), 4);
		card_To_Int.Add(new Vector3(0, 1, 0), 5);
		card_To_Int.Add(new Vector3(0, 0, 0), 6);
	}
}
