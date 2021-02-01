using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//////////////////////////////////////////////////
//  Sc_Menu_Action_Button
//  This menu action is just a simple button. When
//  Pressed, a Unity event is performed

public class Sc_Menu_Action_Button : Sc_Menu_Action_Par
{
    public UnityEvent takeAction;

    public override void Pass_Input() {
        if (Input.GetButtonDown("Submit")) {
            anim.SetBool ("Pressed", true);
            TakeAction();
        } else if (anim.GetBool ("Pressed")) {
            anim.SetBool ("Pressed", false);
            //Play audio once
            //animFunc.disableOnce = true;
        }
    }

    void TakeAction()
    {
        //Play sound here

        //Perform button press
        if (takeAction != null)
        {
            takeAction.Invoke();
        }

        anim.SetBool ("Pressed", false);
        anim.SetBool ("Selected", false);
    }
}
