using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LookAtCamera : MonoBehaviour
{
    [SerializeField]
    float slant;

    [ExecuteInEditMode]
    private void LateUpdate()
    {
        Vector3 cameraFlattenedVector = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;

        transform.rotation = Quaternion.LookRotation(cameraFlattenedVector);

        transform.rotation *= Quaternion.Euler(new Vector3(slant, 0, 0));
    }
}
