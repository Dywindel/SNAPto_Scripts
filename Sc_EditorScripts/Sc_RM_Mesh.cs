using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////
//	Sc_RM_Mesh - Random Mesh for Floors
//  This script randomizes each floor mesh when the gmae is played

public class Sc_RM_Mesh : MonoBehaviour
{
    private MeshFilter ref_MeshFilter;  //Self reference to MeshFilter

    public Mesh[] ref_MeshFilter_Floors;    //List of all the available meshes

    // Start is called before the first frame update
    public void Button_RandomiseMesh()
    {
        ref_MeshFilter = GetComponent<MeshFilter>();

        //Randomize which mesh is used
        ref_MeshFilter.mesh = ref_MeshFilter_Floors[Random.Range(0, ref_MeshFilter_Floors.Length)];
    }

    public void UpdateMesh()
    {
        if (Random.Range(0, 1) == 0)
        {
            ref_MeshFilter = GetComponent<MeshFilter>();
            ref_MeshFilter.mesh = ref_MeshFilter_Floors[Random.Range(0, ref_MeshFilter_Floors.Length)];
        }
    }
}
