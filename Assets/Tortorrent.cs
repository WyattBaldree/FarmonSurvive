using Assets.Scripts.States;
using Assets.Scripts.Timer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tortorrent : Farmon
{
    public GameObject torrentSpinPrefab;

    public TorrentSpinChargeState SpinChargeState;
    public TorrentSpinAttackState SpinAttackState;

    public override void Attack(Farmon targetEnemy)
    {
        SetState(SpinChargeState);
    }

    public override float AttackTime()
    {
        return 4f;
    }

    protected override void Start()
    {
        base.Start();

        idleState = new IdleState(this);
        SpinChargeState = new TorrentSpinChargeState(this);
        SpinAttackState = new TorrentSpinAttackState(this);
    }

    public Projectile MakeSpin(Farmon targetFarmon)
    {
        Projectile spin = Instantiate(torrentSpinPrefab, transform.position, transform.rotation, transform).GetComponent<Projectile>();
        spin.team = team;
        spin.damage = 15 + (int)(10f * (float)Power / 3f);
        spin.specificTarget = targetFarmon;

        spin.EventDestroy.AddListener(AttackOver);

        return spin;
    }

    private void AttackOver()
    {
        SetState(idleState);
    }
}

public class TorrentSpinChargeState : StateMachineState
{
    Tortorrent tortorrent;
    Timer chargeTimer = new Timer();
    Timer flipTimer = new Timer();

    public TorrentSpinChargeState(Tortorrent tortorrent)
    {
        this.tortorrent = tortorrent;
    }

    public override void Enter()
    {
        base.Enter();

        tortorrent.targetTransform = tortorrent.attackTarget.transform;

        chargeTimer.SetTime(2.5f - tortorrent.Speed/40);
        flipTimer.SetTime((chargeTimer.Percent/1.5f) + .01f);
    }

    public override void Tick()
    {
        base.Tick();

        if (flipTimer.Tick(Time.deltaTime))
        {
            tortorrent.mySpriteRenderer.flipX = !tortorrent.mySpriteRenderer.flipX;

            if(chargeTimer.Percent > .7f)
            {
                flipTimer.SetTime(chargeTimer.Percent - 0.7f + .05f);
            }
            else
            {
                flipTimer.SetTime(.1f);
            }
        }

        if (!tortorrent.targetTransform || !tortorrent.attackTarget)
        {
            _stateMachine.ChangeState(tortorrent.idleState);
            return;
        }

        if (chargeTimer.Tick(Time.deltaTime))
        {
            Projectile spin = tortorrent.MakeSpin(tortorrent.attackTarget);
            tortorrent.SetState(tortorrent.SpinAttackState);
            return;
        }

        tortorrent.maxSpeed = 0;

        tortorrent.SeekUnit();
    }
}

public class TorrentSpinAttackState : StateMachineState
{
    Farmon farmon;
    Timer flipTimer = new Timer();
    Timer timeoutTimer = new Timer();

    public TorrentSpinAttackState(Farmon farmon)
    {
        this.farmon = farmon;
    }

    public override void Enter()
    {
        base.Enter();

        farmon.targetTransform = farmon.attackTarget.transform;
        flipTimer.SetTime(.05f);
        flipTimer.autoReset = true;

        timeoutTimer.SetTime(4f);
    }

    public override void Tick()
    {
        base.Tick();

        if (timeoutTimer.Tick(Time.deltaTime))
        {
            _stateMachine.ChangeState(farmon.idleState);
            return;
        }

        if (flipTimer.Tick(Time.deltaTime))
        {
            farmon.mySpriteRenderer.flipX = !farmon.mySpriteRenderer.flipX;
        }

        if (!farmon.targetTransform || !farmon.attackTarget)
        {
            _stateMachine.ChangeState(farmon.idleState);
            return;
        }

        farmon.maxSpeed = (farmon.GetMovementSpeed() + 2) * 3;

        farmon.SeekUnit();
    }
}