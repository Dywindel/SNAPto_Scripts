using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// A reference library for common values
public class GlobInt
{
    // Block types
    public enum BlockType : int {Play, Flor, Puff, Soil, Raft, Lift, Spin};

    // Layer values
    // This has not been implemented yet, but is a good idea
    public enum LayerType : int {IgnoreRaycast = 2, Player = 8, Walls, Puffs, Clouds, Soil, Raft, Sprout, Terrain, Floor, LevelManager, LevelGroup, Lift};

    // Player movement optins
    public enum MoveOption {none, forwards, backwards, clockwise, anticlockwise};
}

// This class records the movement function we want to perform
public class QueueMovement_Translate
{
    // This integer records which movement function we want to perform
    public int queue_Int_MoveFunction;

    // A reference to the transform
    public Transform queue_Trans;

    // Direction we want to move in
    public int queue_NetMovement;

    public QueueMovement_Translate(int Pass_Queue_Int_MoveFunction, Transform pass_Queue_Trans, int pass_Queue_NetMovement)
    {
        queue_Int_MoveFunction = Pass_Queue_Int_MoveFunction;
        queue_Trans = pass_Queue_Trans;
        queue_NetMovement = pass_Queue_NetMovement;
    }
}

public class QueueMovement_Rotate
{
    // Which movement function we want to perform
    public int queue_Int_RotateFunction;
    // Clockwise or anti-clockwise motion
    public GlobInt.MoveOption queue_Rotation;

    // A reference to the transform
    public Transform queue_Trans;

    // Rotation Stage
    public bool queue_RotationStage;

    public QueueMovement_Rotate(int pass_Queue_Int_RotationFunction, GlobInt.MoveOption pass_Queue_Rotation, Transform pass_Queue_Trans, bool pass_queue_RotationStage)
    {
        queue_Int_RotateFunction = pass_Queue_Int_RotationFunction;
        queue_Rotation = pass_Queue_Rotation;
        queue_Trans = pass_Queue_Trans;
        queue_RotationStage = pass_queue_RotationStage;
    }
}

public class QueueMovement_Lift
{
    // This integer records which movement function we want to perform
    public int queue_Int_MoveFunction;

    // This is a reference boolean for either the screws of the lifted objects
    public bool queue_RefBool;

    // A reference to the transform
    public Transform queue_Trans;

    // Special switch case for the lift's movement stage
    public int queue_SwitchCase;

    public QueueMovement_Lift(int Pass_Queue_Int_MoveFunction, bool pass_Queue_RefBool, Transform pass_Queue_Trans, int pass_Queue_SwitchCase)
    {
        queue_Int_MoveFunction = Pass_Queue_Int_MoveFunction;
        queue_RefBool = pass_Queue_RefBool;
        queue_Trans = pass_Queue_Trans;
        queue_SwitchCase = pass_Queue_SwitchCase;
    }
}

// This class is used to record a list of all the elements in the level as they were in a previous state
public class RecStateList
{
    public Vector3 rec_player_Position;
    public int rec_player_faceInt;
    //It's much easier to remember the positions of all the Clouds instead of the Puffs
    public Vector3[] rec_listOf_Cloud_Positions;
    // Remember what parent each cloud has
    public List<Sc_Storm> rec_listOf_Cloud_Parents;
    //Currently, I'm remembering the positions of all Storms due to the effect of falling and recalling position.
    //A possible fix is to just always move the Clouds and never move any Storms
    public Vector3[] rec_listOf_Storm_Positions;
    // Remember each Storms List of Cloud children
    public List<List<Sc_Cloud>> rec_listOf_listOf_Cloud_Children;
    public List<bool> rec_Puff_Full;    //Remembers if the Puff is filled or empty
    public List<bool> rec_Soil_State;   //Remembers the state of the soil
    public List<bool> rec_Lift_Height;  //Remembers if the Lift is raised or lowered
    public List<bool> rec_Lift_isTriggeredOnce; //For now, I'm also including this. Stores if the Lift has been recently triggered

    public RecStateList(Vector3 pass_rec_Player_Position, int pass_rec_player_faceInt, List<bool> pass_rec_Puff_Full,
                        Vector3[] pass_rec_listOf_Cloud_Positions, List<Sc_Storm> pass_rec_listOf_Cloud_Parents,
                        Vector3[] pass_rec_listOf_Storm_Positions, List<List<Sc_Cloud>> pass_Rec_listOf_listOf_Cloud_Children,
                        List<bool> pass_rec_Soil_State, List<bool> pass_rec_Lift_Height, List<bool> pass_rec_Lift_isTriggeredOnce)
    {
        rec_player_Position = pass_rec_Player_Position;
        rec_player_faceInt = pass_rec_player_faceInt;
        rec_listOf_Cloud_Positions = pass_rec_listOf_Cloud_Positions;
        rec_listOf_Cloud_Parents = pass_rec_listOf_Cloud_Parents;
        rec_listOf_Storm_Positions = pass_rec_listOf_Storm_Positions;
        rec_listOf_listOf_Cloud_Children = pass_Rec_listOf_listOf_Cloud_Children;
        rec_Puff_Full = pass_rec_Puff_Full;
        rec_Soil_State = pass_rec_Soil_State;
        rec_Lift_Height = pass_rec_Lift_Height;
        rec_Lift_isTriggeredOnce = pass_rec_Lift_isTriggeredOnce;
    }
}

// This version of the recStateList is serializable, thus allowing it to be saved
[System.Serializable]
public class RecStateList_V2
{
    public float[] rec_player_Position;
    public int rec_player_faceInt;
    // Puzzle states
    public bool[] rec_listOf_Puzzle_SolveState;
    // Artifact States
    public int[] rec_listOf_Artifact_ActionStates;

    //It's much easier to remember the positions of all the Clouds instead of the Puffs
    public float[][] rec_listOf_Cloud_Positions;
    //Currently, I'm remembering the positions of all Storms due to the effect of falling and recalling position.
    //A possible fix is to just always move the Clouds and never move any Storms
    public float[][] rec_listOf_Storm_Positions;
    public List<bool> rec_Puff_Full;    //Remembers if the Puff is filled or empty
    public List<bool> rec_Soil_State;   //Remembers the state of the soil
    public List<bool> rec_Lift_Height;  //Remembers if the Lift is raised or lowered
    public List<bool> rec_Lift_isTriggeredOnce; //For now, I'm also including this. Stores if the Lift has been recently triggered

    public RecStateList_V2(float[] pass_rec_Player_Position, int pass_rec_player_faceInt, bool[] pass_rec_ListOf_Puzzle_SolveState,
                        int[] pass_rec_listOf_Artifact_ActionStates, List<bool> pass_rec_Puff_Full, float[][] pass_rec_listOf_Cloud_Positions,
                        float[][] pass_rec_listOf_Storm_Positions, List<bool> pass_rec_Soil_State, List<bool> pass_rec_Lift_Height,
                        List<bool> pass_rec_Lift_isTriggeredOnce)
    {
        rec_player_Position = pass_rec_Player_Position;
        rec_player_faceInt = pass_rec_player_faceInt;
        rec_listOf_Puzzle_SolveState = pass_rec_ListOf_Puzzle_SolveState;
        rec_listOf_Artifact_ActionStates = pass_rec_listOf_Artifact_ActionStates;
        rec_listOf_Cloud_Positions = pass_rec_listOf_Cloud_Positions;
        rec_listOf_Storm_Positions = pass_rec_listOf_Storm_Positions;
        rec_Puff_Full = pass_rec_Puff_Full;
        rec_Soil_State = pass_rec_Soil_State;
        rec_Lift_Height = pass_rec_Lift_Height;
        rec_Lift_isTriggeredOnce = pass_rec_Lift_isTriggeredOnce;
    }
}

// Turning the list of recorded states into a class
[System.Serializable]
public class RecState_AsList
{
    public List<RecStateList_V2> listOf_RecStateList = new List<RecStateList_V2>();

    // Limit number of undos to avoid saving lag
    private static int maxUndos = 200;

    // Add to the list
    public void AddElement(RecStateList_V2 pass_RecStateList)
    {
        listOf_RecStateList.Add(pass_RecStateList);
        
        if (listOf_RecStateList.Count > maxUndos)
        {
            while (listOf_RecStateList.Count > maxUndos)
            {
                // Delete the first element
                RemoveFirstElement();
            }
        }
    }

    // Call from the list
    public RecStateList_V2 CallElement()
    {
        // Delete the last element and return the second to last element, if there is one
        if (listOf_RecStateList.Count > 1)
        {
            RemoveLastElement();
        }

        // If the list is accidentally empty. Just reinitialise it here
        //Debug.Log(listOf_RecStateList);

        return listOf_RecStateList.Last();
    }

    // Remove the last element from the list
    public void RemoveLastElement()
    {
        listOf_RecStateList.Remove(listOf_RecStateList.Last());
    }

    public void RemoveFirstElement()
    {
        listOf_RecStateList.Remove(listOf_RecStateList.First());
    }
}

// This class keeps track of player progress
[System.Serializable]
public class RecPlayerProgress
{
    // If the player has seen the good or bad ending
    public bool recState_BadEnding_Seen;
    public bool recState_GoodEnding_Seen;

    // The number of complete puzzles
    public float recState_PP_PuzzlePercentage;

    // What scene/zone the player is in
    public int recState_Scene;

    public RecPlayerProgress(   bool pass_RecState_BadEnding_Seen, bool pass_RecState_GoodEnding_Seen,
                                int pass_RecState_Scene, float pass_RecState_PP_PuzzlePercentage)
    {
        recState_BadEnding_Seen = pass_RecState_BadEnding_Seen;
        recState_GoodEnding_Seen = pass_RecState_GoodEnding_Seen;
        recState_Scene = pass_RecState_Scene;
        recState_PP_PuzzlePercentage = pass_RecState_PP_PuzzlePercentage;
    }
}

// This class keeps track of NPC changes?
[System.Serializable]
public class RecNPCStates
{
    
}

// Finally, we have a master class that keeps track of everything
// This only needs to be called when we're saving and dished out upon loading
[System.Serializable]
public class MasterData
{
    public RecState_AsList master_RecStat_AsList;
    public RecPlayerProgress master_RecPlayerProgress;
    public RecNPCStates master_RecNPCStates;

    // I could also record playtime, or in-game time passed
    // And other pieces of data that might be interesting. They won't
    // Take up that much space, so it's cool
    // Amount of moves, rotations etc...

}