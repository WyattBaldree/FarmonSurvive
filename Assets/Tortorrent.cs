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

    public AudioClip ChargeUpSound;
    public AudioClip ThunkSound;

    public float hitStunTime = .2f;

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

        SpinChargeState = new TorrentSpinChargeState(this);
        SpinAttackState = new TorrentSpinAttackState(this);
    }

    public Projectile MakeSpin(Farmon targetFarmon)
    {
        Projectile spin = Instantiate(torrentSpinPrefab, transform.position, transform.rotation, transform).GetComponent<Projectile>();
        spin.team = team;
        spin.damage = 15 + (int)(10f * (float)Power / 3f);
        spin.knockBack = 6;
        spin.hitStunTime = hitStunTime;
        spin.specificTarget = targetFarmon;
        spin.owner = this;

        spin.EventDestroy.AddListener(AttackComplete);

        return spin;
    }

    protected override void AttackComplete()
    {
        base.AttackComplete();
        HitStopSelf(hitStunTime);

        Hud.AudioSource.clip = ThunkSound;
        Hud.AudioSource.volume = .4f;
        Hud.AudioSource.Play();
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

        chargeTimer.SetTime(2f - tortorrent.Speed/40);
        flipTimer.SetTime(.001f);

        tortorrent.Hud.AudioSource.clip = tortorrent.ChargeUpSound;
        tortorrent.Hud.AudioSource.volume = .3f;
        tortorrent.Hud.AudioSource.Play();

        tortorrent.ImmuneToHitStop = true;

        Animator animator = tortorrent.Hud.Animator;
        animator.speed = 0;
        animator.Play(animator.GetCurrentAnimatorStateInfo(0).shortNameHash,0, .6f);
    }

    public override void Exit()
    {
        base.Exit();
        tortorrent.ImmuneToHitStop = false;

        tortorrent.Hud.Animator.speed = 1;
    }

    public override void Tick()
    {
        base.Tick();

        if (flipTimer.Tick(Time.deltaTime))
        {
            tortorrent.mySpriteRenderer.flipX = !tortorrent.mySpriteRenderer.flipX;

            if(chargeTimer.Percent > .8f)
            {
                flipTimer.SetTime(.15f);
            }
            else
            {
                flipTimer.SetTime(.08f);
            }
        }

        if (!tortorrent.targetTransform || !tortorrent.attackTarget)
        {
            tortorrent.SetState(tortorrent.mainState);
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

        farmon.Hud.AudioSource.clip = FarmonController.instance.DashSound;
        farmon.Hud.AudioSource.volume = .4f;
        farmon.Hud.AudioSource.Play();

        farmon.maxSpeed = (farmon.GetMovementSpeed() + 2) * 3;

        Vector3 seek = farmon.Seek();
        farmon.rb.AddForce(seek, ForceMode.Impulse);
    }

    public override void Tick()
    {
        base.Tick();

        if (timeoutTimer.Tick(Time.deltaTime))
        {
            farmon.SetState(farmon.mainState);
            return;
        }

        if (flipTimer.Tick(Time.deltaTime))
        {
            farmon.mySpriteRenderer.flipX = !farmon.mySpriteRenderer.flipX;
        }

        if (!farmon.targetTransform || !farmon.attackTarget)
        {
            farmon.SetState(farmon.mainState);
            return;
        }

        farmon.maxSpeed = (farmon.GetMovementSpeed() + 2) * 3;

        farmon.SeekUnit();
    }
}