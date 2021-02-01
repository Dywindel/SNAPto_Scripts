using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////
//  Sc_Arm_BoneRotate
//  This script rotates a bone to face a game object
//  if that game object is within a trigger

// You CANT scriptable move an object with an animator component attached to it

public class Sc_Arm_BoneRotate : MonoBehaviour
{
    private Transform this_T;   //Reference to this transform
    public Sc_ES_TriggerScript_AnyObject ref_TriggerScript;    //Reference to the trigger script

    private Transform ref_ObjectTrans;  //Reference to the object we want to follow

    void Start()
    {
        this_T = GetComponent<Transform>();
        ref_ObjectTrans = ref_TriggerScript.ref_Object.GetComponent<Transform>();
    }

    //As this is just an animation, I don't need
    //To add this to the update loop
    void Update()
    {
        if (ref_TriggerScript.objectInTrigger)
        {
            //float angle = Mathf.Rad2Deg * Vector2.Angle(new Vector2(transform.position.x, transform.position.z),
            //                            new Vector2(ref_ObjectTrans.position.x, ref_ObjectTrans.position.z));
            //float turnRadians = Mathf.Atan(ref_Distance[0]/ref_Distance[1]);
            //float angle = Mathf.Rad2Deg * turnRadians;

            //transform.rotation = Quaternion.Euler(-90f, angle, 0f);

            //This rotates the object in all 3D space
            Vector3 relativePos = ref_ObjectTrans.position - transform.position;
            Quaternion storeRotation = Quaternion.LookRotation(relativePos, new Vector3(0, 1, 0));

            //If I only want the y axis, then I can syphon out the other two
            Quaternion newRotation = Quaternion.Euler(-90, storeRotation.eulerAngles.y, 0);
            transform.rotation = newRotation;

        }
        else
        {
            //Do Nothing
            //transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
}
