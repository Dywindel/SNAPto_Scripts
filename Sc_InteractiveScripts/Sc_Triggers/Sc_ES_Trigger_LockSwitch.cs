using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_ES_Trigger_LockSwitch
//	When a player (Or other specific object) enters the bounding box of this
//  script. It's boolean is set true forever and cannot be unset

public class Sc_ES_Trigger_LockSwitch : MonoBehaviour
{
    //World references
	private Sc_RB_Values rbV;			//Reference to the values list

    [HideInInspector]
    public bool stayActive = false;

    public UnityEvent e_ActiveLocked;

    void Start()
    {
		Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
		rbV = gM.rbV;
    }

    //When the player enters the trigger box the first time, we can activate the lockswitch
    //And a boolean which keeps it active
    void OnTriggerEnter(Collider hit)
    {
        if (!stayActive)
        {
            GameObject item = hit.gameObject;
            if (item.gameObject.layer == rbV.layer_Player)
            {
                e_ActiveLocked.Invoke();
                stayActive = true;
            }
        }
    }
}
