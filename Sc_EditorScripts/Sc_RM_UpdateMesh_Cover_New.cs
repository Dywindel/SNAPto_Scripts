using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/////////////////////////////////////////////////////
//  Sc_RM_UpdateMesh_Floor_New
//  This script updates the floor to match the correct
//  Mesh object depending on what's around it

// NOTE: The order in which some of these check functions occur is extremely vital.
// Don't go just moving stuff around

// Testing something dumb
// [ExecuteInEditMode]
public class Sc_RM_UpdateMesh_Cover_New : MonoBehaviour
{
    protected Sc_RB_Values rbV;     // Reference to the values list

    // Reference to the layer item we'll be checking when placing the walls
    public string ref_LayerName = "Autumn-Cover";

    // Sparseness of leaves
    [Range(0, 3)]
    public int leafSparseness = 0;
    // Reference to the main mesh object
    public MeshFilter ref_MeshFilter;
    // Reference to the material too
    public MeshRenderer ref_MR;
    // Reference to the possible meshes this object could take
    public Mesh[] ref_MeshFilter_Options;
    // This material list refers to the different sparseness of the covers
    public Material[] ref_Material_Sparsematerial;

    // Because I use OnSceneGUI to update my mesh, I want to make sure I'm always
    // Updating all the meshes around me before and after I move.
    // Here, I record these meshes before and after movement, so I can update them
    private List<Sc_RM_UpdateMesh_Cover_New> listOf_Ref_NeighbourMeshes = new List<Sc_RM_UpdateMesh_Cover_New>();

    void Update_SWITCHEDOFF()
    {
        RecordMeshNeighbour_AsBinary(0);
    }

    // This update alters the mesh regularly, without creating a chain of updates
    // Unless the passed boolean is true
    public void RecordMeshNeighbour_AsBinary(int chainIndex)
    {
        //What if I passed a number as well as a boolean? This way the chain
        //Of checking that occurs could happen X number of times (Maybe twice?)

        if (chainIndex > 0)
        {
            //First, I want to update the previous meshes I was next to
            foreach (Sc_RM_UpdateMesh_Cover_New temp_Script in listOf_Ref_NeighbourMeshes)
            {
                // If they still exist
                if (temp_Script != null)
                    temp_Script.RecordMeshNeighbour_AsBinary(0);
            }
        }

        // First, we reset the list of neighbours
        listOf_Ref_NeighbourMeshes = new List<Sc_RM_UpdateMesh_Cover_New>();

        GameObject rB_GameObject = GameObject.FindGameObjectWithTag("RB");
		rbV = rB_GameObject.GetComponent<Sc_RB_Values>();
        
        // Wake up the dictionary
        rbV.AwakeInEditor();

        // We store the surrounding block value using a decimal conveted from binary
        int bin_BlockNeighbours = 0;

        // First, we record what floor tiles are around it using raycasting
        // We need to check 8 different directions
        // This is easier to do by splitting into two sets
        for (int n = 0; n < rbV.cD; n++)
        {
            for (int m = 0; m < rbV.mD/rbV.cD; m++)
            {
                RaycastHit[] hitInfo;
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

                 //As long as an object is hit
                foreach (RaycastHit item in hitInfo)
                {
                    if(item.collider.gameObject.tag == ref_LayerName)
                    {
                        // For reference
                        Sc_RM_UpdateMesh_Cover_New item_Script = item.collider.gameObject.GetComponent<Sc_RM_UpdateMesh_Cover_New>();
                        // Check we haven't hit this mesh before and it's not us
                        if ((!listOf_Ref_NeighbourMeshes.Contains(item_Script) && (item_Script != this)))
                        {
                            // Then we add it to our hit list
                            listOf_Ref_NeighbourMeshes.Add(item_Script);

                            //If we are chaining mesh updates
                            if (chainIndex > 0)
                            {
                                //Update that mesh too, but don't chain it
                                item_Script.RecordMeshNeighbour_AsBinary(chainIndex - 1);                                
                            }
                            //This means a correspending block is here and we can check off the doOnce boolean
                            checkOnce = true;
                        }
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
        //print (bin_BlockNeighbours);

        // Now we can switch the meshes in each section of this object, depending on the bin_BlockNeighbour number
        List<int> bin_AsList = BinaryConversion(bin_BlockNeighbours);
        UpdateMesh(bin_AsList, bin_BlockNeighbours);
    }

    public void UpdateMesh(List<int> bin_AsList, int pass_bin)
    {
        // First, let's invert all the numbers!
        List<int> bin_AsList_Inverted = MathList(bin_AsList, 1);

        // Let's do this by the size of each list
        // Size 0 v/
        if (bin_AsList.Count == 0)
        {
            // Update the mesh to our new shape
            MeshDictionary(0);
        }
        
        // Size 1 v/
        else if (bin_AsList.Count == 1)
        {
            // If even
            if (bin_AsList[0] % 2 == 0)
                MeshDictionary(1, bin_AsList[0]);
            // If even, the mesh used is the same as if the bin.count is 0
            else
                MeshDictionary(0);
        }

        // Size 2 v/
        else if (bin_AsList.Count == 2)
        {
            // If both numbers are odd, It's an island
            if ((bin_AsList[0]*bin_AsList[1]) % 2 != 0)
                MeshDictionary(0);
            // If one number is even, it's a dead end
            else if ((bin_AsList[0] + bin_AsList[1]) % 2 != 0)
                MeshDictionary(1, MathDictionary(bin_AsList, 0));
            // If both numbers are even, it's a closed corner or a bridge
            else
            {
                // If the difference between both numbers is 4, it's a bridge
                if (Mathf.Abs(bin_AsList[0] - bin_AsList[1]) == 4)
                    MeshDictionary(2, bin_AsList[0]);
                // Otherwise, it's a closed corner
                else
                {
                    // Special case if there's a six involved
                    if (bin_AsList[1] == 6)
                    {
                        if (bin_AsList[0] == 0)
                            MeshDictionary(3, bin_AsList[1]);
                        else
                            MeshDictionary(3, bin_AsList[0]);
                    }
                    else
                    {
                        MeshDictionary(3, bin_AsList[0]);
                    }
                }
            }
        }

        // Size 3 v/
        else if (bin_AsList.Count == 3)
        {
            // If all numbers are even, it's a T-Piece
            if (MathCheck(bin_AsList, 0))
                // To find the angle, we add all the numbers togeter, subtract 6 and invert them
                MeshDictionary(4, 12 - bin_AsList.AsQueryable().Sum());

            // If all three numbers are odd, it's a solitary island
            else if (MathCheck(bin_AsList, 1))
                MeshDictionary(0);
            // If one number is odd
            else if ((bin_AsList.AsQueryable().Sum() % 2) != 0)
            {
                // If these numbers are consequtive
                if (MathCheck(bin_AsList, 2))
                //if (new List<int>{7, 28, 112, 193}.Contains(pass_bin))
                {
                    // Special case for 7
                    if (bin_AsList[2] == 7)
                        MeshDictionary(5, bin_AsList[1]);
                    else
                        MeshDictionary(5, bin_AsList[0]);
                }
                // Otherwise
                else
                {
                    // Grab the two even numbers and put the whole thing back into the second staage above
                    UpdateMesh(MathList(bin_AsList, 0), BinaryReConversion(bin_AsList));
                }
            }
            // If two numbers are odd
            else
            {
                // They're all deadends. Grab the one even number and apply the dead end to it
                UpdateMesh(MathList(bin_AsList, 0), BinaryReConversion(MathList(bin_AsList, 0)));
            }
        }

        // Size 4
        else if (bin_AsList.Count == 4)
        {
            // If all the numbers are even, its a crossroads
            if (MathCheck(bin_AsList, 0))
                MeshDictionary(6, 0);

            // If they're all odd, it's solitary
            else if (MathCheck(bin_AsList, 1))
                MeshDictionary(0);

            // This list if specific to an edge and corner piece
            else if (new List<int>{23, 92, 113, 197}.Contains(pass_bin))
                // invert, grab the even number, use that for rotation
                MeshDictionary(7, MathList(bin_AsList_Inverted, 0)[0]);

            // Same as before, but inverted
            else if (new List<int>{29, 116, 209, 71}.Contains(pass_bin))
                // invert, grab the even number, use that for rotation
                MeshDictionary(8, MathList(bin_AsList_Inverted, 0)[0]);

            // If one of them is odd, I can pass back the three remaining even numbers
            else if (MathDictionary(bin_AsList, 1) == 3)
                UpdateMesh(MathList(bin_AsList, 0), BinaryReConversion(MathList(bin_AsList, 0)));

            // If two of them are odd
            // There could be a consequtive closed corner, or an open corner, or a bridge
            else if (MathDictionary(bin_AsList, 1) == 2)
            {
                // If consequtive numbers, it's a closed corner
                if (MathCheck(bin_AsList, 2))
                {
                    // For consequtive that starts with an odd numbers
                    if (new List<int>{10, 18}.Contains(bin_AsList.AsQueryable().Sum()))
                    {
                        // Special case
                        if (bin_AsList[0] == 0)
                        {
                            if (bin_AsList[1] == 1)
                                // Here we want the first three numbers
                                UpdateMesh(bin_AsList.Take(3).ToList(), BinaryReConversion(bin_AsList.Take(3).ToList()));
                            else
                            {
                                // Engineer a new list
                                List<int> p_List = new List<int>{bin_AsList[0], bin_AsList[2], bin_AsList[3]};
                                UpdateMesh(p_List, BinaryReConversion(p_List));
                            }
                        }
                        else
                            // Here we want the last three numbers
                            UpdateMesh(bin_AsList.Skip(1).Take(3).ToList(), BinaryReConversion(bin_AsList.Skip(1).Take(3).ToList()));
                    }
                    // For consequtive that starts with even numbers
                    else
                    {
                        // Special case
                        if (bin_AsList[1] == 1)
                        {
                            if (bin_AsList[2] == 2)
                                // Here we want the first three numbers
                                UpdateMesh(bin_AsList.Take(3).ToList(), BinaryReConversion(bin_AsList.Take(3).ToList()));
                            else
                            {
                                // We need to engineer a new list
                                List<int> p_List = new List<int>{bin_AsList[0], bin_AsList[2], bin_AsList[3]};
                                UpdateMesh(p_List, BinaryReConversion(p_List));
                            }
                        }
                        else
                            // Grab the first three numbers
                            UpdateMesh(bin_AsList.Take(3).ToList(), BinaryReConversion(bin_AsList.Take(3).ToList()));
                    }
                }
                // If special case, it's also an open corner
                else if (new List<int>{39, 114, 156, 201}.Contains(pass_bin))
                {
                    // Special case
                    if (bin_AsList[0] == 0)
                    {
                        if (bin_AsList[1] == 1)
                            // Here we want the first three numbers
                            UpdateMesh(bin_AsList.Take(3).ToList(), BinaryReConversion(bin_AsList.Take(3).ToList()));
                        else
                        {
                            // Engineer a new list
                            List<int> p_List = new List<int>{bin_AsList[0], bin_AsList[2], bin_AsList[3]};
                            UpdateMesh(p_List, BinaryReConversion(p_List));
                        }
                    }
                    else
                    {
                        if (bin_AsList[0] == 1)
                            // In this case, we want the last three numbers
                            UpdateMesh(bin_AsList.Skip(1).Take(3).ToList(), BinaryReConversion(bin_AsList.Skip(1).Take(3).ToList()));
                        else
                            // Here we want the first three numbers
                            UpdateMesh(bin_AsList.Take(3).ToList(), BinaryReConversion(bin_AsList.Take(3).ToList()));
                    }
                }
                else
                    // For most cases, I can just pass back the two even numbers
                    UpdateMesh(MathList(bin_AsList, 0), BinaryReConversion(MathList(bin_AsList, 0)));
            }
            // If three of them are odd, it's just a dead end
            else if (MathDictionary(bin_AsList, 1) == 1)
            {
                UpdateMesh(MathList(bin_AsList, 0), BinaryReConversion(MathList(bin_AsList, 0)));
            }
        }

        // Size 5 v/
        else if (bin_AsList.Count == 5)
        {
            // If all the inverted numbers are even, it's a deadend
            if (MathCheck(bin_AsList_Inverted, 0))
                // Method taken from size = 3 above
                MeshDictionary(1, 12 - bin_AsList_Inverted.AsQueryable().Sum());
            
            // If all the inverted numbers are odd
            else if (MathCheck(bin_AsList_Inverted, 1))
                // It's that weird shape, where we use the missing odd number as rotation value
                MeshDictionary(10, (MathList(bin_AsList, 2)[0] + 3) % 8);
            
            // If the list of numbers is consequtive
            else if (MathCheck(bin_AsList, 2))
            {
                // If there are more even numbers, it's the open edge shape
                if (MathDictionary(bin_AsList, 1) == 3)
                    // To find the angle, I have to invert the numbers and find the single even value
                    MeshDictionary(9, MathList(bin_AsList_Inverted, 0)[0]);
                // If there are more odd numbers, we can send back the middle three numbers
                else
                {
                    // Send back the odd number, plus 4 and the integers either side
                    int passN = (MathList(bin_AsList_Inverted, 2)[0] + 4);
                    List<int> spliceList = new List<int>{(passN + 1) % 8, passN % 8, (passN - 1) % 8};
                    spliceList.Sort();
                    UpdateMesh(spliceList, BinaryReConversion(spliceList));
                }
            }

            // If no inverted number is consequtive
            else if (MathCheck(bin_AsList_Inverted, 3))
                // We just pass back the even values
                UpdateMesh(MathList(bin_AsList, 0), BinaryReConversion(MathList(bin_AsList, 0)));

            // If one of the inverted numbers is odd, the shape is either an open corner or a bridge
            else if ((bin_AsList_Inverted.AsQueryable().Sum() % 2) != 0)
            {
                // if the two even numbers are on opposite sides, send them back, it's a bridge
                List<int> splice_Even = MathList(bin_AsList, 0);
                if (splice_Even[1] - splice_Even[0] % 8 == 4)
                    UpdateMesh(splice_Even, BinaryReConversion(splice_Even));

                // Else, send back the two even numbers, plus the odd number between them
                else
                {
                    // special case
                    if (splice_Even[0] == 0 && splice_Even[1] == 6)
                        UpdateMesh(new List<int>{0, 6, 7}, BinaryReConversion(new List<int>{0, 6, 7}));
                    else
                        UpdateMesh(new List<int>{splice_Even[0], splice_Even[0] + 1, splice_Even[0] + 2}, BinaryReConversion(new List<int>{splice_Even[0], splice_Even[0] + 1, splice_Even[0] + 2}));
                }
            }

            // For the remaining cases
            else
            {
                // Grab the inverted odd numbers
                List<int> spliceList = MathList(bin_AsList_Inverted, 2);
                // The even number in the inverted slots are next to the odd number we want
                spliceList = MathList(bin_AsList_Inverted, 0);
                // Remove the odd number either side from the main list
                if (bin_AsList.Contains((spliceList[0] + 1) % 8))
                    bin_AsList.Remove((spliceList[0] + 1) % 8);
                else
                    bin_AsList.Remove((spliceList[0] + 7) % 8);
                // Then send back the new list
                UpdateMesh(bin_AsList, BinaryReConversion(bin_AsList));
            }
        }

        // Size 6 v/
        else if (bin_AsList.Count == 6)
        {
            // When the inverted numbers are both even
            if (MathCheck(bin_AsList_Inverted, 0))
            {
                // If they're opposite each other
                List<int> spliceList = MathList(bin_AsList_Inverted, 0);
                if ((spliceList[1] - spliceList[0]) == 4)
                    //send back the two other even numbers
                    UpdateMesh(MathList(bin_AsList, 0), BinaryReConversion(MathList(bin_AsList, 0)));
                else
                {
                    // Special case
                    if ((spliceList[1] == 4) && (spliceList[0] == 2))
                    {
                        spliceList = new List<int>{0, 6, 7};
                        UpdateMesh(spliceList, BinaryReConversion(spliceList));
                    }
                    else
                    {
                        // Send back the two even numbers and the odd number between them
                        spliceList = MathList(bin_AsList, 0);
                        List<int> spliceList_2 = new List<int>{spliceList[0], spliceList[0] + 1, spliceList[1]};
                        UpdateMesh(spliceList_2, BinaryReConversion(spliceList_2));
                    }
                }
            }

            // If both inverted numbers are odd
            else if (MathCheck(bin_AsList_Inverted, 1))
            {
                // If the odd numbers are opposite each other
                List<int> spliceList = MathList(bin_AsList_Inverted, 2);
                if ((spliceList[1] - spliceList[0]) == 4)
                    // This mesh is two opposing corners. We can grab the angle by using the lowest odd number
                    MeshDictionary(12, spliceList[0] - 1);
                else
                {
                    // Special Case
                    if ((spliceList[1] == 7) && (spliceList[0] == 1))
                        MeshDictionary(11, 0);
                    else
                        // This mesh is also a new shape that I can't describe
                        // Grab the angle by finding the midpoint between the two odd numbers
                        MeshDictionary(11, (spliceList.AsQueryable().Sum())/2);
                }
            }

            // Final Cases actually all overlap
            else
            {
                // Grab the inverted even number and remove the odd number next to it
                List<int> spliceList = MathList(bin_AsList_Inverted, 0);
                if (bin_AsList.Contains((spliceList[0] + 1) % 8))
                    bin_AsList.Remove((spliceList[0] + 1) % 8);
                if (bin_AsList.Contains((spliceList[0] + 7) % 8))
                    bin_AsList.Remove((spliceList[0] + 7) % 8);
                // Then send back those remaining numbers
                UpdateMesh(bin_AsList, BinaryReConversion(bin_AsList));
            }
        }

        // Size 7 v/
        else if (bin_AsList.Count == 7)
        {
            // If the inverted list is even
            if (bin_AsList_Inverted[0] % 2 == 0)
            {
                // Remove the two odd numbers beside it
                bin_AsList.Remove((bin_AsList_Inverted[0] + 1) % 8);
                bin_AsList.Remove((bin_AsList_Inverted[0] + 7) % 8);
                // Then send back those remaining numbers
                UpdateMesh(bin_AsList, BinaryReConversion(bin_AsList));
            }
            // If the inverted list is odd, you can use the odd number - 1 to get the orientation
            else
                MeshDictionary(13, (bin_AsList_Inverted[0] + 7) % 8);
        }

        // Size 8 v/
        else if (bin_AsList.Count == 8)
        {
            // There's only one shape possible
            MeshDictionary(14);
        }
    }

    void MeshDictionary(int n, int m = 0)
    {
        ref_MeshFilter.mesh = ref_MeshFilter_Options[n];

        // Unlike the floor, instead of changing the normals of this material, I'll switch between several different
        // That can be set through a slider
        int rand = Random.Range(0, 5);
        ref_MR.material = ref_Material_Sparsematerial[leafSparseness*5 + rand];

        this.transform.rotation = Quaternion.Euler(-90f, 45f*m, 0f);
    }

    int MathDictionary(List<int> listN, int caseN)
    {
        switch (caseN)
        {
            // Return the even number of two numbers
            case 0:
                if (listN[0] % 2 == 0)
                    return listN[0];
                else
                    return listN[1];
            // How many even numbers?
            case 1:
                int totalEven = 0;
                foreach(int j in listN)
                {  
                    if ((j % 2) == 0)
                        totalEven += 1;
                }
                return totalEven;
        }

        return -1;
    }

    bool MathCheck(List<int> listN, int caseN)
    {
        switch (caseN)
        {
            // Check if all numbers are even
            case 0:
                foreach(int j in listN)
                {
                    if ((j % 2) != 0)
                        return false;
                }
                return true;
            // Check if all numbers are odd
            case 1:
                foreach(int j in listN)
                {
                    if ((j % 2) == 0)
                        return false;
                }
                return true;
            // If a list of numbers is consequtive (Including looping)
            case 2:
                for (int j = 0; j < 8; j++)
                {
                    // Create a list of consequtive numbers
                    List<int> spliceList = Enumerable.Range(j, listN.Count).ToList();
                    // Make sure % 8
                    List<int> spliceList_2 = spliceList.Select(i => (i % 8)).ToList();
                    // Sort them
                    spliceList_2.Sort();
                    // Check each number matches
                    int totalCheck = 0;
                    for (int k = 0; k < listN.Count; k++)
                    {
                        if (spliceList_2[k] == listN[k])
                            totalCheck += 1;
                    }
                    if (totalCheck == listN.Count)
                        return true;
                }
                // If none of these lists match, the numbers aren't consequtive
                return false;
            
            // If a list of nunmbers are not next to each other
            case 3:
                int checkN = 0;
                for (int j = 0; j < listN.Count(); j++)
                {
                    checkN = Mathf.Abs(listN[j] - listN[(j + 1) % listN.Count]);
                    if ((checkN == 1) || (checkN == 7))
                        return false;
                }
                return true;
        }

        return false;
    }

    // Splice a list from another list, based on parameters
    List<int> MathList(List<int> listN, int caseN)
    {
        switch (caseN)
        {
            // Return a list of even numbers only
            case 0:
                List<int> pass_ListN = new List<int>();
                foreach (int j in listN)
                {
                    // If even, add it to list
                    if ((j % 2) == 0)
                        pass_ListN.Add(j);
                }
                // Then return the list
                return pass_ListN;
            // Invert list
            case 1:
                List<int> pass_ListN_2 = new List<int>{0, 1, 2, 3, 4, 5, 6, 7};
                foreach (int j in listN)
                    pass_ListN_2.Remove(j);
                return pass_ListN_2;
            // Return a list of odd numbers only
            case 2:
                List<int> pass_ListN_3 = new List<int>();
                foreach (int j in listN)
                {
                    // If odd, add it to list
                    if ((j % 2) != 0)
                        pass_ListN_3.Add(j);
                }
                // Then return the list
                return pass_ListN_3;
        }

        return new List<int>{0};
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

        bin_AsList.Reverse();

        return bin_AsList;
    }

    // Convert a list in a binary number
    int BinaryReConversion(List<int> pass_Bin_AsList)
    {
        int pass_bin = 0;

        foreach (int j in pass_Bin_AsList)
        {
            pass_bin += (int)Mathf.Pow(2, j);
        }

        return pass_bin;
    }
}
