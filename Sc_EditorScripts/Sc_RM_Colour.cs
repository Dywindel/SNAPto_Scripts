using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////
//	Sc_RM_Transform - Transforms mesh randomly

public class Sc_RM_Colour : MonoBehaviour
{
    public Material[] ref_Materials;
    public void Button_RandomiseColour()
    {
        {
            MeshRenderer ref_MR = this.GetComponent<MeshRenderer>();
            ref_MR.material = ref_Materials[Random.Range(0, ref_Materials.Length)];
        }
    }
}
