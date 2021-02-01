using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Audio;
using UnityEngine;

//////////////////////////////////////////////////////////////////////////////////////////
//
//	Sc_AC - Audio Conductor
//  Controlls when and what sounds/FX/Music gets player and through what means

public class Sc_AC : MonoBehaviour
{
    // World variables
    Sc_GM gM;
    private Sc_RB_Animation rbA;

    // Initial Conditions
    public int start_SoundScape = 24;

    // Mixers
    public AudioMixerGroup Mx_Master;
    public AudioMixerGroup Mx_Music;
    public AudioMixerSnapshot[] Mx_Music_Snapshots;
    public AudioMixerGroup Mx_SFX;
    public AudioMixerGroup Mx_Ambience;
    public AudioMixerGroup Mx_NonSFX;

    public Sc_SoundDatabase[] listOf_Sounds_Music;
    public Sc_SoundDatabase[] listOf_Sounds_SFX;
    public Sc_SoundDatabase[] listOf_Sounds_Ambience;

    // To make things easier to read, I'll split the Audio Sources into separate transforms
    private Transform ref_Trans_Music;
    private Transform ref_Trans_SFX;
    private Transform ref_Trans_Ambience;

    // Here is a simple list that checks which SFX Noises will be played after a movement event occurs
    [HideInInspector]
    public bool[] listOf_Push_SFX;

    // Tracks the active Music piece or ambience track
    // 0 - Music, 1 - Ambience
    private Sc_SoundDatabase[] active_Sound = new Sc_SoundDatabase[2];
    void Awake()
    {
        // Create the three transforms and set the correct parent
        ref_Trans_Music = new GameObject().transform;
        ref_Trans_Music.name = "AC_Music";
        ref_Trans_Music.SetParent(this.transform);
        ref_Trans_SFX = new GameObject().transform;
        ref_Trans_SFX.name = "AC_SFX";
        ref_Trans_SFX.SetParent(this.transform);
        ref_Trans_Ambience = new GameObject().transform;
        ref_Trans_Ambience.name = "AC_Ambience";
        ref_Trans_Ambience.SetParent(this.transform);

        // Create the checkList for push SFX
        listOf_Push_SFX = new bool[listOf_Sounds_SFX.Length];
        // And set the to false
        ResetList_Push_SFX();

        // Turn each sound in the database into a gameobject with an audiosource
        foreach (Sc_SoundDatabase s in listOf_Sounds_Music)
        {
            // Add an audio source to the correct parent trasnform
            s.source = ref_Trans_Music.gameObject.AddComponent<AudioSource>();
            // Attach the correct clip
            s.source.clip = s.clip;
            // Attach the correct mixer
            s.source.outputAudioMixerGroup = Mx_Music;
            // Set the right master value
            // NEED a reference to each mixer group
            //s.source.outputAudioMixerGroup = 

            // Attach the revelent settings
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        foreach (Sc_SoundDatabase s in listOf_Sounds_SFX)
        {
            // Add an audio source to the correct parent trasnform
            s.source = ref_Trans_SFX.gameObject.AddComponent<AudioSource>();
            // Attach the correct clip
            s.source.clip = s.clip;
            // Attach the correct mixer
            s.source.outputAudioMixerGroup = Mx_SFX;
            // Set the right master value
            // NEED a reference to each mixer group
            //s.source.outputAudioMixerGroup = 

            // Attach the revelent settings
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        foreach (Sc_SoundDatabase s in listOf_Sounds_Ambience)
        {
            // Add an audio source to the correct parent trasnform
            s.source = ref_Trans_Ambience.gameObject.AddComponent<AudioSource>();
            // Attach the correct clip
            s.source.clip = s.clip;
            // Attach the correct mixer
            s.source.outputAudioMixerGroup = Mx_Ambience;
            // Set the right master value
            // NEED a reference to each mixer group
            //s.source.outputAudioMixerGroup = 

            // Attach the revelent settings
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    void Start()
    {
        // Grab the RB Animator box
        if (GameObject.FindGameObjectWithTag("GM") != null)
            gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
        rbA = GameObject.FindGameObjectWithTag("RB").GetComponent<Sc_RB_Animation>();

        // Grab the snapshots - I'll simplify this later, it's clunky atm
        Mx_Music_Snapshots = new AudioMixerSnapshot[2];
        Mx_Music_Snapshots[0] = Mx_Music.audioMixer.FindSnapshot("LowPassFilter");
        Mx_Music_Snapshots[1] = Mx_Music.audioMixer.FindSnapshot("No_LowPassFilter");

        Play_Soundscape(start_SoundScape);
    }

    public void Play_Music(string name)
    {
        Sc_SoundDatabase s = Array.Find(listOf_Sounds_Music, listOf_Sounds_Music => listOf_Sounds_Music.name == name);
        if (s.name == null)
            return;
        
        Organise_Sound(0, s);
    }

    public void Play_SFX(string name, bool isPitchShifted = true)
    {
        Sc_SoundDatabase s = Array.Find(listOf_Sounds_SFX, listOf_Sounds_SFX => listOf_Sounds_SFX.name == name);
        if (s.name == null)
        {
            Debug.Log("The sound: " + name + " Could not be found");
            return;
        }

        // Sometimes I want to randomize the pitch before the SFX get played
        float pitchShift = 1.0f;
        if (isPitchShifted)
        {
            pitchShift = UnityEngine.Random.Range(0.7f, 1.3f);
            s.source.pitch = pitchShift;
        }

        s.source.Play();
    }

    public void Play_Ambience(string name)
    {
        Sc_SoundDatabase s = Array.Find(listOf_Sounds_Ambience, listOf_Sounds_Ambience => listOf_Sounds_Ambience.name == name);
        if (s.name == null)
            return;
        
        Organise_Sound(1, s);
    }

    // Used to play both ambience and music for a particular atmosphere
    public void Play_Soundscape(int pass_int)
    {
        // Check if we're inside an ending state
        if (gM != null)
        {
            if (gM.endingState == 0 || gM.atmosphere_NormalMode)
            {
                // Do nothing
            }
            else if (gM.endingState == 1)
            {
                pass_int = 17;
            }
            else if (gM.endingState == 2)
            {
                pass_int = 18;
            }
        }

        switch (pass_int)
        {
        // 0 - Autumn
        case 0:
            Play_Music("Theme_Autumn");
            Play_Ambience("Ambient1");
            break;
        // 1 - Storm Sanctuary
        case 1:
            Play_Music("Theme_Sanctuary");
            Play_Ambience("Ambient2");
            break;
        // 2 - Winter
        case 2:
            Play_Music("Theme_Winter");
            Play_Ambience("Ambient2");
            break;
        // 3 -Jungle
        case 3:
            //Play_Music("Theme_Jungle");
            Play_Music("Theme_Empty");
            Play_Ambience("Ambient2");
            break;
        // 6 - Dessert
        case 6:
            break;
        // 8 - Castle
        case 8:
            Play_Music("Theme_Castle");
            Play_Ambience("9_RockyOutcrop_Ambience");
            break;
        // 9 - Rocky Outcrop
        case 9:
            // I can't get fading out to work correctly, so, for now, play an empty track
            Play_Music("Theme_Empty");
            Play_Ambience("9_RockyOutcrop_Ambience");
            break;

        // 10 - Weird Pyramid
        case 10:
            Play_Music("Theme_Siren");
            Play_Ambience("Ambient_Empty");
            break;

        // 17 - Bad Ending
        case 17:
            // Keep this blank, the GM will perform this in a coroutine
            break;
        
        // 18 - Good Ending
        case 18:
            // Keep this blank, the GM will perform this in a coroutine
            break;

        // 19 - Easter Egg - Solid Gold Tree
        case 19:
            Play_Music("Theme_SecretSolidGoldTree");
            // Play_Ambience("9_RockyOutcrop_Ambience");
            break;

        // 20 - Title menu + LoadSave menu
        case 20:
            Play_Music("Theme_Title");
            Play_Ambience("Ambient1");
            break;
        // 21 - Title menu ambience, no title theme
        case 21:
            Play_Ambience("Ambient1");
            break;
        case 24:
            // No sound, no ambience
            break;
        default:
            break;
        }
    }

    public void Play_SFXList()
    {
        // Go through and, if a soundType has been set to true, play it
        for (int i = 0; i < listOf_Push_SFX.Length; i++)
        {
            if (listOf_Push_SFX[i] == true)
                Play_SFX(listOf_Sounds_SFX[i].name);
        }
        // Then, reset the list
        ResetList_Push_SFX();
    }

    // Stop ambience
    public void Stop_Ambience()
    {
        if (active_Sound[1] != null)
            active_Sound[1].source.Stop();
    }

    // Stop Music
    public void Stop_Music()
    {
        if (active_Sound[0] != null)
            active_Sound[0].source.Stop();
    }

    // Fade out Ambience
    public void FadeOut_Ambience()
    {
        if (active_Sound[1] != null)
            StartCoroutine(rbA.Audio_FadeIn(active_Sound[1], false));
    }

    // Fade out Music
    public void FadeOut_Music()
    {
        if (active_Sound[0] != null)
            StartCoroutine(rbA.Audio_FadeIn(active_Sound[0], false));
    }


    // Reset push SFX check list
    private void ResetList_Push_SFX()
    {
        for (int i = 0; i < listOf_Push_SFX.Length; i++)
        {
            listOf_Push_SFX[i] = false;
        }
    }

    private void Organise_Sound(int pass_SoundType, Sc_SoundDatabase pass_Sound)
    {
        // If nothing is currently the active track, make this the active track and start playing it
        if (active_Sound[pass_SoundType] == null)
        {
            active_Sound[pass_SoundType] = pass_Sound;
            StartCoroutine(rbA.Audio_FadeIn(active_Sound[pass_SoundType], true));
        }
        // If the active track is the same as the one we're playinig, do nothing
        else if (active_Sound[pass_SoundType] == pass_Sound)
        {
            // Do Nothing
        }
        // If the active track is diffent. Then, we have some stuff to do
        else
        {
            // For now, let's just stop the current track, set the new active music and start the supplied track
            StartCoroutine(rbA.Audio_FadeIn(active_Sound[pass_SoundType], false));
            active_Sound[pass_SoundType] = pass_Sound;
            StartCoroutine(rbA.Audio_FadeIn(active_Sound[pass_SoundType], true));
        }
    }

    public void FadeMusicTo_LowPassFilter(bool Bool_FadeTo_LowPassFilter)
    {
        if (Bool_FadeTo_LowPassFilter)
        {
            Mx_Music_Snapshots[0].TransitionTo(rbA.tlp);
        }
        else
        {
            Mx_Music_Snapshots[1].TransitionTo(rbA.tlp);
        }
    }
}
