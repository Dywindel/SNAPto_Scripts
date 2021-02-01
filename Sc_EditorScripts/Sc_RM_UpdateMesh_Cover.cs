using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////////////////////////////////////////////////////
//  Sc_RM_UpdateMesh_Floor
//  This script holds the function and any relevent variables in order to change how the
//  Mesh of a floor tile looks like, depending on what's around it

// Testing something dumb
[ExecuteInEditMode]
public class Sc_RM_UpdateMesh_Cover : MonoBehaviour
{
    protected Sc_RB_Values rbV;		    //Reference to the values list

    // Sparseness of leaves
    [Range(0, 3)]
    public int leafSparseness = 0;
    private int store_LeafSparseness = -1;

    // Update the material once per 1000 ticks
    private int ticks = 0;
    private int ticksMax = 100;
    
    public Material[] leafMaterials;

    // These objects are for the cover
    public MeshFilter[] ref_MeshFilter_Cover;
    // Also, store a reference to the materials of each cover too
    public MeshRenderer[] ref_MeshRender_Cover;

    // List of all meshs that could be used
    // In future this will appear in a static library folder
    public Mesh[] ref_Meshes_Cover_Empty;
    public Mesh[] ref_Meshes_Cover_Full;

    // This vairable checks if another, similar, object is sitting atop us
    private bool isStacked = false;

    // Because I use OnSceneGUI to update my mesh, I want to make sure I'm always
    // Updating all the meshes around me before and after I move.
    // Here, I record these meshes before and after movement, so I can update them
    private List<Sc_RM_UpdateMesh_Cover> listOf_Ref_NeighbourMeshes = new List<Sc_RM_UpdateMesh_Cover>();

    void Updated()
    {
        RecordMeshNeighbour_AsBinary(0);
    }

    // This update alters the mesh regularly, without creating a chain of updates
    // Unless the passed boolean is true
    public void RecordMeshNeighbour_AsBinary(int chainIndex)
    {
        // First, I want to update the previous meshes I was next to
        foreach (Sc_RM_UpdateMesh_Cover temp_Script in listOf_Ref_NeighbourMeshes)
        {
            temp_Script.RecordMeshNeighbour_AsBinary(0);
        }
        // Then, I can reset the previous list
        listOf_Ref_NeighbourMeshes = new List<Sc_RM_UpdateMesh_Cover>();

		GameObject rB_GameObject = GameObject.FindGameObjectWithTag("RB");
        rbV = rB_GameObject.GetComponent<Sc_RB_Values>();

        // Wake up the dictionary
        rbV.AwakeInEditor();

        // We store the surrounding block value using a decimal conveted from binary
        int bin_BlockNeighbours = 0;

        // We can check if there's a block above, which will be useful later, when drawing grass
        RaycastHit[] hitInfo = null;
        hitInfo = Physics.RaycastAll(transform.position, rbV.int_To_Card[5], 1.0f);
        isStacked = false;

        // I'm gonna take this out for now, because it just looks a bit shit
        /*
        foreach (RaycastHit item in hitInfo)
        {
            if(item.collider.gameObject.tag == "Cover-Autumn")
            {
                isStacked = true;
            }
        }*/

        // First, we record what floor tiles are around it using raycasting
        // We need to check 8 different directions
        // This is easier to do by splitting into two sets
        for (int n = 0; n < rbV.cD; n++)
        {
            for (int m = 0; m < rbV.mD/rbV.cD; m++)
            {
                hitInfo = null;
                // In the regular case, we just draw a line from the transform to a neighbouring block
                if (m == 0)
                {
                    hitInfo = Physics.RaycastAll(transform.position, rbV.int_To_Card[n], 1.0f);
                }
                // In the odd case, we have to stagger the movement and starting position of the ray
                else
                {
                    hitInfo = Physics.RaycastAll(transform.position + rbV.int_To_Card[n], rbV.int_To_Card[(n + 1) % rbV.cD], 1.0f);
                }

                // This is my theory at least
                // Then, we check what we've found. If any of the objects we find have the tag ("Appropriate name")
                // Then we add a binary multiple to the overal value and go straight to the next number in the forloop

                // prepare the doOnce boolean
                bool checkOnce = false;

                // As long as an object is hit
                foreach (RaycastHit item in hitInfo)
                {
                    if(item.collider.gameObject.tag == "Cover-Autumn")
                    {
                        // If we are chaining mesh updates
                        if (chainIndex > 0)
                        {
                            // Update that mesh too, but don't chain it
                            item.collider.gameObject.GetComponent<Sc_RM_UpdateMesh_Cover>().RecordMeshNeighbour_AsBinary(0);
                            // Record it in a list so it can be update when this object leaves
                            listOf_Ref_NeighbourMeshes.Add(item.collider.gameObject.GetComponent<Sc_RM_UpdateMesh_Cover>());
                        }
                        // This means a correspending block is here and we can check off the doOnce boolean
                        checkOnce = true;
                    }
                }

                // If we have found any blocks
                if (checkOnce)
                {
                    // We update the binary value
                    bin_BlockNeighbours += (int)(Mathf.Pow(4, n) * Mathf.Pow(2, m));
                }
            }
        }

        // Print the binary value to check what we've found
        // print (bin_BlockNeighbours);

        // Now we can switch the meshes in each section of this object, depending on the bin_BlockNeighbour number
        UpdateMesh(bin_BlockNeighbours);
    }

    public void UpdateMesh(int pass_bin)
    {
        // First, if the sparseness value has changed, then change the materials for each cover
        if (store_LeafSparseness != leafSparseness)
        {
            // Update each material
            foreach (MeshRenderer temp_Cover in ref_MeshRender_Cover)
            {
                temp_Cover.material = leafMaterials[leafSparseness];
            }

            store_LeafSparseness = leafSparseness;
        }

        // First, store the binary data as a list of numbers
        List<int> bin_AsList = BinaryConversion(pass_bin);

        // Let's start with the center
        // Nothing needs to happen!

        // I'll generate the random floats here and make then equal for each 4 cover pieces, so the material's line up
        //Vector2 randTextureOffset = new Vector2 (Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
        //Vector2 randTextureScale = new Vector2 (Random.Range(0.2f, 0.3f), Random.Range(0.2f, 0.3f));
        int rand = Random.Range(0, 5);

        for (int n = 0; n < rbV.cD; n++)
        {
            for (int m = 0; m < rbV.mD/rbV.cD; m++)
            {
                // Edges
                if (m == 0)
                {
                    // if our list contains n, then we use a blank edge
                    if (bin_AsList.Contains(2*n))
                    {
                        // For base shapes
                        ref_MeshFilter_Cover[n].mesh = ref_Meshes_Cover_Empty[n];
                        // Also, Update and randomize the material?
                        //if (ticks % ticksMax == 0)
                        //{
                        ref_MeshRender_Cover[n].material = leafMaterials[leafSparseness*5 + rand];
                        //}
                        //ref_MeshRender_Cover[n].material.SetTextureOffset("_MainTex", randTextureOffset);
                        //ref_MeshRender_Cover[n].material.mainTextureScale = randTextureScale;
                    }
                    else
                    {
                        // For covering grass
                        ref_MeshFilter_Cover[n].mesh = ref_Meshes_Cover_Full[n];
                        // If there is similar block above us that has grass, don't draw in the grass
                        // For the specific case when there isn't a block in the direction
                        // I've removed this for now because it looks a bit shit
                        /*
                        if (isStacked)
                            ref_MeshFilter_Cover[n].gameObject.SetActive(false);
                        else
                            ref_MeshFilter_Cover[n].gameObject.SetActive(true);
                        */
                        // Also, Update and randomize the material?
                        /*
                        if (ticks % ticksMax == 0)
                            ref_MeshRender_Cover[n].material = leafMaterials[leafSparseness];
                        */
                        ref_MeshRender_Cover[n].material = leafMaterials[leafSparseness*5 + rand];
                        //ref_MeshRender_Cover[n].material.mainTextureOffset = randTextureOffset;
                        //ref_MeshRender_Cover[n].material.mainTextureScale = randTextureScale;
                    }
                }
            }
        }

        // Update once per 1000 ticks
        ticks = (ticks + 1) % ticksMax;
    }

    // Convert a number into a list of powers of 2
    List<int> BinaryConversion(int pass_bin)
    {
        List<int> bin_AsList = new List<int>();

        for (int i = rbV.mD - 1; i >= 0; i--)
        {
            if (pass_bin >= Mathf.Pow(2, i))
            {
                bin_AsList.Add(i);
                pass_bin -= (int)Mathf.Pow(2, i);
            }
        }

        return bin_AsList;
    }
}
