using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_DayNightCycle_DirectionLight : MonoBehaviour
{
	// References to World Objects
	protected GameObject gM_GameObject;		// Reference to the GM game object
	protected Sc_GM gM;					// Reference to the GM
    Sc_LD lD;                           //Reference to the lD
    Sc_RB_Clock rbC;                   // Reference to the World Clock

    Light ref_DirLight;                  // Reference to the directional light

    // 0 - For the current mode, 1 - For the previous mode, 2 - For the mixed/transition mode
    Color[] temp_ColorLerp = {Color.black, Color.black, Color.black};

    // Shadow values
    private float shadowStrengthMax = 0.43f;
    private float shadowStrengthMin = 0.43f;
    public float sunlightShadowSpeed = 1.0f;

    void Start()
    {
        // Grab the Game Manager, dictionary, movement library...
        lD = GameObject.FindGameObjectWithTag("LD").GetComponent<Sc_LD>();
        rbC = GameObject.FindGameObjectWithTag("RB").GetComponent<Sc_RB_Clock>();

        // Grab the directional light
        ref_DirLight = this.gameObject.GetComponent<Light>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // If I'm testing lighting setups, I can ignore the script below and just set the current colour to the test colour
        if (lD.isTestingColors)
        {
            ref_DirLight.color = lD.testing_DL_ColourSchemes;
        }
        else if (lD.isDemandingColours)
        {
            ref_DirLight.color = lD.DL_DemandColour;
        }
        else
        {
            // For normal operation, just lerp between different times of day
            if (!lD.isTransitioning)
            {
                if (lD.alwaysDay)
                    ref_DirLight.color = lD.cL_colorMatrix[lD.current_LightMode][0];
                else
                {
                    LightMode_ColourLerp_V2(lD.dL_colorMatrix[lD.current_LightMode], 0, (float)rbC.cT_Hr_F_Norm);
                    ref_DirLight.color = temp_ColorLerp[0];
                }
            }
            // During transitioning lighting
            else
            {
                // Here, we find two values of the colorLerp and lerp between them
                // Using the current value of the stopwatch and the stopwatch time length
                LightMode_ColourLerp_V2(lD.dL_colorMatrix[lD.current_LightMode], 0, (float)rbC.cT_Hr_F_Norm);
                LightMode_ColourLerp_V2(lD.dL_colorMatrix[lD.previous_LightMode], 1, (float)rbC.cT_Hr_F_Norm);

                // Now, lerp between those two
                temp_ColorLerp[2] = Color.Lerp(temp_ColorLerp[0], temp_ColorLerp[1], lD.lightMode_TransitionTimer/lD.lightMode_TransitionLength);

                // Update the lighting color
                ref_DirLight.color = temp_ColorLerp[2];
            }

            // For rotation of the in-game shadows
            // For the Y-axis, the rotation is just one revolution per day
            ref_DirLight.transform.rotation = Quaternion.Euler(ref_DirLight.transform.rotation.eulerAngles.x, 360.0f*(float)rbC.cT_Hr_F_Norm + 22.5f,  ref_DirLight.transform.rotation.eulerAngles.z);
            // For the X-axis the length of shadows is sinusoidal, getting shorter during midday/midnight and longer during dawn/dusk
            ref_DirLight.transform.rotation = Quaternion.Euler(((rbC.sunHeight_Max - rbC.sunHeight_Min)/2.0f)*Mathf.Sin((float)rbC.cT_Hr_F_Norm*2*Mathf.PI) + (rbC.sunHeight_Max - (((rbC.sunHeight_Max - rbC.sunHeight_Min)/2.0f))),
                                                                ref_DirLight.transform.rotation.eulerAngles.y,  ref_DirLight.transform.rotation.eulerAngles.z);

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
        // Check if we're in daytime mode
        if (lD.alwaysDay)
            temp_ColorLerp[pass_int] = pass_ColorArray[0];
        //Daytime
        else if (pass_CurTime_Day >= 0.0f && pass_CurTime_Day < rbC.dayLength_Day)
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
}
