using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_GenerateMesh_Straw
//  This is a concept script to generate a field of straw plants
//  That I can then manipulate

public class Sc_GenerateMesh_Straw : MonoBehaviour
{
    //Maybe try adding some randomness into the triangular grid using random generator

    public float strawSize = 1;
    public int gridSize = 16;

    private MeshFilter ref_MeshFilter;

    // Start is called before the first frame update
    private void Start()
    {
        ref_MeshFilter = GetComponent<MeshFilter>();
        ref_MeshFilter.mesh = GenerateMesh_Straw();
    }

    private Mesh GenerateMesh_Straw()
    {
        Mesh m_Mesh = new Mesh();

        var m_verticies = new Vector3[4];    //Store the vertices of a plane

        m_verticies[0] = new Vector3(-strawSize, strawSize);
        m_verticies[1] = new Vector3(0, 0, strawSize);
        m_verticies[2] = new Vector3(0, 0, -strawSize);
        m_verticies[3] = new Vector3(strawSize, strawSize);

        m_Mesh.vertices = m_verticies;

        m_Mesh.normals = new Vector3[] {Vector3.up, Vector3.up};

        m_Mesh.triangles = new int[] {0, 1, 3, 0, 2, 3};

        return m_Mesh;
    }
}
