using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

// This very simple script fades out the studio logo video and then reveals the title screen of the game
// It also turns off controller input for this section
// It also allows the player to to skip the intro.

public class Sc_TitleScreen_StudioIntro : MonoBehaviour
{
    public VideoPlayer ref_VideoPlayer;
    public Image ref_Background;

    private bool startFade = false;

    void Start() 
    {
        ref_VideoPlayer.loopPointReached += LoadTitleScreen;
    }

    // Once the video has finished and faded out, present the title screen by disabling the videoplayer
    // And fading out the backing screen
    void LoadTitleScreen(UnityEngine.Video.VideoPlayer vp)
    {
        // Disable the video
        ref_VideoPlayer.enabled = false;
        StartCoroutine(FadeOut(0.0f, 1.0f, 2.0f));
    }

    IEnumerator FadeOut(float aValue, float aTime, float dTime)
    {
        // Delay before fading out
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / dTime)
        {
            yield return null;
        }

        float alpha = ref_Background.material.color.a;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            Color p_Color = new Color(0.0f, 0.0f, 0.0f, Mathf.Lerp(alpha, aValue, t));
            ref_Background.color = p_Color;
            yield return null;
        }
    }









}
