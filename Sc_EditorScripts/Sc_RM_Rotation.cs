using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////
//	Sc_RM_Rotation - Rotates mesh randomly

public class Sc_RM_Rotation : MonoBehaviour
{
    public bool bool_QuarterTurn = true;
    public bool bool_AnalogueTurn = false;
    public bool bool_SlightTilt = false;    // Creates a slight tilt in a tile or shape to add variation

    public void Button_RandomiseRotation()
    {
        //Rotate the transform randomly
        if (bool_QuarterTurn)
        {
            transform.rotation = Quaternion.Euler(-90f, 0f, 90*Random.Range(0, 4));
        }
        else if (bool_AnalogueTurn)
        {
            transform.rotation = Quaternion.Euler(-90f, 0f, 1*Random.Range(0, 360));
        }
        else if (bool_SlightTilt)
        {
            transform.rotation = Quaternion.Euler(-90f, Random.Range(-5, 5), Random.Range(-5, 5));
        }
    }

    /*
    void Update()
    {
        if (Random.Range(0, 100000) == 0)
        {
            //Rotate the transform randomly
            if (bool_QuarterTurn)
            {
                transform.rotation = Quaternion.Euler(-90f, 0f, 90*Random.Range(0, 4));
            }
            else if (bool_AnalogueTurn)
            {
                transform.rotation = Quaternion.Euler(-90f, 0f, 1*Random.Range(0, 360));
            }
        }
    }*/
}
