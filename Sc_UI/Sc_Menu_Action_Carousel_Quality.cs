using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_Menu_Action_Carousel_Quality : Sc_Menu_Action_Carousel
{
    public override void Pass_Start()
    {
        index = QualitySettings.GetQualityLevel();

        ref_Text.SetText(listOf_Carousel_Items[index]);
    }

    // Change the quality settings
    public override void TakeAction()
    {
        // Set the quality value
        QualitySettings.SetQualityLevel(index, true);

        anim.SetTrigger("Pressed");
    }
}
