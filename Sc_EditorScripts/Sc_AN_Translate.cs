using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////
//
//  This sscript causes an object to move over time
//  As specified in the editor

public class Sc_AN_Translate : MonoBehaviour
{
    // Grab the object's transform
    private Transform ref_trans;
    // Start position
    private Vector3 ref_StartPos;

    void Start()
    {
        // Get the transform
        ref_trans = this.gameObject.transform;
        // Get the start position
        ref_StartPos = ref_trans.position;

        
    }
}
