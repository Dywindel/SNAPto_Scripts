using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_GenerateWaterPlane
//  This script generates a large plane of water which we can then
//  Manipulate

public class Sc_GenerateWaterPlane : MonoBehaviour
{

    //Maybe try adding some randomness into the triangular grid using random generator

    public float planeSize = 1;
    public float v_Random = 0.01f;
    public int gridSize = 16;

    private MeshFilter ref_MeshFilter;

    // Start is called before the first frame update
    private void Start()
    {
        ref_MeshFilter = GetComponent<MeshFilter>();
        ref_MeshFilter.mesh = GenerateMesh();
    }

    //######################//
    //  FUNCTIONS           //
    //######################//

    //This functions generates a mesh
    private Mesh GenerateMesh()
    {
        //Create a new mesh
        Mesh m_Mesh = new Mesh();

        var m_verticies = new List<Vector3>();    //Store the vertices of a plane
        var m_normals = new List<Vector3>();      //Store the normals of the plane
        var m_uVs = new List<Vector2>();          //Reference to its UV map

        //This generates a mesh with a grid of points in a square array
        for (int i = 0; i < gridSize + 1; i++)
        {
            for (int j = 0; j < gridSize + 1; j++)
            {
                //Starting at (-0.5, -0.5) create a series of vertices along the x and z plane at planesize intervals
                m_verticies.Add(new Vector3(-planeSize * 0.5f + Random.Range(-v_Random, v_Random) + planeSize * (i / ((float)gridSize)),
                                            0,
                                            -planeSize * 0.5f + Random.Range(-v_Random, v_Random) + planeSize * (j / ((float)gridSize))));
                //Normals always point up
                m_normals.Add(Vector3.up);
                //UVs
                m_uVs.Add(new Vector2(i / (float)gridSize, j / (float)gridSize));
            }
        }

        //This takes those previously generated points and draws individual triangles from them
        var v_triangles = new List<int>();
        var v_vertCount = gridSize + 1;
        for (int i = 0; i < v_vertCount * v_vertCount - v_vertCount; i++)
        {
            if ((i + 1) % v_vertCount == 0)
            {
                continue;
            }
            v_triangles.AddRange(new List<int>()
            {
                i + 1 + v_vertCount, i + v_vertCount, i,
                i, i + 1, i + v_vertCount + 1
            });
        }

        //Now send all the data we've create into the mesh
        m_Mesh.SetVertices(m_verticies);
        m_Mesh.SetNormals(m_normals);
        m_Mesh.SetUVs(0, m_uVs);
        m_Mesh.SetTriangles(v_triangles, 0);

        return m_Mesh;
    }
}
