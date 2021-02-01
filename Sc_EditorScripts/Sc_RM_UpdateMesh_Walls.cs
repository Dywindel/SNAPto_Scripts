using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////////////////////////////////////////////////////
//  Sc_RM_UpdateMesh_Floor
//  This script holds the function and any relevent variables in order to change how the
//  Mesh of a floor tile looks like, depending on what's around it

public class Sc_RM_UpdateMesh_Walls : MonoBehaviour
{
    // How far to check for other walls to change mesh with
    public float rayCast_wallDistance = 1;
    protected Sc_RB_Values rbV;     // Reference to the values list

    // Reference to the layer item we'll be checking when placing the walls
    public string ref_LayerName = "Wall-Canyon-NoCover";

    //Reference to each mesh object that makes the base mesh
    // And its material
    public MeshFilter[] ref_MeshFilter;

    //List of all meshs that could be used
    //In future this will appear in a static library folder
    public Mesh[] ref_Meshes_Center;
    public Mesh[] ref_Meshes_Edge;
    public Mesh[] ref_Meshes_Corner;

    // I will need a reference to each material too, if I decide to use normals
    public bool hasNormals = false;
    public Material[] ref_Material_Edge;
    public Material[] ref_Material_Corner;

    //This vairable checks if another, similar, object is sitting atop us
    private bool isStacked = false;

    //Because I use OnSceneGUI to update my mesh, I want to make sure I'm always
    //Updating all the meshes around me before and after I move.
    //Here, I record these meshes before and after movement, so I can update them
    private List<Sc_RM_UpdateMesh_Walls> listOf_Ref_NeighbourMeshes = new List<Sc_RM_UpdateMesh_Walls>();

    void Update_SwitchedOff()
    {
        RecordMeshNeighbour_AsBinary(0);
    }

    public void TestScript()
    {
        print ("Hi");
    }

    //This update alters the mesh regularly
    //A chainIndex value is passed to update nearby meshes
    public void RecordMeshNeighbour_AsBinary(int chainIndex)
    {
        //What if I passed a number as well as a boolean? This way the chain
        //Of checking that occurs could happen X number of times (Maybe twice?)

        //First, I want to update the previous meshes I was next to
        foreach (Sc_RM_UpdateMesh_Walls temp_Script in listOf_Ref_NeighbourMeshes)
        {
            temp_Script.RecordMeshNeighbour_AsBinary(0);
        }
        //Then, I can reset the previous list
        listOf_Ref_NeighbourMeshes = new List<Sc_RM_UpdateMesh_Walls>();

        GameObject rB_GameObject = GameObject.FindGameObjectWithTag("RB");
		rbV = rB_GameObject.GetComponent<Sc_RB_Values>();
        
        //Wake up the dictionary
        rbV.AwakeInEditor();

        //We store the surrounding block value using a decimal conveted from binary
        int bin_BlockNeighbours = 0;

        //We can check if there's a block above, which will be useful later, when drawing grass
        RaycastHit[] hitInfo = null;
        hitInfo = Physics.RaycastAll(transform.position, rbV.int_To_Card[5], rayCast_wallDistance);
        isStacked = false;
        foreach (RaycastHit item in hitInfo)
        {
            if(item.collider.gameObject.tag == ref_LayerName)
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
                    hitInfo = Physics.RaycastAll(transform.position, rbV.int_To_Card[n], rayCast_wallDistance);
                }
                //In the odd case, we have to stagger the movement and starting position of the ray
                else
                {
                    hitInfo = Physics.RaycastAll(transform.position + (rbV.int_To_Card[n]*rayCast_wallDistance), rbV.int_To_Card[(n + 1) % rbV.cD], rayCast_wallDistance);
                }

                //This is my theory at least
                //Then, we check what we've found. If any of the objects we find have the tag ("Appropriate name")
                //Then we add a binary multiple to the overal value and go straight to the next number in the forloop

                //prepare the doOnce boolean
                bool checkOnce = false;

                //As long as an object is hit
                foreach (RaycastHit item in hitInfo)
                {
                    if(item.collider.gameObject.tag == ref_LayerName)
                    {
                        //If we are chaining mesh updates
                        if (chainIndex > 0)
                        {
                            //Update that mesh too, but don't chain it
                            item.collider.gameObject.GetComponent<Sc_RM_UpdateMesh_Walls>().RecordMeshNeighbour_AsBinary(chainIndex - 1);
                            //Record it in a list so it can be update when this object leaves
                            listOf_Ref_NeighbourMeshes.Add(item.collider.gameObject.GetComponent<Sc_RM_UpdateMesh_Walls>());
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
                        // Update material normal
                        if (hasNormals)
                            ref_MeshFilter[2*n].gameObject.GetComponent<MeshRenderer>().material = ref_Material_Edge[0];
                    }
                    else
                    {
                        //For base shapes
                        ref_MeshFilter[2*n].mesh = ref_Meshes_Edge[1];
                        // Update material normal
                        if (hasNormals)
                            ref_MeshFilter[2*n].gameObject.GetComponent<MeshRenderer>().material = ref_Material_Edge[1];
                    }
                }
                //Corners
                if (m == 1)
                {
                    if (!bin_AsList.Contains(2*n) && !bin_AsList.Contains(2*n + 1) && !bin_AsList.Contains((2*n + 2) % rbV.mD))
                    {
                        ref_MeshFilter[2*n + m].mesh = ref_Meshes_Corner[3];
                        if (hasNormals)
                            ref_MeshFilter[2*n + m].gameObject.GetComponent<MeshRenderer>().material = ref_Material_Corner[3];
                    }
                    else if (bin_AsList.Contains(2*n) && !bin_AsList.Contains(2*n + 1) && !bin_AsList.Contains((2*n + 2) % rbV.mD))
                    {
                        ref_MeshFilter[2*n + m].mesh = ref_Meshes_Corner[2];
                        if (hasNormals)
                            ref_MeshFilter[2*n + m].gameObject.GetComponent<MeshRenderer>().material = ref_Material_Corner[2];
                    }
                    else if (!bin_AsList.Contains(2*n) && bin_AsList.Contains(2*n + 1) && !bin_AsList.Contains((2*n + 2) % rbV.mD))
                    {
                        ref_MeshFilter[2*n + m].mesh = ref_Meshes_Corner[3];
                        if (hasNormals)
                            ref_MeshFilter[2*n + m].gameObject.GetComponent<MeshRenderer>().material = ref_Material_Corner[3];
                    }
                    else if (!bin_AsList.Contains(2*n) && !bin_AsList.Contains(2*n + 1) && bin_AsList.Contains((2*n + 2) % rbV.mD))
                    {
                        ref_MeshFilter[2*n + m].mesh = ref_Meshes_Corner[1];
                        if (hasNormals)
                            ref_MeshFilter[2*n + m].gameObject.GetComponent<MeshRenderer>().material = ref_Material_Corner[1];
                    }
                    else if (bin_AsList.Contains(2*n) && bin_AsList.Contains(2*n + 1) && !bin_AsList.Contains((2*n + 2) % rbV.mD))
                    {
                        ref_MeshFilter[2*n + m].mesh = ref_Meshes_Corner[2];
                        if (hasNormals)
                            ref_MeshFilter[2*n + m].gameObject.GetComponent<MeshRenderer>().material = ref_Material_Corner[2];
                    }
                    else if (bin_AsList.Contains(2*n) && !bin_AsList.Contains(2*n + 1) && bin_AsList.Contains((2*n + 2) % rbV.mD))
                    {
                        ref_MeshFilter[2*n + m].mesh = ref_Meshes_Corner[4];
                        if (hasNormals)
                            ref_MeshFilter[2*n + m].gameObject.GetComponent<MeshRenderer>().material = ref_Material_Corner[4];
                    }
                    else if (!bin_AsList.Contains(2*n) && bin_AsList.Contains(2*n + 1) && bin_AsList.Contains((2*n + 2) % rbV.mD))
                    {
                        ref_MeshFilter[2*n + m].mesh = ref_Meshes_Corner[1];
                        if (hasNormals)
                            ref_MeshFilter[2*n + m].gameObject.GetComponent<MeshRenderer>().material = ref_Material_Corner[1];
                    }
                    else if (bin_AsList.Contains(2*n) && bin_AsList.Contains(2*n + 1) && bin_AsList.Contains((2*n + 2) % rbV.mD))
                    {
                        ref_MeshFilter[2*n + m].mesh = ref_Meshes_Corner[0];
                        if (hasNormals)
                            ref_MeshFilter[2*n + m].gameObject.GetComponent<MeshRenderer>().material = ref_Material_Corner[0];
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
