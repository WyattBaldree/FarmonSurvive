using Assets.Scripts.States;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Player : Farmon
{
    public static Player instance;

    public override void Attack(Farmon targetUnit)
    {
        
    }

    protected override void Awake()
    {
        Assert.IsNull(instance, "There should only ever be one player.");
        instance = this;
    }

    protected override void OnDestroy()
    {
        instance = null;
    }

    protected override void Start()
    {
        base.Start();

        idleState = new PlayerState(this);
        maxSpeed = 2;
    }

    protected void OnDrawGizmos()
    {
        DebugExtension.DrawCircle(transform.position, Vector3.up, Color.green, nearPlayerDistance);
    }

    public override float AttackTime()
    {
        return 0;
    }
}


public class PlayerState : StateMachineState
{
    Farmon farmon;

    public PlayerState(Farmon farmon)
    {
        this.farmon = farmon;
    }

    public override void Enter()
    {
        base.Enter();

        farmon.targetTransform = farmon.GetTargetEnemy().transform;
    }

    public override void Tick()
    {
        base.Tick();

        Vector3 cameraForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
        Vector3 cameraRight = Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized;
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            moveDirection += cameraForward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveDirection -= cameraForward;
        }

        if (Input.GetKey(KeyCode.D))
        {
            moveDirection += cameraRight;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            moveDirection -= cameraRight;
        }

        if (moveDirection != Vector3.zero) farmon.MoveInDirection(moveDirection);
    }
}