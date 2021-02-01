using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////
//  Sc_Menu_Action_Par
//  This script is the action parent for all
//  Menu related actions. These actions could be
//  A button, carousel, etc etc.

public class Sc_Menu_Action_Par : MonoBehaviour
{
    protected Sc_AC aC;
    protected Sc_RB_Animation rBA;
    protected Sc_MenuController_Basic menuController;
    [HideInInspector]
    public Animator anim;
    public int thisIndex;

    public void Start() 
    {
        aC = GameObject.FindGameObjectWithTag("AC").GetComponent<Sc_AC>();
        rBA = GameObject.FindGameObjectWithTag("RB").GetComponent<Sc_RB_Animation>();
        // Grab the menu button controller
        menuController = this.gameObject.transform.parent.GetComponent<Sc_MenuController_Basic>();
        // Add yourself to the list
        menuController.listOf_ChildActions.Add(this);
        anim = GetComponent<Animator>();

        // Pass the start function for any particularly special items
        Pass_Start();
    }

    // When the menu first loads into the world
    public virtual void Pass_Start()
    {
        // Do Nothing
    }

    // When the menu is selected again
    public virtual void Reset_State()
    {
        // Do Nothing
    }

    public virtual void pass_Update()
    {
        if (menuController.index == thisIndex) {
            anim.SetBool ("Selected", true);
            Pass_Input();
        } else {
            anim.SetBool ("Selected", false);
        }
    }

    public virtual void Pass_Input()
    {
        // Do Nothing
    }
}
