using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_LD - Lighting Director
//	Keeps track of Lighting mode for different areas and updates appropriate scripts when
//  The lighting mode changes (If the area changes)

public class Sc_LD : MonoBehaviour
{
    // World references
    Sc_GM gM;

    //  Lighting Modes - Reference folder in Scrivener

    //  Main Sections
    //  0 - Autumn Meadows
    //  1 - Storm Sanctuary
    //  2 - Winter Castle
    //  3 - Jungle
    //  4 - ???
    //  5 - ???
    //  6 - ???
    //  7 - ???
    //  8 - ???

    //  Smaller Areas
    //  9 - Rocky Outcrop

    // Special
    // 17 - Bad Ending
    // 18 - Good Ending
    // 19 - Easter Egg

    [System.NonSerialized]
    public int previous_LightMode = 1;
    [System.NonSerialized]
    public int current_LightMode = 1;

    // World references
    private Sc_DayNightCycle_DirectionLight ref_DayAndNightCycle_DirectionLight;
    private Sc_DayNightCycle_CloudLayer ref_DayAndNightCycle_CloudLayer;
    public Sc_SkyShadow ref_SkyShadow;
    [HideInInspector]
    public List<Sc_UpdateZoneMaterials> ref_UpdateZoneMaterials_Script;

    // This allows me to test colour schemes before deciding on which to use
    public bool isTestingColors = false;
    public bool isDemandingColours = false;
    public Color testing_DL_ColourSchemes = new Color(0f, 0f, 0f, 1f);
    public Color testing_CL_ColourSchemes = new Color(0f, 0f, 0f, 1f);
    public Color DL_DemandColour = new Color(0f, 0f, 0f, 1f);
    public Color CL_DemandColour = new Color(0f, 0f, 0f, 1f);

    public float testing_SkyShadow_Alpha;

    // Manually setup colour zone
    public bool isTestingColorZone = false;
    public int testColorZone = 1;

    // Twirl Effect
    public Transform ref_TwirlFocus;

    // Keep it daytime
    public bool alwaysDay = false;

    // Light colours for each mode

    /////////////////////
    // Direction Light //
    /////////////////////

    // MAIN AREAS

    private Color[] dL_0_AutMed = { new Color (1.00f, 0.98f, 0.20f, 1.0f),
                                    new Color (1.00f, 1.00f, 1.00f, 1.0f),
                                    new Color (1.00f, 0.70f, 0.30f, 1.0f),
                                    new Color (1.00f, 1.00f, 1.00f, 1.0f)};
    private Color[] dL_1_StmSan = { new Color (1.00f, 1.00f, 1.00f, 1.0f),
                                    new Color (1.00f, 1.00f, 1.00f, 1.0f),
                                    new Color (0.50f, 0.78f, 1.0f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f)};
    private Color[] dL_2_WinCas = { new Color (0.37f, 0.78f, 0.78f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f),
                                    new Color (0.00f, 0.53f, 1.00f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f)};
    private Color[] dL_3_Jungle = { new Color (0.76f, 1.00f, 0.58f, 1.0f),
                                    new Color (1.0f, 0.0f, 1.0f, 1.0f),
                                    new Color (0.00f, 1.00f, 0.27f, 1.0f),
                                    new Color (1.0f, 0.0f, 1.0f, 1.0f)};
    private Color[] dL_4_SumDes = { new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f)};
    private Color[] dL_5_WinCas = { new Color (0.54f, 0.78f, 0.79f, 1.0f),
                                    new Color (0.54f, 0.70f, 0.79f, 1.0f),
                                    new Color (0.54f, 0.63f, 0.79f, 1.0f),
                                    new Color (0.54f, 0.70f, 0.79f, 1.0f)};
    private Color[] dL_6_Desert = { new Color (1.00f, 1.00f, 0.89f, 1.0f),
                                    new Color (0.50f, 0.55f, 0.79f, 1.0f),
                                    new Color (0.00f, 0.24f, 1.00f, 1.0f),
                                    new Color (0.50f, 0.55f, 0.78f, 1.0f)};
    private Color[] dL_7_SumDes = { new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f)};
    private Color[] dL_8_ChuCas = { new Color (0.78f, 1.00f, 0.96f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.00f, 0.63f, 0.29f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f)};

    private Color[] dL_Empty    = { new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f)};
    
    // SMALLER AREAS
    private Color[] dL_9_RocOut = { new Color (0.94f, 1.00f, 0.76f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f)};

    // Special

    private Color[] dL_17_Bad    = {new Color (0.50f, 0.65f, 0.79f, 1.0f),
                                    new Color (0.50f, 0.65f, 0.79f, 1.0f),
                                    new Color (0.50f, 0.65f, 0.79f, 1.0f),
                                    new Color (0.50f, 0.65f, 0.79f, 1.0f)};
    
    private Color[] dL_18_Good   = {new Color (0.20f, 0.62f, 0.61f),
                                    new Color (0.20f, 0.62f, 0.61f),
                                    new Color (0.20f, 0.62f, 0.61f),
                                    new Color (0.20f, 0.62f, 0.61f)};


    /////////////////
    // Cloud Layer //
    /////////////////

    // MAIN AREAS

    private Color[] cL_0_AutMed = { new Color (1.00f, 0.75f, 0.24f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f),
                                    new Color (1.00f, 0.77f, 0.46f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f)};
    private Color[] cL_1_StmSan = { new Color (0.55f, 0.82f, 0.73f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f),
                                    new Color (0.57f, 0.75f, 0.69f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f)};
    private Color[] cL_2_WinCas = { new Color (0.39f, 0.76f, 0.73f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (0.06f, 0.00f, 0.22f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f)};
    private Color[] cL_3_Jungle = { new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f)};
    private Color[] cL_4_SumDes = { new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f)};
    private Color[] cL_5_WinCas = { new Color (1.00f, 1.000f, 1.00f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f)};
    private Color[] cL_6_Desert = { new Color (0.92f, 1.00f, 0.67f, 1.0f),
                                    new Color (0.68f, 0.78f, 0.76f, 1.0f),
                                    new Color (0.35f, 0.53f, 0.55f, 1.0f),
                                    new Color (0.68f, 0.78f, 0.76f, 1.0f)};
    private Color[] cL_7_WinCas = { new Color (1.00f, 1.000f, 1.00f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f)};
    private Color[] cL_8_ChuCas = { new Color (0.76f, 0.53f, 0.70f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f),
                                    new Color (0.60f, 0.60f, 0.60f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f)};

    private Color[] cL_Empty    = { new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f),
                                    new Color (1.00f, 1.000f, 1.00f, 1.0f)};
    
    // SMALLER AREAS
    private Color[] cL_9_RocOut = { new Color (0.64f, 0.92f, 0.92f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f),
                                    new Color (1.0f, 1.0f, 1.0f, 1.0f)};

    // Special

    private Color[] cL_17_Bad    = {new Color (0.86f, 0.73f, 0.98f, 1.0f),
                                    new Color (0.86f, 0.73f, 0.98f, 1.0f),
                                    new Color (0.86f, 0.73f, 0.98f, 1.0f),
                                    new Color (0.86f, 0.73f, 0.98f, 1.0f)};

    private Color[] cL_18_Good   = {new Color (0.30f, 0.65f, 0.75f),
                                    new Color (0.30f, 0.65f, 0.75f),
                                    new Color (0.30f, 0.65f, 0.75f),
                                    new Color (0.30f, 0.65f, 0.75f)};

    // For the sky shadow cast onto the ground
    // 0 - Which shadow mode, 1 - Shadow intensity

    // MAIN AREAS
    private float sh_0_AutMed = 0.50f;
    private float sh_1_StmSan = 0.10f;
    private float sh_2_WinCas = 0.0f;
    private float sh_3_Jungle = 0.0f;
    private float sh_4_WinCas = 0.0f;
    private float sh_5_WinCas = 0.0f;
    private float sh_6_Desert = 0.0f;
    private float sh_7_WinCas = 0.0f;
    private float sh_8_ChuCas = 0.0f;

    private float sh_Empty    = 0.0f;

    // SMALLER AREAS
    private float sh_9_RocOut = 0.0f;

    // Create the colorMatrix using a jagged array
    [HideInInspector]
    public Color[][] dL_colorMatrix = new Color[25][];
    [HideInInspector]
    public Color[][] cL_colorMatrix = new Color[25][];
    [System.NonSerialized]
    public float[] sh_floatMatrix = new float[25];

    // Transition time between lightmodes
    [System.NonSerialized]
    public float lightMode_TransitionLength = 10.0f;    // This is reference vairable to the length of transition time
    [System.NonSerialized]
    public float lightMode_TransitionTimer = 0.0f;      // This is the float that counts down
    [System.NonSerialized]
    public bool isTransitioning = false;
    // For storing the next lightmode
    private bool isQueueLightMode = false;
    private int queueLightMode = 0;

    void Start()
    {
        // World References
        if (GameObject.FindGameObjectWithTag("GM") != null)
            gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();

        // Grab the direction Light script, the cloud layer script and sky shadows script
        ref_DayAndNightCycle_DirectionLight = GameObject.FindGameObjectWithTag("Direction Light").GetComponent<Sc_DayNightCycle_DirectionLight>();
        ref_DayAndNightCycle_CloudLayer = GameObject.FindGameObjectWithTag("Cloud Layer").GetComponent<Sc_DayNightCycle_CloudLayer>();
        if (GameObject.FindGameObjectWithTag("Sky Shadow") != null)
        {
            ref_SkyShadow = GameObject.FindGameObjectWithTag("Sky Shadow").GetComponent<Sc_SkyShadow>();
            // Enable the object
            ref_SkyShadow.gameObject.SetActive(true);
        }

        // Compile the colorMatrix for the DL, CL and shadows
        dL_colorMatrix[0] = dL_0_AutMed;
        dL_colorMatrix[1] = dL_1_StmSan;
        dL_colorMatrix[2] = dL_2_WinCas;
        dL_colorMatrix[3] = dL_3_Jungle;
        dL_colorMatrix[4] = dL_4_SumDes;
        dL_colorMatrix[5] = dL_5_WinCas;
        dL_colorMatrix[6] = dL_6_Desert;
        dL_colorMatrix[7] = dL_7_SumDes;
        dL_colorMatrix[8] = dL_8_ChuCas;

        dL_colorMatrix[9] = dL_9_RocOut;

        dL_colorMatrix[10] = dL_Empty;
        dL_colorMatrix[11] = dL_Empty;
        dL_colorMatrix[12] = dL_Empty;
        dL_colorMatrix[13] = dL_Empty;
        dL_colorMatrix[14] = dL_Empty;
        dL_colorMatrix[15] = dL_Empty;
        dL_colorMatrix[16] = dL_Empty;
        dL_colorMatrix[17] = dL_17_Bad;
        dL_colorMatrix[18] = dL_18_Good;
        dL_colorMatrix[19] = dL_Empty;
        dL_colorMatrix[20] = dL_Empty;
        dL_colorMatrix[21] = dL_Empty;
        dL_colorMatrix[22] = dL_Empty;
        dL_colorMatrix[23] = dL_Empty;
        dL_colorMatrix[24] = dL_Empty;

        cL_colorMatrix[0] = cL_0_AutMed;
        cL_colorMatrix[1] = cL_1_StmSan;
        cL_colorMatrix[2] = cL_2_WinCas;
        cL_colorMatrix[3] = cL_3_Jungle;
        cL_colorMatrix[4] = cL_4_SumDes;
        cL_colorMatrix[5] = cL_5_WinCas;
        cL_colorMatrix[6] = cL_6_Desert;
        cL_colorMatrix[7] = cL_7_WinCas;
        cL_colorMatrix[8] = cL_8_ChuCas;

        cL_colorMatrix[9] = cL_9_RocOut;

        cL_colorMatrix[10] = cL_Empty;
        cL_colorMatrix[11] = cL_Empty;
        cL_colorMatrix[12] = cL_Empty;
        cL_colorMatrix[13] = cL_Empty;
        cL_colorMatrix[14] = cL_Empty;
        cL_colorMatrix[15] = cL_Empty;
        cL_colorMatrix[16] = cL_Empty;
        cL_colorMatrix[17] = cL_17_Bad;
        cL_colorMatrix[18] = cL_18_Good;
        cL_colorMatrix[19] = cL_Empty;
        cL_colorMatrix[20] = cL_Empty;
        cL_colorMatrix[21] = cL_Empty;
        cL_colorMatrix[22] = cL_Empty;
        cL_colorMatrix[23] = cL_Empty;
        cL_colorMatrix[24] = cL_Empty;

        sh_floatMatrix[0] = sh_0_AutMed;
        sh_floatMatrix[1] = sh_1_StmSan;
        sh_floatMatrix[2] = sh_2_WinCas;
        sh_floatMatrix[3] = sh_3_Jungle;
        sh_floatMatrix[4] = sh_4_WinCas;
        sh_floatMatrix[5] = sh_5_WinCas;
        sh_floatMatrix[6] = sh_6_Desert;
        sh_floatMatrix[7] = sh_7_WinCas;
        sh_floatMatrix[8] = sh_8_ChuCas;

        sh_floatMatrix[9] = sh_9_RocOut;

        sh_floatMatrix[10] = sh_Empty;
        sh_floatMatrix[11] = sh_Empty;
        sh_floatMatrix[12] = sh_Empty;
        sh_floatMatrix[13] = sh_Empty;
        sh_floatMatrix[14] = sh_Empty;
        sh_floatMatrix[15] = sh_Empty;
        sh_floatMatrix[16] = sh_Empty;
        sh_floatMatrix[17] = sh_Empty;
        sh_floatMatrix[18] = sh_Empty;
        sh_floatMatrix[19] = sh_Empty;
        sh_floatMatrix[20] = sh_Empty;
        sh_floatMatrix[21] = sh_Empty;
        sh_floatMatrix[22] = sh_Empty;
        sh_floatMatrix[23] = sh_Empty;
        sh_floatMatrix[24] = sh_Empty;

        // Set the LD to the initial colour for where the player is or just a basic yellow scene
        // Update_LD(0);
    }

    //  This function updates all the necessary scripts when the lighting mode changes
    public void Update_LD(int pass_LightMode)
    {
        // If we've just reached one of the two endings
        if (gM != null)
        {
            if (gM.endingState == 0 || gM.atmosphere_NormalMode)
            {
                // Do nothing
            }
            else if (gM.endingState == 1)
                pass_LightMode = 17;
            else if (gM.endingState == 2)
                pass_LightMode = 18;
        }

        // Ignore the request for lighting updates if we are already in the same lighting mode
        if (current_LightMode != pass_LightMode)
        {
            // Or if we are currently transitioning
            if (!isTransitioning)
            {
                // Update current lightmode and store previous
                previous_LightMode = current_LightMode;
                current_LightMode = pass_LightMode;

                // Update boolean and start a timer
                isTransitioning = true;
                lightMode_TransitionTimer = lightMode_TransitionLength;

                /*
                // Update the zone cloud material
                foreach (Sc_UpdateZoneMaterials temp_UpdateMAterialsScript in ref_UpdateZoneMaterials_Script)
                    StartCoroutine(temp_UpdateMAterialsScript.LerpMaterials(current_LightMode));
                */
            }
            // If we are transitioning, let's store the lightmode and we'll use it later
            else
            {
                queueLightMode = pass_LightMode;
                isQueueLightMode = true;
            }
        }
    }

    // For the transition timer
    void Update()
    {
        // While the lightMode is transitioning
        if (isTransitioning)
        {
            // Countdown the main timer with each frame
            lightMode_TransitionTimer -= Time.deltaTime;

            // If the time reaches zero or lower, stop transitioning
            if (lightMode_TransitionTimer <= 0.0)
            {
                // We've finished transitioning, but it there anything further to do?
                isTransitioning = false;

                // We need to confirm if we have another lightmode queued up
                if (isQueueLightMode)
                {
                    isQueueLightMode = false;
                    Update_LD(queueLightMode);
                }
            }
        }
        
        // For colour zone testing
        if (isTestingColorZone)
        {
            Update_LD(testColorZone);
        }
    }

    // Switch on/off the twirl mode
    public void Switch_TwirlMode(bool pass_Bool)
    {
        ref_DayAndNightCycle_CloudLayer.GetComponent<Sc_DayNightCycle_CloudLayer>().ref_CloudLayers[0].GetComponent<Sc_UpdateZoneMaterials>().Switch_TwirlMode(pass_Bool);
    }
}
