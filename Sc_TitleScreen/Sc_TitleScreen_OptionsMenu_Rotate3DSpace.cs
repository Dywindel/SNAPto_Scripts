using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_TitleScreen_OptionsMenu_Rotate3DSpace : MonoBehaviour
{
    
    Transform ref_Trans;
    float curTime = 0.0f;
    Vector3 randomRotationSpeed = new Vector3(0, 0, 0);
    Vector3 randomRotation = new Vector3(0, 0, 0);
    Vector3 randomRotation_Offset = new Vector3(0, 0, 0);

    float maxRotationSpeed = 5;

    // Start is called before the first frame update
    void Start()
    {
        ref_Trans = this.gameObject.transform;

        // Randomise rotation speed
        randomRotationSpeed = new Vector3(  Random.Range(-maxRotationSpeed, maxRotationSpeed),
                                            Random.Range(-maxRotationSpeed, maxRotationSpeed),
                                            Random.Range(-maxRotationSpeed, maxRotationSpeed));

        // Get the shape's initial rotation position
        randomRotation_Offset = new Vector3(ref_Trans.eulerAngles.x, ref_Trans.eulerAngles.y, ref_Trans.eulerAngles.z);
    }

    // Update is called once per frame
    void Update()
    {
        curTime += Time.deltaTime % 360;

        // Rotate object by random value in 3D space
        randomRotation = new Vector3(randomRotationSpeed[0]*curTime, randomRotationSpeed[1]*curTime, randomRotationSpeed[2]*curTime);
        ref_Trans.rotation = Quaternion.Euler(randomRotation + randomRotation_Offset);
    }
}
