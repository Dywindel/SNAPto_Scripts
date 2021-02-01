using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_DayNightCycle_CloudLayer : MonoBehaviour
{
    // References to World Objects
	protected GameObject gM_GameObject;		// Reference to the GM game object
	protected Sc_GM gM;					// Reference to the GM
    Sc_LD lD;                           //Reference to the lD
    Sc_RB_Clock rbC;                   // Reference to the World Clock

    public GameObject[] ref_CloudLayers;  // Grab a reference to the cloud layers
    private MeshRenderer[] ref_Mat;      // Reference to each of the clouds attached to this parent
    // The way this works is I use the same colour for each cloud layer, but a slightly different Alpha value

    // 0 - For the current mode, 1 - For the previous mode, 2 - For the mixed/transition mode
    Color[] temp_ColorLerp = {Color.black, Color.black, Color.black};

    // This float value pushes the alpha amount of each cloud layer further towards opaque
    private float alphaPush = 1.0f;

    // Material scrolling speed
    public float materialScrollSpeed = 0.01f;

    void Start()
    {
        // Grab the Game Manager, dictionary, movement library...
        lD = GameObject.FindGameObjectWithTag("LD").GetComponent<Sc_LD>();
        rbC = GameObject.FindGameObjectWithTag("RB").GetComponent<Sc_RB_Clock>();

        // Enable the clouds - They're off during editing, which is easier for me
        // And grab their mesh renderers
        ref_Mat = new MeshRenderer[ref_CloudLayers.Length];
        for (int i = 0; i < ref_CloudLayers.Length; i ++)
        {
            ref_CloudLayers[i].SetActive(true);
            ref_Mat[i] = ref_CloudLayers[i].GetComponent<MeshRenderer>();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
         // If I'm testing lighting setups, I can ignore the script below and just set the current colour to the test colour
        if (lD.isTestingColors)
        {
            Update_CloudLayers(lD.testing_CL_ColourSchemes);
        }
        else if (lD.isDemandingColours)
        {
            Update_CloudLayers(lD.CL_DemandColour);
        }
        else
        {
            // For normal operation, just lerp between different times of day
            if (!lD.isTransitioning)
            {
                if (lD.alwaysDay)
                    Update_CloudLayers(lD.cL_colorMatrix[lD.current_LightMode][0]);
                else
                {
                    LightMode_ColourLerp(lD.cL_colorMatrix[lD.current_LightMode], 0, (float)rbC.cT_Hr_F_Norm);

                    Update_CloudLayers(temp_ColorLerp[0]);
                }
            }
            // During transitioning lighting
            else
            {
                // Here, we find two values of the colorLerp and lerp between them
                // Using the current value of the stopwatch and the stopwatch time length
                LightMode_ColourLerp(lD.cL_colorMatrix[lD.current_LightMode], 0, (float)rbC.cT_Hr_F_Norm);
                LightMode_ColourLerp(lD.cL_colorMatrix[lD.previous_LightMode], 1, (float)rbC.cT_Hr_F_Norm);

                // Now, lerp between those two
                temp_ColorLerp[2] = Color.Lerp(temp_ColorLerp[0], temp_ColorLerp[1], lD.lightMode_TransitionTimer/lD.lightMode_TransitionLength);

                Update_CloudLayers(temp_ColorLerp[2]);                
            }
        }
    }
    
    // This colour lerp lerps between just day and night
    void LightMode_ColourLerp(Color[] pass_ColorArray, int pass_int, float pass_CurTime_Day)
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

    // Update the colour of each cloud layer
    void Update_CloudLayers(Color pass_Temp_ColorLerp)
    {
        // Update the lighting color
        for (int i = 0; i < ref_Mat.Length; i++)
        {
            foreach (Material part_Material in ref_Mat[i].materials)
                part_Material.SetColor("_ColourValley", pass_Temp_ColorLerp);
        }
    }
}
