using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////
//	Sc_RM_Material - Random Material offset
//  Randomizes the material offset of a mesh

[ExecuteInEditMode]
public class Sc_RM_Material : MonoBehaviour
{
    public float rand_minScale;
    public float rand_maxScale;

    private MeshRenderer ref_MeshRenderer;

     Vector2 ref_vec = new Vector2(0f, 0f);

    //This is now a button
    public void Button_RandomiseMaterial()
    {
        ref_MeshRenderer = GetComponent<MeshRenderer>();

        //Randomize the material offset and scale
        ref_MeshRenderer.material.mainTextureOffset = new Vector2(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
        ref_MeshRenderer.material.mainTextureScale = new Vector2(Random.Range(rand_minScale, rand_maxScale), Random.Range(rand_minScale, rand_maxScale));
    }

    /*
    void Update()
    {
        if (Random.Range(0, 30) == 0)
        {
            //Because it's fun
            //ref_MeshRenderer.material.mainTextureOffset = new Vector2(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            ref_vec = new Vector2(Random.Range(0.9f, 1.1f), Random.Range(0.9f, 1.1f));
        }

        ref_MeshRenderer.material.mainTextureScale = Vector2.Lerp(ref_MeshRenderer.material.mainTextureScale,
                                                                    ref_vec,
                                                                    Time.deltaTime * 2f);
    }*/
}
