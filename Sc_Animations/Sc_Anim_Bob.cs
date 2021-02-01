using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script allows an object to bob up and down

public class Sc_Anim_NPC_Bob : MonoBehaviour
{
    // World Reference
    Sc_RB_Clock wC;

    // Reference to the object transform
    private Transform ref_Trans;
    // And their initial position
    private Vector3 ref_InitialPosition;

    // Which axis direction?
    public bool switchAxis = false;

    // Private timer
    private float i = 0.0f;
    // Animation speed
    [Range(1, 50)]
    public int bobSpeed = 1;
    // Bob Height
    [Range(0.0f, 1.0f)]
    public float bobHeight = 0.25f;

    void Start()
    {
        wC = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>().wC;
        
        // Grab the transform and its first position
        ref_Trans = this.gameObject.transform;
        ref_InitialPosition = ref_Trans.position;

        // Start with a random phase, or it's gonna look weird if they all bob together
        i = Random.Range(0.0f, 1.0f);
    }

    void Update()
    {
        // Move in a sin like pattern
        float ref_Float = (Mathf.Sin(Mathf.PI*2*i) + 1f)*(bobHeight/2f);

        // Reset i when it gets too big
        i = (i + (bobSpeed*Time.deltaTime/10f)) % 1;

        if (!switchAxis)
            ref_Trans.localPosition = ref_InitialPosition + new Vector3(0, ref_Float, 0);
        else
            ref_Trans.localPosition = ref_InitialPosition + new Vector3(0, 0, ref_Float);
    }
}
