using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Player : MonoBehaviour
{
    public static Player instance;

    public Rigidbody2D myRigidBody;

    public float speed = 1;

    private void Awake()
    {
        Assert.IsNull(instance, "There should only ever be one player.");
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    private void Update()
    {
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            moveDirection += Vector3.up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveDirection += Vector3.down;
        }

        if (Input.GetKey(KeyCode.A))
        {
            moveDirection += Vector3.left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveDirection += Vector3.right;
        }

        myRigidBody.velocity = moveDirection.normalized * speed;
    }
}
