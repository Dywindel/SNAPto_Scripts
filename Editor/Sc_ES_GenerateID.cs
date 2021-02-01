using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

//////////////////////////////
// Sc_ES_GenerateID
// Used to generate a unique ID for each Cloud

[CustomEditor(typeof(Sc_ES_GenerateUniqueCloudID))]
public class Sc_ES_GenerateUniqueCloudID_Button : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Sc_ES_GenerateUniqueCloudID ref_Script = (Sc_ES_GenerateUniqueCloudID)target;

        if (GUILayout.Button("Generare Unique Cloud IDs"))
        {
            ref_Script.Button_GenerateUnitqueCloudIDs();
        }

        if (GUILayout.Button("Generare Unique Puzzle IDs"))
        {
            ref_Script.Button_GenerateUnitquePuzzleIDs();
        }

        if (GUILayout.Button("Reset All Unique IDs"))
        {
            ref_Script.Button_ResetAllValues();
        }
    }
}
