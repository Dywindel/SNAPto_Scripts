using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is going to swing the bell back and forth and around in a circle, ish

public class Sc_AnimationObject_BellSwinging : MonoBehaviour
{
    private Transform ref_Trans;

    private float time = 0.0f;
    private float[] AngleVals = {0.0f, 0.0f, 0.0f};
    public float[] OffsetAngles = {0f, 0f, 0f};
    public float maxAngle = 10f;
    public float rotationSpeed = 10f;
    private float [] timeFactor = {1f, 1f, 1f};

    // Start is called before the first frame update
    void Start()
    {
        ref_Trans = this.transform;

        // Inject some randomness so the bell doesn't always do the same thing
        timeFactor[1] = Random.Range(10.1f, 10.9f);
        timeFactor[2] = Random.Range(1.1f, 1.4f);
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        // I want to sinusoidally move through the x and z rotation (Or x and y if there's some dicontinuity)

        // Set new x value
        AngleVals[0] = maxAngle*Mathf.Sin(time);
        AngleVals[2] = maxAngle*Mathf.Sin(time/timeFactor[2]);

        // Y spinning
        if (rotationSpeed > 0)
            AngleVals[1] += Mathf.Sin(time/timeFactor[1])/rotationSpeed;
        else
            AngleVals[1] += 0;

        ref_Trans.rotation = Quaternion.Euler(AngleVals[0] + OffsetAngles[0], AngleVals[1] + OffsetAngles[1], AngleVals[2] + OffsetAngles[2]);

    }
}
