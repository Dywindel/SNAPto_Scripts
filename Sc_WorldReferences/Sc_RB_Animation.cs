using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_MovementLibrary - Movement Library
//	This script stores the common functions that I'll be using across many scripts

// [Note*] Do lifts need to be added to the doublecheck list?
// [Note*] How to do player rotation with the GatherWindow?

public class Sc_RB_Animation : MonoBehaviour {


	//##############//
	//	VARIABLES	//
	//##############//

	//References to World Objects
	private Sc_GM gM;
	protected Sc_UM uM;
	protected Sc_AC aC;
	protected Sc_RB_Values rbV;
	private Sc_Player player;

	// Player movement speed
    [Range(0.5f, 3.0f)]
    public float tp_Factor = 1.00f; 

	// Animation speed settings
	[HideInInspector]
    public float tp_Fixed = 0.15f;            //Object movement Speed - this will be the same for all objects
	[HideInInspector]
    public float tr_Fixed = 0.10f;            //Player Rotation Speed - This is for half a quarter turn
	[HideInInspector]
	public float tf_Fixed = 0.4f;				//Block Fall speed
	[HideInInspector]
    public float tp = 0.15f;            //Object movement Speed - this will be the same for all objects
	[HideInInspector]
    public float tr = 0.10f;            //Player Rotation Speed - This is for half a quarter turn
	[HideInInspector]
	public float tf = 0.4f;				//Block Fall speed
	[System.NonSerialized]

	public float tl = 0.21f;			//Lift speed factor. 0.21 is a good value for a sigmoid lift function
	[System.NonSerialized]
	public float tex = 1.0f;			//Text fade in speed

	public float tlp = 1.0f;			// How quickly the low pass filter effect transitions

	// Text scrolling speed
	[System.NonSerialized]
	private float txSc = 0.02f;
	public bool bool_isScrolling = false;	// is true whenever text is scrolling across the screen
	public Coroutine ref_Coroutine = null;

	// Banner colour change
	private float tBn = 2f;

	// Audio fade in
	private float tAu= 5f;

	// Lift Animation variables speeds
	[System.NonSerialized]
	private float tlMax = 8f;		// The point we start on the sigmoid curve
	private float tlLinear = 0.9f;	// This factor is used to help control the linear speed of the lift
	// These values are calculated before the lift operates
	[System.NonSerialized]
	public int t_Case = 0;			// The switch case for lift movement

	// Player boolean that allows Storms to record AfterActions as long as they have been
	// Move by the Player
	private bool playerMovedStorm = false;

	// For queueing movement, we need a list of items to move
	List<Transform> listOf_Transforms;
	List<Vector3> listOf_NetMoves;

	// Break out of a movement, if there is an additional queued movement, at this movement percentage
	private float movement_breakOut_Percentage = 0.80f;

	// List of all the queued movement classes
	[HideInInspector]
	public List<QueueMovement_Translate> listOf_QueueMovement_Translate;
		// List of all the queued movement classes
	[HideInInspector]
	public List<QueueMovement_Rotate> listOf_QueueMovement_Rotate;
		// List of all the queued movement classes
	[HideInInspector]
	public List<QueueMovement_Lift> listOf_QueueMovement_Lift;

	////////////////////
	// INITIALISATION //
	////////////////////

	// Use this for initialization
	void Start () 
	{
		//Grab the Game Manager, dictionary, movement library...
		GameObject gM_GameObject = GameObject.FindGameObjectWithTag("GM");
		gM = gM_GameObject.GetComponent<Sc_GM>();
		uM = gM.uM;
		aC = gM.aC;
		rbV = gM.rbV;
		player = gM.player;

		// Refresh the list
		listOf_Transforms = new List<Transform>();
		listOf_NetMoves = new List<Vector3>();

		// Queued Movmeent
		listOf_QueueMovement_Translate = new List<QueueMovement_Translate>();
		listOf_QueueMovement_Rotate = new List<QueueMovement_Rotate>();
		listOf_QueueMovement_Lift = new List<QueueMovement_Lift>();
    }

	////////////////////////
	// QUEUEING FUNCTIONS //
	////////////////////////

	// To avoid staggering movement, I'm going to queue the movemens the player's wants to make using the functions below

	// Run all queued movements stored in the queue movements list
	public void Run_QueueuedMovements()
	{
		// Run all translation movements
		foreach (QueueMovement_Translate qtMove in listOf_QueueMovement_Translate)
		{
			// Switch cases for each possible movement type
			switch (qtMove.queue_Int_MoveFunction)
			{
				// 0 - Storm translate
				case 0:
				{
					// Grab a reference
					Sc_Storm temp_Storm = qtMove.queue_Trans.GetComponent<Sc_Storm>();

					// Set the movement boolean
					uM.moveBool_Storm = true;

					// Add script to double check list (For Storms)
					uM.listOf_AllStorm_DoubleCheck.Add(temp_Storm);

					// Update the block discrete positions (Just before movement)
					temp_Storm.Update_DiscretePositions(qtMove.queue_NetMovement);

					// Play sound
					// So, for Storms, the way this works is we set to true if a type of sound needs to be played
					// Then play each of that sound type Once. As opposed to each storm getting to play the same sound multiple times?
					// I don't know
					aC.listOf_Push_SFX[temp_Storm.intSoundType] = true;

					// Perform movement coroutine
					StartCoroutine(Storm_Translate(qtMove.queue_NetMovement, qtMove.queue_Trans));
					break;
				}
				// 1 - Player translate
				case 1:
				{
					// Set the player movement boolean to true
					uM.moveBool_Player = true;

					// Update the block discrete positions (Just before movement)
					qtMove.queue_Trans.GetComponent<Sc_Player>().Update_DiscretePositions(qtMove.queue_NetMovement);

					// Function for playing sounds
					aC.Play_SFX("SFX_PlayerTranslate");

					// The player is moving, so we can't update it's next movement at the moment
					uM.actBool_GatherWindow = false;

					// Perform the player translate coroutine
					StartCoroutine(Player_Translate(qtMove.queue_NetMovement, qtMove.queue_Trans));
					break;
				}
				// 2 - Storm Falling
				case 2:
				{
					// Set the movement boolean
					uM.moveBool_Storm = true;

					// Add script to double check list (For Storms)
					uM.listOf_AllStorm_DoubleCheck.Add(qtMove.queue_Trans.GetComponent<Sc_Storm>());

					// Update the block discrete positions (Just before movement)
					qtMove.queue_Trans.GetComponent<Sc_Storm>().Update_DiscretePositions(qtMove.queue_NetMovement);

					// Play sound

					// Particle Effect Check
					qtMove.queue_Trans.GetComponent<Sc_Storm>().Check_CloudLayerBreach();

					// Perform fall coroutine
					StartCoroutine(Storm_Translate(qtMove.queue_NetMovement, qtMove.queue_Trans));
					break;
				}
				// Etc...
			}
		}

		// Run all rotation movements
		foreach (QueueMovement_Rotate qrMove in listOf_QueueMovement_Rotate)
		{
			// Switch cases for each rotation type
			switch(qrMove.queue_Int_RotateFunction)
			{
				// For player movements
				case 0:
					// Set the player movement boolean to true
					uM.moveBool_Player = true;

					// Function for playing sounds
					// Only play this sound for the first rotation stage (When Rotstage is false)
					if (qrMove.queue_RotationStage == false)
						aC.Play_SFX("SFX_PlayerRotate");

					//We won't change the faceInt until the second state of rotation
					
					//Update the player faceInt

					//We only play sound during the first rotation stage

					// The block discrete position doesn't need to change

					// The player is moving, so we can't update it's next movement at the moment
					// We only collect input at the end of the second rotation stage

					// Perform the player translate coroutine
					StartCoroutine(Player_Rotate(qrMove.queue_Rotation, qrMove.queue_Trans, qrMove.queue_RotationStage));
					break;
				
				// For an interactive rotating pillar
				case 1:
					// Set the Rotor movement boolean to true
					uM.moveBool_Rotor = true;

					// Because we're passing the player's rotation, we actually want to animate the pillar in the opposite direction

					if (qrMove.queue_Rotation == GlobInt.MoveOption.clockwise)
					{
						qrMove.queue_Trans.GetComponent<Sc_Rotor>().Rotate(false);
						StartCoroutine(Rotor_Rotate(qrMove.queue_Trans.GetComponent<Sc_Rotor>().ref_RotorPart.transform, false));
					}
					else
					{
						qrMove.queue_Trans.GetComponent<Sc_Rotor>().Rotate(true);
						StartCoroutine(Rotor_Rotate(qrMove.queue_Trans.GetComponent<Sc_Rotor>().ref_RotorPart.transform, true));
					}
					break;
			}
		}

		// Run all lift movements
		foreach (QueueMovement_Lift qlMove in listOf_QueueMovement_Lift)
		{
			// Switch cases for each possible movement type
			switch (qlMove.queue_Int_MoveFunction)
			{
				// 0 - Lift movement
				case 0:
				{
					// Set the movement boolean
					uM.moveBool_Lift = false;

					// Temporary reference to the lift script
					// Currently, this is NULL, but I'm not sure why?
					Sc_Block_Lift temp_Lift = qlMove.queue_Trans.GetComponent<Sc_Block_Lift>();

					// Here is where I update all the variables that control how the lift operates
					// We first update the currentFloor
					if (qlMove.queue_RefBool)
						temp_Lift.currentFloor += 1;
					else
						temp_Lift.currentFloor -= 1;

					// Here I can figure out when the floor blocks should and should not activate
					// RevealHide_LiftComponents(currentFloor == topFloor);
					temp_Lift.Check_RevealHide_LiftComponents(qlMove.queue_RefBool, temp_Lift.currentFloor);

					// [Note*] Do lifts needs to be added to the DoubleCheck List?

					// Update the block discrete positions (Just before movement)
					if (qlMove.queue_RefBool)
						qlMove.queue_Trans.GetComponent<Sc_Block_Lift>().cloud_Parent.storm_Parent.Update_DiscretePositions(rbV.vtUp);
					else
						qlMove.queue_Trans.GetComponent<Sc_Block_Lift>().cloud_Parent.storm_Parent.Update_DiscretePositions(rbV.vtDn);

					// Play sound

					// Perform movement coroutine
					StartCoroutine(Move_Lift(qlMove.queue_Trans, qlMove.queue_RefBool, qlMove.queue_SwitchCase));
					
					// Would be nice to have each lift block component rotate as the lift goes up and down
					foreach (Sc_Block_Floor temp_LiftScrew in temp_Lift.obj_InvsFloor_Ref)
					{
						// Make the lift screw rotate
						// WARNING: This may cause some collision issues because the gameobject is rotating (It may not point in the right direction
						// When performing collision detection any more
						StartCoroutine(Lift_RotateScrew(temp_LiftScrew.gameObject.transform, qlMove.queue_RefBool, t_Case));
					}
					
					break;
				}

				// 1 - Other items movings ontop of lift
				case 1:
				{
					// Update the block discrete positions (Just before movement)
					/*
					if (qlMove.queue_RefBool)
						qlMove.queue_Trans.GetComponent<Sc_Storm>().Update_DiscretePositions(rbV.vtUp);
					else
						qlMove.queue_Trans.GetComponent<Sc_Storm>().Update_DiscretePositions(rbV.vtDn);
					*/

					// Play sound

					// Perform movement coroutine
					StartCoroutine(Move_Lift(qlMove.queue_Trans, qlMove.queue_RefBool, qlMove.queue_SwitchCase));
					break;
				}
			}
		}

		// Here is the best opportunity to play sounds
		
		// Empty all the lists
		listOf_QueueMovement_Translate = new List<QueueMovement_Translate>();
		listOf_QueueMovement_Rotate = new List<QueueMovement_Rotate>();
		listOf_QueueMovement_Lift = new List<QueueMovement_Lift>();
	}

	////////////////////////
	// MOVEMENT FUNCTIONS //
	////////////////////////

    //Translation movement
	// 0 - Storm Translate
	public IEnumerator Storm_Translate(int netMove_Int, Transform trans)
	{
		Vector3 netMove_Vector = rbV.int_To_Card[netMove_Int];
		Vector3 currentPosition = trans.position;
		for (float t = 0; t < 1.0f; t += Time.deltaTime/tp)
		{
			//Move this blockGroup gameobject
			trans.position = Vector3.Lerp(currentPosition, currentPosition + netMove_Vector, t);

			yield return null;
		}

		//Then set the final movement position
		trans.position = currentPosition + netMove_Vector;

		//Reset the moving object boolean
		uM.moveBool_Storm = false;

		// If the Storm is the last item to move
		if (playerMovedStorm)
		{
			// It is allows to record AfterActions, once
			gM.Player_HasPerformedAction_AfterAction();
			playerMovedStorm = false;
		}
	}

	// 1 - Player Translate
	public IEnumerator Player_Translate(int netMove_Int, Transform trans)
	{
		Vector3 netMove_Vector = rbV.int_To_Card[netMove_Int];
		Vector3 currentPosition = trans.position;

		// Round the final position to the nearest whole integer
		Vector3 finalPos_Rounded = new Vector3(	Mathf.Round(currentPosition.x + netMove_Vector.x),
												Mathf.Round(currentPosition.y + netMove_Vector.y),
												Mathf.Round(currentPosition.z + netMove_Vector.z));

		for (float t = 0; t < 1.0f; t += Time.deltaTime/tp)
		{
			// Move this blockGroup gameobject
			trans.position = Vector3.Lerp(currentPosition, finalPos_Rounded, t);

			// When we near the end of the movement
			// We can reopen the Gather Window
			if (t >= movement_breakOut_Percentage)
				uM.actBool_GatherWindow = true;

			yield return null;

		}

		// Then set the final movement position
		trans.position = finalPos_Rounded;

		//Reset the moving object boolean
		uM.moveBool_Player = false;

		// Reset the gathering window - This is vital, otherwise stuttering can cause the player to stop randomly
		uM.actBool_GatherWindow = true;

		// Update the AfterAction function in the GM
		gM.Player_HasPerformedAction_AfterAction();
	}

	// 2 - Storm Falling
	public IEnumerator Storm_Fall(Vector3 netMove, Transform trans)
	{
		Vector3 currentPosition = trans.position;
		for (float t = 0; t < 1.0f; t += Time.deltaTime/tp)
		{
			//Move this blockGroup gameobject
			trans.position = Vector3.Lerp(currentPosition, currentPosition + netMove, t);

			yield return null;
		}

		//Then set the final movement position
		trans.position = currentPosition + netMove;

		//Reset the moving object boolean
		uM.moveBool_Storm = false;
	}


	// N/A - Player Rotate has it's own Class
	public IEnumerator Player_Rotate(GlobInt.MoveOption pass_Movement, Transform trans, bool rotStage_Two)
	{
		Quaternion currentRotation = trans.rotation;
		Quaternion rot_Dir;

		if (pass_Movement == GlobInt.MoveOption.clockwise)
			rot_Dir = Quaternion.Euler(0, 45, 0);
		else
			rot_Dir = Quaternion.Euler(0, -45, 0);

		for (float t = 0; t < 1.0f; t += Time.deltaTime/tr)
		{
			trans.rotation = Quaternion.Lerp(currentRotation, currentRotation*rot_Dir, t);

			// When we near the end of the movement (During the second rotation phase)
			// We can reopen the Gather Window
			// [Note*] Not working, come back later

			yield return null;
		}

		//Manually set the movement position
		trans.rotation = currentRotation*rot_Dir;

		// Reset the moving object boolean
		uM.moveBool_Player = false;

		// Update the AfterAction function in the GM (Only for the second stage of rotation)
		// Storms move slightly slower than the player rotates. In order to not cause clashes with the system
		// I will allow Storms to recrod AfterActions as long as the player is responsible for the Storm's movement
		if (rotStage_Two)
		{
			// If there are no other Storms moving, then the player controls the AfterAction command
			if (!uM.moveBool_Storm)
				gM.Player_HasPerformedAction_AfterAction();
			// Otherwise, the Storm translate will record the movement
			else
				playerMovedStorm = true;
		}
	}

	// Rotate Pillar
	public IEnumerator Rotor_Rotate(Transform trans, bool bool_Clockwise)
	{
		Quaternion currentRotation = trans.rotation;
		Quaternion rot_Dir;

		if (!bool_Clockwise)
			rot_Dir = Quaternion.Euler(0, 0, -90);
		else
			rot_Dir = Quaternion.Euler(0, 0, 90);

		for (float t = -tlMax; t < tlMax; t += Time.deltaTime/(tp*tl*0.5f))
		{
			trans.rotation = Quaternion.Lerp(currentRotation, currentRotation*rot_Dir, f_Sigmoid(t));
			yield return null;
		}

		// Reset the moving object boolean
		uM.moveBool_Rotor = false;

		//Manually set the movement position
		trans.rotation = currentRotation*rot_Dir;

		yield return null;
	}

	// 1 - Lift screw rotations
	public IEnumerator Lift_RotateScrew(Transform trans, bool bool_Clockwise, int pass_SwitchCase)
	{
		Quaternion currentRotation = trans.rotation;
		Quaternion rot_Dir;

		if (!bool_Clockwise)
			rot_Dir = Quaternion.Euler(0, 120, 0);
		else
			rot_Dir = Quaternion.Euler(0, -120, 0);

				// Switch statement for different cases
		switch (pass_SwitchCase)
		{
		// Case 0: Ramp up slowly, then slow down for single floor movement
		case 0:
			for (float t = -tlMax; t < tlMax; t += Time.deltaTime/(tp*tl*0.5f))
			{
				trans.rotation = Quaternion.Lerp(currentRotation, currentRotation*rot_Dir, f_Sigmoid(t));
				yield return null;
			}
			break;

		// Case 1: Ramp up slow
		case 1:
			// This is achieved by only using the first half of the sigmoid
			for (float t = -tlMax; t < 0f; t += Time.deltaTime/(tp*tl*1f))
			{
				trans.rotation = Quaternion.Lerp(currentRotation, currentRotation*rot_Dir, 2*f_Sigmoid(t));
				yield return null;
			}
			break;
		
		// Case 2: Ramp down slow
		case 2:
			// Second half of sigmoid
			for (float t = 0f; t < tlMax; t += Time.deltaTime/(tp*tl*1f))
			{
				trans.rotation = Quaternion.Lerp(currentRotation, currentRotation*rot_Dir, 2*(f_Sigmoid(t) - 0.5f));
				yield return null;
			}
			break;

		// Case 3: Linear movement
		case 3:
			// Will have to play around with this
			for (float t = 0; t < 1f; t += Time.deltaTime/tp*tlLinear)
			{
				trans.rotation = Quaternion.Lerp(currentRotation, currentRotation*rot_Dir, t);
				yield return null;
			}
			break;
		}

		//Manually set the movement position
		trans.rotation = currentRotation*rot_Dir;

		yield return null;
	}

	// 0 - Lift, moving lift
	public IEnumerator Move_Lift(Transform trans, bool pass_isRising, int pass_SwitchCase)
	{

		//Change the netMovement depending on whether we're rising or falling
		Vector3 netMove = new Vector3(0f, -1f, 0f);
		if (pass_isRising) {
			netMove = -netMove;
		}

		//I'll worry about how movement looks, later
		Vector3 currentPosition = trans.position;

		// Switch statement for different cases
		switch (pass_SwitchCase)
		{
		// Case 0: Ramp up slowly, then slow down for single floor movement
		case 0:
			for (float t = -tlMax; t < tlMax; t += Time.deltaTime/(tp*tl*0.5f))
			{
				trans.position = Vector3.Lerp(currentPosition, currentPosition + netMove, f_Sigmoid(t));
				yield return null;
			}
			break;

		// Case 1: Ramp up slow
		case 1:
			// This is achieved by only using the first half of the sigmoid
			for (float t = -tlMax; t < 0f; t += Time.deltaTime/(tp*tl*1f))
			{
				trans.position = Vector3.Lerp(currentPosition, currentPosition + netMove, 2*f_Sigmoid(t));
				yield return null;
			}
			break;
		
		// Case 2: Ramp down slow
		case 2:
			// Second half of sigmoid
			for (float t = 0f; t < tlMax; t += Time.deltaTime/(tp*tl*1f))
			{
				trans.position = Vector3.Lerp(currentPosition, currentPosition + netMove, 2*(f_Sigmoid(t) - 0.5f));
				yield return null;
			}
			break;

		// Case 3: Linear movement
		case 3:
			// Will have to play around with this
			for (float t = 0; t < 1f; t += Time.deltaTime/tp*tlLinear)
			{
				trans.position = Vector3.Lerp(currentPosition, currentPosition + netMove, t);
				yield return null;
			}
			break;
		}

		//Then set the final movement position
		trans.position = currentPosition + netMove;

		//Reset the moving object boolean
		uM.moveBool_Lift = false;
	}


	/////////////////////////////
	// NON-ESSENTIAL ANIMATION //
	/////////////////////////////

	// Light, gradual change, used for the lantern puzzle
	public IEnumerator Light_GradualChange(Light ref_Light, SpriteRenderer pass_sr, bool getBrighter)
	{
		float cur_Intensity = ref_Light.intensity;
		float new_Intensity = cur_Intensity + 1f;
		if (!getBrighter)
			new_Intensity = cur_Intensity - 1f;

		// For the shadow colour, I could just change the alpha by a percentage of the light intensity value
		float cur_ShadowAlpah = cur_Intensity/10f;
		float new_ShadowAlpha = new_Intensity/10f;

		for (float t = -tlMax; t < tlMax; t += Time.deltaTime/(tp*tl*0.5f))
		{
			// Change the light intensity
			ref_Light.intensity = Mathf.Lerp(cur_Intensity, new_Intensity, f_Sigmoid(t));
			// Change the sprite renderer shadow intensity
			pass_sr.color = new Color(1f, 1f, 1f, Mathf.Lerp(cur_ShadowAlpah, new_ShadowAlpha, f_Sigmoid(t)));
			yield return null;
		}

		// Manually set the light intensity
		ref_Light.intensity = new_Intensity;

		yield return null;
	}

	// Rocks, falling
	public IEnumerator Rocks_Fall_GradualSpeedUp(Transform ref_Trans, int pass_FallDistance)
	{
		Vector3 netMove_Vector = rbV.int_To_Card[4]*pass_FallDistance;
		Vector3 currentPosition = ref_Trans.position;

		// Round the final position to the nearest whole integer
		Vector3 finalPos_Rounded = new Vector3(	Mathf.Round(currentPosition.x + netMove_Vector.x),
												Mathf.Round(currentPosition.y + netMove_Vector.y),
												Mathf.Round(currentPosition.z + netMove_Vector.z));

		// Basically an exponential curve
		for (float t = 0; t < 1.0f; t += Time.deltaTime/10f)
		{
			ref_Trans.position = Vector3.Lerp(currentPosition, finalPos_Rounded, f_Squared(t));
			yield return null;
		}

		// Manually set the final position
		ref_Trans.position = finalPos_Rounded;

		yield return null;
	}

	// Cloud twirling up/down
	public IEnumerator Clouds_ChangeFloat(Material pass_Mat, string pass_String, float pass_SetValue)
	{
		float startValue = pass_Mat.GetFloat(pass_String);

		for (float t = -20f; t < 20f; t += Time.deltaTime/1f)
		{
			pass_Mat.SetFloat(pass_String, Mathf.Lerp(startValue, pass_SetValue, f_Sigmoid(t)));
			yield return null;
		}

		// Manually set the final position
		pass_Mat.SetFloat(pass_String, pass_SetValue);

		yield return null;
	}

	public float f_Sigmoid(float x) {
		return 1 / (1 + (float)Math.Exp(-x*0.5f));
	}
	public float f_InvSigmoid(float x) {
		float ret_X;

		// If the number is too small, just use fixed animation values for now
		if (x <= 0.00001)
			ret_X = -10f;
		else if (x >= 0.99999)
			ret_X = 10f;
		else
			ret_X = (-2 * Mathf.Log((1/x) - 1));
		return ret_X;
	}

	public float f_Squared(float x)
	{
		return (float)Math.Pow(x, 2);
	}

	///////////////////
	// UI ANIMATIONS //
	///////////////////

	// Here the text will fade in by alpha and slide up slightly from the base of the screen
	public IEnumerator Text_FadeIn(TextMeshProUGUI pass_TextMeshPro, bool pass_Rising)
	{
		//dictates which fade direction we're going in
		float q = 0;
		if (!pass_Rising)
			q = 255;

		for (float t = 0; t < 1.0f; t += Time.deltaTime/tex)
		{
			//Change the alpha value
			pass_TextMeshPro.faceColor = new Color32(255, 255, 255, (byte)Mathf.Abs(q - t*255));

			//Implement text panning here

			yield return null;
		}

		//Set the final alpha value
		pass_TextMeshPro.faceColor = new Color32(255, 255, 255, (byte)Mathf.Abs(q - 255));

		yield return null;
	}

	// This causes the text to display one letter at a time
	public IEnumerator Text_TypeIn(TextMeshProUGUI pass_TextMeshPro, string sentence)
	{
		pass_TextMeshPro.text = "";

		// For SFX
		int int_SFX = 0;

		bool_isScrolling = true;
		foreach (char letter in sentence.ToCharArray())
		{
			pass_TextMeshPro.text += letter;

			// Play SoundFX, randomly between two effects, every other two letters
			if (int_SFX % 4 == 0)
			{
				if (UnityEngine.Random.Range(0f, 1f) <= 0.5f)
					aC.Play_SFX("SFX_Writing01");
				else
					aC.Play_SFX("SFX_Writing02");
			}
			int_SFX += 1;

			yield return new WaitForSeconds(txSc);
		}
		bool_isScrolling = false;
	}

	// For music transitions
	public IEnumerator Audio_FadeIn(Sc_SoundDatabase pass_S, bool pass_FadeIn)
	{
		float maxVol = pass_S.volume;

		// If we're fading in, start playing the song
		// If we're fading out, reverse the rise off the timer
		float q = 0.0f;
		if (pass_FadeIn)
		{
			// yield return new WaitForSeconds(tAu*0.75f);
			pass_S.source.Play();
		}
		else
			q = 1.0f;

		// We're moving from 0 to the limit specified by the SoundDatabase class
		for (float t = 0; t < 1.0f; t += Time.deltaTime/tAu)
		{
			pass_S.source.volume = maxVol*(float)Mathf.Abs(q - t);
			yield return null;
		}

		// Stop playing this piece if it's fading out
		if (!pass_FadeIn)
			pass_S.source.Stop();

		yield return null;
	}

	public IEnumerator Cloth_ChangeColour(SkinnedMeshRenderer ref_Mesh)
    {
        for (float t = 0; t < 1.0f; t += Time.deltaTime/tBn)
		{
			ref_Mesh.materials[0].SetFloat("_Mix", t);
			yield return null;
        }

		// Manual set the value to it's final term
		ref_Mesh.materials[0].SetFloat("_Mix", 1f);

        yield return null;
    }

	////////////
	// TIMERS //
	////////////

	public IEnumerator Cutscene_ShotTimer(float pass_Timer, Sc_Camera pass_Script)
	{
		yield return new WaitForSeconds(pass_Timer);

		pass_Script.bool_LevelCamera = false;

		yield return null;
	}

	////////////
	// SOUNDS //
	////////////

	void soundCheck(int pass_Int)
	{

	}

}