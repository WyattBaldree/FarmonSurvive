using Assets.Scripts.States;
using Assets.Scripts.Timer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scrimp : Farmon
{
    public GameObject fireBallPrefab;

    public override void Attack(Farmon targetEnemy)
    {
        Vector3 unitToEnemy = targetEnemy.GetUnitVectorToMe(transform.position);

        Projectile fireBall = Instantiate(fireBallPrefab, transform.position, transform.rotation).GetComponent<Projectile>();
        fireBall.damage = 10 + (int)(10f * (float)Power / 5f);
        fireBall.transform.localScale *= (1f + (float)Focus / 5f);
        fireBall.pierce += Focus / 3;
        fireBall.OnHitDelegate = (unit) => {
            unit.EffectList.AddEffect(new Effect("burn;1;10"));
        };
        ConstantVelocity cv = fireBall.gameObject.AddComponent<ConstantVelocity>();
        cv.velocity = unitToEnemy.normalized * (10f + Reflex/2f);
        cv.ignoreGravity = false;

        //fireBall = Farmon.Instantiate(farmon.fireBallPrefab, farmon.transform.position, farmon.transform.rotation).GetComponent<Projectile>();
        //fireBall.rigidBody.velocity = Quaternion.Euler(0, 15, 0) * unitToEnemy * 5f;

        //fireBall = Farmon.Instantiate(farmon.fireBallPrefab, farmon.transform.position, farmon.transform.rotation).GetComponent<Projectile>();
        //fireBall.rigidBody.velocity = Quaternion.Euler(0, -15, 0) * unitToEnemy * 5f;
    }

    protected override void Start()
    {
        base.Start();

        idleState = new IdleState(this);
    }

    public override float AttackTime()
    {
        return 3.5f - GetModifiedFocus() / 30f - GetModifiedSpeed() / 30f;
    }
}