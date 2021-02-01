using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////////////////////////////////////////////////////
//  Sc_RM_UpdateMesh_Path
//  This script holds the function and any relevent variables in order to change how the
//  Material of a Path tile looks like, depending on what's around it

//[ExecuteInEditMode]
public class Sc_RM_UpdateMaterial_Path : MonoBehaviour
{
    protected Sc_RB_Values rbV;		    //Reference to the values list

    //Reference to each material that makes up the base mesh
    public MeshRenderer ref_MeshRenderer;
    //The materials I will be replacing them with
    public Material[] ref_Materials;

    //Because I use OnSceneGUI to update my mesh, I want to make sure I'm always
    //Updating all the meshes around me before and after I move.
    //Here, I record these meshes before and after movement, so I can update them
    private List<Sc_RM_UpdateMaterial_Path> listOf_Ref_NeighbourMeshes = new List<Sc_RM_UpdateMaterial_Path>();

    /*
    void Update()
    {
        RecordNeighbours_AsBinary(0);
    }*/

    //This update alters the mesh regularly, without creating a chain of updates
    //Unless the passed boolean is true
    public void RecordNeighbours_AsBinary(int chainIndex)
    {
        //First, I want to update the previous meshes I was next to
        foreach (Sc_RM_UpdateMaterial_Path temp_Script in listOf_Ref_NeighbourMeshes)
        {
            temp_Script.RecordNeighbours_AsBinary(0);
        }
        //Then, I can reset the previous list
        listOf_Ref_NeighbourMeshes = new List<Sc_RM_UpdateMaterial_Path>();

        GameObject rB_GameObject = GameObject.FindGameObjectWithTag("RB");
        rbV = rB_GameObject.GetComponent<Sc_RB_Values>();
        
        //Wake up the dictionary
        rbV.AwakeInEditor();

        //We store the surrounding block value using a decimal conveted from binary
        int bin_BlockNeighbours = 0;

        //First, we record what floor tiles are around it using raycasting
        //We need to check 8 different directions
        //This is easier to do by splitting into two sets
        for (int n = 0; n < rbV.cD; n++)
        {
            for (int m = 0; m < rbV.mD/rbV.cD; m++)
            {
                RaycastHit[] hitInfo = null;
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
                    if(item.collider.gameObject.tag == "Path-Autumn")
                    {
                        //If we are chaining mesh updates
                        if (chainIndex > 0)
                        {
                            //Update that mesh too, but don't chain it
                            item.collider.gameObject.GetComponent<Sc_RM_UpdateMaterial_Path>().RecordNeighbours_AsBinary(0);
                            //Record it in a list so it can be update when this object leaves
                            listOf_Ref_NeighbourMeshes.Add(item.collider.gameObject.GetComponent<Sc_RM_UpdateMaterial_Path>());
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

        //Now we can switch the meshes in each section of this object, depending on the bin_BlockNeighbour number
        UpdateMaterial(bin_BlockNeighbours);
    }

    public void UpdateMaterial(int pass_bin)
    {
        //First, store the binary data as a list of numbers
        List<int> bin_AsList = BinaryConversion(pass_bin);

        //To make this way easier, I need to separae the corner values (1, 3, 5, 7)
        //So I don't have to worry about whether there are other corner tiles beside the path
        //I can do this by removing every odd number and dividing by 2
        bin_AsList.Remove(1);
        bin_AsList.Remove(3);
        bin_AsList.Remove(5);
        bin_AsList.Remove(7);

        List<int> card_Bin_AsList = new List<int>();
        foreach (int i in bin_AsList)
        {
            if (i % 2 == 0)
            {
                card_Bin_AsList.Add(i/2);
            }
        }


        //If our list contains no numbers, we can use a center piece
        if (card_Bin_AsList.Count == 0)
        {
            ref_MeshRenderer.material = ref_Materials[0];
        }
        //For 1 value, it's a deadend
        else if (card_Bin_AsList.Count == 1)
        {
            //We only care about the even directions for now
            ref_MeshRenderer.material = ref_Materials[1];
            //If rotation is based on n
            transform.rotation = Quaternion.Euler(-90f, 0f, -270f + 90f*card_Bin_AsList[0]);
        }
        else if (card_Bin_AsList.Count == 2)
        {
            //First we add all the numbers together
            //If the summation is even, its a straight path
            if ((card_Bin_AsList[0] + card_Bin_AsList[1]) % 2 == 0)
            {
                //If the first value is 2, I know which direction the path is going
                //Remember, the bin list counts downards
                if (card_Bin_AsList[0] == 2)
                {
                    ref_MeshRenderer.material = ref_Materials[3];
                    transform.rotation = Quaternion.Euler(-90f, 0f, 90f);
                }
                //Otherwise the path goes the other way
                else
                {
                    ref_MeshRenderer.material = ref_Materials[3];
                    transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
                }
            }
            //If its odd, its a curved path
            else
            {
                //Just multiply the rotation by the first integer in the list
                //Special case if numbers are 3 and 0
                ref_MeshRenderer.material = ref_Materials[2];
                if (card_Bin_AsList[0] == 3 && card_Bin_AsList[1] == 0)
                {
                    transform.rotation = Quaternion.Euler(-90f, 0f, 90f*card_Bin_AsList[1]);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(-90f, 0f, 90f*card_Bin_AsList[0]);
                }
            }
        }
        //For tee pieces
        else if (card_Bin_AsList.Count == 3)
        {
            //A REVERSED order can be found by their summation (minus 3)
            int temp_int = card_Bin_AsList[0] + card_Bin_AsList[1] + card_Bin_AsList[2] - 3;

            ref_MeshRenderer.material = ref_Materials[4];
            //If rotation is based on n
            transform.rotation = Quaternion.Euler(-90f, 0f, -90f*temp_int);
        }
        //For all four directions
        else if (card_Bin_AsList.Count == 4)
        {
            ref_MeshRenderer.material = ref_Materials[5];
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
