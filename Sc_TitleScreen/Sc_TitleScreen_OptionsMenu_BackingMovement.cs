using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script makes the backing objects for the options menu move in a spirograph fashion

public class Sc_TitleScreen_OptionsMenu_BackingMovement : MonoBehaviour
{
    Transform Ref_Trans;
    Vector3 positionCircle = new Vector3(0, 0, 0);
    Vector3 positionRotation = new Vector3(0, 0, 0);
    Vector3 positionRotation_Offset = new Vector3(0, 0, 0);
    float rotationSpeed = 1f;
    float movementSpeed = 0.5f;
    float innerSwingSpeed = 1.9f;
    float radiusMax = 5f;

    float curTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        Ref_Trans = this.gameObject.transform;

        // Grab the intial offset
        positionRotation_Offset = new Vector3(Ref_Trans.eulerAngles.x, Ref_Trans.eulerAngles.y, Ref_Trans.eulerAngles.z);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        curTime += Time.deltaTime % 360;

        // First calculate rotation
        positionRotation = new Vector3(positionRotation[0], rotationSpeed*curTime, positionRotation[2]);
        Ref_Trans.rotation = Quaternion.Euler(positionRotation + positionRotation_Offset);

        float radius = radiusMax*(Mathf.Sin(Mathf.Deg2Rad*innerSwingSpeed*curTime) + 1f)/2f;
        // Calculate the position of the shape on the radius of a circle
        positionCircle = new Vector3(   radius*Mathf.Sin(Mathf.Deg2Rad*movementSpeed*curTime),
                                        Ref_Trans.position.y,
                                        radius*Mathf.Cos(Mathf.Deg2Rad*movementSpeed*curTime));
        Ref_Trans.position = positionCircle;
    }
}
