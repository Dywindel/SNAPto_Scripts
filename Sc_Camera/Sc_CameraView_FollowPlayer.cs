using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_CameraView_FollowPlayer : MonoBehaviour
{
   private GameObject player;

   void Start()
   {
       Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
       player = gM.player.gameObject;
   }

   void Update ()
	{
        transform.position = Vector3.Lerp(transform.position, player.transform.position, Time.deltaTime * 2.0f);
    }
}
