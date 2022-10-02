using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public Transform targetTransform;

    public Rigidbody2D myRigidBody;

    public float speed = 1.0f;

    protected float targetDistance = 1.0f;

    

    // Update is called once per frame
    protected virtual void Update()
    {
        MoveToTarget();
    }

    void MoveToTarget()
    {
        Vector3 toTarget = (targetTransform.position - transform.position).normalized;

        Vector3 targetDistancePoint = targetTransform.position - toTarget * targetDistance;

        Vector3 toTargetDistancePoint = (targetDistancePoint - transform.position).normalized;

        myRigidBody.velocity = toTargetDistancePoint * speed;
    }
}