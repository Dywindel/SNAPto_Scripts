using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//This script is the editor script pairing for the
//RM Material script. It simply allows me to randomise
//the material of an object in editor mode, before playing

[CustomEditor(typeof(Sc_RM_Mesh))]
public class Sc_ES_RM_Mesh : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Sc_RM_Mesh ref_Script = (Sc_RM_Mesh)target;

        if (GUILayout.Button("Randomise Mesh"))
        {
            ref_Script.Button_RandomiseMesh();
        }
    }
}

[CustomEditor(typeof(Sc_RM_Position))]
public class Sc_ES_RM_Position : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Sc_RM_Position ref_Script = (Sc_RM_Position)target;

        if (GUILayout.Button("Randomise Position"))
        {
            ref_Script.Button_RandomisePosition();
        }
    }
}

[CustomEditor(typeof(Sc_RM_Rotation))]
public class Sc_ES_RM_Rotation : Editor
{
    int i = 0;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Sc_RM_Rotation ref_Script = (Sc_RM_Rotation)target;

        //if (GUILayout.Button("Randomise Rotation"))
        //{
        if (i % 10 == 0)
            ref_Script.Button_RandomiseRotation();
        //}

        i = i + 1;
    }
}

[CustomEditor(typeof(Sc_RM_Scale))]
public class Sc_ES_RM_Scale : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Sc_RM_Scale ref_Script = (Sc_RM_Scale)target;

        if (GUILayout.Button("Randomise Scale Large"))
        {
            ref_Script.Button_RandomiseScale_Large();
        }
        if (GUILayout.Button("Randomise Scale Small"))
        {
            ref_Script.Button_RandomiseScale_Small();
        }
    }
}