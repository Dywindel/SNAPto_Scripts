using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script performs whichever actions is decided by this
// Levels Marker's design
// The inspector decides which marker type this is

public class Sc_LevelStatusMarker_Complete : MonoBehaviour
{
    // World References
    private Sc_AC aC;
    private Sc_RB_Animation rbA;

    // Set the level marker type
    // 0 - Generic, 1 - Cloth, 2 - Plant pot, 3 - ???
    public int markerType = 0;

    // Animate a cloth component when this puzzle is complete
    public Cloth ref_LevelMarker_Cloth;
    public SkinnedMeshRenderer ref_Cloth_Material;

    // For the scounce component
    public GameObject ref_LevelMarker_Scounce_Topper;
    public GameObject ref_LevelMarker_Scounce_FirePS;

    // For the winter light
    public GameObject ref_LevelMarker_Lantern_Light;
    public MeshRenderer ref_LevelMarker_Lantern_BulbMaterial;
    public Material ref_LevelMarker_Lantern_BulbMaterial_On;

    void Start()
    {
        Sc_GM gM = GameObject.FindGameObjectWithTag("GM").GetComponent<Sc_GM>();
        aC = gM.aC;
        rbA = gM.rbA;
    }

    public void LevelMarker_Animate()
    {
        // For a generic item
        if (markerType == 0)
        {
            // Just disable the level Marker
            this.gameObject.SetActive(false);
        }

        // For a cloth component
        else if (markerType == 1)
        {
            // Animate the cloth
            if (ref_LevelMarker_Cloth != null)
            {
                ref_LevelMarker_Cloth.externalAcceleration = new Vector3(-5f, -10f, 6f);
            }
            
            // Coroutine for changing the cloth colour
            StartCoroutine(rbA.Cloth_ChangeColour(ref_Cloth_Material));

            // Play SFX
            aC.Play_SFX("SFX_Banner_Flap");
        }

        // For a Scounce component
        else if (markerType == 2)
        {
            // Make the topper embers and fire PS appear
            if (ref_LevelMarker_Scounce_Topper != null)
            {
                ref_LevelMarker_Scounce_Topper.SetActive(true);
            }
            if (ref_LevelMarker_Scounce_FirePS != null)
            {
                ref_LevelMarker_Scounce_FirePS.SetActive(true);
            }

            // Play SFX
            aC.Play_SFX("SFX_Fire_Fwoosh");
        }

        // For a winter component
        else if (markerType == 3)
        {
            // Make the light switch on
            if (ref_LevelMarker_Lantern_Light != null)
            {
                ref_LevelMarker_Lantern_Light.GetComponent<Light>().range = 5f;
            }
            // Change the lgiht bulb material to look like it's on
            ref_LevelMarker_Lantern_BulbMaterial.material = ref_LevelMarker_Lantern_BulbMaterial_On;

            // Play SFX
            aC.Play_SFX("SFX_Switch_Breaker");
        }
    }
}
