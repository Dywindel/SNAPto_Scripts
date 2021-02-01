using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Just rotate the in-game camera slowly in a cirle, showing off some of the game's areas and maybe a few puzzles.
// Also relates back to rotation

public class Sc_TitleScreen_RotatingCamera : MonoBehaviour
{
    
    private Sc_RB_Clock rbC;
    private Transform ref_MC;
    public Vector3 cameraAngle_Offset = new Vector3(50, 0, 0);
    Vector3 cameraAngle_Rotate = new Vector3(0, 0, 0);
    public Vector3 cameraPosition_Offset = new Vector3(0, 10, 0);
    Vector3 cameraPosition_Circle = new Vector3(0, 0, 0);
    public float cameraRotationSpeed = 1;
    public float circleRadius = 5.0f;
    float curTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        // World Clock
        rbC = GameObject.FindGameObjectWithTag("RB").GetComponent<Sc_RB_Clock>();

        // Grab the main camera's tranform
        ref_MC = this.transform;
    }

    void LateUpdate()
    {
        curTime += Time.deltaTime % 360;

        // Cycle 360 the y axis of the main camera value
        cameraAngle_Rotate = new Vector3(cameraAngle_Rotate[0], cameraRotationSpeed*curTime, cameraAngle_Rotate[2]);
        ref_MC.rotation = Quaternion.Euler(cameraAngle_Rotate + cameraAngle_Offset);

        // Cycle the position of x and z around in a circle
        cameraPosition_Circle = new Vector3(-circleRadius*Mathf.Sin(Mathf.Deg2Rad*cameraRotationSpeed*curTime),
                                            cameraPosition_Circle[1],
                                            -circleRadius*Mathf.Cos(Mathf.Deg2Rad*cameraRotationSpeed*curTime));
        ref_MC.position = cameraPosition_Circle + cameraPosition_Offset;

    }
}
