using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_GizmoMesh : MonoBehaviour
{
    [SerializeField]
    private Mesh[] gizmoMesh;
    [SerializeField]
    private Color[] gizmoColor;
    [SerializeField]
    private bool gizmoGrid = false;

    // Gizmo icon for Dialogue boxes
    private void OnDrawGizmos()
    {
        // Will allow the gizmo to rotate with the object by setting its position/rotation/scale in local space
        Gizmos.matrix = this.transform.localToWorldMatrix;

        if (gizmoMesh.Length == gizmoColor.Length)
        {
            for (int i = 0; i < gizmoMesh.Length; i++)
            {
                Gizmos.color = gizmoColor[i];
                if (gizmoGrid)
                {
                    Gizmos.DrawWireMesh(gizmoMesh[i], Vector3.zero, Quaternion.identity);
                }
                else
                {
                    // Because we're in local space, the gizmo's position should now be the origin
                    Gizmos.DrawMesh(gizmoMesh[i], Vector3.zero, Quaternion.identity);
                }

            }
        }
    }
}
