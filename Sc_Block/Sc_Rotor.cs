using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

///////////////////////////////////////////////////
//
//	Sc_Rotors - Rotor objects
//	This is a blank script tests for rotor objects

public class Sc_Rotor : MonoBehaviour
{
    public Sc_AC aC;
    public Sc_RB_Animation rbA;
    public GameObject ref_RotorPart;

    void Start()
    {
        Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
        aC = gM.aC;
        rbA = gM.rbA;
    }

    // This is a blank rotation script as rotating can have different effects
    public virtual void Rotate(bool bool_Clockwise = false)
    {
        // Nothing happens
    }
}