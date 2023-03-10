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
        return 10f;
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
        spin.damage = 15 + Power / 2;
        spin.knockBack = 6;
        spin.hitStunTime = hitStunTime;
        spin.specificTarget = targetFarmon;
        spin.owner = this;
        spin.team = team;
        spin.CreateSound = ChargeUpSound;
        spin.HitSound = ThunkSound;

        spin.EventDestroy.AddListener(AttackComplete);

        return spin;
    }

    protected override void AttackComplete()
    {
        base.AttackComplete();
        HitStopSelf(hitStunTime);
    }

    protected override void GetLevelUpBonusStats(out int gritPlus, out int powerPlus, out int agilityPlus, out int focusPlus, out int luckPlus, out int pointsPlus)
    {
        base.GetLevelUpBonusStats(out gritPlus, out powerPlus, out agilityPlus, out focusPlus, out luckPlus, out pointsPlus);

        if (level % 2 == 0)
        {
            gritPlus++;
        }
        else
        {
            agilityPlus++;
        }
    }

    public override void OnLevelUp()
    {

    }
}

public class TorrentSpinChargeState : StateMachineState
{
    Tortorrent tortorrent;
    Timer chargeTimer = new Timer();
    Timer flipTimer = new Timer();

    Collider spinCollider;
    SpriteRenderer spinSpriteRenderer;

    public TorrentSpinChargeState(Tortorrent tortorrent)
    {
        this.tortorrent = tortorrent;
    }

    public override void Enter()
    {
        base.Enter();

        Farmon attackFarmon = tortorrent.GetAttackTargetFarmon();

        tortorrent.targetTransform = attackFarmon.transform;

        chargeTimer.SetTime(2f - tortorrent.Agility/Farmon.StatMax);
        flipTimer.SetTime(.001f);

        tortorrent.ImmuneToHitStop = true;

        Animator animator = tortorrent.Hud.Animator;
        animator.speed = 0;
        animator.Play(animator.GetCurrentAnimatorStateInfo(0).shortNameHash,0, .6f);

        Projectile spin = tortorrent.MakeSpin(attackFarmon);
        spinCollider = spin.GetComponent<Collider>();
        spinCollider.enabled = false;
        spinSpriteRenderer = spin.GetComponentInChildren<SpriteRenderer>();
    }

    public override void Exit()
    {
        base.Exit();
        tortorrent.ImmuneToHitStop = false;

        tortorrent.Hud.Animator.speed = 1;

        spinCollider.enabled = true;
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

        Farmon attackFarmon = tortorrent.GetAttackTargetFarmon();
        if (!tortorrent.targetTransform || !attackFarmon)
        {
            tortorrent.SetState(tortorrent.mainState);
            return;
        }

        spinSpriteRenderer.color = new Color(spinSpriteRenderer.color.r,
                                             spinSpriteRenderer.color.g,
                                             spinSpriteRenderer.color.b, 
                                             1.0f - chargeTimer.Percent);

        if (chargeTimer.Tick(Time.deltaTime))
        {
            tortorrent.SetState(tortorrent.SpinAttackState);
            return;
        }

        tortorrent.maxSpeed = 0;

        tortorrent.SeekUnit(false);
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
        Farmon attackFarmon = farmon.GetAttackTargetFarmon();

        farmon.targetTransform = attackFarmon.transform;
        flipTimer.SetTime(.05f);
        flipTimer.autoReset = true;

        timeoutTimer.SetTime(4f);

        farmon.Hud.AudioSource.clip = FarmonController.instance.DashSound;
        farmon.Hud.AudioSource.volume = .2f;
        farmon.Hud.AudioSource.Play();

        farmon.ImmuneToHitStop = true;

        farmon.maxSpeed = (farmon.GetMovementSpeed() + 2) * 3;

        Vector3 seek = farmon.Seek(false);
        farmon.rb.AddForce(seek, ForceMode.Impulse);
    }

    public override void Exit()
    {
        base.Exit();

        farmon.ImmuneToHitStop = false;
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

        Farmon attackFarmon = farmon.GetAttackTargetFarmon();
        if (!farmon.targetTransform || !attackFarmon)
        {
            farmon.SetState(farmon.mainState);
            return;
        }

        farmon.maxSpeed = (farmon.GetMovementSpeed() + 2) * 3;

        farmon.SeekUnit(false);
    }
}