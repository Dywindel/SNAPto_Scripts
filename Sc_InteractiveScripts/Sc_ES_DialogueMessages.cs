using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

////////////////////////////////////////////////
//
//  Sc_ES_DialogueMessages
//  Stores what dialogue will be spoken by a character
//  Is triggered externally

public class Sc_ES_DialogueMessages : MonoBehaviour
{
    // References to World Objects
	protected Sc_RB_Animation rbA;		//Reference to the animation list

    // List of child triggers
    [HideInInspector]
    public Sc_ES_DialogueTrigger[] listOf_DialogueTriggers;

    // Which Trigger was activated
    [HideInInspector]
    public int dialogueTrigger_Active = 0;

    // Multidimensional string for NPC dialogue
    public MultidimensionalString[] messageStore = new MultidimensionalString[2];

    // Whether the character is facinig the player at different stages
    public bool[] set_isActiveFacing;
    private bool[] listOf_isActiveFacing;
    private int current_activeFaceDirection = 0;

    public Sc_RotateTowardsPlayer ref_ScriptRotateTowardsPlayer;

    // Which message number we're on
    [HideInInspector]
    public int messageInt = 0;
    // Which paragraph we're on
    [HideInInspector]
    public int paragraphInt = 0;

    // Sometimes we might want to activate a unity event
    public UnityEvent e_ActiveLocked;

    void Start()
    {
        // Find each identity trigger in children and pass them relevent details
        listOf_DialogueTriggers = this.GetComponentsInChildren<Sc_ES_DialogueTrigger>();
        for (int i = 0; i < listOf_DialogueTriggers.Length; i++)
        {
            // Pass this script
            listOf_DialogueTriggers[i].ref_Sc_DialogueMessages = this;
            // Pass it an identity value
            listOf_DialogueTriggers[i].dialgoueTrigger_Identity = i;
        }

        // Grab the Game Manager, dictionary, movement library...
		Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
		rbA = gM.rbA;

        // Set the boolean triggers to what the player has set
        listOf_isActiveFacing = new bool[messageStore.Length];
        for (int i = 0; i < listOf_isActiveFacing.Length; i++)
        {
            // Read from the set list in the editor to see if the npc should be facing the player
            if (set_isActiveFacing.Length == listOf_isActiveFacing.Length)
            {
                listOf_isActiveFacing[i] = set_isActiveFacing[i];
            }
            else
            {
                // The npc is always facing the player
                listOf_isActiveFacing[i] = true;
            }
        }

        // At the start, update if the NPC is facing or not
        Update_ifActiveFacing();
    }

    public void Update_ifActiveFacing()
    {
        if (ref_ScriptRotateTowardsPlayer != null)
        {
            ref_ScriptRotateTowardsPlayer.Act_ReturnToFaceDirection(listOf_isActiveFacing[current_activeFaceDirection]);
        }

        if (current_activeFaceDirection + 1 < listOf_isActiveFacing.Length)
            current_activeFaceDirection += 1;
    }

    // Cycle to the next message in the list
    // When you reach the final message, end the diaglogue and increase the interaction counter
    // When the player interacts again, a new set of dialogue will play
    public void CycleToNext_Diagloue()
    {
        messageInt += 1;

        if (messageInt >= messageStore[paragraphInt].Length)
        {
            // Update the active facing direction
            Update_ifActiveFacing();

            // Disable messages
            messageInt = -1;
            // Increase the paragraph counter, if we haven't reached the end yet
            if (paragraphInt + 1 < messageStore.Length)
            {
                paragraphInt += 1;
                
            }
            // If this is the last paragraph stored, run an event, if that even exists
            else if (e_ActiveLocked != null)
            {
                e_ActiveLocked.Invoke();
            }
        }
    }

    // This is a class I found online, user: Mortennobel
    // It allows me to create a 2D array of strings for NPC dialogue
    [System.Serializable]
    public class MultidimensionalString
    {
        public string[] stringArray = new string[0];

        public string this[int index] {
        get {
                return stringArray[index];
        }
        set { 
                stringArray[index] = value; 
            }
        }
        public int Length {
            get {
                return stringArray.Length;
            }
        }
        public long LongLength {
            get {
                return stringArray.LongLength;
            }
        }
    }
}
