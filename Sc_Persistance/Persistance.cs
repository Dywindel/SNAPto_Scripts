using System.Collections;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/////////////////////////////////////
//
//  Sc_Persistance
//  This script stores and loads save data
//  And convers data into hex/bin format

public static class Persistance
{
    // The data that would be saved is as follows:
    // The savefile ID
    // RecState_AsList - Which is a list of the positions of all objects
    // And they're current state
    // -Something about player progress-

    // Persistance data path can only be gathered from the main thread. I'll do that here with a simple
    // Variable and class and call it when the game starts up
    // This is perform by the Sc_SH script
    public static string persistanceDataPath = "";
    public static void Get_PersistanceDataPath()
    {
        persistanceDataPath = Application.persistentDataPath;
    }

    // For the sake of the loading/saving data in the main menu, have another save file for this information? PlayerPrefs?
    public static void Save_MasterData(int saveFileID, MasterData pass_MasterData)
    {
        // Create a string where the file will be saved
        if (saveFileID <= 2 && saveFileID >= 0)
        {
            // For different savefile ID's
            string path = Return_SavePath(saveFileID);

            // Grab a binary formatter
            BinaryFormatter formatter = new BinaryFormatter();
            // Data exchanges are handled using a filestream
            FileStream stream = new FileStream(path, FileMode.Create);

            // We use a try block, just in case something goes wrong
            try
            {    
                // Convert the data into binary and write it to the file.
                formatter.Serialize(stream, pass_MasterData);
            }
            finally
            {
                stream.Close();
            }

            // Next, update the PlayerPrefs, if one exists, for the small data used
            // By the save selection menu. Here we just store the completion percentage
            //PlayerPrefs.SetFloat("saveID_" + saveFileID, pass_MasterData.master_RecPlayerProgress.recState_PP_PuzzlePercentage);
        }
        else
        {
            Debug.Log("SaveFileID Not available");
        }
    }

    // Load the data
    public static MasterData Load_MasterData(int saveFileID)
    {
        // First, grab the path to the file
        if (saveFileID <= 2 && saveFileID >= 0)
        {
            // For different savefile ID's
            string path = Return_SavePath(saveFileID);

            // Check the file exists
            if (File.Exists(path))
            {
                // Grab a binary formatter
                BinaryFormatter formatter = new BinaryFormatter();
                // Data exchanges are handled using a filestream
                FileStream stream = new FileStream(path, FileMode.Open);

                try
                {
                    // Deseriealize the file
                    MasterData pass_MasterData = formatter.Deserialize(stream) as MasterData;
                    return pass_MasterData;
                }
                finally
                {
                    stream.Close();
                }
            }
            else
            {
                Debug.Log("Save file not found in " + path);
                return null;
            }
        }
        else
        {
            Debug.Log("SaveFileID Not available");
            return null;
        }
    }

    
    public static string Return_PlayerPrefs(int saveFileID)
    {
        float puzzlepercentage_AsFloat = PlayerPrefs.GetFloat("saveID_" + saveFileID);
        puzzlepercentage_AsFloat = Mathf.Round(puzzlepercentage_AsFloat * 1000f) / 10f;
        string puzzlepercentage_AsString = puzzlepercentage_AsFloat.ToString() + "%";
        return puzzlepercentage_AsString;
    }

    // Check if a save file exists
    public static bool CheckIf_SaveExists(int saveFileID)
    {
        // First, we check if the master dataFile exists? This defines if our save
        // says 'Continue' or 'New Game'
        
        // First, check the playerprefs key exists
        if (PlayerPrefs.HasKey("saveID_" + saveFileID))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // When the player wants to start again
    public static void Delete_Save(int saveFileID)
    {
        // Delete the player prefs
        PlayerPrefs.DeleteKey("saveID_" + saveFileID);

        // Delete the main savefile
        string path = Return_SavePath(saveFileID);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    // Function for grabbing the save path
    public static string Return_SavePath(int saveFileID)
    {
        string path = persistanceDataPath + "/PurpleSave.snpt";
        if (saveFileID == 1)
        {
            path = persistanceDataPath + "/BlueSave.snpt";
        }
        else if (saveFileID == 2)
        {
            path = persistanceDataPath + "/RedSave.snpt";
        }

        return path;
    }

    // There is room here to create a worldWide saves database, where the player's saves
    // Are stored across all games, even after deletion.
    // But, that's additional content stuff
}
