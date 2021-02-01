using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script rotates an object to face the player
// It creates a 2D ring in the planar direction as
// a trigger for when the player is near

public class Sc_RotateTowardsPlayer : MonoBehaviour
{
    // World references
    private Sc_RB_Values rbV;			//Reference to the values list

    // If this effect is active
    public bool bool_IsActive = true;

    [Range(1, 10)]
    public float speed = 1;
    public bool CameraMode = false;
    public int triggerArea = 3;
    private SphereCollider ref_SphereCollider;
    // This is the neutral position a character faces when they aren't following the player
    public Vector3 ref_NeutralPosition = new Vector3(0, 0, -1);

    // This boolean actively causes following, when set true by player being in active trigger sphere
    private bool activelyFollowing = false;
    // Becomes true while the character is rotating back into position
    private bool return_Animation = false;
    private float return_Timer = 0f;
    private float return_TimeLimit = 2f;

    // For rotation in all three directions
    public bool planarRotation = true;
    // Do we want to NPCs to rotate to a specific direction when finished?
    public bool restingDirection = true;

    GameObject player;      // Reference to player

    void Start()
    {
        Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();

        // Grab the player
        player = gM.player.gameObject;

        // Grab reference values
		rbV = gM.rbV;

        // Create the trigger box
        ref_SphereCollider = this.gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
        // And set it's size and set it to istrigger
        ref_SphereCollider.radius = triggerArea;
        ref_SphereCollider.isTrigger = true;
    }

    // When the player enters the sphere collider
    void OnTriggerEnter(Collider hit)
    {
        GameObject item = hit.gameObject;
        if (item.gameObject.layer == rbV.layer_Player)
        {
            activelyFollowing = true;
            // Also, reset the timer and switch off the animation, just in case
            return_Animation = false;
            return_Timer = 0f;
        }
    }
    // And again for when the player leaves
    void OnTriggerExit(Collider hit)
    {
        GameObject item = hit.gameObject;
        if (item.gameObject.layer == rbV.layer_Player)
        {
            activelyFollowing = false;

            Act_ReturnToFaceDirection(bool_IsActive);
        }
    }

    public void Act_ReturnToFaceDirection(bool pass_IsActive)
    {
        bool_IsActive = pass_IsActive;
        
        // I don't just want the character to switch off movement, I want them to also
        // Rotate back to their original location, which will take a few seconds
        // Turn on the return animation and start the timer
        if (restingDirection)
            return_Animation = true;
        return_Timer = 0f;
    }

    // Speed is not making a difference for some reason
    void Update()
    {
        if (activelyFollowing && bool_IsActive)
        {
            if (Input.GetButton("Reset") && CameraMode)
                RotateTowards(Camera.main.transform.position - transform.position);
            else
                RotateTowards(player.transform.position - transform.position);
        }
        else if (return_Animation)
        {
            // As long as we haven't reached the time limit
            if (return_Timer < return_TimeLimit)
            {
                // We start by facing back towards our neutral position
                RotateTowards(ref_NeutralPosition);
                // We increment the timer
                return_Timer += Time.deltaTime;
            }
            // If we have reached the time limit
            else
            {
                // Stop the returnAnimation
                return_Animation = false;
            }
        }
    }

    void RotateTowards(Vector3 p_Target)
    {
        // Set the y value to zero so we only rotate in the planar radius
        if (planarRotation)
            p_Target.y = 0;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(p_Target, Vector3.up), speed*Time.deltaTime);
    }
}
