using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiralOut : MonoBehaviour
{
    [SerializeField]
    Rigidbody myRigidBody;
    
    public float RotationOffset = 0;

    public float MoveAwaySpeed = 1;

    public float RotationSpeed = 1;

    public float MaxSpeed = 5;

    Vector3 origin;

    float time = 0;

    private void Start()
    {
        origin = transform.position;
    }

    private void Update()
    {
        time += Time.deltaTime;

        Vector3 targetVelocity = (origin + GetPosition()) - transform.position;

        if(targetVelocity.magnitude > MaxSpeed)
        {
            targetVelocity = targetVelocity.normalized * MaxSpeed;
        }

        myRigidBody.velocity = new Vector3(targetVelocity.x, myRigidBody.velocity.y, targetVelocity.z);
    }

    Vector3 GetPosition()
    {
        Vector3 unrotated = new Vector3(time * MoveAwaySpeed, 0);

        Vector3 rotated = Quaternion.Euler(0, RotationOffset + time * RotationSpeed, 0) * unrotated;

        return rotated;
    }
}
