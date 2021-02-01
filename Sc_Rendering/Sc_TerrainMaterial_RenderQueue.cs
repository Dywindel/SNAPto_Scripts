using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is a quick solution to stop flickering effects due to terrain rendering order mistakes when
//Using transparency in Unity

public class Sc_TerrainMaterial_RenderQueue : MonoBehaviour
{
    //Reference to world managers
    private Sc_GM gM;					//Reference to the GM
    private Sc_RenderManager renderManager;                   //Reference to the render manager

    //Reference to the layers of terrain
    public MeshRenderer[] ref_Objects;

    //Apply the materials and set the render queue
    void Start()
    {
        //Grab the Game Manager, dictionary, movement library...
		GameObject gM_GameObject = GameObject.FindGameObjectWithTag("GM");
		gM = gM_GameObject.GetComponent<Sc_GM>();
        renderManager = gM.rRm;

        for (int i = 0; i < ref_Objects.Length; i++)
        {
            ref_Objects[i].material.renderQueue = renderManager.rend_Terrain_Region + 10*i;
        }
    }
}
