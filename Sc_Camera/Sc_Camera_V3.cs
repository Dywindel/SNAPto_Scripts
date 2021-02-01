using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_Camera_V3 : MonoBehaviour
{
    ////////////////////////////////////////////////////
    //
    //  Sc_Camera_V2 - This cameria script uses an orbit
    //  like calculation method to position the camera

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

    private Sc_GM gM;		//Reference to the GM
    private Sc_UM uM;       //Reference to the update manager

	public float cameraSensitivity = 90;
	public float climbSpeed = 4;
	public float normalMoveSpeed = 10;
	public float slowMoveFactor = 0.25f;
	public float fastMoveFactor = 3;
 
	private float rotationX = 0.0f;
	private float rotationY = 0.0f;

    //Camera movement speed
    private float speed = 2.0f;
    //Camera position radius from player
    [HideInInspector]
    private float radius = 6.5f;

    //Max angle movement
    private float maxAngle = 29.0f * Mathf.PI / 180.0f;

    //Free camera movement
	[HideInInspector]
	public bool freeCam = false;

    //Reference to the player
	private GameObject player;
    //Reference to the LookAt Object
    public GameObject lookAtMe;

    //What the camera intends to look at
	private Vector3 cameraFocusPoint;
    //For the specific offset of camera position in the world
    private Vector3 position_Offset = new Vector3(0f, 8f, -5f); //Normal
    //This angle offset is for the resting view of the camera pointing onto the player
    private float angle_Offset = 60 * Mathf.PI / 180.0f;
    //This angle offset is what allows the player to see all around them
    private float angle_Viewing_Offset = 45 * Mathf.PI / 180.0f;
    //For the right stick control
    private Vector3 rightStick_Offset = new Vector3(0f, 0f, 0f);

    // Use this for initialization
	void Start ()
	{
		//Grab the Game Manager, dictionary, movement library...
		GameObject gM_GameObject = GameObject.FindGameObjectWithTag("GM");
		gM = gM_GameObject.GetComponent<Sc_GM>();
        uM = gM.uM;

		player = gM.player.gameObject;

		freeCam = false;
	}

    //LateUpdate is used to allow position calculations to finish before moving the camera
	void LateUpdate ()
	{
		if (Input.GetButtonDown("freeCam") && gM.isFreeCam_Active)
		{
			freeCam = (freeCam == false) ? true : false;

			if (!freeCam)
			{
				//Also rest camera rotation once
				//transform.rotation = rotation_Offset;
			}
		}
        
        if (!freeCam)
        {
            //Transform position
            cameraFocusPoint = player.transform.position + rightStick_Offset; // + position_Offset;
            transform.position = Vector3.Lerp(transform.position, cameraFocusPoint, Time.deltaTime * speed);
            
            //Rotation direction
            //Only update this when the player is NOT moving (Or this may cause motion sickness)
            //Or, I could set it so that it only updates if the transform position isn't lerping
            //if (!uM.bool_PlayerMoving)
            //if (Vector3.Magnitude(transform.position) - Vector3.Magnitude(Vector3.Lerp(transform.position, cameraFocusPoint, Time.deltaTime * speed)) < 0.05f)
            //{
            transform.LookAt(lookAtMe.transform);

                //Maybe a better bit of code
                //Vector3 direction = player.transform.position - transform.position;
                //Quaternion toRotation = Quaternion.FromToRotation(transform.forward, direction);
                //transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, speed * Time.deltaTime);
            //}
        }
    }

    //We can do the maths in Update
    void Update()
    {
        print ((angle_Offset - maxAngle*Input.GetAxis("RightStickY"))*180.0f/Mathf.PI);

        rightStick_Offset = new Vector3(radius*(Input.GetAxis("RightStickX")*(1 + Input.GetAxis("RightStickY"))), 
                                    radius*(Input.GetAxis("RightStickY") + 1)*(1 + Input.GetAxis("RightStickX")),
                                    -radius);
    }
}
