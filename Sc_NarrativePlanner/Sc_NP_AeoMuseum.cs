using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////////////////////////////////////////////////////
//
//  This is a Narrative Planner
//  for Aeo's museum plotline. It controls which items
//  appear in the museum, when. And where Aeo is

public class Sc_NP_AeoMuseum : MonoBehaviour
{
    // World References
    private Sc_GM gM;
    
    // This stores a reference to each artifact script in the game, that the player can collect
    [System.NonSerialized]
    public List<Sc_NP_AeoMuseum_Artifact> listOf_Artifacts = new List<Sc_NP_AeoMuseum_Artifact>();
    // Not all artifacts count towards completion
    [System.NonSerialized]
    public int listOf_Artifacts_ThatCountTowardsCompletion;

    // Museum Progress
    [HideInInspector]
    public float mP_ProgressState_Total = 0.0f;

    void Awake()
    {
        // Grab world references
        gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
    }

    void Start()
    {
        StartCoroutine(Delay_Start());
    }

    // Delay start trick too allow artifacts to add themselves to this object
    IEnumerator Delay_Start()
    {
        // Wait three frames
        yield return null;
        yield return null;
        yield return null;

        listOf_Artifacts_ThatCountTowardsCompletion = 0;
        for (int i = 0; i < listOf_Artifacts.Count; i++)
        {
            if (listOf_Artifacts[i].bool_PlayerProgress)
                listOf_Artifacts_ThatCountTowardsCompletion += 1;
        }

        yield return null;
    }

    // This updates the museum plot, switching on and off each item
    // Depending on where the story is
    public void Update_MuseumPlot()
    {
        // Update all artifacts
        foreach (Sc_NP_AeoMuseum_Artifact temp_artifact in listOf_Artifacts)
        {
            temp_artifact.Update_Artifact();
        }
    }

    // Checks progress of item states
    // For now, this just checks if a museum item is in case 0 or not
    public void Update_MuseumProgress()
    {
        float temp_mP_ProgressState_Total = 0;
        // Add up each mP_Progress state value
        for (int i = 0; i < listOf_Artifacts.Count; i++)
        {
            // Add its current progress value, if it counts towards progress
            if (listOf_Artifacts[i].bool_PlayerProgress)
                temp_mP_ProgressState_Total += listOf_Artifacts[i].mP_Artifact_ProgressState;
        }
        mP_ProgressState_Total = temp_mP_ProgressState_Total;

        // Update GM total game progression
        gM.Update_TotalGame_CP();
    }
}
