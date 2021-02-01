using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////////////////////
//
//  Sc_ES_FallingBlocks - Falling Blocks Script
//  This script causes a game object and everything attached to
//  it to fall a fixed amount and them dissapear, following a 
//  falling curve inside the rBA

public class Sc_ES_FallingBlocks : MonoBehaviour
{
    // World references
    private Sc_GM gM;
    private Sc_AC aC;
    private Sc_RB_Animation rbA;

    // Reference to the game object that's falling
    public GameObject ref_FallingItems;
    public GameObject ref_InvisibleWalls; // As the object have no invisible walls themselves. This will be switched off after falling
    public int fallDistance;
    public float activationChance = 0.5f; // Chance of activating fall
    
    private bool isFallen = false;

    // Can attach a particle system if wanted
    public ParticleSystem ref_PS;

    // Start is called before the first frame update
    void Start()
    {
        gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
        aC = gM.aC;
        rbA = gM.rbA;
    }

    // Perform the fall script
    public void Action_Fall()
    {
        if (!isFallen)
        {
            // Falling doesn't occur after good ending
            if (!gM.goodEnding_Saved)
            {
                // Chance of not activating it
                if (Random.Range(0.0f, 1.0f) <= activationChance)
                {
                    // The piece has fallen
                    isFallen = true;

                    // Turn off the invisible walls
                    ref_InvisibleWalls.SetActive(false);

                    // Play SFX
                    aC.Play_SFX("SFX_RockFall");

                    // Rubble and dust effect
                    if (ref_PS != null)
                        ref_PS.Stop();
                        ref_PS.Play();

                    // Run animation
                    StartCoroutine(rbA.Rocks_Fall_GradualSpeedUp(ref_FallingItems.transform, fallDistance));
                }
            }
        }
    }
}
