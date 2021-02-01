using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//This script is the editor script pairing for the
//UpdateMesh_Floor script. It simply updates the meshes
//While we're in editor mode, but does not run in play mode
/*
[CustomEditor(typeof(Sc_RM_UpdateMesh_Floor))]
public class Sc_ES_UpdateMesh_Floor : Editor
{
    //Calls the function if the object is selected
    public void OnSceneGUI()
    {
        Sc_RM_UpdateMesh_Floor ref_Script = (Sc_RM_UpdateMesh_Floor)target;

        ref_Script.RecordMeshNeighbour_AsBinary(false);
    }
}*/

[CustomEditor(typeof(Sc_RM_UpdateMesh_Floor_New))]
public class Sc_ES_UpdateMesh_Floor_New : Editor
{
    int i = 0;

    //Calls the function if the object is selected
    public void OnSceneGUI()
    {
        if ((i % 10) == 0)
        {
            Sc_RM_UpdateMesh_Floor_New ref_Script = (Sc_RM_UpdateMesh_Floor_New)target;

            ref_Script.RecordMeshNeighbour_AsBinary(1);
        }
        i += 1;
    }
}

[CustomEditor(typeof(Sc_RM_UpdateMesh_Cover_New))]
public class Sc_ES_UpdateMesh_Cover_New : Editor
{
    int i = 0;

    //Calls the function if the object is selected
    public void OnSceneGUI()
    {
        if ((i % 10) == 0)
        {
            Sc_RM_UpdateMesh_Cover_New ref_Script = (Sc_RM_UpdateMesh_Cover_New)target;

            ref_Script.RecordMeshNeighbour_AsBinary(1);
        }
        i += 1;
    }
}

// This is for the solid wall blocks that are modular, with no cover
[CustomEditor(typeof(Sc_RM_UpdateMesh_Walls))]
public class Sc_ES_UpdateMesh_Walls : Editor
{
    //Calls the function if the object is selected
    public void OnSceneGUI()
    {
        Sc_RM_UpdateMesh_Walls ref_Script = (Sc_RM_UpdateMesh_Walls)target;

        ref_Script.RecordMeshNeighbour_AsBinary(1);
    }
}

// This is the editor script for updating covered items
[CustomEditor(typeof(Sc_RM_UpdateMesh_Cover))]
public class Sc_ES_UpdateMesh_Covers : Editor
{
    //Calls the function if the object is selected
    public void OnSceneGUI()
    {
        Sc_RM_UpdateMesh_Cover ref_Script = (Sc_RM_UpdateMesh_Cover)target;

        ref_Script.RecordMeshNeighbour_AsBinary(1);
    }
}

// This is the edtior script for updating paths
[CustomEditor(typeof(Sc_RM_UpdateMaterial_Path))]
public class Sc_ES_UpdateMaterial_Path : Editor
{
    //Calls the function if the object is selected
    public void OnSceneGUI()
    {
        Sc_RM_UpdateMaterial_Path ref_Script = (Sc_RM_UpdateMaterial_Path)target;

        ref_Script.RecordNeighbours_AsBinary(1);
    }
}