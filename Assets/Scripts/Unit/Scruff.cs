using Assets.Scripts.States;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scruff : Farmon
{
    public GameObject scruffTacklePrefab;
    private ScruffTackleState tackleState;

    public float hitStunTime = .2f;

    protected override void Start()
    {
        base.Start();

        mainState = new IdleState(this);

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
        tackle.knockBack = 6;
        tackle.hitStunTime = hitStunTime;
        tackle.pierce = 0;
        tackle.specificTarget = targetEnemy;
        tackle.owner = this;

        tackle.EventDestroy.AddListener(AttackComplete);

        SetState(tackleState);
    }

    protected override void AttackComplete()
    {
        base.AttackComplete();
        HitStopSelf(hitStunTime);
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
        farmon.ImmuneToHitStop = true;
    }

    public override void Exit()
    {
        base.Exit();
        farmon.ImmuneToHitStop = false;
    }

    public override void Tick()
    {
        base.Tick();

        if(!farmon.targetTransform || !farmon.attackTarget)
        {
            _stateMachine.ChangeState(farmon.mainState);
            return;
        }

        farmon.maxSpeed = (farmon.GetMovementSpeed() + 2) * 3;

        farmon.SeekUnit();
    }
}