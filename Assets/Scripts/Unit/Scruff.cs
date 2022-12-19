using Assets.Scripts.States;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scruff : Farmon
{
    public GameObject scruffTacklePrefab;
    private ScruffTackleState tackleState;

    protected override void Start()
    {
        base.Start();

        idleState = new IdleState(this);

        tackleState = new ScruffTackleState(this);
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void Attack(Farmon targetEnemy)
    {
        Projectile tackle = Instantiate(scruffTacklePrefab, transform.position, transform.rotation, transform).GetComponent<Projectile>();
        tackle.team = team;
        tackle.damage = 15 + (int)(10f * (float)Power / 3f);
        tackle.transform.localScale *= sphereCollider.radius * 1.2f / .35f;
        tackle.pierce = 0;
        tackle.specificTarget = targetEnemy;

        tackle.EventDestroy.AddListener(AttackOver);

        SetState(tackleState);
    }

    private void AttackOver()
    {
        SetState(idleState);
    }

    public override float AttackTime()
    {
        return 4f - GetModifiedSpeed()/13;
    }
}

public class ScruffTackleState : StateMachineState
{
    Farmon farmon;

    public ScruffTackleState(Farmon farmon)
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

        if(!farmon.targetTransform || !farmon.attackTarget)
        {
            _stateMachine.ChangeState(farmon.idleState);
            return;
        }

        farmon.maxSpeed = (farmon.GetMovementSpeed() + 2) * 3;

        farmon.SeekUnit();
    }
}