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
        base.Attack(targetEnemy);
        AttackData tackleAttackData = new AttackData(10 + Power / 3, 6, false, shootSound, hitSound);

        SetState(new ScrimpAttackState(this, targetEnemy.loadedFarmonMapId, tackleAttackData, 1f, 0.3f));
    }

    public void CreateProjectile(uint farmonIdToAttack)
    {
        Projectile fireBall = Instantiate(fireBallPrefab, transform.position, transform.rotation).GetComponent<Projectile>();
        AttackData fireballAttackData = new AttackData(5 + Power / 5, 4, false, shootSound, hitSound);
        fireBall.transform.localScale *= (1f + (float)Focus / (StatMax * 2));
        fireBall.OnHitDelegate = (unit) => {
            perkList.TryGetValue(new PerkFiendFire().PerkName, out int fiendFireAbility);

            if (fiendFireAbility > 0)
            {
                int fireDamage = fiendFireAbility * (3 + (GetModifiedFocus()/5));
                unit.EffectList.Burn.AddEffect(4, fireDamage);
            }
        };
        fireBall.Initialize(fireballAttackData, this, team);
        fireBall.SpecificTargetId = farmonIdToAttack;
        fireBall.LevelCollision = false;

        //This is a basic attack so add the basic attack component.
        BasicProjectile bp = fireBall.gameObject.AddComponent<BasicProjectile>();
        bp.Velocity = (10f + Agility / 2f);
        bp.TargetFarmonId = farmonIdToAttack;

        //ConstantVelocity cv = fireBall.gameObject.AddComponent<ConstantVelocity>();
        //cv.velocity = unitToEnemy.normalized * (10f + Agility/2f);
        //cv.ignoreGravity = false;

        //fireBall = Farmon.Instantiate(farmon.fireBallPrefab, farmon.transform.position, farmon.transform.rotation).GetComponent<Projectile>();
        //fireBall.rigidBody.velocity = Quaternion.Euler(0, 15, 0) * unitToEnemy * 5f;

        //fireBall = Farmon.Instantiate(farmon.fireBallPrefab, farmon.transform.position, farmon.transform.rotation).GetComponent<Projectile>();
        //fireBall.rigidBody.velocity = Quaternion.Euler(0, -15, 0) * unitToEnemy * 5f;
    }

    public override void OnLevelUp()
    {
        switch (level)
        {
            case 2:
                PerkSelectionScreen.instance.Popup(this, new Perk[] { new PerkFiendFire(), new PerkForked() });
                break;
            case 6:
                PerkSelectionScreen.instance.Popup(this, new Perk[] { new PerkFlameAspect() });
                break;
            case 10:
                PerkSelectionScreen.instance.Popup(this, new Perk[] { new PerkFiendFire(), new PerkForked() });
                break;
            case 14:
                EvolutionScreen.instance.Popup(this,"Screevil");
                break;
            case 18:
                PerkSelectionScreen.instance.Popup(this, new Perk[] { new PerkFiendFire(), new PerkForked() });
                break;
            case 22:
                PerkSelectionScreen.instance.Popup(this, new Perk[] { new PerkSecretStash() });
                break;
            case 26:
                PerkSelectionScreen.instance.Popup(this, new Perk[] { new PerkFrenzy(), new PerkThickHide() });
                break;
            case 30:
                PerkSelectionScreen.instance.Popup(this, new Perk[] { new PerkFiendFire(), new PerkForked() });
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

public class ScrimpAttackState : ProjectileAttackState
{
    public ScrimpAttackState(Farmon thisFarmon, uint farmonIdToAttack, AttackData attackData, float chargeTime, float hitStun = 0.3F) : base(thisFarmon, farmonIdToAttack, attackData, chargeTime, hitStun)
    {
    }

    public override void OnAttack()
    {
        base.OnAttack();

        ((Scrimp)farmon).CreateProjectile(_farmonIdToAttack);
        farmon.AttackComplete();

        //farmon.SetState(farmon.mainBattleState);
        farmon.GetNextAction();
    }
}