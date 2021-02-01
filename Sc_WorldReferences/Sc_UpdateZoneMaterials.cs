using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////
//
//  Sc_UpdateZoneMaterials
//  When given a que by the LD, this script changes the
//  material of the upper cloud layer via a particular and complex coroutine

public class Sc_UpdateZoneMaterials : MonoBehaviour
{
    Sc_LD lD;                           //Reference to the lD
    Sc_RB_Animation rbA;
    Sc_RB_Clock rbC;                   // Reference to the World Clock
    Material ref_Mat;

    // Check if we're in twirl mode or not
    [HideInInspector]
    public bool bool_inTwirlMode = false;

    // 0 - For the current mode, 1 - For the previous mode, 2 - For the mixed/transition mode
    Color[] temp_ColorLerp = {Color.black, Color.black, Color.black};

    // Start is called before the first frame update
    void Start()
    {
        // This script cannot call the GM because the GM does exist in the title scene, where this script is also called
        lD = GameObject.FindGameObjectWithTag("LD").GetComponent<Sc_LD>();
        rbA = GameObject.FindGameObjectWithTag("RB").GetComponent<Sc_RB_Animation>();
        rbC = GameObject.FindGameObjectWithTag("RB").GetComponent<Sc_RB_Clock>();

        // Send a reference of yourself to the LD
        lD.ref_UpdateZoneMaterials_Script.Add(this);

        ref_Mat = this.GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // If I'm testing lighting setups, I can ignore the script below and just set the current colour to the test colour
        if (lD.isTestingColors)
        {
            ref_Mat.SetColor ("_ColourValley", lD.testing_CL_ColourSchemes);
        }
        else
        {
            // For normal operation, just lerp between different times of day
            if (!lD.isTransitioning)
            {
                LightMode_ColourLerp_V2(lD.cL_colorMatrix[lD.current_LightMode], 0, (float)rbC.cT_Hr_F_Norm);
                ref_Mat.SetColor ("_ColourValley", temp_ColorLerp[0]);
            }
            // During transitioning lighting
            else
            {
                // Here, we find two values of the colorLerp and lerp between them
                // Using the current value of the stopwatch and the stopwatch time length
                LightMode_ColourLerp_V2(lD.cL_colorMatrix[lD.current_LightMode], 0, (float)rbC.cT_Hr_F_Norm);
                LightMode_ColourLerp_V2(lD.cL_colorMatrix[lD.previous_LightMode], 1, (float)rbC.cT_Hr_F_Norm);

                // Now, lerp between those two
                temp_ColorLerp[2] = Color.Lerp(temp_ColorLerp[0], temp_ColorLerp[1], lD.lightMode_TransitionTimer/lD.lightMode_TransitionLength);

                // Update the lighting color
                ref_Mat.SetColor ("_ColourValley", temp_ColorLerp[2]);
            }
        }
    }

    //This function takes four colours and lerps between them depending on the current time
    void LightMode_ColourLerp_V1(Color[] pass_ColorArray, int pass_int, float pass_CurTime_Day)
    {
        //Daytime
        if (pass_CurTime_Day >= 0.0f && pass_CurTime_Day < 0.4f)
        {
            //Lerp between this and the next lighting scene
            temp_ColorLerp[pass_int] = Color.Lerp(pass_ColorArray[0], pass_ColorArray[1], pass_CurTime_Day*2.5f);
        }
        //Evening
        else if(pass_CurTime_Day >= 0.4f && pass_CurTime_Day < 0.5f)
        {
            temp_ColorLerp[pass_int] = Color.Lerp(pass_ColorArray[1], pass_ColorArray[2], (pass_CurTime_Day - 0.4f)*10f);
        }
        //Night
        else if(pass_CurTime_Day >= 0.5f && pass_CurTime_Day < 0.9f)
        {
            temp_ColorLerp[pass_int] = Color.Lerp(pass_ColorArray[2], pass_ColorArray[3], (pass_CurTime_Day - 0.5f)*2.5f);
        }
        //Morning
        else if (pass_CurTime_Day >= 0.9f && pass_CurTime_Day < 1.0f)
        {
            temp_ColorLerp[pass_int] = Color.Lerp(pass_ColorArray[3], pass_ColorArray[0], (pass_CurTime_Day - 0.9f)*10f);
        }
    }

    // This colour lerp function just lerps between a day and night colour, without using any evening colours
    void LightMode_ColourLerp_V2(Color[] pass_ColorArray, int pass_int, float pass_CurTime_Day)
    {
        //Daytime
        if (pass_CurTime_Day >= 0.0f && pass_CurTime_Day < rbC.dayLength_Day)
        {
            temp_ColorLerp[pass_int] = pass_ColorArray[0];
        }
        //Evening
        else if(pass_CurTime_Day >= rbC.dayLength_Day && pass_CurTime_Day < (rbC.dayLength_Day + rbC.dayLength_Transition))
        {
            temp_ColorLerp[pass_int] = Color.Lerp(pass_ColorArray[0], pass_ColorArray[2], (pass_CurTime_Day - rbC.dayLength_Day)*(1f/rbC.dayLength_Transition));
        }
        //Night
        else if(pass_CurTime_Day >= (rbC.dayLength_Day + rbC.dayLength_Transition) && pass_CurTime_Day < (1f - rbC.dayLength_Transition))
        {
            temp_ColorLerp[pass_int] = pass_ColorArray[2];
        }
        //Morning
        else if (pass_CurTime_Day >= (1f - rbC.dayLength_Transition) && pass_CurTime_Day < 1.0f)
        {
            temp_ColorLerp[pass_int] = Color.Lerp(pass_ColorArray[2], pass_ColorArray[0], (pass_CurTime_Day - (1f - rbC.dayLength_Transition))*(1f/rbC.dayLength_Transition));
        }
    }

    // This is a vortex effect that occurs when the player gets the bad ending
    // As the central tulip is beginning to suck up all the cloud layer]
    public void Switch_TwirlMode(bool pass_Bool)
    {
        // When Twirl mode is on
        if (pass_Bool)
        {
            // If we're not already in twirl mode
            if (!bool_inTwirlMode)
            {
                bool_inTwirlMode = true;

                StartCoroutine(rbA.Clouds_ChangeFloat(ref_Mat, "_TimeScale", 0.75f));
                StartCoroutine(rbA.Clouds_ChangeFloat(ref_Mat, "_TwirlStrength", 2f));
            }

            ref_Mat.SetVector("_TwirlOffset", new Vector2(-(this.gameObject.transform.parent.position.x - lD.ref_TwirlFocus.transform.position.x), this.gameObject.transform.parent.position.z - lD.ref_TwirlFocus.transform.position.z)/16f);
        }
        else
        {
            // Switch off the effect if we are in twirl mode
            if (bool_inTwirlMode)
            {
                bool_inTwirlMode = false;

                StartCoroutine(rbA.Clouds_ChangeFloat(ref_Mat, "_TimeScale", 0.05f));
                StartCoroutine(rbA.Clouds_ChangeFloat(ref_Mat, "_TwirlStrength", 0f));
            }
        }
    }
}
