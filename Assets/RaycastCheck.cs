using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RaycastCheck : MonoBehaviour
{

    bool debugBool = false;

    RaycastHit HitNorth;

    
    void Update()
    {
        Physics.queriesHitBackfaces = true;

        if (Physics.Raycast(transform.position, Vector3.down, out HitNorth, 5, LayerMask.GetMask("Default")))
        {
            if (Vector3.Dot(HitNorth.normal, Vector3.down) < 0)
            {
                debugBool = true;
            }
            else
            {
                debugBool = false;
            }
        }

        Physics.queriesHitBackfaces = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 1f);
        if (debugBool) Gizmos.color = new Color(0, 1, 0, 1f);
        Gizmos.DrawSphere(HitNorth.point, .1f);
    }
}
