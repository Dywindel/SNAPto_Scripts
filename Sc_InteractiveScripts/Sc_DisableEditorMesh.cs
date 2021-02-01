using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////
//
//	Sc_DisableEditorMesh - Disable Editor Mesh
//	This script can be attached to any object and it
//	will switch off the mesh during gameplay. Useful
//	For applying to editor objects

public class Sc_DisableEditorMesh : MonoBehaviour {

	void Start () 
	{
		MeshRenderer meshRef = this.gameObject.GetComponent<MeshRenderer>();
        meshRef.enabled = false;
	}
}
