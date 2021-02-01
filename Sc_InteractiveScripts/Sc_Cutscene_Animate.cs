using System.Collections;
using System.Collections.Generic;
using UnityEngine;

////////////////////////////////////////////////
//
//  Sc_ES_Cutscene_Animate
//  This script contains the functions to play
//  An attached animator script

public class Sc_Cutscene_Animate : MonoBehaviour
{
    //Obj References
    Animator ref_Animation;

    // Start is called before the first frame update
    void Start()
    {
        //Get references
        ref_Animation = GetComponent<Animator>();
    }

    public void Animate(string pass_String)
    {
        ref_Animation.SetBool(pass_String, true);
    }
}
