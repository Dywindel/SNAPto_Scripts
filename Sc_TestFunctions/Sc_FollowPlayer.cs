using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script allows the attached object to follow the player at a defined rate
// Through a defined plane

public class Sc_FollowPlayer : MonoBehaviour
{
    // World references
    private Sc_RB_Values rbV;			//Reference to the values list

    public bool planar = true;     // only X and Y/Z movement
    public bool yAxisOnly = false;  // For only Z/Y movement
    public bool exponentialMoveSpeed = true;     // For movement speed type
    public bool reverse = false;    // If running from the player
    public bool instantMoveSpeed;   // To basically teleport
    public int speed = 200000;   // Relative exponential speed of object movement
    public bool isCloudLayer = false;    // There are some specific effects that should only effect the cloud layer
    public Vector3 offset = new Vector3(0f, 0f, 0f);
    public GameObject followThis;   // Follow the player is this is empty

    GameObject player;      // Reference to player

    void Start()
    {
        Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
        player = gM.player.gameObject;

        // Grab reference values
		rbV = gM.rbV;
    }

    void Update()
    {
        Vector3 objectPos = new Vector3(0f, 0f, 0f);
        Vector3 focusPos;

        if (followThis == null)
            objectPos = player.transform.position;
        else
            objectPos = followThis.transform.position;

        if (planar)
            focusPos = new Vector3(objectPos.x, this.transform.position.y, player.transform.position.z);
        else if(yAxisOnly)
        {
            focusPos = new Vector3(this.transform.position.x, objectPos.y, this.transform.position.z);
        }
        else
            focusPos = objectPos;
        
        // Add offset
        focusPos = focusPos + offset;
        
        Vector3 thisPos = this.transform.position;

        if (reverse)
        {
            // Collect the reverse of the player's position
            Vector3 reverse_PlayerPos = thisPos - (focusPos - thisPos);
            this.transform.position = Vector3.Lerp(thisPos, reverse_PlayerPos, 0.0001f);
        }
        else if (exponentialMoveSpeed)
        {
            // To create a more exponential speed of the cloud, I can find the magnitude of the distances
            float distPos = Vector3.Distance(thisPos, focusPos);
            this.transform.position = Vector3.Lerp(thisPos, focusPos, distPos*distPos/((float)speed*100000f));
        }
        else if (instantMoveSpeed)
        {
            this.transform.position = focusPos;
        }
        else
        {
            // For a more linear movement speed
            this.transform.position = Vector3.Lerp(thisPos, focusPos, 0.01f);
        }

        // This effect causes the y position from the player to increase as the player moves forward in the Z direction
        if (isCloudLayer)
        {
            float temp_yPos = (-(player.transform.position.z + 10f)/22f) + 2f;
            if (temp_yPos >= rbV.baseLevel + 1f)
                temp_yPos = rbV.baseLevel + 1f;
            else if (temp_yPos <= -rbV.baseLevel + 1f)
                temp_yPos = -rbV.baseLevel + 1f;
            // The y value will depend on the player's Z value
            focusPos = new Vector3(focusPos.x, temp_yPos, focusPos.z);
            
            // For a more linear movement speed
            this.transform.position = Vector3.Lerp(thisPos, focusPos, 0.01f);
        }
    }
}
