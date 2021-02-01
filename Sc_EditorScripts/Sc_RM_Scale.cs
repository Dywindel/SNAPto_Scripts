using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////
//	Sc_RM_Scales - Scales mesh randomly

public class Sc_RM_Scale : MonoBehaviour
{
    //public float scale_UpperLimit = 0.5f;
    //public float scale_LowerLimit = 0.25f;

    public bool scaleZ = false;

    public void Button_RandomiseScale_Small()
    {
        float passScale = Random.Range(0.25f, 0.5f);

        if (!scaleZ)
        {
            //Scales the mesh randomly
            transform.localScale = new Vector3(passScale, passScale, passScale);
        }
        else
        {
            //Scales the mesh randomly only in the Z axis
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, passScale);
        }
    }

    public void Button_RandomiseScale_Large()
    {
        float passScale = Random.Range(0.5f, 0.75f);
        if (!scaleZ)
        {
            //Scales the mesh randomly
            transform.localScale = new Vector3(passScale, passScale, passScale);
        }
        else
        {
            //Scales the mesh randomly only in the Z axis
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, passScale);
        }
    }

    /*
    void Update()
    {
        if (Random.Range(0, 100000) == 0)
        {
            float passScale = Random.Range(scale_LowerLimit, scale_UpperLimit);
            transform.localScale = new Vector3(passScale, passScale, passScale);
        }
    }*/
}
