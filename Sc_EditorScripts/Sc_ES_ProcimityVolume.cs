using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Change the volume base on plaer proximity

public class Sc_ES_ProcimityVolume : MonoBehaviour
{
    private Sc_RB_Values rbV;			//Reference to the values list
    GameObject player;      // Reference to player

    AudioSource ref_AudioSource;
    private SphereCollider ref_SphereCollider;

    private bool activelyHearing = false;
    public int triggerArea = 24;

    // Update less often
    int updateCount = 0;
    int updateEvery = 10;

    // Start is called before the first frame update
    void Start()
    {
        Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();

        // Grab the player
        player = gM.player.gameObject;

        // Grab reference values
		rbV = gM.rbV;

        // Grab the audiosource
        ref_AudioSource = this.GetComponent<AudioSource>();

        // Create the trigger box
        ref_SphereCollider = this.gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
        // And set it's size and set it to istrigger
        ref_SphereCollider.radius = triggerArea;
        ref_SphereCollider.isTrigger = true;
    }

    // When the player enters the sphere collider, start running the sound and change it's volume
    void OnTriggerEnter(Collider hit)
    {
        GameObject item = hit.gameObject;
        if (item.gameObject.layer == rbV.layer_Player)
            activelyHearing = true;
    }
    // And again for when the player leaves
    void OnTriggerExit(Collider hit)
    {
        GameObject item = hit.gameObject;
        if (item.gameObject.layer == rbV.layer_Player)
            activelyHearing = false;
    }

    void Update()
    {
        if (activelyHearing)
        {
            // Update less per frame
            if (updateCount == 0)
            {
                float pass_Volume = 1f - Mathf.Pow(Vector3.Magnitude(player.transform.position - this.transform.position)/(triggerArea + 1f), 1f / 4f);

                ref_AudioSource.volume = pass_Volume;

                updateCount = updateEvery;
            }
            else
            {
                updateCount -= 1;
            }
        }
        else
        {
            ref_AudioSource.volume = 0.0f;
        }
    }
}
