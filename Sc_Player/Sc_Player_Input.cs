using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_Player_Input - Player Input Script
//	This takes the key presses from the player and rewrites them in a format for
//	the player script to read

public class Sc_Player_Input : MonoBehaviour {


	//##############//
	//	VARIABLES	//
	//##############//

	//References to World Objects
	private Sc_GM gM;					//Reference to the GM
	private Sc_UM uM;
	private Sc_RB_Values rbV;			//Reference to the values list
	//private Sc_WorldClock wC;			//Reference to the world clock
	private Sc_Camera camera;			//Reference to the camera
	private Sc_Player player;			//Reference to the player

	//Store the current input values that will be accessed by the other player scripts
	//Check for any axis movement
	public bool axisMove = false;
	[HideInInspector]
	public bool call_AxisMove = false;
	public Vector3 inputAxis;
	// For dialogue and interactions, check if the input direction is being held down
	private bool input_Dir_Held = false;
	private int store_MoveInt;
	[HideInInspector]
	public Vector3 call_InputAxis;

	//Trying boolean arrays, 0 - back, 1 - RB, 2 - LB...
	[HideInInspector]
	public bool[] jen_SelfBool;
	[HideInInspector]
	public bool[] jen_ActiveBool;
	[HideInInspector]
	public bool[] jen_ReturnedBool;
	[HideInInspector]
	public float[] jen_Timer;

	//Check if back button has been pressed
	[HideInInspector]
	public bool bool_Back = false;
	[HideInInspector]
	public float back_Timer = 0.0f;
	//Check if the player wants to FezStyle swivel the camera
	[HideInInspector]
	public bool bool_RB = false;
	[HideInInspector]
	public float rB_Timer = 0.0f;

	// Check for the reset button
	[HideInInspector]
	public bool bool_Reset = false;

	[HideInInspector]
	public int fezStyle_Swivel_Int = 0;
	
	//For the timed check of the directional buttons
	[HideInInspector]
	public int pass_MoveInt;
	//private float move_Timer = 0.0f;
	//private float store_Move_Timer = 0.0f;

	// Number of generic timed buttons, or 'jen' buttons
	private int jenButtons = 4;

	// For bullet-time (In more complex puzzles)
	public float pass_BulletTime_Val = 0.0f;

	// Special dev mode
	[HideInInspector]
	public float devTimer = 0f;
	private bool doFlyOnce = true;


	//##################//
	//	INITIALISATION	//
	//##################//

	void Start()
	{
		//Grab the World Scripts
		GameObject gM_GameObject = GameObject.FindGameObjectWithTag("GM");
		gM = gM_GameObject.GetComponent<Sc_GM>();
		uM = gM.uM;
		rbV = gM.rbV;
		camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Sc_Camera>();

		//wC = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_WorldClock>();
		player = gM.player;

		//Set everything to its starting value
		jen_Timer = new float[jenButtons];
		jen_SelfBool = new bool[jenButtons];
		jen_ActiveBool = new bool[jenButtons];
		jen_ReturnedBool = new bool[jenButtons];
		for (int i = 0; i < jenButtons; i++)
		{
			jen_SelfBool[i] = false;
			jen_ActiveBool[i] = false;
			jen_ReturnedBool[i] = false;
			jen_Timer[i] = 0.0f;
		}
	}

	//If I want a delayed back button then I'm going to need to update control input via update
	//Currently the back button is lagging considerably
	void Update()
	{
		// Lazy dev mode
		if (Input.GetKey(KeyCode.F) && (Input.GetKey(KeyCode.L)) && (Input.GetKey(KeyCode.Y)))
		{
			devTimer += Time.deltaTime;

			if (devTimer > 3f)
			{
				if (doFlyOnce)
				{
					gM.isFloorOn = !gM.isFloorOn;
					gM.aC.Play_SFX("SFX_PartyHorn");

					// Make confetti!
					ParticleSystem[] player_PS = gM.player.GetComponentsInChildren<ParticleSystem>();
					foreach (ParticleSystem temp_PS in player_PS)
					{
						temp_PS.Play();
					}

					doFlyOnce = false;
				}
			}
		}
		else
		{
			devTimer = 0f;
			doFlyOnce = true;
		}

		//Back button - Currently, this works fine
		//back_TimedStateFunction();

		//Grab the button inputs
		jen_ActiveBool[0] = Input.GetButton("Back");
		jen_ActiveBool[1] = Input.GetButton("RB");
		jen_ActiveBool[2] = Input.GetButton("LB");
		jen_ActiveBool[3] = Input.GetButton("Reset");

		//New version for generic timed buttons
		for (int i = 0; i < jenButtons; i++)
		{
			jen_SelfBool[i] = generic_TimedStateFunction(i);
		}

		//Set the swivel camera number
		if (jen_SelfBool[1])
		{
			fezStyle_Swivel_Int = -1;
		}
		else if (jen_SelfBool[2])
		{
			fezStyle_Swivel_Int = 1;
		}
		else
		{
			fezStyle_Swivel_Int = 0;
		}

		// Bullet time value from trigger or ither specified button
		// This is passed always through update
		pass_BulletTime_Val = Input.GetAxis("Slow");
		// Only positive values though
		if (pass_BulletTime_Val < 0)
			gM.tp_bulletTime = 0f;
		else
			gM.tp_bulletTime = pass_BulletTime_Val;

		//Don't collect input whilst in freeCam mode
		if (!gM.isFreeCam_On || !uM.boolCheck_Paused)
		{
			//Directioal input
			Collect_Input();
		}
	}


	//##############//
	//	FUNCTIONS	//
	//##############//

	//This generic script allows us to hold down a button and repeat an action after a fixed period
	//Or tapped the button quickly
	bool generic_TimedStateFunction(int pass_buttonInt)
	{
		if (jen_ActiveBool[pass_buttonInt])
		{
			//The button is false, unless we've just pressed it, or a specific amount of time has passed
			jen_ReturnedBool[pass_buttonInt] = false;

			if (jen_Timer[pass_buttonInt] == 0.0f)
			{
				jen_ReturnedBool[pass_buttonInt] = true;
			}
			else if (jen_Timer[pass_buttonInt] >= rbV.jen_TimeDelay[pass_buttonInt])
			{
				//Reset the timer and button
				jen_Timer[pass_buttonInt] = 0.0f;
				jen_ReturnedBool[pass_buttonInt] = true;
			}
			
			jen_Timer[pass_buttonInt] += Time.deltaTime;

		}
		else
		{
			//Reset the timer and the button
			jen_Timer[pass_buttonInt] = 0.0f;
			jen_ReturnedBool[pass_buttonInt] = false;
		}

		return jen_ReturnedBool[pass_buttonInt];
	}

	//This _Can't_ be update if I am calling some of this variables later on. I don't want these variables to change as I call them
	void Collect_Input()
	{
		//Grab the button inputs
		Vector3 inputAxis = new Vector3(Math.Sign(Input.GetAxis("Horizontal")), 0, Math.Sign(Input.GetAxis("Vertical")));

		//I don't love this, it works, but I feel it might cause some issues later on as the script becomes more complicated
		//back_TimedStateFunction();

		//Favour horizontal over diagonal movement
		if (Mathf.Abs(inputAxis.x) >= Mathf.Abs(inputAxis.z))
		{
			inputAxis.z = 0;
		}
		else if (Mathf.Abs(inputAxis.z) > Mathf.Abs(inputAxis.x))
		{
			inputAxis.x = 0;
		}

		//First, check if there is any movement
		if (inputAxis.x + inputAxis.z == 0)
		{
			pass_MoveInt = -1;

			axisMove = false;
			input_Dir_Held = false;
			store_MoveInt = -1;
		}
		else
		{
			axisMove = true;

			//moveInt = dC.card_To_Int[inputAxis];
			pass_MoveInt = rbV.card_To_Int[inputAxis];

			if (pass_MoveInt == store_MoveInt)
				input_Dir_Held = true;
			store_MoveInt = pass_MoveInt;
		}
	}

	public void pass_CollectInput()
	{
		//the player script has a bunch of values which it will borrow from the input at one time.
		//This way the input can continue to run without effect any of the values sotred in player
		player.inp_Back = jen_SelfBool[0];
		player.inp_Reset = jen_SelfBool[3];
		player.fezStyle_CameraFacing_Int = (player.fezStyle_CameraFacing_Int + 4 + fezStyle_Swivel_Int) % 4;
		player.inp_AxisMove = axisMove;
		player.moveInt = pass_MoveInt;
		player.inp_Dir_Held = input_Dir_Held;

		// For bullet time
		gM.tp_bulletTime = pass_BulletTime_Val;
	}
}
