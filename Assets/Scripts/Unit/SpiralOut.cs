using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiralOut : MonoBehaviour
{
    public float rotationOffset = 0;

    [SerializeField]
    Rigidbody myRigidBody;
    [SerializeField]
    float moveAwaySpeed = 1;
    [SerializeField]
    float rotationSpeed = 1;

    Vector3 origin;

    float time = 0;

    private void Start()
    {
        origin = transform.position;
    }

    private void Update()
    {
        time += Time.deltaTime;

        myRigidBody.MovePosition(origin + GetPosition());
    }

    Vector3 GetPosition()
    {
        Vector3 unrotated = new Vector3(time * moveAwaySpeed, 0);

        Vector3 rotated = Quaternion.Euler(0, rotationOffset + time * rotationSpeed, 0) * unrotated;

        return rotated;
    }
}
