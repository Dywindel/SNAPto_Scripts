using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

///////////////////////////////////////
// Sc_ES_GenerateUniqueCloudID
// This script gives each Cloud in the game
// Scene a unique ID, which is used to track it during saving/loading

public class Sc_ES_GenerateUniqueCloudID : MonoBehaviour
{
    // This function grabs all the clouds in the scene and gives them a unique ID
    public void Button_GenerateUnitqueCloudIDs()
    {
        Sc_Cloud temp_Cloud;
        GameObject[] listOf_Clouds_As_GameObjects = GameObject.FindGameObjectsWithTag("Cloud");

        // In order to assure that old clouds don't lose their id numbers for save and load files, I need to make sure
        // New clouds only grab new numbers
        // First, generate a list of numbers based on the current number of clouds
        List<int> listOf_Numbers = Create_IntSequenceList(listOf_Clouds_As_GameObjects.Length);

        // Go through each cloud and, if any IDs are already ID'd, remove those values
        for (int i = 0; i < listOf_Clouds_As_GameObjects.Length; i++)
        {
            temp_Cloud = listOf_Clouds_As_GameObjects[i].GetComponent<Sc_Cloud>();
            if (temp_Cloud.id != -1)
                listOf_Numbers.Remove(temp_Cloud.id);
        }

        // Grab each Cloud object and give them a unique ID, i
        // As long as they have not been id'd
        // Give that cloud an id from the list of numbers
        
        for (int i = 0; i < listOf_Clouds_As_GameObjects.Length; i++)
        {
            temp_Cloud = listOf_Clouds_As_GameObjects[i].GetComponent<Sc_Cloud>();
            // Tell Unity to record the changes you've applied to any objects
            Undo.RecordObject(temp_Cloud, "Apply ID value to object");
            if (temp_Cloud.id == -1)
            {
                // Assign the new cloud an id, then remove that possibility from the list
                temp_Cloud.id = listOf_Numbers.First();
                listOf_Numbers.Remove(listOf_Numbers.First());
            }
        }
    }

    // This generates unique IDs for the puzzles and the zone markers, if they don't already have one
    public void Button_GenerateUnitquePuzzleIDs()
    {
        Sc_LevelManager temp_LM;
        GameObject[] listOf_LM_As_GameObjects = GameObject.FindGameObjectsWithTag("Level Manager");

        // First create an int list
        List<int> listOf_Numbers_LM = Create_IntSequenceList(listOf_LM_As_GameObjects.Length);

        // Go through and remove values that already have IDs applied to them
        for (int i = 0; i < listOf_LM_As_GameObjects.Length; i++)
        {
            temp_LM = listOf_LM_As_GameObjects[i].GetComponent<Sc_LevelManager>();
            if (temp_LM.id != -1)
                listOf_Numbers_LM.Remove(temp_LM.id);
        }

        // Give each object a unique ID, as long as that ID isn't taken
        for (int i = 0; i < listOf_LM_As_GameObjects.Length; i++)
        {
            temp_LM = listOf_LM_As_GameObjects[i].GetComponent<Sc_LevelManager>();
            // Tell Unity to record the changes you've applied to any objects
            Undo.RecordObject(temp_LM, "Apply ID value to object");
            if (temp_LM.id == -1)
            {
                // Assign the new LM an id, then remove that possibility from the list
                temp_LM.id = listOf_Numbers_LM.First();
                listOf_Numbers_LM.Remove(listOf_Numbers_LM.First());
            }
        }

        Sc_ZoneMonitor temp_ZM;
        GameObject[] listOf_ZM_As_GameObjects = GameObject.FindGameObjectsWithTag("Zone Monitor");

        // First create an int list
        List<int> listOf_Numbers_ZM = Create_IntSequenceList(listOf_ZM_As_GameObjects.Length);

        // Go through and remove values that already have IDs applied to them
        for (int i = 0; i < listOf_ZM_As_GameObjects.Length; i++)
        {
            temp_ZM = listOf_ZM_As_GameObjects[i].GetComponent<Sc_ZoneMonitor>();
            if (temp_ZM.id != -1)
                listOf_Numbers_ZM.Remove(temp_ZM.id);
        }

        // Give each object a unique ID, as long as that ID isn't taken
        for (int i = 0; i < listOf_ZM_As_GameObjects.Length; i++)
        {
            temp_ZM = listOf_ZM_As_GameObjects[i].GetComponent<Sc_ZoneMonitor>();
            // Tell Unity to record the changes you've applied to any objects
            Undo.RecordObject(temp_ZM, "Apply ID value to object");
            if (temp_ZM.id == -1)
            {
                // Assign the new ZM an id, then remove that possibility from the list
                temp_ZM.id = listOf_Numbers_ZM.First();
                listOf_Numbers_ZM.Remove(listOf_Numbers_ZM.First());
            }
        }
    }

    // Reset the puzzle id values, just in case I need to
    public void Button_ResetAllValues()
    {
        Sc_Cloud temp_Cloud;
        Sc_LevelManager temp_LM;
        GameObject[] listOf_Clouds_As_GameObjects = GameObject.FindGameObjectsWithTag("Cloud");
        GameObject[] listOf_LM_As_GameObjects = GameObject.FindGameObjectsWithTag("Level Manager");

        for (int i = 0; i < listOf_Clouds_As_GameObjects.Length; i++)
        {
            temp_Cloud = listOf_Clouds_As_GameObjects[i].GetComponent<Sc_Cloud>();
            // Tell Unity to record the changes you've applied to any objects
            Undo.RecordObject(temp_Cloud, "Apply ID value to object");
            temp_Cloud.id = -1;
        }

        for (int i = 0; i < listOf_LM_As_GameObjects.Length; i++)
        {
            temp_LM = listOf_LM_As_GameObjects[i].GetComponent<Sc_LevelManager>();
            // Tell Unity to record the changes you've applied to any objects
            Undo.RecordObject(temp_LM, "Apply ID value to object");
            temp_LM.id = -1;
        }
    }

    List<int> Create_IntSequenceList(int pass_Length)
    {
        List<int> listOf_Numbers = new List<int>();
        for (int i = 0; i < pass_Length; i++)
        {
            listOf_Numbers.Add(i);
        }

        return listOf_Numbers;
    }
}
