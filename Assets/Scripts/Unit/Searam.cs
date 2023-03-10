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

public class JumpState : StateMachineState
{
    Farmon unit;

    Vector3 startingPosition, endingPosition;

    float jumpTime = 1;

    float h;

    Timer jumpTimer = new Timer();

    public JumpState(Farmon thisUnit, Vector3 startingPos, Vector3 endingPos, float height, float duration = 1f)
    {
        unit = thisUnit;
        startingPosition = startingPos;

        Vector2 randomFlatVector = Random.insideUnitCircle;
        Vector3 slightOffset = new Vector3(randomFlatVector.x, 0, randomFlatVector.y) * .01f;
        endingPosition = endingPos + slightOffset;

        jumpTime = duration;
        h = height;
    }

    public override void Enter()
    {
        base.Enter();
        jumpTimer.SetTime(jumpTime);

        unit.maxSpeed = 0;
        unit.rb.isKinematic = true;
    }

    public override void Tick()
    {
        base.Tick();

        if (jumpTimer.Tick(Time.deltaTime))
        {
            unit.SetState(unit.mainState);
            unit.rb.MovePosition(MathParabola.Parabola(startingPosition, endingPosition, h, .9f));
            return;
        }

        float jumpPercent = Mathf.Max(0, 0.9f - jumpTimer.Percent);

        unit.rb.MovePosition(MathParabola.Parabola(startingPosition, endingPosition, h, jumpPercent));
    }

    public override void Exit()
    {
        base.Exit();
        unit.rb.isKinematic = false;
    }
}