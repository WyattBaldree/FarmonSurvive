using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float RotateSpeed = 5;

    public Vector3 RotationAxis = Vector3.up;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(RotationAxis, 5 * Time.deltaTime);
    }
}
