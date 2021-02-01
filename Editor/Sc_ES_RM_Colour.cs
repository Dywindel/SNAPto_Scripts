using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//This script is the editor script pairing for the
//RM Material script. It simply allows me to randomise
//the material of an object in editor mode, before playing

[CustomEditor(typeof(Sc_RM_Colour))]
public class Sc_ES_RM_Colour : Editor
{
    int i = 0;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Sc_RM_Colour ref_Script = (Sc_RM_Colour)target;

        //if ((i % 100000) == 0)
        //    ref_Script.Button_RandomiseColour();

        i = i + 1;
    }
}