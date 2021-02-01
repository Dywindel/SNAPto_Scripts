using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////
//	Sc_RM_Transform - Transforms mesh randomly

public class Sc_RM_Position : MonoBehaviour
{
    public float transformLimit = 0.45f;

    // Just effects up and down positioning
    public bool heightOnly = false;

    // Make sure we keep the start position
    bool doOnce = false;

    //Store base position
    Vector3 basePosition;

    public void Button_RandomisePosition()
    {
        if (!doOnce)
        {
            basePosition = transform.position;
            doOnce = true;
        }

        if (!heightOnly)
        {
            float passValueX = Random.Range(-transformLimit, transformLimit);
            float passValueZ = Random.Range(-transformLimit, transformLimit);
            //Move the mesh randomly
            transform.position = basePosition + new Vector3(passValueX, 0, passValueZ);
        }
        else
        {
            float passValuey = Random.Range(-transformLimit, transformLimit);
            //Move the mesh randomly
            transform.position = basePosition + new Vector3(0f, passValuey, 0f);
        }
    }

    /*
    void Update()
    {
        if (Random.Range(0, 100000) == 0)
        {
            float passValueX = Random.Range(-transformLimit, transformLimit);
            float passValueZ = Random.Range(-transformLimit, transformLimit);
            transform.position = basePosition + new Vector3(passValueX, 0, passValueZ);
        }
    }*/
}
