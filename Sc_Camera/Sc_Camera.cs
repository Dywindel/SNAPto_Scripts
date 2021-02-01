using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_Camera : MonoBehaviour {
 
	/*
	EXTENDED FLYCAM
		Desi Quintans (CowfaceGames.com), 17 August 2012.
		Based on FlyThrough.js by Slin (http://wiki.unity3d.com/index.php/FlyThrough), 17 May 2011.
 
	LICENSE
		Free as in speech, and free as in beer.
 
	FEATURES
		WASD/Arrows:    Movement
		          Q:    Climb
		          E:    Drop
                      Shift:    Move faster
                    Control:    Move slower
                        End:    Toggle cursor locking to screen (you can also press Ctrl+P to toggle play mode on and off).
	*/
 
	protected Sc_GM gM;					//Reference to the GM
	protected Sc_RB_Animation rbA;	//Reference to the movement files

	public float cameraSensitivity = 90;
	public float climbSpeed = 4;
	public float normalMoveSpeed = 10;
	public float slowMoveFactor = 0.25f;
	public float fastMoveFactor = 3;
 
	private float rotationX = 0.0f;
	private float rotationY = 0.0f;

	// Cutscenes can take command of the camera
	[System.NonSerialized]
	public bool bool_LevelCamera = false;
	[System.NonSerialized]
	public bool bool_DialogueCamera = false;

	// Reference to the player
	private GameObject player;

	// What the camera intends to look at
	private Vector3 cameraFocusPoint = new Vector3(0, 0, 0);
	private Quaternion cameraRotationPoint;

	// Camera offset from player center
	// private Vector3 position_Offset = new Vector3(0f, 6f, -4f); //For 50 degress camera angle
	// Original positions at 0, 8, -5 for 60x camera angle
	// Low angle skyshot position at 0, 1, -5
	private Vector3 position_Offset = new Vector3(0f, 8f, -5f); // Normal
	//private Vector3 position_Offset = new Vector3(0f, 8f, 0f); //Looking down
	// For zoomed in (0f, 4f, -2.5f)
	// Original camera tilt at x = 60f
	// Low sky shot at x = -10f
	private Quaternion rotation_Offset = Quaternion.Euler(60f, 0f, 0f);
	
	// This allows the cutscene to run after it has been called DoOnce above
	[HideInInspector]
	Vector3 focusPoint = new Vector3(0, 0, 0);
	[HideInInspector]
	Quaternion rotationPoint = new Quaternion(0, 0, 0, 0);
	[HideInInspector]
	public Vector3 levelCamera_FocusPoint;
	[HideInInspector]
	public Quaternion levelCamera_RotationPoint;
	[HideInInspector]
	public Vector3 dialogueCamera_FocusPoint;
	[HideInInspector]
	public Quaternion dialogueCamera_RotationPoint;
	[HideInInspector]
	public Vector3 previousCamera_FocusPoint;	// Where the camera was looking in the previous setup
	[HideInInspector]
	public Quaternion previousCamera_RotationPoint;

	// Camera translation speed
    private float speed_CameraTranslation = 2.0f;
	// Camera rotation speed
	private float speed_CameraRotation = 1.0f;
	// Camera speed when transitioning to new level
	private float speed_LevelTransition = 1.5f;

	// Camera transition values
	float float_CameraTransition_LevelIn_Percentage;	// Amount camera is focusing on levelFocus
	float float_CameraTransition_LevelOut_Percentage;	// And after exiting a level
	float speed_CameraTransition_IntoLevelFocus = 1f;
	float speed_CameraTransition_OutOfLevelFocus = 1.8f;

	// Same as above, but for rotation values
	float float_CameraRotation_LevelIn_Percentage;	// Amount camera is focusing on levelFocus
	float float_CameraRotation_LevelOut_Percentage;	// And after exiting a level
	float speed_CameraRotation_IntoLevelFocus = 1.5f;
	float speed_CameraRotation_OutOfLevelFocus = 1.5f;

	// Use this for initialization
	void Start ()
	{
		focusPoint = position_Offset;
		rotationPoint = rotation_Offset;

		// For zoomed in position (0f, 4f, -2.5f)
		// position_Offset.Set(0f, 8f, -5f);

		// Grab the Game Manager, dictionary, movement library...
		GameObject gM_GameObject = GameObject.FindGameObjectWithTag("GM");
		gM = gM_GameObject.GetComponent<Sc_GM>();
		rbA = gM.rbA;

		player = gM.player.gameObject;
		transform.rotation = rotation_Offset;

		gM.isFreeCam_On = false;
		// Screen.lockCursor = true;

		// StartCoroutine(Start_CameraPosition_FrameDelay());
	}

	// Update camera position one frame later
	public IEnumerator Start_CameraPosition_FrameDelay()
	{
		// For some reason, 1 seconds works better than one frame. I dunno.
		// yield return new WaitForSeconds(1f);
		yield return new WaitForSeconds(0.5f);

		// Set the level_CameraFocus to the player's position, se there's no jarring camera movement
		previousCamera_FocusPoint = player.transform.position + position_Offset;
		previousCamera_RotationPoint = rotationPoint;

		// At the start, before the first frame, jump the camera to where the player is
		transform.position = player.transform.position + position_Offset;

		yield return null;
	}
	
	// LateUpdate is used to allow position calculations to finish before moving the camera
	void LateUpdate ()
	{
		// Dialogue camera superseeds cutscene camera
		if (!bool_DialogueCamera && !bool_LevelCamera)
		{
			if (Input.GetButtonDown("freeCam") && gM.isFreeCam_Active)
			{
				gM.isFreeCam_On = (gM.isFreeCam_On == false) ? true : false;

				if (!gM.isFreeCam_On)
				{
					// Also reset camera rotation once
					transform.rotation = rotation_Offset;
				}
			}

			if (!gM.isFreeCam_On)
			{
				// This is a bit complex, but had to be done because I can't use coroutines inside LateUpdate()
				// Basically, I'm sliding the camera's focus onto a vector3 which is, itself, also sliding its focus between the player and 
				// The level focus.
				if (float_CameraTransition_LevelOut_Percentage < 1)
				{
					float_CameraTransition_LevelOut_Percentage += Time.deltaTime/speed_CameraTransition_OutOfLevelFocus;
					float_CameraTransition_LevelIn_Percentage -= Time.deltaTime/speed_CameraTransition_OutOfLevelFocus;
				}
				else
				{
					float_CameraTransition_LevelOut_Percentage = 1f;
					float_CameraTransition_LevelIn_Percentage = 0;
				}
				cameraFocusPoint = Vector3.Lerp(previousCamera_FocusPoint, player.transform.position + position_Offset, float_CameraTransition_LevelOut_Percentage);
				transform.position = Vector3.Lerp(transform.position, cameraFocusPoint, Time.deltaTime * speed_CameraTranslation);

				// Trying the same approach as above for camera rotation
				if (float_CameraRotation_LevelOut_Percentage < 1)
				{
					float_CameraRotation_LevelOut_Percentage += Time.deltaTime/speed_CameraRotation_OutOfLevelFocus;
					float_CameraRotation_LevelIn_Percentage -= Time.deltaTime/speed_CameraRotation_OutOfLevelFocus;
				}
				else
				{
					float_CameraRotation_LevelOut_Percentage = 1f;
					float_CameraRotation_LevelIn_Percentage = 0;
				}
				cameraRotationPoint = Quaternion.Lerp(previousCamera_RotationPoint, rotation_Offset, float_CameraRotation_LevelOut_Percentage);
				transform.rotation = Quaternion.Lerp(transform.rotation, cameraRotationPoint, Time.deltaTime * speed_CameraRotation);
			}
		}
		// Used when a camera enters 'level' or 'wideshot' mode
		else
		{
			Vector3 focusPoint = position_Offset;
			Quaternion rotationPoint = rotation_Offset;

			// Dialogue superseeds wideshots
			if (bool_LevelCamera)
			{
				focusPoint = levelCamera_FocusPoint;
				rotationPoint = levelCamera_RotationPoint;
				previousCamera_FocusPoint = levelCamera_FocusPoint;
				previousCamera_RotationPoint = levelCamera_RotationPoint;
			}
			else if (bool_DialogueCamera)
			{
				focusPoint = dialogueCamera_FocusPoint;
				rotationPoint = dialogueCamera_RotationPoint;
				previousCamera_FocusPoint = dialogueCamera_FocusPoint;
				previousCamera_RotationPoint = rotationPoint = dialogueCamera_RotationPoint;
;
			}

			// Can't use coroutines inside LateUpdate
			if (float_CameraTransition_LevelIn_Percentage < 1)
			{
				float_CameraTransition_LevelIn_Percentage += Time.deltaTime/speed_CameraTransition_IntoLevelFocus;
				float_CameraTransition_LevelOut_Percentage -= Time.deltaTime/speed_CameraTransition_IntoLevelFocus;
			}
			else
			{
				float_CameraTransition_LevelIn_Percentage = 1f;
				float_CameraTransition_LevelOut_Percentage = 0;
			}
			cameraFocusPoint = Vector3.Lerp(player.transform.position + position_Offset, focusPoint, float_CameraTransition_LevelIn_Percentage);
			transform.position = Vector3.Lerp(transform.position, cameraFocusPoint, Time.deltaTime * speed_LevelTransition);

			// Trying the same approach as above for camera rotation
			if (float_CameraRotation_LevelIn_Percentage < 1)
			{
				float_CameraRotation_LevelIn_Percentage += Time.deltaTime/speed_CameraRotation_IntoLevelFocus;
				float_CameraRotation_LevelOut_Percentage -= Time.deltaTime/speed_CameraRotation_IntoLevelFocus;
			}
			else
			{
				float_CameraRotation_LevelIn_Percentage = 1f;
				float_CameraRotation_LevelOut_Percentage = 0;
			}
			cameraRotationPoint = Quaternion.Lerp(rotation_Offset, rotationPoint, float_CameraRotation_LevelIn_Percentage);
			transform.rotation = Quaternion.Lerp(transform.rotation, cameraRotationPoint, Time.deltaTime * speed_CameraRotation);
		}
	}

	void Update ()
	{
		if (gM.isFreeCam_On)
		{
			rotationX += Input.GetAxis("RightStickX") * cameraSensitivity * Time.deltaTime;
			rotationY += Input.GetAxis("RightStickY") * cameraSensitivity * Time.deltaTime;
			rotationY = Mathf.Clamp (rotationY, -90, 90);
	
			transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
			transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
	
			if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift))
			{
				transform.position += transform.forward * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
				transform.position += transform.right * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
			}
			else if (Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl))
			{
				transform.position += transform.forward * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
				transform.position += transform.right * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
			}
			else
			{
				transform.position += transform.forward * normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
				transform.position += transform.right * normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
			}
	
	
			if (Input.GetKey (KeyCode.Q)) {transform.position += transform.up * climbSpeed * Time.deltaTime;}
			if (Input.GetKey (KeyCode.E)) {transform.position -= transform.up * climbSpeed * Time.deltaTime;}
	
			if (Input.GetKeyDown (KeyCode.End))
			{
				// Screen.lockCursor = (Screen.lockCursor == false) ? true : false;
			}
		}
	}
}
