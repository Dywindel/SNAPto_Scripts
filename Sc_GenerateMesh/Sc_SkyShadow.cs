using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script controls the shadow terrain that will fly over the player, creating a shadow effect.
// It will, for now, move slowly from left to right
// It will move its position a great distance if it is too far away from the player

public class Sc_SkyShadow : MonoBehaviour
{
    // References to World Objects
    Sc_RB_Clock rbC;                   // Reference to the World Clock
    Sc_LD lD;                           //Reference to the lD

    private GameObject player;   // Reference to the player

    private Material ref_SkyShadow_Material;

    void Start()
    {
        // Grab the Game Manager, dictionary, movement library...
        Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
        rbC = gM.wC;
        lD = gM.lD;

        player = gM.player.gameObject;    // Find the player
        ref_SkyShadow_Material = this.gameObject.GetComponent<MeshRenderer>().material; // Grab the sky shadow material
    }

    void LateUpdate()
    {
        // Whilst testing colours, we use the given value by the editor
        if (lD.isTestingColors)
        {
            // I don't actually know what values to set or what sort of cloud shapes I want
            // So I can't really do anything with this atm
        }
        // For all other cases
        else
        {
            // I can't do this atm because I don't know what sort of sky shadows I want
        }
    }
}
