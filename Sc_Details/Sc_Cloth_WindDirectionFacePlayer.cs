using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_Cloth_WindDirectionFacePlayer : MonoBehaviour
{
    // World references
    private Sc_RB_Values rbV;			//Reference to the values list

    private Cloth ref_Cloth;

    GameObject player;      // Reference to player

    public float speed = 5f;

    void Start()
    {
        Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
        player = gM.player.gameObject;

        ref_Cloth = this.GetComponent<Cloth>();

        // Grab reference values
		rbV = gM.rbV;
    }

    void Update()
    {
        // Need to normalise is too
        if (Input.GetButton("Reset"))
            ref_Cloth.externalAcceleration = -speed*Vector3.Normalize(player.transform.position - transform.position);
        else
            ref_Cloth.externalAcceleration = speed*Vector3.Normalize(player.transform.position - transform.position);
    }
}
