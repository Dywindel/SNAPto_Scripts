using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This warping script won't work because the player Mi move the player along in increments

public class Sc_ES_WarpPlayerIllusion : MonoBehaviour
{
    //Reference to every moving object in the scene
	private Sc_Player player;
    private Camera cam;


    //Variables
    float setDist = 35;
    public bool warpLeft = true;
        
    void Start()
    {
        Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
        player = gM.player;
        cam = Camera.main;
    }

    public void WarpIllusion()
    {
        //Stop its movement coroutine
        StopCoroutine(player.ref_Coroutine);

        //If the player is inside the trigger, warp them by an exact ammount
        //along with the camera component
        float dist;
        if (warpLeft)
            dist = -setDist;
        else
            dist = setDist;
        player.gameObject.transform.position = new Vector3( Mathf.Round(player.gameObject.transform.position.x) + dist,
                                                            (player.gameObject.transform.position.y),
                                                            player.gameObject.transform.position.z);
        cam.transform.position = cam.transform.position + new Vector3(dist, 0, 0);
    }
}
