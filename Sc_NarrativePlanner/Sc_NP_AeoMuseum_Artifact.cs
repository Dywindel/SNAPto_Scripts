using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////////////////////////////////////////////////////
//
//  This script is used for each artifact and has
//  A reference to two different game objects. It
//  controls the state of each object depending on
//  Their situation

public class Sc_NP_AeoMuseum_Artifact : MonoBehaviour
{
    // World references
    private Sc_NP_AeoMuseum nP;
    private Sc_AC aC;

    // The index value of the artifact stored in the NP script
    public int id = -1;

    // Set whether this item counts towards player progression
    public bool bool_PlayerProgress = true;

    [HideInInspector]
    public int artifact_ActionState = 0;

    public GameObject gO_Artifact_Start;
    public GameObject gO_Artifact_Median_1;
    public GameObject gO_Artifact_Median_2;
    public GameObject gO_Artifact_Museum;

    public GameObject gO_Artifact_Good;

    // Mark the progress of this item. Some items will require many states
    // Mark this from 0 - 1
    [HideInInspector]
    public float mP_Artifact_ProgressState = 0;

    void Start()
    {
        // Send this script to the NP processor
        Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
        nP = gM.nP;
        nP.listOf_Artifacts.Add(this);

        aC = gM.aC;

        // Make sure this artifact is in its correct state
        Update_Artifact();
    }

    public void Update_Artifact()
    {
        // Deactivate everything
        DeactivateAll();

        switch (artifact_ActionState)
        {
            // If in start mode
            case 0:
                gO_Artifact_Start.SetActive(true);
                break;
            // Nothing needs to happen
            case 1:
                break;
            // If in museum
            case 2:
                gO_Artifact_Museum.SetActive(true);
                // This is the final state for this artifact
                mP_Artifact_ProgressState = 1f;
                break;
            // Special Case where both Start and museum stay active
            case 3:
                gO_Artifact_Museum.SetActive(true);
                gO_Artifact_Start.SetActive(true);
                // This is the final state for this artifact
                mP_Artifact_ProgressState = 1f;
                break;
            // Activate median 1
            case 4:
                gO_Artifact_Median_1.SetActive(true);
                // This item is x part way through it's progress
                mP_Artifact_ProgressState = 0.4f;
                break;
            // Activate median 2
            case 5:
                gO_Artifact_Median_2.SetActive(true);
                // This item is x part way through it's progress
                mP_Artifact_ProgressState = 0.6f;
                break;
            // 'Good' Ending?
            case 6:
                gO_Artifact_Good.SetActive(true);
                // The, I guess, what could be described as, good ending
                mP_Artifact_ProgressState = 1.0f;
                break;
        }

        // Update progress
        // Don't update this artifact if it's not based on player progress
        if (bool_PlayerProgress)
            nP.Update_MuseumProgress();
    }

    // Switch of all artifact locations
    public void DeactivateAll()
    {
        gO_Artifact_Start.SetActive(false);
        if (gO_Artifact_Median_1 != null)
            gO_Artifact_Median_1.SetActive(false);
        if (gO_Artifact_Median_2 != null)
            gO_Artifact_Median_2.SetActive(false);
        gO_Artifact_Museum.SetActive(false);
        if (gO_Artifact_Good != null)
            gO_Artifact_Good.SetActive(false);
    }

    public void Found_Artifact()
    {
        // Play SFX
        aC.Play_SFX("SFX_Collect_Rustle");

        // Update artifact state
        artifact_ActionState = 2;
        Update_Artifact();
    }

    public void Found_Progress(int pass_state = 2)
    {
        // No SFX

        // Update artifact state
        artifact_ActionState = pass_state;
        
        Update_Artifact();
    }

    public void Found_Aeo()
    {
        // Play SFX

        // Update artifact state
        artifact_ActionState = 3;
        Update_Artifact();
    }

    // Find progress, but delay the effect of progress for a few seconds
    public void Delay_Found_Progress(int pass_state = 2)
    {
        StartCoroutine(Coroutine_Delay_Found_Progress(pass_state));
    }

    IEnumerator Coroutine_Delay_Found_Progress(int pass_state)
    {
        yield return new WaitForSeconds(10);

        Found_Progress(2);
        
        yield return null;
    }
}
