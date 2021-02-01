using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Sc_Menu_Action_Carousel_Volume : Sc_Menu_Action_Carousel
{
    public string ref_Volume;

    public AudioMixer audioMixer;

    public override void Pass_Start()
    {
        // Grab the current volume of the master
        index = GetVolumeFunction();
        storeIndex = index;

        SetVolumeFunction(index);

        ref_Text.SetText(listOf_Carousel_Items[index]);
    }

    public override void TakeAction()
    {
        //Play sound here

        //Set Volume
        SetVolumeFunction(index);

        anim.SetTrigger("Pressed");
    }

    void SetVolumeFunction(int pass_Index)
    {
        float pass_Index_AsFloat = (float)pass_Index/10.0f;

        if (pass_Index_AsFloat <= 0)
            pass_Index_AsFloat = 0.0001f;

        //I'm using log(4) to set the volume
        float newVolume = ((Mathf.Log(pass_Index_AsFloat)/(Mathf.Log(4))) * 20);
        audioMixer.SetFloat(ref_Volume, newVolume);
    }

    // Reverse get the volume index value from the Audio Mixer
    int GetVolumeFunction()
    {
        float getVolume;
        audioMixer.GetFloat(ref_Volume, out getVolume);

        float newVolumeIndex_AsFloat = Mathf.Pow(10, (getVolume/20f)*(Mathf.Log(4)));
        int newVolumeIndex = (int)Mathf.Round(10f*newVolumeIndex_AsFloat);

        return newVolumeIndex;
    }
}
