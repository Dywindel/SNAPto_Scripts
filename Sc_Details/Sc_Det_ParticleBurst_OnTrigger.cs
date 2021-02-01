using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_Det_ParticleBurst_OnTrigger : MonoBehaviour
{
    private ParticleSystem partSys;

    void Start()
    {
        //Grab the particle system and the boxCollider
        partSys = this.gameObject.GetComponent<ParticleSystem>();
    }

    //When the player enters the boxCollider region, play the particle effect
    private void OnTriggerEnter(Collider other)
    {
        //Any block script can create this particle effect, actually
        if (other.gameObject.GetComponent<Sc_Block>() != null)
        {
            if (Random.Range(0, 0) == 0) {
                //Play the particle effect
                partSys.Emit(10);
            }
        }
    }

    //When the player exits the boxCollider region, play the particle effect again
    private void OnTriggerExit(Collider other)
    {
        //Any block script can create this particle effect, actually
        if (other.gameObject.GetComponent<Sc_Block>() != null)
        {
            if (Random.Range(0, 0) == 0) {
                //Play the particle effect
                partSys.Emit(10);
            }
        }
    }
}
