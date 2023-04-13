using Assets.Scripts.States;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scruff : Farmon
{
    public GameObject scruffTacklePrefab;
    private ScruffTackleState tackleState;

    public float hitStunTime = .2f;

    public AudioClip tackleSound;
    public AudioClip hitSound;

    protected override void Start()
    {
        base.Start();
    }

    public override void Attack(Farmon targetEnemy)
    {
        AttackData tackleAttackData = new AttackData(10 + Power / 3, 6, hitStunTime, false, tackleSound, hitSound);

        SetState(new ScruffTackleState(this, attackTarget, tackleAttackData));
    }

    public override float AttackTime()
    {
        return 10f - GetModifiedAgility()/8;
    }

    protected override void GetLevelUpBonusStats(out int gritPlus, out int powerPlus, out int agilityPlus, out int focusPlus, out int luckPlus, out int pointsPlus)
    {
        base.GetLevelUpBonusStats(out gritPlus, out powerPlus, out agilityPlus, out focusPlus, out luckPlus, out pointsPlus);

        if (level % 2 == 0)
        {
            powerPlus++;
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

public class ScruffTackleState : MeleeAttackState
{
    public ScruffTackleState(Farmon farmon, uint targetFarmonInstanceID, AttackData attackData) : base(farmon, targetFarmonInstanceID,  attackData)
    {
    }

    public override void Tick()
    {
        _farmon.maxSpeed = (_farmon.GetMovementSpeed() + 2) * 3;
        base.Tick();
    }

    public override void OnAttack()
    {
        base.OnAttack();

        _farmon.rb.velocity = Vector3.zero;

        _stateMachine.ChangeState(_farmon.mainState);
        _farmon.AttackComplete();
    }
}