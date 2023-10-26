using Assets.Scripts.States;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scruff : Farmon
{
    public GameObject scruffTacklePrefab;

    public AudioClip tackleSound;
    public AudioClip hitSound;

    protected override void Start()
    {
        base.Start();
    }

    public override void Attack(Farmon _farmon)
    {
        base.Attack(_farmon);
        AttackData tackleAttackData = new AttackData(10 + Power / 3, 6, false, tackleSound, hitSound);

        SetState(new ScruffTackleState(this, _farmon.loadedFarmonMapId, tackleAttackData, .5f));
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
    public ScruffTackleState(Farmon farmon, uint targetFarmonInstanceID, AttackData attackData, float hitStun) : base(farmon, targetFarmonInstanceID,  attackData, hitStun)
    {
    }

    public override void OnAttack()
    {
        base.OnAttack();
        farmon.AttackComplete();
        //_farmon.SetState(_farmon.mainBattleState);
        farmon.GetNextAction();
    }
}