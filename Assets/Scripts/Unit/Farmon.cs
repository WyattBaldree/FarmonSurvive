using System.Collections;
using UnityEngine;
using Assets.Scripts.States;

public class Farmon : Unit
{
    public GameObject fireBallPrefab;

    StateMachine farmonStateMachine;

    string name = "Farmon";

    Timer changeTargetLocationTimer;

    public static Vector3 GetRandomTargetLocation()
    {
        Vector3 targetOffset = Quaternion.Euler(0, 0, Random.Range(0, 359)) * new Vector3(0, Random.Range(1.5f, 3.5f), 0);
        return Player.instance.transform.position + targetOffset;
    }

    protected override void Start()
    {
        base.Start();

        farmonStateMachine = new StateMachine();

        farmonStateMachine.AddState("Attack", new AttackState(this));
        farmonStateMachine.AddState("Follow", new FollowState(this));
        farmonStateMachine.AddState("Spawn", new SpawnState(this));

        farmonStateMachine.InitializeStateMachine(farmonStateMachine.stateDictionary["Spawn"]);

        targetTransform = new GameObject(name + "Flag").transform;
        targetTransform.parent = Player.instance.transform;

        targetTransform.position = GetRandomTargetLocation();
        changeTargetLocationTimer = new Timer();
        changeTargetLocationTimer.SetTime(3f + Random.Range(0f, 2f));
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();

        farmonStateMachine.Tick();

        if (changeTargetLocationTimer.Tick(Time.deltaTime))
        {
            targetTransform.position = GetRandomTargetLocation();
            changeTargetLocationTimer.SetTime(3f + Random.Range(0f, 2f));
        }
    }

    private void OnDrawGizmos()
    {
        if(EnemyController.instance && EnemyController.instance.ClosestEnemy) Debug.DrawLine(transform.position, EnemyController.instance.ClosestEnemy.transform.position);
        if (targetTransform) Debug.DrawLine(transform.position, targetTransform.position);
    }
}

public class AttackState : StateMachineState
{
    Farmon farmon;
    Timer fireBallTimer;

    public AttackState(Farmon thisFarmon)
    {
        farmon = thisFarmon;

        fireBallTimer = new Timer();
    }

    public override void Enter()
    {
        base.Enter();

        farmon.myRigidBody.velocity = Vector3.zero;

        fireBallTimer.SetTime(.75f + Random.Range(0, 0.1f));
    }

    public override void Tick()
    {
        base.Tick();

        farmon.MoveToTarget(farmon.speed/4);

        if (fireBallTimer.Tick(Time.deltaTime))
        {
            Enemy targetEnemy = EnemyController.instance.ClosestEnemy;

            if (targetEnemy)
            {

                Vector3 unitToEnemy = targetEnemy.GetUnitVectorToMe(farmon.transform.position);

                Projectile fireBall = Farmon.Instantiate(farmon.fireBallPrefab, farmon.transform.position, farmon.transform.rotation).GetComponent<Projectile>();
                fireBall.rigidBody.velocity = unitToEnemy * 5f;

                fireBall = Farmon.Instantiate(farmon.fireBallPrefab, farmon.transform.position, farmon.transform.rotation).GetComponent<Projectile>();
                fireBall.rigidBody.velocity = Quaternion.Euler(0, 0, 15) * unitToEnemy * 5f;

                fireBall = Farmon.Instantiate(farmon.fireBallPrefab, farmon.transform.position, farmon.transform.rotation).GetComponent<Projectile>();
                fireBall.rigidBody.velocity = Quaternion.Euler(0, 0, -15) * unitToEnemy * 5f;
            }

            _stateMachine.ChangeState(_stateMachine.stateDictionary["Follow"]);
        }
    }
}

public class FollowState : StateMachineState
{
    Farmon farmon;
    Timer followTimer;

    public FollowState(Farmon thisFarmon)
    {
        farmon = thisFarmon;
        followTimer = new Timer();
    }

    public override void Enter()
    {
        base.Enter();

        followTimer.SetTime(Random.Range(0f, 3f));
    }

    public override void Tick()
    {
        base.Tick();

        farmon.MoveToTarget(farmon.speed);

        if (followTimer.Tick(Time.deltaTime))
        {
            _stateMachine.ChangeState(_stateMachine.stateDictionary["Attack"]);
        }
    }
}

public class SpawnState : StateMachineState
{
    Farmon farmon;
    Timer spawnTimer;

    public SpawnState(Farmon thisFarmon)
    {
        farmon = thisFarmon;
        spawnTimer = new Timer();
        spawnTimer.SetTime(3f);
    }

    public override void Tick()
    {
        base.Tick();

        farmon.MoveToTarget(farmon.speed);

        if (spawnTimer.Tick(Time.deltaTime))
        {
            _stateMachine.ChangeState(_stateMachine.stateDictionary["Follow"]);
        }
    }
}