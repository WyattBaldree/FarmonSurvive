using Assets.Scripts.States;
using Assets.Scripts.Timer;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NewBattleState : StateMachineState
{
    protected Farmon farmon;
    public NewBattleState(Farmon thisFarmon)
    {
        farmon = thisFarmon;
    }

    public override void Tick()
    {
        base.Tick();
        //First handle navigation
        NavigationTick();
        //Then handle battle logic
        BattleTick();
        //Then handle the animation logic
        AnimationTick();
    }

    protected abstract void NavigationTick();

    protected abstract void BattleTick();

    protected abstract void AnimationTick();

    public virtual void OnAttack()
    {
    }
}

public class NewIdleState : NewBattleState
{
    public NewIdleState(Farmon thisFarmon) : base(thisFarmon)
    {
    }

    protected override void NavigationTick()
    {
        farmon.MovementIdle();
    }

    protected override void BattleTick()
    {
        
    }

    protected override void AnimationTick()
    {
    }
}

public class NewAttackState : NewBattleState
{
    uint attackTarget;
    public NewAttackState(Farmon thisFarmon, uint farmonIdToAttack) : base(thisFarmon)
    {
        attackTarget = farmonIdToAttack;
    }

    protected override void NavigationTick()
    {
        Farmon farmonToAttack = Farmon.GetFarmonInstanceFromLoadedID(attackTarget, true);

        //if our target is doesn't exist, don't move
        if (!farmonToAttack)
        {
            farmon.MovementIdle();
            return;
        }

        //if we can see the target farmon and the target farmon is in range, wander around it
        if (farmon.CanSeeFarmon(farmonToAttack) && farmon.InAttackRange(farmonToAttack))
        {
            //Wander around target.
            farmon.StayInRange(farmonToAttack.transform.position, farmon.GetAttackWanderDistance() / 2, farmon.GetAttackWanderDistance());
        }
        //else, pathfind to the target.
        else
        {
            farmon.NavigateToFarmon(farmonToAttack);
        }
    }

    protected override void BattleTick()
    {
        Farmon farmonToAttack = Farmon.GetFarmonInstanceFromLoadedID(attackTarget, true);

        //if our target doesn't exist, go to the idle state
        if (!farmonToAttack)
        {
            farmon.GetNextAction();
            return;
        }

        if (farmon.attackReady)
        {
            farmon.AttemptAttack(farmonToAttack);
        }

        //if a friendly ability is ready, if there is a friendly farmon in attack range, enter this farmon's specific ability state.
    }

    protected override void AnimationTick()
    {
    }
}

public class NewProtectState : NewBattleState
{
    uint protectTarget;
    public NewProtectState(Farmon thisFarmon, uint farmonIdToProtect) : base(thisFarmon)
    {
        protectTarget = farmonIdToProtect;
    }

    protected override void NavigationTick()
    {
        Farmon farmonToProtect = Farmon.GetFarmonInstanceFromLoadedID(protectTarget, true);

        //if our target is doesn't exist, don't move
        if (!farmonToProtect)
        {
            farmon.MovementIdle();
            return;
        }

        //if we can see the target farmon and the target farmon is in range, wander around it
        if (farmon.CanSeeFarmon(farmonToProtect) && farmon.InAttackRange(farmonToProtect))
        {
            //Wander around target.
            farmon.StayInRange(farmonToProtect.transform.position, farmon.GetPotectWanderDistance() / 2, farmon.GetPotectWanderDistance());
        }
        //else, pathfind to the target.
        else
        {
            farmon.NavigateToFarmon(farmonToProtect);
        }
    }

    protected override void BattleTick()
    {
        Farmon farmonToProtect = Farmon.GetFarmonInstanceFromLoadedID(protectTarget, true);

        //if our target doesn't exist, go to the idle state
        if (!farmonToProtect)
        {
            farmon.GetNextAction();
            return;
        }

        if (farmon.attackReady)
        {
            farmon.AttemptAttackOnNearestEnemyUnit();
        }

        //if a friendly ability is ready, if there is a friendly farmon in attack range, enter this farmon's specific ability state.
    }

    protected override void AnimationTick()
    {
        
    }
}

public class DefendState : NewBattleState
{
    GridSpace _defendLocation;

    public DefendState(Farmon thisFarmon, GridSpace defendLocation) : base(thisFarmon)
    {

        _defendLocation = defendLocation;
    }

    protected override void NavigationTick()
    {

        //if our target is doesn't exist, don't move
        if (_defendLocation == null)
        {
            farmon.MovementIdle();
            return;
        }

        bool withinGrid = H.Flatten(_defendLocation.Center - farmon.transform.position).magnitude < LevelController.Instance.gridSize;

        //if we can see the target farmon and the target farmon is in range, wander around it
        if (farmon.CanSeePosition(_defendLocation.Center) && withinGrid)
        {
            //Wander around target.
            farmon.StayInRange(_defendLocation.Center, farmon.GetPotectWanderDistance() / 2, farmon.GetPotectWanderDistance());
        }
        //else, pathfind to the target.
        else
        {
            farmon.NavigateToPosition(_defendLocation.Center);
        }
    }

    protected override void BattleTick()
    {
        //if our target doesn't exist, go to the idle state
        if (_defendLocation == null)
        {
            farmon.GetNextAction();
            return;
        }

        if (farmon.attackReady)
        {
            farmon.AttemptAttackOnNearestEnemyUnit();
        }

        //if a friendly ability is ready, if there is a friendly farmon in attack range, enter this farmon's specific ability state.
    }

    protected override void AnimationTick()
    {
    }
}

public class IdleState : StateMachineState
{
    Farmon farmon;


    public IdleState(Farmon thisUnit)
    {
        farmon = thisUnit;
    }

    public override void Enter()
    {
        base.Enter();

        farmon.maxSpeed = farmon.GetMovementSpeed();
    }

    public override void Tick()
    {
        base.Tick();
        farmon.MovementIdle();
    }
}

/*public class AttackState : StateMachineState
{
    Farmon farmon;
    StateMachine subStateMachine;
    FollowPathState followPathState;
    BattleState battleState;

    public AttackState(Farmon thisFarmon)
    {
        farmon = thisFarmon;

        subStateMachine = new StateMachine();

        followPathState = new FollowPathState(farmon);
        battleState = new BattleState(farmon);

        subStateMachine.InitializeStateMachine(battleState);
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Tick()
    {
        base.Tick();

        Farmon attackFarmon = farmon.GetAttackTargetFarmon();

        // If we have a target, 
        if (attackFarmon)
        {
            farmon.targetTransform = attackFarmon.transform;

            bool isAbleToAttack = farmon.IsInAttackPosition();

            //if not able to attack, path towards the enemy
            if (subStateMachine.CurrentState != followPathState && !isAbleToAttack)
            {
                subStateMachine.ChangeState(followPathState);
            }
            else if (subStateMachine.CurrentState != battleState && isAbleToAttack)
            {
                subStateMachine.ChangeState(battleState);
            }

            subStateMachine.Tick();
        }
        else
        {
            // our attack target has died
            // get a new target!

            //farmon.MovementWander();

            farmon.mainState = new IdleState(farmon);
            farmon.SetState(farmon.mainState);
        }
    }
}*/

/*public class DefendState : StateMachineState
{
    Farmon farmon;
    StateMachine subStateMachine;
    FollowPathState followPathState;
    ProtectState protectState;

    public DefendState(Farmon thisFarmon)
    {
        farmon = thisFarmon;

        subStateMachine = new StateMachine();

        followPathState = new FollowPathState(farmon);
        protectState = new ProtectState(farmon);

        subStateMachine.InitializeStateMachine(protectState);
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Tick()
    {
        base.Tick();

        Farmon protectFarmon = farmon.GetProtectTargetFarmon();

        // If we have a target, 
        if (protectFarmon)
        {
            farmon.targetTransform = protectFarmon.transform;

            bool isInProtectingDistance = farmon.IsInProtectPosition();

            //Attack the last enemy to attack the protected farmon!

            //if not, path towards the enemy
            if (subStateMachine.CurrentState != followPathState && !isInProtectingDistance)
            {
                subStateMachine.ChangeState(followPathState);
            }
            else if (subStateMachine.CurrentState != protectState && isInProtectingDistance)
            {
                subStateMachine.ChangeState(protectState);
            }

            subStateMachine.Tick();
        }
        else
        {
            // our attack target has died
            // get a new target!

            //farmon.MovementWander();

            farmon.mainState = new IdleState(farmon);
            farmon.SetState(farmon.mainState);
        }
    }
}*/

/*public class BattleState : StateMachineState
{
    Farmon farmon;

    public BattleState(Farmon thisFarmon)
    {
        farmon = thisFarmon;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Tick()
    {
        farmon.maxSpeed = farmon.GetMovementSpeed();

        //Move to stay in range of the attackTarget
        farmon.debugColor = Color.red;
        farmon.StayInRange(farmon.targetTransform.position, farmon.targetRange - 2, farmon.targetRange);

        farmon.rb.AddForce(farmon.AvoidEdges());

        if (farmon.attackReady)
        {
            Farmon attackFarmon = farmon.GetAttackTargetFarmon();

            // Call this farmon's implementation of attack.
            farmon.Attack(attackFarmon);
            farmon.attackReady = false;
        }
    }
}*/

/*public class ProtectState : StateMachineState
{
    Farmon farmon;
    StateMachine subStateMachine;
    FollowPathState followPathState;
    BattleState battleState;

    public ProtectState(Farmon thisFarmon)
    {
        farmon = thisFarmon;

        subStateMachine = new StateMachine();

        followPathState = new FollowPathState(farmon);
        battleState = new BattleState(farmon);

        subStateMachine.InitializeStateMachine(battleState);
    }

    public override void Enter()
    {
        base.Enter();
    }

    Farmon GetNextAttackTarget()
    {
        Farmon protectFarmon = farmon.GetProtectTargetFarmon();

        //First see if the last farmon to damage our protected farmon is a valid target.
        Farmon potentialTarget = protectFarmon.LastFarmonToDamageMe;

        if (potentialTarget && IsValidTarget(potentialTarget))
        {
            // The last target to damage our protectTarget is valid and can be attacked.
            return potentialTarget;
        }
        else
        {
            //Loop through all farmon and find a valid target
            foreach (Farmon currentTarget in Farmon.farmonList)
            {
                if (farmon == currentTarget) continue;
                if (farmon.team == currentTarget.team) continue;

                if (IsValidTarget(currentTarget))
                {
                    // Found a valid farmon!
                    return currentTarget;
                }
            }
        }

        return null;
    }

    bool IsValidTarget(Farmon targetFarmon)
    {
        Farmon protectFarmon = farmon.GetProtectTargetFarmon();

        Vector3 protectTargetToEnemy = targetFarmon.transform.position - protectFarmon.transform.position;

        float potentialTargetDistance = H.Flatten(protectTargetToEnemy).magnitude;

        bool hitWall = Physics.Raycast(protectFarmon.transform.position, protectTargetToEnemy.normalized, protectTargetToEnemy.magnitude, LayerMask.GetMask("Default"));

        if (!hitWall && potentialTargetDistance < protectFarmon.targetRange + 4)
        {
            // The last target to damage our protectTarget is valid and can be attacked.
            return true;
        }

        return false;
    }

    public override void Tick()
    {
        farmon.maxSpeed = farmon.GetMovementSpeed();

        Farmon attackFarmon = farmon.GetAttackTargetFarmon();

        // if our attack target is no longer valid, find a new one.
        if (!attackFarmon || !IsValidTarget(attackFarmon))
        {
            farmon.attackTarget = GetNextAttackTarget().loadedFarmonMapId;
        }

        // If we are ready to attack and there is a valid target near our protect target attack the valid target.
        if (attackFarmon && farmon.attackReady)
        {
            farmon.targetTransform = attackFarmon.transform;
            bool isInAttackingPosition = farmon.IsInAttackPosition();

            //if not, path towards the enemy
            if (subStateMachine.CurrentState != followPathState)
            {
                subStateMachine.ChangeState(followPathState);
            }

            if (isInAttackingPosition)
            {
                //// Call this farmon's implementation of attack.
                farmon.Attack(attackFarmon);
                farmon.attackReady = false;
            }
            else
            {
                subStateMachine.Tick();
            }
        }
        else
        {
            farmon.StayInRange(farmon.targetTransform.position, .5f, 3f);

            farmon.rb.AddForce(farmon.AvoidEdges());
        }
    }
}*/

// In the melee attack state, farmon moves towards the attackTarget farmon and, after getting within attack range, lunges at the attackTarget farmon.
public class MeleeAttackState : NewBattleState
{
    protected AttackData _attackData;

    protected Farmon _farmon;

    protected float _hitStun;

    protected uint _farmonIdToAttack;

    public MeleeAttackState(Farmon thisFarmon, uint farmonIdToAttack, AttackData attackData, float hitStun = .3f) : base(thisFarmon)
    {
        _farmon = farmon;
        _hitStun = hitStun;
        _attackData = attackData;

        _farmonIdToAttack = farmonIdToAttack;
    }

    /*public override void Enter()
    {
        base.Enter();
        _farmon.ImmuneToHitStop = true;

        _farmon.maxSpeed = (_farmon.GetMovementSpeed() + 2) * 3;
    }

    public override void Exit()
    {
        base.Exit();
        _farmon.ImmuneToHitStop = false;
    }

    public override void Tick()
    {
        base.Tick();


        Farmon attackFarmon = Farmon.loadedFarmonMap[_targetFarmonInstanceID];
        _farmon.targetTransform = attackFarmon.transform;

        if (!_farmon.targetTransform || !attackFarmon)
        {
            _stateMachine.ChangeState(_farmon.mainState);
            return;
        }

        if (jumping && _farmon.rb.velocity.y < 0.001f && _farmon.Grounded)
        {
            jumping = false;
        }

        if (!jumping) _farmon.SeekUnit(false, true);

        Vector3 toTargetEnemy = attackFarmon.Hud.SpriteQuad.transform.position - _farmon.Hud.SpriteQuad.transform.position;
        float distanceToEnemy = Vector3.Distance(_farmon.transform.position, attackFarmon.transform.position);

        //If we have the jump perk and a flying enemy is less than 1.5 blocks away, jump at them.
        _farmon.perkList.TryGetValue(new PerkJump().PerkName, out int jumpAbility);
        if (jumpAbility > 0 && distanceToEnemy < 1.5f * LevelController.Instance.gridSize && attackFarmon.Flying && _farmon.Grounded)
        {
            _farmon.rb.velocity = Vector3.zero;
            _farmon.rb.AddForce((toTargetEnemy / LevelController.Instance.gridSize) * 1.4f, ForceMode.Impulse);
            jumping = true;
        }

        float radiusCombined = _farmon.sphereCollider.radius + attackFarmon.sphereCollider.radius;

        if (distanceToEnemy - radiusCombined < _lungeRange)
        {
            _farmon.Hud.SpriteQuad.transform.DOPunchPosition(toTargetEnemy, 0.5f, 1, 0);
            _farmon.Hud.SpriteQuad.transform.DORestart();
            //DOTween.Play(_farmon.Hud.SpriteQuad.transform);
            OnAttack();
        }
    }*/

    public override void Exit()
    {
        base.Exit();
        farmon.AttackComplete();
    }

    protected override void NavigationTick()
    {
        Farmon farmonToAttack = Farmon.GetFarmonInstanceFromLoadedID(_farmonIdToAttack, true);

        //if our target doesn't exist, go to the idle state
        if (!farmonToAttack)
        {
            farmon.MovementIdle();
            return;
        }

        //if we can see the target farmon and the target farmon is in range, fly directly towards it 
        if (farmon.CanSeeFarmon(farmonToAttack) && farmon.InAttackRange(farmonToAttack))
        {
            //Fly towards the target.
            farmon.FlyTowardsPosition(farmonToAttack.transform.position, 15 + 10 * (farmon.GetModifiedAgility()/Farmon.StatMax));
        }
        //else, pathfind to the target.
        else
        {
            farmon.NavigateToFarmon(farmonToAttack);
        }
    }

    protected override void BattleTick()
    {
        Farmon farmonToAttack = Farmon.GetFarmonInstanceFromLoadedID(_farmonIdToAttack, true);

        //if our target doesn't exist, go to the idle state
        if (!farmonToAttack)
        {
            farmon.GetNextAction();
            return;
        }

        //if we can see the target farmon and the target farmon is in range, wander around it
        float distanceToTarget = (farmonToAttack.transform.position - farmon.transform.position).magnitude;
        float attackDistance = farmonToAttack.sphereCollider.radius + farmon.sphereCollider.radius + LevelController.Instance.gridSize / 10f;

        if (distanceToTarget < attackDistance)
        {
            //hit the target
            OnAttack();
        }
    }

    public override void OnAttack()
    {
        base.OnAttack();

        Farmon attackFarmon = Farmon.GetFarmonInstanceFromLoadedID(_farmonIdToAttack, true);

        SphereCollider sc = _farmon.sphereCollider;

        Vector3 toTargetEnemy = attackFarmon.Hud.SpriteQuad.transform.position - _farmon.Hud.SpriteQuad.transform.position;
        _farmon.Hud.SpriteQuad.transform.DOPunchPosition(toTargetEnemy, 0.5f, 1, 0);
        _farmon.Hud.SpriteQuad.transform.DORestart();

        bool hit = attackFarmon.AttemptDamage(_attackData,
                                              _hitStun,
                                              sc.transform.position + sc.center,
                                              (attackFarmon.transform.position - _farmon.transform.position).normalized,
                                              _farmon);

        if (hit)
        {
            //Spawn a hit effect.
            Vector3 farmonToMe = (_farmon.transform.position - attackFarmon.transform.position).normalized;
            GameObject hitEffect = GameObject.Instantiate(FarmonController.instance.HitEffectPrefab, attackFarmon.transform);
            hitEffect.transform.position = attackFarmon.transform.position + farmonToMe * attackFarmon.sphereCollider.radius;
            hitEffect.transform.localScale = (1.5f) * Vector3.one;

            _farmon.Hud.AudioSource.clip = _attackData.HitSound;
            _farmon.Hud.AudioSource.volume = .2f;
            _farmon.Hud.AudioSource.Play();

            _farmon.HitStopSelf(_hitStun);
        }

        _farmon.rb.velocity = Vector3.zero;
    }

    protected override void AnimationTick()
    {
    }
}

// In the melee attack state, farmon moves towards the attackTarget farmon and, after getting within attack range, lunges at the attackTarget farmon.
public class ProjectileAttackState : NewBattleState
{
    protected AttackData _attackData;

    protected Farmon _farmon;

    protected float _hitStun;
    protected float _chargeTime;

    protected bool _charging;
    protected Timer _chargingTimer;

    protected uint _farmonIdToAttack;

    readonly Timer flashTimer = new Timer();
    bool flashFlag = false;


    public ProjectileAttackState(Farmon thisFarmon, uint farmonIdToAttack, AttackData attackData, float chargeTime, float hitStun = .3f) : base(thisFarmon)
    {
        _farmon = farmon;
        _hitStun = hitStun;
        _chargeTime = chargeTime;
        _attackData = attackData;
        _charging = false;


        _chargingTimer = new Timer();
        _chargingTimer.SetTime(_chargeTime);
        _chargingTimer.autoReset = true;

        _farmonIdToAttack = farmonIdToAttack;

        flashTimer.SetTime(.1f);
        flashTimer.autoReset = true;
    }

    public override void Exit()
    {
        base.Exit();
        farmon.mySpriteRenderer.color = new Color(1, 1, 1, 1);
    }

    protected override void NavigationTick()
    {
        Farmon farmonToAttack = Farmon.GetFarmonInstanceFromLoadedID(_farmonIdToAttack, true);

        //if our target doesn't exist, go to the idle state
        if (!farmonToAttack)
        {
            farmon.MovementIdle();
            return;
        }

        if (_charging)
        {
            farmon.MovementIdle();
        }
        //else, pathfind to the target.
        else
        {
            farmon.NavigateToFarmon(farmonToAttack);
        }
    }

    protected override void BattleTick()
    {
        Farmon farmonToAttack = Farmon.GetFarmonInstanceFromLoadedID(_farmonIdToAttack, true);

        //if our target doesn't exist, go to the idle state
        if (!farmonToAttack)
        {
            farmon.GetNextAction();
            return;
        }

        // if charging, charge up then attack
        if (_charging)
        {
            if (_chargingTimer.Tick(Time.deltaTime))
            {
                OnAttack();
            }
        }
        else
        {
            //if not charging, wait until we are in attack range then start charging.
            //if we can see the target farmon and the target farmon is in range, fly directly towards it 
            if (farmon.CanSeeFarmon(farmonToAttack) && farmon.InAttackRange(farmonToAttack))
            {
                //Fly towards the target.
                _charging = true;
            }
        }
    }

    protected override void AnimationTick()
    {
        if (_charging)
        {
            if (flashTimer.Tick(Time.deltaTime))
            {
                if (flashFlag)
                {
                    farmon.mySpriteRenderer.color = new Color(.6f, .6f, .6f, 1);
                }
                else
                {
                    farmon.mySpriteRenderer.color = new Color(1, 1, 1, 1);
                }
                flashFlag = !flashFlag;
            }
        }
    }
}