using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////
//	Sc_RM_Test - Randomizes a textbox

public class Sc_RM_Text : MonoBehaviour
{
    //Grab the text script so we can change what is written in the box on startup
    private Sc_ES_MessagePrompt ref_DispText_Script;

    //The text strings to be read out
    [TextArea]
    public string[] messages;

    void Start()
    {
        //Grab the text script
        ref_DispText_Script = GetComponent<Sc_ES_MessagePrompt>();
        ref_DispText_Script.message = messages[Random.Range(0, messages.Length)];
    }

    /*
    void Update()
    {
        if (Random.Range(0, 100000) == 0)
        {
            float passScale = Random.Range(scale_LowerLimit, scale_UpperLimit);
            transform.localScale = new Vector3(passScale, passScale, passScale);
        }
    }*/
}
