using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_GenerateNoise
//  This script generates a noise profile for the water

public class Sc_GenerateNoise : MonoBehaviour
{
    // References to World Objects
    Sc_RB_Clock rbC;                    // Reference to the World Clock

    public float power = 3;
    public float scale = 1;
    public float timeScale_Fixed;

    private float xOffset;
    private float yOffset;
    private MeshFilter ref_MeshFilter;

    // Start is called before the first frame update
    void Start()
    {
        // Grab the Game Manager, dictionary, movement library...
        rbC = GameObject.FindGameObjectWithTag("RB").GetComponent<Sc_RB_Clock>();

        ref_MeshFilter = GetComponent<MeshFilter>();
        MakeNoise();
    }

    // Update is called once per frame
    void Update()
    {
        // Update timescale depending on timeSpeed variable in the world clock
        float timeScale = timeScale_Fixed*Mathf.Sqrt(Mathf.Abs((float)rbC.timeSpeed));

        MakeNoise();
        xOffset += Time.deltaTime * timeScale;
        if (yOffset <= 0.3)
            yOffset += Time.deltaTime * timeScale;
        if(yOffset >= power)
            yOffset -= Time.deltaTime * timeScale;
    }

    void MakeNoise()
    {
        Vector3[] m_vecticies = ref_MeshFilter.mesh.vertices;

        for(int i = 0; i < m_vecticies.Length; i++)
        {
            m_vecticies[i].y = CalculateHeight(m_vecticies[i].x, m_vecticies[i].z) * power;
        }

        ref_MeshFilter.mesh.vertices = m_vecticies;

        ref_MeshFilter.mesh.RecalculateNormals();
    }

    float CalculateHeight(float x, float y)
    {
        float xCord = x * scale + xOffset;
        float yCord = y * scale + yOffset;

        return Mathf.PerlinNoise(xCord, yCord);
    }
}
