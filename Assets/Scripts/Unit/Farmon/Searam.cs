using Assets.Scripts.States;
using Assets.Scripts.Timer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Searam : Farmon
{
    public GameObject fireBallPrefab;

    public override void Attack(Farmon targetEnemy)
    {
        AttackComplete();
    }

    public override float AttackTime()
    {
        return 3.5f - GetModifiedFocus() / 30f - GetModifiedAgility() / 30f;
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