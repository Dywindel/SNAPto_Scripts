using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////
//  This scripts lights up the emission
//  Of a material when the player stands on it
//  Then fades away afterwards

public class Sc_LightUpBox : MonoBehaviour
{
    private float speed_LightBrightening = 0.1f;
    private float speed_LightOn = 1.0f;
    private float speed_LightDimming = 2.0f;
    private float lightTimer = 0.0f;

    // For colour options
    private float emissionBright_Max = 0.5f;

    private bool isPlayerPresent = false;

    // Reference to light box object
    Material ref_Mat;
    // Reference to temperary colour
    Color temp_Color;

    // 0 - Off, 1 - Brightening, 2 - On, 3 - Dimming
    private int lightState = 0;

    void Start()
    {
        ref_Mat = this.gameObject.GetComponent<MeshRenderer>().material;
    }

    // Lights up the box
    public void LightOn()
    {
        // The player is here
        isPlayerPresent = true;
        // The box is lighting up, switch on
        lightState = 1;
        // Start a timer
        lightTimer = 0;
    }
    public void LightOff()
    {
        // The player is no longer here
        isPlayerPresent = false;
        // The box will stay lit for a while, then dim slowly
        lightState = 2;
        // Reset the timer
        lightTimer = 0;
    }

    void Update()
    {
        if (lightState == 1)
        {
            // Increase the lightTimer until it reaches the time limit, at which point the light will stay on
            lightTimer += Time.deltaTime;

            if (lightTimer >= speed_LightBrightening)
            {
                lightState = 2;
            }
            else
            {
                // Also, increase the brightness of the light
                temp_Color = new Color(emissionBright_Max*lightTimer/speed_LightBrightening,
                                        emissionBright_Max*lightTimer/speed_LightBrightening,
                                        emissionBright_Max*lightTimer/speed_LightBrightening);
                ref_Mat.SetColor("_EmissionColor", temp_Color);
            }
        }
        else if(lightState == 2)
        {
            // Wait until the player leaves the box
            if (!isPlayerPresent)
            {
                // Count up until the timer reaches the next milestone, don't change the material
                lightTimer += Time.deltaTime;
                if (lightTimer >= speed_LightOn)
                {
                    // Enter the dimming stage
                    lightState = 3;
                    lightTimer = speed_LightDimming;
                }
            }
        }
        else if(lightState == 3)
        {
            // Time the light to dim
            lightTimer -= Time.deltaTime;

            if (lightTimer <= 0)
            {
                // Return to the off state
                lightState = 0;
            }
            else
            {
                // Also, increase the brightness of the light
                temp_Color = new Color(emissionBright_Max*lightTimer/speed_LightDimming,
                                       emissionBright_Max*lightTimer/speed_LightDimming,
                                       emissionBright_Max*lightTimer/speed_LightDimming);
                ref_Mat.SetColor("_EmissionColor", temp_Color);
            }

           
        }
    }
}
