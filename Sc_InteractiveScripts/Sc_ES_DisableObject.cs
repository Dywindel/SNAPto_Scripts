using System.Collections;
using System.Collections.Generic;
using UnityEngine;

////////////////////////////////////////////////
// Sc_ES_DisableObject
// Disables the game object when script is run

public class Sc_ES_DisableObject : MonoBehaviour
{
    public void DisableSelf()
    {
        this.gameObject.SetActive(false);
    }
}
