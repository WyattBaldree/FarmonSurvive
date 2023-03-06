using Assets.Scripts.States;
using Assets.Scripts.Timer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scrimp : Farmon
{
    public GameObject fireBallPrefab;

    public AudioClip shootSound;
    public AudioClip hitSound;

    public override void Attack(Farmon targetEnemy)
    {
        Vector3 unitToEnemy = targetEnemy.GetUnitVectorToMe(transform.position);

        Projectile fireBall = Instantiate(fireBallPrefab, transform.position, transform.rotation).GetComponent<Projectile>();
        fireBall.damage = 5 + Power / 5;
        fireBall.transform.localScale *= (1f + (float)Focus / (StatMax*2));
        fireBall.pierce += Focus / 3;
        fireBall.knockBack = 4;
        fireBall.hitStunTime = .15f;
        fireBall.OnHitDelegate = (unit) => {
            perkList.TryGetValue(new PerkFiendFire().PerkName, out int fiendFireAbility);

            if (fiendFireAbility > 0)
            {
                int fireDamage = 3 * fiendFireAbility;
                unit.EffectList.AddEffect(new Effect("burn;" + fireDamage + ";4"));
            }
        };
        fireBall.owner = this;
        fireBall.team = team;
        fireBall.CreateSound = shootSound;
        fireBall.HitSound = hitSound;

        ConstantVelocity cv = fireBall.gameObject.AddComponent<ConstantVelocity>();
        cv.velocity = unitToEnemy.normalized * (10f + Agility/2f);
        cv.ignoreGravity = false;

        //fireBall = Farmon.Instantiate(farmon.fireBallPrefab, farmon.transform.position, farmon.transform.rotation).GetComponent<Projectile>();
        //fireBall.rigidBody.velocity = Quaternion.Euler(0, 15, 0) * unitToEnemy * 5f;

        //fireBall = Farmon.Instantiate(farmon.fireBallPrefab, farmon.transform.position, farmon.transform.rotation).GetComponent<Projectile>();
        //fireBall.rigidBody.velocity = Quaternion.Euler(0, -15, 0) * unitToEnemy * 5f;

        AttackComplete();
    }

    public override float AttackTime()
    {
        return 6f - GetModifiedFocus() / 30f - GetModifiedAgility() / 30f;
    }

    public override void DistributeLevelUpPerks()
    {
        switch (level)
        {
            case 2:
                PerkSelectionScreen.instance.Popup(this, new Perk[] { new PerkFiendFire() });
                break;
            case 6:
                PerkSelectionScreen.instance.Popup(this, new Perk[] { new PerkFlameAspect() });
                break;
            case 10:
                PerkSelectionScreen.instance.Popup(this, new Perk[] { new PerkFrenzy(), new PerkSecretStash() });
                break;
            case 14:
                Evolve("Screevil");
                break;
        }
    }

    protected override void GetLevelUpBonusStats(out int gritPlus, out int powerPlus, out int agilityPlus, out int focusPlus, out int luckPlus, out int pointsPlus)
    {
        base.GetLevelUpBonusStats(out gritPlus, out powerPlus, out agilityPlus, out focusPlus, out luckPlus, out pointsPlus);

        if(level%2 == 0)
        {
            powerPlus++;
        }
        else
        {
            agilityPlus++;
        }
    }
}