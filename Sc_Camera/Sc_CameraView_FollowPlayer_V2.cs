using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_CameraView_FollowPlayer_V2 : MonoBehaviour
{
   private GameObject player;

   private Vector3 lookAtMe_FocalPoint;
   private Vector3 lookAtMe_RightStick;

   //Distance of movement
   private float radius = 3.0f;

   void Start()
   {
       Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
       player = gM.player.gameObject;
   }

   void Update ()
	{
        //RightStick Movement
        lookAtMe_RightStick = new Vector3(  radius*Input.GetAxis("RightStickX"),
                                            0.0f,
                                            radius*Input.GetAxis("RightStickY"));

        //Adding up all effects
        lookAtMe_FocalPoint = player.transform.position + lookAtMe_RightStick;

        transform.position = Vector3.Lerp(transform.position, lookAtMe_FocalPoint, Time.deltaTime * 2.0f);
    }
}
