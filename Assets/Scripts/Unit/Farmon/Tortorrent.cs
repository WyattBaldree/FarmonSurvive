using Assets.Scripts.States;
using Assets.Scripts.Timer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tortorrent : Farmon
{
    public GameObject torrentSpinPrefab;

    public AudioClip ChargeUpSound;
    public AudioClip ThunkSound;

    [HideInInspector]
    public int shellHitCount = 0;

    public GameObject spinEffect;

    public override void Attack(Farmon targetEnemy)
    {
        SetState(new TorrentSpinChargeState(this));
    }

    public override float AttackTime()
    {
        return 10f;
    }

    protected override void Start()
    {
        base.Start();

        //SpinChargeState = new TorrentSpinChargeState(this);
        //SpinAttackState = new TorrentSpinAttackState(this);
    }

    public override void AttackComplete()
    {
        base.AttackComplete();
        HitStopSelf(.2f);
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
    SpriteRenderer spinSpriteRenderer;

    public TorrentSpinChargeState(Tortorrent tortorrent)
    {
        this.tortorrent = tortorrent;
    }

    public override void Enter()
    {
        base.Enter();

        Farmon attackFarmon = null;//tortorrent.GetAttackTargetFarmon();
        tortorrent.targetTransform = attackFarmon.transform;

        chargeTimer.SetTime(2f - tortorrent.Agility/Farmon.StatMax);
        flipTimer.SetTime(.001f);

        //Tortorrent cannot be knocked out of its charge
        tortorrent.ImmuneToHitStop = true;

        //Set the animation to the second frame so it looks like tortorrent is in its shell.
        Animator animator = tortorrent.Hud.Animator;
        animator.speed = 0;
        animator.Play(animator.GetCurrentAnimatorStateInfo(0).shortNameHash,0, .6f);

        //Create the spin sprite
        tortorrent.spinEffect = GameObject.Instantiate(tortorrent.torrentSpinPrefab, tortorrent.transform.position, tortorrent.transform.rotation, tortorrent.transform);
        spinSpriteRenderer = tortorrent.spinEffect.GetComponentInChildren<SpriteRenderer>();

        //Play the charge up sound.
        tortorrent.Hud.AudioSource.clip = tortorrent.ChargeUpSound;
        tortorrent.Hud.AudioSource.volume = .2f;
        tortorrent.Hud.AudioSource.Play();
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

        Farmon attackFarmon = null;// tortorrent.GetAttackTargetFarmon();
        if (!tortorrent.targetTransform || !attackFarmon)
        {
            //tortorrent.SetState(tortorrent.mainState);
            return;
        }

        spinSpriteRenderer.color = new Color(spinSpriteRenderer.color.r,
                                             spinSpriteRenderer.color.g,
                                             spinSpriteRenderer.color.b, 
                                             1.0f - chargeTimer.Percent);

        if (chargeTimer.Tick(Time.deltaTime))
        {
            tortorrent.Hud.AudioSource.clip = FarmonController.instance.DashSound;
            tortorrent.Hud.AudioSource.volume = .2f;
            tortorrent.Hud.AudioSource.Play();
            
            tortorrent.shellHitCount = 3;
            
            AttackData attackData = new AttackData(10 + tortorrent.Power / 3, 6, false, null, tortorrent.ThunkSound);
            //tortorrent.SetState(new TorrentSpinAttackState(tortorrent, tortorrent.attackTarget, attackData));

            return;
        }

        tortorrent.maxSpeed = tortorrent.GetMovementSpeed();

        tortorrent.MovementIdle();
    }
}

public class TorrentSpinAttackState : MeleeAttackState
{
    Timer flipTimer = new Timer();
    Timer timeoutTimer = new Timer();
    Tortorrent tortorrent;

    public TorrentSpinAttackState(Farmon farmon, uint farmonIdToAttack, AttackData attackData) : base(farmon, farmonIdToAttack, attackData)
    {
        tortorrent = (Tortorrent)farmon;
    }

    public override void Tick()
    {
        _farmon.maxSpeed = (_farmon.GetMovementSpeed() + 3) * 2;
        base.Tick();
    }

    public override void OnAttack()
    {
        base.OnAttack();

        tortorrent.shellHitCount--;

        //If we are not out of attacks, attack again.
        if (tortorrent.shellHitCount > 0)
        {
            // Search for our next target. Exclude the last farmon hit.
            List<Farmon> lastFarmonHit = new List<Farmon>() { Farmon.loadedFarmonMap[_farmonIdToAttack] };
            List<Farmon> attackTargetList = Farmon.SearchFarmon(    tortorrent,
                                                                    Farmon.FarmonFilterEnum.enemyTeam,  
                                                                    Farmon.FarmonSortEnum.nearest, 
                                                                    LevelController.Instance.gridSize * 4,
                                                                    lastFarmonHit);
            
            //If a target was found, attack it.
            if (attackTargetList.Count > 0)
            {
                AttackData attackData = new AttackData(10 + tortorrent.Power / 3, 6, false, null, tortorrent.ThunkSound);
                tortorrent.SetState(new TorrentSpinAttackState(tortorrent, attackTargetList[0].loadedFarmonMapId, attackData));
                return;
            }
        }


        //If a new attack was not started, just return to the main state.
        GameObject.Destroy(tortorrent.spinEffect);
        tortorrent.SetState(_farmon.mainBattleState);

        _farmon.AttackComplete();
    }
}