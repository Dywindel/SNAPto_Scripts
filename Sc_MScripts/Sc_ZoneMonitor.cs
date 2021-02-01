using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_ZoneMonitor - Zone Monitor
//	This script monitors levels in a specific zone, those levels
//  That are attached to a puzzle list inside one of the zones
//  It is an intermediary between LevelManager and the PG

public class Sc_ZoneMonitor : MonoBehaviour
{
    protected Sc_PG pG;                             //Reference to the Puzzle Group script
    public Sc_LevelManager[] listOf_LevelManagers;    // This grabs all the levels in the area and keeps them safe

    // Statuses
    public int id = -1;
    [HideInInspector]
    public int zoneID = 0;
    [HideInInspector]
    public bool bool_ZoneMonitorComplete = false;
    // For now, the level complete marker is just a meshRenderer
    private MeshRenderer ref_ZoneMonitor_StatusMarker;
    // Number of puzzles connected to this zone marker
    public int nm_Pz = 0;
    // Number of Puzzles connected to this zone marker, that have also been solved
    public int nm_Pz_Slv = 0;

    // Sample petal
    public GameObject ref_Petal;
    public GameObject[] listOf_Petals;

    // Start is called before the first frame update
    void Start()
    {
        // Grab world objects
        Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
        pG = gM.pG;

        // Add yourself to the PG's list of monitors
        pG.listOf_ZoneMonitors.Add(this);
        // Find what your index is and set that value
        zoneID = pG.listOf_ZoneMonitors.IndexOf(this);

        // Grab all the levels in this zone
        listOf_LevelManagers = this.transform.parent.GetComponentsInChildren<Sc_LevelManager>();
        // This is the total number of puzzles
        nm_Pz = listOf_LevelManagers.Length;
        // Send them a reference to yourself and set their levelID
        for (int i = 0; i < nm_Pz; i++)
        {
            listOf_LevelManagers[i].zM = this;
            listOf_LevelManagers[i].levelID[0] = i;
            listOf_LevelManagers[i].levelID[1] = zoneID;
        }

        // Setup Petals
        Setup_Petals();

        ref_ZoneMonitor_StatusMarker = this.gameObject.GetComponent<MeshRenderer>();
    }

    // Sends the currently active level
    public void Send_ActiveLevel(Sc_LevelManager pass_LevelManager)
    {
        // Explain to the PG what is currently active
        pG.active_LevelManager = pass_LevelManager;
        pG.active_ZoneMonitor = this;
    }

    // And deactivates the level
    public void Send_DeactiveLevel()
    {
        pG.active_LevelManager = null;
        pG.active_ZoneMonitor = null;
    }

    // When a level is complete, we update the appearance of the Zone Monitor
    // This also includes a state for when all the levels are complete
    // We can pass the levelManager in order to animate the appropriate part of the Zone Monitor
    public void Update_ZoneMonitorStatus(Sc_LevelManager pass_LevelManager)
    {
        // Animate relevant petal
        Animate_Petal(pass_LevelManager.levelID[0]);

        // Update the number of solved levels connected to this zone marker
        nm_Pz_Slv += 1;

        // Run through each level and see if there are any that aren't complete
        bool bool_NoIncompleteLevels = true;
        foreach (Sc_LevelManager temp_LevelManager in listOf_LevelManagers)
        {
            if (!temp_LevelManager.bool_levelComplete)
                bool_NoIncompleteLevels = false;
        }

        // If all the levels are complete, do a cool thing
        if (bool_NoIncompleteLevels)
        {
            // Set zone monitor status to complete
            bool_ZoneMonitorComplete = true;

            // Perform the completion animation or load the completion state
            // Water fountain or something
            //ref_ZoneMonitor_StatusMarker.enabled = false;
        }
    }

    // Place all the petals first and deactivate them
    void Setup_Petals()
    {
        listOf_Petals = new GameObject[nm_Pz];

        // Calculate spacing angle of petals
        float petals_AngleSpacing = 360f/nm_Pz;

        // Create the petals
        for (int i = 0; i < nm_Pz; i++)
        {
            listOf_Petals[i] = Instantiate(ref_Petal, this.transform.position, Quaternion.Euler(-90f, i * petals_AngleSpacing, 0f));
            // Set the parent to this zoneMonitor
            listOf_Petals[i].gameObject.transform.SetParent(this.gameObject.transform);
            listOf_Petals[i].SetActive(false);
        }
    }

    // Animates a petal after it's relevant puzzle has been solved
    public void Animate_Petal(int p_LevelID)
    {
        listOf_Petals[p_LevelID].SetActive(true);
    }
}
