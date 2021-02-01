using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

////////////////////////////////////////////////
//
//  Sc_Camera_MainMenu - Main Menu Camera
//  This camera is used for the main menu 
//  Before starting the game

public class Sc_Camera_MainMenu : MonoBehaviour
{
    [HideInInspector]
    //Position and rotation variables for each menu
    public Vector3[] vect_Main = {new Vector3(0.72f, -8.24f, -1.32f), new Vector3 (70.386f, 7.791f, -6.477f)};

    [HideInInspector]
    public Vector3[] vect_Load = {new Vector3(-10.66f, -19.85f, 4.07f), new Vector3(0, -90, 0)};

    public Vector3[] vect_Setup = {new Vector3(-6f, -28f, -18f), new Vector3(0, 0, 0)};

    [HideInInspector]
    public Vector3[] vect_Exit = {new Vector3(0, 0, -1000f), new Vector3(0, 0, 0)};


    //MAke sure the camera starts in the start position
    void Start()
    {
        this.gameObject.transform.position = vect_Main[0];
        this.gameObject.transform.eulerAngles = vect_Main[1];
    }

    public IEnumerator MoveCamera(Vector3[] pass_End)
    {
        Transform currentTransform = this.gameObject.transform;
        Vector3 currentPosition = this.gameObject.transform.position;
        Vector3 currentRotation = this.gameObject.transform.eulerAngles;
        //Remeber, gM.tp is 0.2f
        for (float t = -50.0f; t < 50.0f; t += Time.deltaTime/0.05F)
        {
            //Move this blockGroup gameobject
            currentTransform.position = Vector3.Lerp(currentPosition, pass_End[0], f_Sigmoid(t));

            //Lerp angle can be used to ensure the rotation of an object follows the shortest distance
            //When moving through 360. Quarternion.Lerp is the more concise method
            float anglex = Mathf.LerpAngle(currentRotation.x, pass_End[1].x, f_Sigmoid(t));
            float angley = Mathf.LerpAngle(currentRotation.y, pass_End[1].y, f_Sigmoid(t));
            float anglez = Mathf.LerpAngle(currentRotation.z, pass_End[1].z, f_Sigmoid(t));

            currentTransform.eulerAngles = new Vector3(anglex, angley, anglez);

            yield return null;
        }

        currentTransform.position = pass_End[0];
        currentTransform.eulerAngles = pass_End[1];
    }

    public float f_Sigmoid(float x) {
		return 1 / (1 + (float)Math.Exp(-x*0.15f));
	}
}
