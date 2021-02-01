using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////////////////////////////////////////////////////
//  Sc_RM_UpdateMesh_Floor
//  This script holds the function and any relevent variables in order to change how the
//  Mesh of a floor tile looks like, depending on what's around it

//Testing something dumb
//[ExecuteInEditMode]
public class Sc_RM_UpdateMesh_Floor : MonoBehaviour
{
    protected Sc_RB_Values rbV;     // Reference to the values list

    //Reference to each mesh object that makes the base mesh
    public MeshFilter[] ref_MeshFilter;
    //These objects are for the cover
    public MeshFilter[] ref_MeshFilter_Cover;

    //List of all meshs that could be used
    //In future this will appear in a static library folder
    public Mesh[] ref_Meshes_Center;
    public Mesh[] ref_Meshes_Edge;
    public Mesh[] ref_Meshes_Corner;
    public Mesh[] ref_Meshes_Cover_Empty;
    public Mesh[] ref_Meshes_Cover_Full;

    //This vairable checks if another, similar, object is sitting atop us
    private bool isStacked = false;

    //Because I use OnSceneGUI to update my mesh, I want to make sure I'm always
    //Updating all the meshes around me before and after I move.
    //Here, I record these meshes before and after movement, so I can update them
    //private List<Sc_RM_UpdateMesh_Floor> listOf_Ref_NeighbourMeshes = new List<Sc_RM_UpdateMesh_Floor>();

    void Update_SWITCHEDOFF()
    {
        RecordMeshNeighbour_AsBinary(false);
    }

    //This update alters the mesh regularly, without creating a chain of updates
    //Unless the passed boolean is true
    public void RecordMeshNeighbour_AsBinary(bool isChain)
    {
        //What if I passed a number as well as a boolean? This way the chain
        //Of checking that occurs could happen X number of times (Maybe twice?)

        /*
        //First, I want to update the previous meshes I was next to
        foreach (Sc_RM_UpdateMesh_Floor temp_Script in listOf_Ref_NeighbourMeshes)
        {
            temp_Script.RecordMeshNeighbour_AsBinary(false);
        }
        //Then, I can reset the previous list
        listOf_Ref_NeighbourMeshes = new List<Sc_RM_UpdateMesh_Floor>();
        */

        GameObject rB_GameObject = GameObject.FindGameObjectWithTag("RB");
		rbV = rB_GameObject.GetComponent<Sc_RB_Values>();
        
        //Wake up the dictionary
        rbV.AwakeInEditor();

        //We store the surrounding block value using a decimal conveted from binary
        int bin_BlockNeighbours = 0;

        //We can check if there's a block above, which will be useful later, when drawing grass
        RaycastHit[] hitInfo = null;
        hitInfo = Physics.RaycastAll(transform.position, rbV.int_To_Card[5], 1.0f);
        isStacked = false;
        foreach (RaycastHit item in hitInfo)
        {
            if(item.collider.gameObject.tag == "Wall-Canyon")
            {
                isStacked = true;
            }
        }

        //First, we record what floor tiles are around it using raycasting
        //We need to check 8 different directions
        //This is easier to do by splitting into two sets
        for (int n = 0; n < rbV.cD; n++)
        {
            for (int m = 0; m < rbV.mD/rbV.cD; m++)
            {
                hitInfo = null;
                //In the regular case, we just draw a line from the transform to a neighbouring block
                if (m == 0)
                {
                    hitInfo = Physics.RaycastAll(transform.position, rbV.int_To_Card[n], 1.0f);
                }
                //In the odd case, we have to stagger the movement and starting position of the ray
                else
                {
                    hitInfo = Physics.RaycastAll(transform.position + rbV.int_To_Card[n], rbV.int_To_Card[(n + 1) % rbV.cD], 1.0f);
                }

                //This is my theory at least
                //Then, we check what we've found. If any of the objects we find have the tag ("Appropriate name")
                //Then we add a binary multiple to the overal value and go straight to the next number in the forloop

                //prepare the doOnce boolean
                bool checkOnce = false;

                //As long as an object is hit
                foreach (RaycastHit item in hitInfo)
                {
                    if(item.collider.gameObject.tag == "Wall-Canyon")
                    {
                        //If we are chaining mesh updates
                        if (isChain)
                        {
                            //Update that mesh too, but don't chain it
                            item.collider.gameObject.GetComponent<Sc_RM_UpdateMesh_Floor>().RecordMeshNeighbour_AsBinary(false);
                            //Record it in a list so it can be update when this object leaves
                            //listOf_Ref_NeighbourMeshes.Add(item.collider.gameObject.GetComponent<Sc_RM_UpdateMesh_Floor>());
                        }
                        //This means a correspending block is here and we can check off the doOnce boolean
                        checkOnce = true;
                    }
                }

                //If we have found any blocks
                if (checkOnce)
                {
                    //We update the binary value
                    bin_BlockNeighbours += (int)(Mathf.Pow(4, n) * Mathf.Pow(2, m));
                }
            }
        }

        //Print the binary value to check what we've found
        //print (bin_BlockNeighbours);

        //Now we can switch the meshes in each section of this object, depending on the bin_BlockNeighbour number
        UpdateMesh(bin_BlockNeighbours);
    }

    public void UpdateMesh(int pass_bin)
    {
        //First, store the binary data as a list of numbers
        List<int> bin_AsList = BinaryConversion(pass_bin);

        //If there is similar block above us that has grass, don't draw in the grass
        //For the specific case when there isn't a block in the direction
        /*
        foreach (MeshFilter temp_GO in ref_MeshFilter_Cover)
        {
            if (isStacked)
                temp_GO.gameObject.SetActive(false);
            else
                temp_GO.gameObject.SetActive(true);
        }*/

        //First, let's just update the four edges. These should be pretty easy to do

        //Let's start with the center
        //Nother needs to happen!

        for (int n = 0; n < rbV.cD; n++)
        {
            for (int m = 0; m < rbV.mD/rbV.cD; m++)
            {
                //Edges
                if (m == 0)
                {
                    //if our list contains n, then we use a blank edge
                    if (bin_AsList.Contains(2*n))
                    {
                        //For base shapes
                        ref_MeshFilter[2*n].mesh = ref_Meshes_Edge[0];
                        ref_MeshFilter_Cover[n].mesh = ref_Meshes_Cover_Empty[n];
                    }
                    else
                    {
                        //For base shapes
                        ref_MeshFilter[2*n].mesh = ref_Meshes_Edge[1];
                        //For covering grass
                        ref_MeshFilter_Cover[n].mesh = ref_Meshes_Cover_Full[n];
                        //If there is similar block above us that has grass, don't draw in the grass
                        //For the specific case when there isn't a block in the direction
                        if (isStacked)
                            ref_MeshFilter_Cover[n].gameObject.SetActive(false);
                        else
                            ref_MeshFilter_Cover[n].gameObject.SetActive(true);
                    }
                }
                //Corners
                if (m == 1)
                {
                    if (!bin_AsList.Contains(2*n) && !bin_AsList.Contains(2*n + 1) && !bin_AsList.Contains((2*n + 2) % rbV.mD))
                    {
                        ref_MeshFilter[2*n + m].mesh = ref_Meshes_Corner[3];
                    }
                    else if (bin_AsList.Contains(2*n) && !bin_AsList.Contains(2*n + 1) && !bin_AsList.Contains((2*n + 2) % rbV.mD))
                    {
                        ref_MeshFilter[2*n + m].mesh = ref_Meshes_Corner[2];
                    }
                    else if (!bin_AsList.Contains(2*n) && bin_AsList.Contains(2*n + 1) && !bin_AsList.Contains((2*n + 2) % rbV.mD))
                    {
                        ref_MeshFilter[2*n + m].mesh = ref_Meshes_Corner[3];
                    }
                    else if (!bin_AsList.Contains(2*n) && !bin_AsList.Contains(2*n + 1) && bin_AsList.Contains((2*n + 2) % rbV.mD))
                    {
                        ref_MeshFilter[2*n + m].mesh = ref_Meshes_Corner[1];
                    }
                    else if (bin_AsList.Contains(2*n) && bin_AsList.Contains(2*n + 1) && !bin_AsList.Contains((2*n + 2) % rbV.mD))
                    {
                        ref_MeshFilter[2*n + m].mesh = ref_Meshes_Corner[2];
                    }
                    else if (bin_AsList.Contains(2*n) && !bin_AsList.Contains(2*n + 1) && bin_AsList.Contains((2*n + 2) % rbV.mD))
                    {
                        ref_MeshFilter[2*n + m].mesh = ref_Meshes_Corner[4];
                    }
                    else if (!bin_AsList.Contains(2*n) && bin_AsList.Contains(2*n + 1) && bin_AsList.Contains((2*n + 2) % rbV.mD))
                    {
                        ref_MeshFilter[2*n + m].mesh = ref_Meshes_Corner[1];
                    }
                    else if (bin_AsList.Contains(2*n) && bin_AsList.Contains(2*n + 1) && bin_AsList.Contains((2*n + 2) % rbV.mD))
                    {
                        ref_MeshFilter[2*n + m].mesh = ref_Meshes_Corner[0];
                    }
                }
            }
        }
    }

    //Convert a number into a list of powers of 2
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
