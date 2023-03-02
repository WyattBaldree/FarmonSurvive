using Assets.Scripts.States;
using Assets.Scripts.Timer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Galeon : Farmon
{
    [SerializeField]
    GameObject tornadoPrefab;

    [SerializeField]
    GameObject fireballPrefab;

    [SerializeField]
    AudioClip tornadoSound;
    [SerializeField]
    AudioClip fireBallSound;
    [SerializeField]
    AudioClip fireBallHitSound;

    int hitCount = 0;

    public override void Attack(Farmon targetEnemyFarmon)
    {
        Projectile fireBall = Instantiate(fireballPrefab, transform.position, transform.rotation).GetComponent<Projectile>();
        fireBall.damage = 5 + Power/2;
        fireBall.transform.localScale *= (1f + (float)Focus / 5f);
        fireBall.pierce = 2;
        fireBall.knockBack = 4;
        fireBall.hitStunTime = .15f;
        fireBall.owner = this;
        fireBall.team = team;
        fireBall.CreateSound = fireBallSound;
        fireBall.HitSound = fireBallHitSound;

        fireBall.OnHitDelegate = (unit) => {
            FireballHit();
        };
        //Every 3 Hits triggers a tornado burst!

        Vector3 unitToEnemy = targetEnemyFarmon.GetUnitVectorToMe(transform.position) * 3f;
        unitToEnemy = Vector3.ProjectOnPlane(unitToEnemy, Vector3.up).normalized;

        ConstantVelocity cv = fireBall.gameObject.AddComponent<ConstantVelocity>();
        cv.velocity = unitToEnemy.normalized * (5f + Agility/10f);
        cv.ignoreGravity = true;

        AttackComplete();
    }

    public override float AttackTime()
    {
        return 9f - GetModifiedAgility()/15;
    }

    private void FireballHit()
    {
        hitCount++;

        if(hitCount >= 3)
        {
            hitCount -= 3;
            LaunchTornados();
        }
    }

    private void LaunchTornados()
    {
        //step should be divisible into 360
        int angleBetweenTornados = 120;
        for(int angle = 0; angle < 360; angle += angleBetweenTornados)
        {
            Projectile tornado = Instantiate(tornadoPrefab, transform.position, transform.rotation).GetComponent<Projectile>();
            tornado.damage = 2;
            tornado.pierce = 1;
            tornado.hitStunTime = .1f;
            tornado.knockBack = 2;
            tornado.lifeTime = 10;
            tornado.owner = this;
            tornado.team = team;
            if (angle == 0) tornado.CreateSound = tornadoSound;

            SpiralOut spiralOut = tornado.GetComponent<SpiralOut>();
            spiralOut.RotationOffset = angle;
            spiralOut.MoveAwaySpeed = 10;
            spiralOut.RotationSpeed = 120;
            spiralOut.MaxSpeed = 6;
        }


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
            focusPlus++;
        }
    }

    public override void DistributeLevelUpPerks()
    {
        
    }
}


