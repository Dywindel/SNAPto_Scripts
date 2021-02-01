using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_RM - Render Manager
//  This script stores the render layer order of some objects when in certain states

public class Sc_RenderManager : MonoBehaviour
{
    //This integer values store the region order in which certain objects will be rendered
    [HideInInspector]
    public int rend_Puffs_Region = 3100;
    [HideInInspector]
    public int rend_Terrain_Region = 2900;

    //This array of material colds all the possible meshes for the clouds, which will be
    //loaded randomly into the level when it starts up
    public Mesh[] ref_MeshFilter_Clouds;

    //Some random variation in appearance
    [HideInInspector]
    public float rend_random_Angle = 1f;
    [HideInInspector]
    public float rend_random_trans = 0.05f;
    [HideInInspector]
    public float rend_random_scale = 0.1f;

    // This is the simple art style used for the floor blocks
    public GameObject ref_SimpleMode_Floor;
    public GameObject ref_SimpleMode_SoilDry;
    public GameObject ref_SimpleMode_SoilWatered;
}
