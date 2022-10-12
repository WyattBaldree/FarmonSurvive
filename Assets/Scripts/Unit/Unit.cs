using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public Transform targetTransform;

    public Rigidbody2D myRigidBody;
    public Collider2D myCollider;

    public float speed = 1.0f;

    protected float targetDistance = 0f;
    GameObject shadow;

    protected virtual void Start()
    {
        shadow = Instantiate(UnitController.instance.ShadowPrefab, this.transform.position + Vector3.down * myCollider.bounds.extents.y, Quaternion.identity, this.transform);
        shadow.transform.localScale = myCollider.bounds.extents*2;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    public void MoveToTarget(float moveSpeed)
    {
        Vector3 toTarget = (targetTransform.position - transform.position).normalized;

        Vector3 targetDistancePoint = targetTransform.position - toTarget * targetDistance;

        Vector3 toTargetDistancePoint = (targetDistancePoint - transform.position).normalized;

        myRigidBody.velocity = toTargetDistancePoint * moveSpeed;
    }
}