using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is a quick solution to stop flickering effects due to cloud rendering order mistakes when
//Using transparency in Unity

public class Sc_CloudMaterial_RenderQueue : MonoBehaviour
{
    //Reference to world managers
    private Sc_GM gM;					//Reference to the GM
    private Sc_RenderManager renderManager;                   //Reference to the render manager

    //Reference to the three blocks themselves
    public GameObject[] ref_Objects;

    //Apply the materials and set the render queue
    void Start()
    {
        //Grab the Game Manager, dictionary, movement library...
        renderManager = GameObject.FindGameObjectWithTag("RB").GetComponent<Sc_RenderManager>();

        for (int i = 0; i < ref_Objects.Length; i++)
        {
            //Give the transform a slight random fluctuation to improve variation in clouds
            /*
            ref_Objects[i].GetComponent<Transform>().localPosition = new Vector3(Random.Range(-rM.rend_random_trans, rM.rend_random_trans),
                                                                                Random.Range(-rM.rend_random_trans, rM.rend_random_trans),
                                                                                Random.Range(-rM.rend_random_trans, rM.rend_random_trans));
            ref_Objects[i].GetComponent<Transform>().eulerAngles = new Vector3(Random.Range(-rM.rend_random_Angle, rM.rend_random_Angle),
                                                                                Random.Range(-rM.rend_random_Angle, rM.rend_random_Angle),
                                                                                Random.Range(-rM.rend_random_Angle, rM.rend_random_Angle));
            ref_Objects[i].GetComponent<Transform>().localScale = new Vector3(1f, 1f, 1f) + new Vector3(Random.Range(-rM.rend_random_scale, rM.rend_random_scale),
                                                                                Random.Range(-rM.rend_random_scale, rM.rend_random_scale),
                                                                                Random.Range(-rM.rend_random_scale, rM.rend_random_scale));
            //Randomize which mesh is used
            ref_Objects[i].GetComponent<MeshFilter>().mesh = rM.ref_MeshFilter_Clouds[Random.Range(0, rM.ref_MeshFilter_Clouds.Length)];
            //Set the renderqueue for fixing transparency problems
            */
            ref_Objects[i].GetComponent<MeshRenderer>().material.renderQueue = renderManager.rend_Puffs_Region + 10*i;
        }
    }
}
