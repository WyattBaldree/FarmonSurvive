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
        base.Attack(targetEnemy);
        AttackData tortorrentAttackData = new AttackData(5, 6, false, ChargeUpSound, ThunkSound);

        SetState(new TortorrentChargeState(this, targetEnemy.loadedFarmonMapId, tortorrentAttackData, 3, .3f));
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

public class TortorrentChargeState : ChargeAttackState
{
    public TortorrentChargeState(Farmon thisFarmon, uint farmonIdToAttack, AttackData attackData, float chargeTime, float hitStun = 0.3F) : base(thisFarmon, farmonIdToAttack, attackData, chargeTime, hitStun)
    {
    }

    public override void OnAttack()
    {
        base.OnAttack();

        farmon.mainBattleState = new TortorrentAttackState(farmon, _farmonIdToAttack, _attackData, _hitStun);
        farmon.SetState(farmon.mainBattleState);
    }
}

public class TortorrentAttackState : MeleeAttackState
{
    private int _charges;
    public TortorrentAttackState(Farmon thisFarmon, uint farmonIdToAttack, AttackData attackData, float hitStun = 0.3F, int charges = 3) : base(thisFarmon, farmonIdToAttack, attackData, hitStun)
    {
        _charges = charges;
    }

    public override void OnAttack()
    {
        base.OnAttack();

        _charges--;

        List<Farmon> closestEnemies = Farmon.SearchFarmon(farmon, Farmon.FarmonFilterEnum.enemyTeam, Farmon.FarmonSortEnum.nearestFlat);
        //Remove the enemy we just attacked so we don't just keep attacking the same enemy.
        closestEnemies.Remove(Farmon.GetFarmonInstanceFromLoadedID(_farmonIdToAttack, false));

        //Remove all enemies over 4 spaces away.
        closestEnemies.RemoveAll((f) =>
        {
            return Vector3.Distance(f.transform.position, farmon.transform.position) > LevelController.Instance.gridSize * 4;
        });

        if (_charges >= 0 && closestEnemies.Count > 0)
        {
            farmon.mainBattleState = new TortorrentAttackState(farmon, closestEnemies[0].loadedFarmonMapId, _attackData, _hitStun, _charges-1);
            farmon.SetState(farmon.mainBattleState);
        }
        else
        {
            farmon.AttackComplete();
            farmon.GetNextAction();
        }
    }
}



//public class TorrentSpinChargeState : StateMachineState
//{
//    Tortorrent tortorrent;
//    Timer chargeTimer = new Timer();
//    Timer flipTimer = new Timer();
//    SpriteRenderer spinSpriteRenderer;

//    public TorrentSpinChargeState(Tortorrent tortorrent)
//    {
//        this.tortorrent = tortorrent;
//    }

//    public override void Enter()
//    {
//        base.Enter();

//        Farmon attackFarmon = null;//tortorrent.GetAttackTargetFarmon();
//        tortorrent.targetTransform = attackFarmon.transform;

//        chargeTimer.SetTime(2f - tortorrent.Agility/Farmon.StatMax);
//        flipTimer.SetTime(.001f);

//        //Tortorrent cannot be knocked out of its charge
//        tortorrent.ImmuneToHitStop = true;

//        //Set the animation to the second frame so it looks like tortorrent is in its shell.
//        Animator animator = tortorrent.Hud.Animator;
//        animator.speed = 0;
//        animator.Play(animator.GetCurrentAnimatorStateInfo(0).shortNameHash,0, .6f);

//        //Create the spin sprite
//        tortorrent.spinEffect = GameObject.Instantiate(tortorrent.torrentSpinPrefab, tortorrent.transform.position, tortorrent.transform.rotation, tortorrent.transform);
//        spinSpriteRenderer = tortorrent.spinEffect.GetComponentInChildren<SpriteRenderer>();

//        //Play the charge up sound.
//        tortorrent.Hud.AudioSource.clip = tortorrent.ChargeUpSound;
//        tortorrent.Hud.AudioSource.volume = .2f;
//        tortorrent.Hud.AudioSource.Play();
//    }

//    public override void Exit()
//    {
//        base.Exit();
//        tortorrent.ImmuneToHitStop = false;

//        tortorrent.Hud.Animator.speed = 1;
//    }

//    public override void Tick()
//    {
//        base.Tick();


//        if (flipTimer.Tick(Time.deltaTime))
//        {
//            tortorrent.mySpriteRenderer.flipX = !tortorrent.mySpriteRenderer.flipX;

//            if(chargeTimer.Percent > .8f)
//            {
//                flipTimer.SetTime(.15f);
//            }
//            else
//            {
//                flipTimer.SetTime(.08f);
//            }
//        }

//        Farmon attackFarmon = null;// tortorrent.GetAttackTargetFarmon();
//        if (!tortorrent.targetTransform || !attackFarmon)
//        {
//            //tortorrent.SetState(tortorrent.mainState);
//            return;
//        }

//        spinSpriteRenderer.color = new Color(spinSpriteRenderer.color.r,
//                                             spinSpriteRenderer.color.g,
//                                             spinSpriteRenderer.color.b, 
//                                             1.0f - chargeTimer.Percent);

//        if (chargeTimer.Tick(Time.deltaTime))
//        {
//            tortorrent.Hud.AudioSource.clip = FarmonController.instance.DashSound;
//            tortorrent.Hud.AudioSource.volume = .2f;
//            tortorrent.Hud.AudioSource.Play();

//            tortorrent.shellHitCount = 3;

//            AttackData attackData = new AttackData(10 + tortorrent.Power / 3, 6, false, null, tortorrent.ThunkSound);
//            //tortorrent.SetState(new TorrentSpinAttackState(tortorrent, tortorrent.attackTarget, attackData));

//            return;
//        }

//        tortorrent.maxSpeed = tortorrent.GetMovementSpeed();

//        tortorrent.MovementIdle();
//    }
//}

//public class TorrentSpinAttackState : MeleeAttackState
//{
//    Timer flipTimer = new Timer();
//    Timer timeoutTimer = new Timer();
//    Tortorrent tortorrent;

//    public TorrentSpinAttackState(Farmon farmon, uint farmonIdToAttack, AttackData attackData) : base(farmon, farmonIdToAttack, attackData)
//    {
//        tortorrent = (Tortorrent)farmon;
//    }

//    public override void Tick()
//    {
//        _farmon.maxSpeed = (_farmon.GetMovementSpeed() + 3) * 2;
//        base.Tick();
//    }

//    public override void OnAttack()
//    {
//        base.OnAttack();

//        tortorrent.shellHitCount--;

//        //If we are not out of attacks, attack again.
//        if (tortorrent.shellHitCount > 0)
//        {
//            // Search for our next target. Exclude the last farmon hit.
//            List<Farmon> lastFarmonHit = new List<Farmon>() { Farmon.loadedFarmonMap[_farmonIdToAttack] };
//            List<Farmon> attackTargetList = Farmon.SearchFarmon(    tortorrent,
//                                                                    Farmon.FarmonFilterEnum.enemyTeam,  
//                                                                    Farmon.FarmonSortEnum.nearest, 
//                                                                    LevelController.Instance.gridSize * 4,
//                                                                    lastFarmonHit);

//            //If a target was found, attack it.
//            if (attackTargetList.Count > 0)
//            {
//                AttackData attackData = new AttackData(10 + tortorrent.Power / 3, 6, false, null, tortorrent.ThunkSound);
//                tortorrent.SetState(new TorrentSpinAttackState(tortorrent, attackTargetList[0].loadedFarmonMapId, attackData));
//                return;
//            }
//        }


//        //If a new attack was not started, just return to the main state.
//        GameObject.Destroy(tortorrent.spinEffect);
//        tortorrent.SetState(_farmon.mainBattleState);

//        _farmon.AttackComplete();
//    }
//}