using System.Collections;
using System.Collections.Generic;
using UnityEngine;

////////////////////////////////////////////////
//  Sc_ES_TurnToStone
//  Turns an object to stone effect

public class Sc_ES_TurnToStone : MonoBehaviour
{
    private Material ref_Material;
    public Color startColor;
    private Color endColor = new Color(0.7f, 0.7f, 0.7f);

    // Start is called before the first frame update
    void Start()
    {
        ref_Material = GetComponent<SkinnedMeshRenderer>().materials[0];
        ref_Material.color = startColor;
    }

    public void Active_TurnedToStone()
    {
        StartCoroutine(TurnedToStone());
    }

    //Turn to Stone
	private IEnumerator TurnedToStone()
	{
		for (float t = 0; t < 1.0f; t += Time.deltaTime/8.0f)
		{
			ref_Material.color = Color.Lerp(startColor, endColor, t);

			yield return null;
		}

		ref_Material.color = endColor;
	}
}
