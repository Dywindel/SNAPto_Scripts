using System.Collections;
using System.Collections.Generic;
using UnityEngine;


////////////////////////////////////////////////
//  Sc_ES_RigidifyCloth
//  Hards a cloth object, looks like its turning to stone

public class Sc_ES_RigidifyCloth : MonoBehaviour
{
    private Cloth ref_Cloth;

    // Start is called before the first frame update
    void Start()
    {
        ref_Cloth = GetComponent<Cloth>();
    }

    public void Activate_RigidifyCloth()
    {
        StartCoroutine(RigidifyCloth());
    }

    private IEnumerator RigidifyCloth()
    {
        for (float t = 0; t < 1.0f; t += Time.deltaTime/12.0f)
		{
            ref_Cloth.stretchingStiffness = Mathf.Lerp(0.2f, 1.0f, t);
            ref_Cloth.bendingStiffness = Mathf.Lerp(0.2f, 1.0f, t);

			ref_Cloth.damping = Mathf.Lerp(0.0f, 1.0f, t);

			yield return null;
		}

        ref_Cloth.stretchingStiffness = 1.0f;
        ref_Cloth.bendingStiffness = 1.0f;

        ref_Cloth.damping = 1.0f;
    }
}
