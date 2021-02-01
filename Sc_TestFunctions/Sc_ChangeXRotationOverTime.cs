using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_ChangeXRotationOverTime : MonoBehaviour
{
    public float speed = 1.0f;

    public bool zAxis = false;

    // Update is called once per frame
    void Update()
    {
        if (zAxis)
            transform.Rotate (new Vector3(0, Time.deltaTime * speed, 0), Space.World);
        else
            transform.Rotate (new Vector3(Time.deltaTime * speed, 0, 0), Space.World);
    }
}
