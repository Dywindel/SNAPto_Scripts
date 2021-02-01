using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_ES_OnTrigger_Switch
//	When a player (Or other specific object) enters the bounding box of this
//  script. It's boolean is set true. When the said object leaves
//  It's boolean is set to false

public class Sc_ES_Trigger_Switch : MonoBehaviour
{
    //World references
	private Sc_RB_Values rbV;			//Reference to the values list

    //Main Boolean
    [HideInInspector]
    public bool active = false;

    public UnityEvent e_Active;

    public UnityEvent e_Deactive;

    void Start()
    {
		Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
		rbV = gM.rbV;
    }

    //When the player enters the trigger box the first time, we can activate the level
    void OnTriggerEnter(Collider hit)
    {
        GameObject item = hit.gameObject;
        if (item.gameObject.layer == rbV.layer_Player)
        {
            if (e_Active != null)
            {
                e_Active.Invoke();
            }
        }
    }

    void OnTriggerExit(Collider hit)
    {
        GameObject item = hit.gameObject;
        if (item.gameObject.layer == rbV.layer_Player)
        {
            if (e_Deactive != null)
                e_Deactive.Invoke();
        }
    }
}
