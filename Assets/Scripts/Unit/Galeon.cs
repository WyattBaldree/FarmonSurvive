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
    AudioClip TornadoSound;

    [SerializeField]
    GameObject fireballPrefab;

    public override void Attack(Farmon targetEnemyFarmon)
    {
        SpiralOut tornado = Instantiate(tornadoPrefab, transform.position, transform.rotation).GetComponent<SpiralOut>();
        tornado.rotationOffset = 0;
        tornado.GetComponent<Projectile>().owner = this;

        tornado = Instantiate(tornadoPrefab, transform.position, transform.rotation).GetComponent<SpiralOut>();
        tornado.rotationOffset = 90;
        tornado.GetComponent<Projectile>().owner = this;

        tornado = Instantiate(tornadoPrefab, transform.position, transform.rotation).GetComponent<SpiralOut>();
        tornado.rotationOffset = 180;
        tornado.GetComponent<Projectile>().owner = this;

        tornado = Instantiate(tornadoPrefab, transform.position, transform.rotation).GetComponent<SpiralOut>();
        tornado.rotationOffset = 270;
        tornado.GetComponent<Projectile>().owner = this;

        Hud.AudioSource.clip = TornadoSound;
        Hud.AudioSource.volume = .3f;
        Hud.AudioSource.Play();

        Projectile fireBall = Instantiate(fireballPrefab, transform.position, transform.rotation).GetComponent<Projectile>();
        fireBall.damage = Power;
        fireBall.transform.localScale *= (1f + (float)Focus / 5f);
        fireBall.pierce = 100;
        fireBall.knockBack = 4;
        fireBall.hitStunTime = .15f;
        fireBall.owner = this;

        Vector3 unitToEnemy = targetEnemyFarmon.GetUnitVectorToMe(transform.position) * 3f;
        unitToEnemy = Vector3.ProjectOnPlane(unitToEnemy, Vector3.up).normalized;

        ConstantVelocity cv = fireBall.gameObject.AddComponent<ConstantVelocity>();
        cv.velocity = unitToEnemy.normalized * (5f + Reflex/10f);
        cv.ignoreGravity = true;

        AttackComplete();
    }

    public override float AttackTime()
    {
        return 5f - GetModifiedSpeed()/20;
    }
}


