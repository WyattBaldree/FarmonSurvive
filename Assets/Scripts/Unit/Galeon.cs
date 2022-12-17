using Assets.Scripts.States;
using Assets.Scripts.Timer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Galeon : Enemy
{
    [SerializeField]
    GameObject tornadoPrefab;

    [SerializeField]
    GameObject fireballPrefab;

    public override void Attack(Farmon targetEnemyFarmon)
    {
        SpiralOut tornado = Instantiate(tornadoPrefab, transform.position, transform.rotation).GetComponent<SpiralOut>();
        tornado.rotationOffset = 0;

        tornado = Instantiate(tornadoPrefab, transform.position, transform.rotation).GetComponent<SpiralOut>();
        tornado.rotationOffset = 90;

        tornado = Instantiate(tornadoPrefab, transform.position, transform.rotation).GetComponent<SpiralOut>();
        tornado.rotationOffset = 180;

        tornado = Instantiate(tornadoPrefab, transform.position, transform.rotation).GetComponent<SpiralOut>();
        tornado.rotationOffset = 270;

        Projectile fireBall = Instantiate(fireballPrefab, transform.position, transform.rotation).GetComponent<Projectile>();
        fireBall.damage = Power;
        fireBall.transform.localScale *= (1f + (float)Focus / 5f);
        fireBall.pierce = 100;

        Vector3 unitToEnemy = targetEnemyFarmon.GetUnitVectorToMe(transform.position) * 3f;
        unitToEnemy = Vector3.ProjectOnPlane(unitToEnemy, Vector3.up).normalized;

        ConstantVelocity cv = fireBall.gameObject.AddComponent<ConstantVelocity>();
        cv.velocity = unitToEnemy.normalized * (5f + Reflex);
        cv.ignoreGravity = true;
    }

    protected override void Start()
    {
        base.Start();

        idleState = new IdleState(this);
    }

    public override float AttackTime()
    {
        return 4f - GetModifiedSpeed()/20;
    }
}


