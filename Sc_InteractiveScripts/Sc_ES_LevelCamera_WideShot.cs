using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_WiderCamera
//  When the player enters the trigger box, pan the camera back
//  So we can get a better view of what's happening

public class Sc_ES_LevelCamera_WideShot : MonoBehaviour
{
    protected Sc_RB_Animation rbA;		//Reference to the animation list

    // Cutscene Setup
    public Transform levelCamera_Position;

    // Reference to the main camera
    private Sc_Camera ref_Camera;

    // Camera will use the angle of the positioned camera to point in a new direction

    void Start()
    {
        // Grab the Game Manager, dictionary, movement library...
		Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
		rbA = gM.rbA;

        ref_Camera = GameObject.FindGameObjectWithTag("MainCamera").gameObject.GetComponent<Sc_Camera>();
    }

    public void LevelCamera_On()
    {
        // Set the camera position and rotation
        ref_Camera.levelCamera_FocusPoint = levelCamera_Position.position;
        // We will always send the reference camera's angle 
        ref_Camera.levelCamera_RotationPoint = levelCamera_Position.rotation;
        // Set the camera to move
        ref_Camera.bool_LevelCamera = true;
    }

    public void LevelCamera_Off()
    {
        // Turn off level camera
        ref_Camera.bool_LevelCamera = false;
    }
}
