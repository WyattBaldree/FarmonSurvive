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
        AttackData fireBallAttackData = new AttackData( 5 + Power/2, 4, fireBallSound, fireBallHitSound);

        fireBall.transform.localScale *= (1f + (float)Focus / 5f);
        fireBall.Pierce = 2;
        fireBall.OnHitDelegate = (unit) => {
            FireballHit();
        };
        fireBall.Initialize(fireBallAttackData, this, team);

        //This is a basic attack so add the basic attack component.
        BasicProjectile bp = fireBall.gameObject.AddComponent<BasicProjectile>();
        bp.Velocity = (5f + Agility / 10f);
        bp.TargetFarmonId = targetEnemyFarmon.loadedFarmonMapId;
        
        /*Vector3 unitToEnemy = targetEnemyFarmon.GetUnitVectorToMe(transform.position) * 3f;
        unitToEnemy = Vector3.ProjectOnPlane(unitToEnemy, Vector3.up).normalized;

        ConstantVelocity cv = fireBall.gameObject.AddComponent<ConstantVelocity>();
        cv.velocity = unitToEnemy.normalized * (5f + Agility/10f);
        cv.ignoreGravity = true;*/

        AttackComplete();
    }

    public override float AttackTime()
    {
        return 9f - GetModifiedAgility()/15;
    }

    private void FireballHit()
    {
        //Every 3 Hits triggers a tornado burst!
        hitCount++;

        if(hitCount >= 3)
        {
            hitCount -= 3;
            LaunchTornados();
        }
    }

    private void LaunchTornados()
    {
        //angleBetweenTornados should be divisible into 360
        int angleBetweenTornados = 120;
        for(int angle = 0; angle < 360; angle += angleBetweenTornados)
        {
            Projectile tornado = Instantiate(tornadoPrefab, transform.position, transform.rotation).GetComponent<Projectile>();

            AttackData tornadoAttackData = new AttackData(2, 2, angle == 0 ? tornadoSound : null, null);

            tornado.Pierce = 1;
            tornado.LifeTime = 10;
            tornado.Initialize(tornadoAttackData, this, team);

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

    public override void OnLevelUp()
    {
        
    }
}