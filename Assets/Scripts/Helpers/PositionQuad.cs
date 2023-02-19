using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PositionQuad : MonoBehaviour
{
    private SphereCollider sc;
    [SerializeField]
    private Transform quad;

    [ExecuteInEditMode]
    public void Update()
    {
        if(sc == null)
        {
            sc = GetComponentInParent<SphereCollider>();
        }

        if (sc)
        {
            quad.position = sc.transform.position + sc.center + (sc.radius * Vector3.down);
        }
    }

    private void OnValidate()
    {
        Update();
    }
}