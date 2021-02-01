using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////
//
//	Sc_SH - Save Handler
//	This script always exists throughout the playthrough, in both title menu and
//  The game itself. This will be used to Load/Save the main game.
//  This script must function natively across all scenes
//
//  Additionally, this is where we get the persistant data path when the game first loads up
//  (Because multithreading can't do persistant data path)

public class Sc_SH : MonoBehaviour
{
    static Sc_SH instance;

    // The Save ID the player will use to load and save games
    [HideInInspector]
    public int saveID = 0;
    [HideInInspector]
    public bool loadGame = true;
    [HideInInspector]
    public bool isLoading = true;

    // This object must be persistent between scenes
    void Awake()
    {
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

    // Get the persistant data path
    void Start()
    {
        Persistance.Get_PersistanceDataPath();
    }
}
