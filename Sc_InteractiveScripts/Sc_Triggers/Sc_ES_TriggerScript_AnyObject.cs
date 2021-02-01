using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////
//  Sc_Arm_BoneRotate
//  If the specified object enters the trigger space
//  of this script, it will hold true

public class Sc_ES_TriggerScript_AnyObject : MonoBehaviour
{
    public GameObject ref_Object;

    [System.NonSerialized]
    public bool objectInTrigger;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == ref_Object)
        {
            objectInTrigger = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == ref_Object)
        {
            objectInTrigger = false;
        }
    }
}
