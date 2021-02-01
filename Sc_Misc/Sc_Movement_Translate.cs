using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_Movement_Translate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Translate(0f, 0f, Time.deltaTime*2.5f);
    }
}
