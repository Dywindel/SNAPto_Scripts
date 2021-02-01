using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Sc_Menu_Action_Carousel_MovementSpeed : Sc_Menu_Action_Carousel
{
    public string ref_MovementSpeed;

    public override void Pass_Start()
    {
        //This doesn't fix the issue, but I'm too lazy to do anything about it atm.
        index = start_Index;
        storeIndex = index;

        SetMovementSpeed(index);

        ref_Text.SetText(listOf_Carousel_Items[index]);
    }

    public override void TakeAction()
    {
        //Play sound here

        //Set Volume
        SetMovementSpeed(index);

        anim.SetBool ("Pressed", false);
        anim.SetBool ("Selected", false);
    }

    void SetMovementSpeed(int pass_Index)
    {
        float pass_Index_AsFloat = (((float)pass_Index)*0.25f) + 0.75f;

        // Set the movement speed here
        rBA.tp_Factor = pass_Index_AsFloat;
    }
}
