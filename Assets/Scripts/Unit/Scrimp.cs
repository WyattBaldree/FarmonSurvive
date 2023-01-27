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
        fireBall.knockBack = 4;
        fireBall.hitStunTime = .15f;
        fireBall.OnHitDelegate = (unit) => {
            unit.EffectList.AddEffect(new Effect("burn;1;10"));
        };
        fireBall.owner = this;
        ConstantVelocity cv = fireBall.gameObject.AddComponent<ConstantVelocity>();
        cv.velocity = unitToEnemy.normalized * (10f + Reflex/2f);
        cv.ignoreGravity = false;

        //fireBall = Farmon.Instantiate(farmon.fireBallPrefab, farmon.transform.position, farmon.transform.rotation).GetComponent<Projectile>();
        //fireBall.rigidBody.velocity = Quaternion.Euler(0, 15, 0) * unitToEnemy * 5f;

        //fireBall = Farmon.Instantiate(farmon.fireBallPrefab, farmon.transform.position, farmon.transform.rotation).GetComponent<Projectile>();
        //fireBall.rigidBody.velocity = Quaternion.Euler(0, -15, 0) * unitToEnemy * 5f;

        AttackComplete();
    }

    public override float AttackTime()
    {
        return 2f - GetModifiedFocus() / 80f - GetModifiedSpeed() / 80f;
    }
}