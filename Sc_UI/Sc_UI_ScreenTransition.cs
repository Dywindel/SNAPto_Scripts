using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Sc_UI_ScreenTransition : MonoBehaviour
{
    // World References
    private Sc_GM gM;
    private Sc_AC aC;
    private Sc_RB_Animation rbA;
    private Sc_SH sH;
    // We want to keep the scene transition object between scenes
    static Sc_UI_ScreenTransition instance;

    // Reference to backing image
    public Image ref_BackingImage;

    // Reference to animator
    Animator ref_Anim;
    public Animator ref_Anim_Tri;
    public Animator ref_Anim_Dia;
    // Reference to loading icon
    public GameObject ref_LoadingIcon;

    // We can't do anything else whilst this is happening
    bool isTransitioning = false;

    // Speed of transition, written by hand atm
    float transitionSpeed = 0.5f;
    float waitTime = 0.5f;

    // Let's the loading icon animate for a bit, but doesn't switch it off if it needs to load again
    private bool loadingIcon_KeepOn = false;

    void Awake()
    {
        // The transition screen will start deactivated. This makes it easier for me to test it without
        // Having to turn it off and on again
        ref_BackingImage.enabled = true;
        ref_Anim = ref_BackingImage.GetComponent<Animator>();
        
        // Ensure this object is persistant between scenes
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        if (GameObject.FindGameObjectWithTag("AC") != null)
            aC = GameObject.FindGameObjectWithTag("AC").GetComponent<Sc_AC>();
        if (GameObject.FindGameObjectWithTag("RB") != null)
            rbA = GameObject.FindGameObjectWithTag("RB").GetComponent<Sc_RB_Animation>();
        sH = GameObject.FindGameObjectWithTag("SH").GetComponent<Sc_SH>();
    }

    public void activate_MenuTransitions(Action p_Action)
    {
        if (!isTransitioning)
        {
            isTransitioning = true;
            StartCoroutine(routine_Trans_PerfromAction(p_Action));
        }
    }

    public void activate_SceneTransitions(string p_SceneName, bool isSnapTransition = false)
    {
        if (!isTransitioning)
        {
            isTransitioning = true;
            StartCoroutine(routine_Trans_LoadScene(p_SceneName, isSnapTransition));
        }
    }

    public void activate_SceneTransitions_WhileSaving(string p_SceneName, bool isSnapTransition = false)
    {
        if (!isTransitioning)
        {
            isTransitioning = true;
            StartCoroutine(routine_Trans_LoadScene_WaitForSave(p_SceneName, isSnapTransition));
        }
    }

    public void activate_QuitGame()
    {
        StartCoroutine(routine_ExitGame());
    }

    IEnumerator routine_Trans_LoadScene_WaitForSave(string p_SceneName, bool isSnapTransition)
    {
        // Wait for the game to finish saving
        Sc_GM gM = null;
        if (GameObject.FindGameObjectWithTag("GM") != null)
            gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
        while (gM.bool_IsSavingActive || gM.bool_IsSavingReady)
        {
            yield return null;
        }

        StartCoroutine(routine_Trans_LoadScene(p_SceneName, isSnapTransition));

        yield return null;
    }

    // Transition between scenes
    IEnumerator routine_Trans_LoadScene(string p_SceneName, bool isSnapTransition)
    {
        if (GameObject.FindGameObjectWithTag("AC") != null)
            aC = GameObject.FindGameObjectWithTag("AC").GetComponent<Sc_AC>();
        if (GameObject.FindGameObjectWithTag("RB") != null)
            rbA = GameObject.FindGameObjectWithTag("RB").GetComponent<Sc_RB_Animation>();

        ref_BackingImage.enabled = true;

        // Don't let the loading icon accidently get switched off
        loadingIcon_KeepOn = true;

        if (!isSnapTransition)
        {
            sH.loadGame = true;
            ref_BackingImage.enabled = true;
            ref_Anim.SetBool("FadeIn", true);
            yield return new WaitForSeconds(transitionSpeed);
        }
        else
        {
            sH.loadGame = true;
            sH.isLoading = true;
            // Shut off all other sounds
            aC.Stop_Ambience();
            aC.Stop_Music();
            // Play snapping Sound FX
            aC.Play_SFX("SFX_Snap", false);
            
            ref_Anim.SetBool("SnapTransition", true);
            ref_Anim.SetBool("FadeIn", true);
        }

        // Loading icon activate
        ref_LoadingIcon.SetActive(true);
        yield return null;

        // Before we load the scene, make sure all saving is complete?
        if (GameObject.FindGameObjectWithTag("GM") != null)
        {
            gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
            // Wait until saving is done
            while (gM.bool_IsSavingActive || gM.bool_IsSavingReady)
            {
                yield return null;
            }
        }

        // Load scene or whatever or load the scene slightly early with preloading
        // Grab information about loading the scene
        AsyncOperation operation = SceneManager.LoadSceneAsync(p_SceneName);
        // Keep running until loaded
        while (!operation.isDone)
        {
            // This only appears twice during the entire loading time
            // There aren't enough frames to create an interesting loading menu atm
            // There must be abother way to do this
            yield return null;
        }

        // Wait for the game to load
        if (isSnapTransition)
        {
            // While the game is loading, do nothing
            while (sH.isLoading)
            {
                yield return null;
            }

            // After loading, give the game a few seconds to move the camera
            yield return new WaitForSeconds(0.5f);
        }
        
        // Deactivate loading icon
        // Instead of switching off the loadingIcon, have it animate for a few more seconds, then switch off
        loadingIcon_KeepOn = false;
        StartCoroutine(UI_LoadingIcon_Hold(2f));
        //ref_LoadingIcon.SetActive(false);

        ref_Anim.SetBool("FadeIn", false);
        yield return new WaitForSeconds(transitionSpeed);
        ref_BackingImage.enabled = false;
        isTransitioning = false;
        yield return null;
    }


    // Transitioning between menus in the same scene
    IEnumerator routine_Trans_PerfromAction(Action p_Action)
    {
        ref_BackingImage.enabled = true;
        ref_Anim.SetBool("FadeIn", true);
        yield return new WaitForSeconds(transitionSpeed);

        // Perfrom the passed action
        p_Action();

        ref_Anim.SetBool("FadeIn", false);
        yield return new WaitForSeconds(transitionSpeed);
        ref_BackingImage.enabled = false;
        isTransitioning = false;
    }

    // Fadeout to exit game
    IEnumerator routine_ExitGame()
    {
        ref_BackingImage.enabled = true;
        ref_Anim.SetBool("FadeIn", true);
        yield return new WaitForSeconds(transitionSpeed);

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // Keep the loading icon on screen for a few more seconds
	public IEnumerator UI_LoadingIcon_Hold(float holdTime)
	{
        ref_LoadingIcon.SetActive(true);
		yield return new WaitForSeconds(1f);
		ref_Anim_Tri.SetBool("FadeOut", false);
		ref_Anim_Dia.SetBool("FadeOut", false);
		yield return new WaitForSeconds(holdTime);
		ref_Anim_Tri.SetBool("FadeOut", true);
		ref_Anim_Dia.SetBool("FadeOut", true);
        yield return new WaitForSeconds(1f);
        if (!loadingIcon_KeepOn)
		    ref_LoadingIcon.SetActive(false);
		yield return null;
	}
}
