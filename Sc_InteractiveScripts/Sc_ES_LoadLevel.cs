using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sc_ES_LoadLevel : MonoBehaviour
{
    // World references
	private Sc_RB_Values rbV;			//Reference to the values list
    
    void Start()
    {
		Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
		rbV = gM.rbV;
    }

    void OnTriggerEnter(Collider hit)
    {
        GameObject item = hit.gameObject;
        print (item);
        if (item.gameObject.layer == rbV.layer_Player)
        {
            SceneManager.LoadScene("Test_ArtStyles");
        }
    }
}
