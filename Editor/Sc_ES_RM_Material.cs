using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//This script is the editor script pairing for the
//RM Material script. It simply allows me to randomise
//the material of an object in editor mode, before playing


[CustomEditor(typeof(Sc_RM_Material))]
public class Sc_ES_RM_Material : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Sc_RM_Material ref_Script = (Sc_RM_Material)target;

        if (GUILayout.Button("Randomise MAterial"))
        {
            ref_Script.Button_RandomiseMaterial();
        }
    }
}
