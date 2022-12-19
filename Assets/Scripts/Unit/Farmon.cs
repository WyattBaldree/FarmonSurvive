using Assets.Scripts.States;
using Assets.Scripts.Timer;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class Farmon : Vehicle
{
    string name = "Farmon";

    public static List<Vehicle> unitList = new List<Vehicle>();

    public enum TeamEnum
    {
        enemy,
        player
    }

    public enum TargetingTypeEnum
    {
        closestToMe,
        closestToPlayer
    }

    [SerializeField]
    private FarmonHud farmonHud;

    public TargetingTypeEnum targetingType;

    [HideInInspector]
    public SpriteRenderer mySpriteRenderer;
    [HideInInspector]
    public Bar healthBar;

    private StateMachine farmonStateMachine;

    public string unitName = "Unit";
    public bool isPlayerFarmon = false;
    public int level = 1;
    [HideInInspector]
    public int attributePoints = 1;
    [HideInInspector]
    public int perkPoints = 1;

    private int maxHealth = 3;
    private int health = 3;

    public bool attackReady = false;
    Timer attackTimer = new Timer();

    public EffectList EffectList = new EffectList();

    public float targetRange = 5f;

    public static float alertDistance = 7;
    public static float giveUpDistance = 9f;

    public Farmon attackTarget;

    //Hover Highlight
    Highlight _hoverHighlight;
    private HighlightList _highlightList;

    //States
    public StateMachineState idleState;
    public StateMachineState spawnState;

    //Effects
    private Timer burnTimer = new Timer();

    [SerializeField, HideInInspector]
    private int grit = 1;
    [SerializeField]
    private int baseGrit = 10;

    public int Grit {
        get => grit;
        set
        {
            grit = Math.Min(Math.Max(baseGrit, value), 40);

            // Update max health
            float healthPercent = health / maxHealth;
            maxHealth = grit * 10;

            // Adjust remaining health after the maxHealthChange
            SetHealth((int)Mathf.Ceil((float)maxHealth * healthPercent));

            // Set the size of the farmon
            SetSize(grit);
            farmonHud.SpriteQuad.GetComponent<SpriteRenderer>().transform.localScale = (1f + (grit / 40f)) * Vector3.one;
            if (shadow)
            {
                shadow.transform.position = sphereCollider.transform.position + sphereCollider.center + (sphereCollider.radius * Vector3.down * .8f);
                shadow.transform.localScale = sphereCollider.radius * 2f * Vector3.one;
            }

            statsChangedEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    [SerializeField, HideInInspector]
    private int power = 1;
    [SerializeField]
    private int basePower = 10;
    public int Power
    {
        get => power;
        set
        {
            power = Math.Min(Math.Max(basePower, value), 40);
            maxForce = 50;
            statsChangedEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    [SerializeField, HideInInspector]
    private int reflex = 1;
    [SerializeField]
    private int baseReflex = 10;
    public int Reflex
    {
        get => reflex;
        set
        {
            reflex = Math.Min(Math.Max(baseReflex, value), 40);
            statsChangedEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    [SerializeField, HideInInspector]
    private int focus = 1;
    [SerializeField]
    private int baseFocus = 10;
    public int Focus
    {
        get => focus;
        set
        {
            focus = Math.Min(Math.Max(baseFocus, value), 40);
            statsChangedEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    [SerializeField, HideInInspector]
    private int speed = 1;
    [SerializeField]
    private int baseSpeed = 10;
    public int Speed
    {
        get => speed;
        set
        {
            speed = Math.Min(Math.Max(baseSpeed, value), 40);
            statsChangedEvent?.Invoke(this, EventArgs.Empty);
        }
    }

    public abstract float AttackTime();

    GameObject shadow;

    [HideInInspector]
    public Farmon ClosestEnemyUnit = null;

    [HideInInspector]
    public Farmon ClosestEnemyUnitNearPlayer = null;

    [HideInInspector]
    public Farmon ClosestFriendlyUnit = null;

    public TeamEnum team;

    public EventHandler statsChangedEvent;

    protected override void Start()
    {
        base.Start();

        Assert.IsNotNull(farmonHud);

        mySpriteRenderer = farmonHud.SpriteQuad.GetComponent<SpriteRenderer>();
        Assert.IsNotNull(mySpriteRenderer);

        healthBar = farmonHud.HealthBar.GetComponent<Bar>();
        Assert.IsNotNull(healthBar);

        farmonStateMachine = new StateMachine();
        spawnState = new SpawnState(this);

        farmonStateMachine.InitializeStateMachine(spawnState);

        shadow = Instantiate(FarmonController.instance.ShadowPrefab, transform.position + sphereCollider.center + ((sphereCollider.radius - 0.01f) * Vector3.down), Quaternion.Euler(90, 0, 0), this.transform);

        _highlightList = mySpriteRenderer.GetComponent<HighlightList>();

        GetClosestEnemyUnit();
        GetClosestFriendlyUnit();

        Grit = grit;
        Power = power;
        Reflex = reflex;
        Focus = focus;
        Speed = speed;

        SetHealth(maxHealth);

        attackTimer.autoReset = true;
        attackTimer.SetTime(AttackTime());

        burnTimer.autoReset = true;
        burnTimer.SetTime(1.5f);
    }

    private void OnValidate()
    {
        Grit = grit;
        Power = power;
        Reflex = reflex;
        Focus = focus;
        Speed = speed;
    }

    public Farmon GetTargetEnemy()
    {
        switch (targetingType)
        {
            case TargetingTypeEnum.closestToPlayer:
                return Player.instance.ClosestEnemyUnit;
            case TargetingTypeEnum.closestToMe:
                return ClosestEnemyUnit;
            default:
                return null;
        }
    }

    public virtual float GetMovementSpeed()
    {
        return 3 + Speed / 6;
    }

    protected override void Awake()
    {
        base.Awake();
        unitList.Add(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        unitList.Remove(this);
    }

    private void OnMouseUp()
    {
        StatsScreen.instance.TargetUnit = this;
    }

    private void OnMouseEnter()
    {
        _hoverHighlight = _highlightList.AddHighlight(Color.white, 1);
    }

    private void OnMouseExit()
    {
        _highlightList.RemoveHighlight(_hoverHighlight);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (!FarmonController.paused)
        {
            GetClosestEnemyUnit();
            GetClosestFriendlyUnit();

            if (attackReady == false && attackTimer.Tick(Time.deltaTime))
            {
                attackReady = true;
            }

            if (EffectList.Burn > 0 && burnTimer.Tick(Time.deltaTime))
            {
                TakeDamage((int)EffectList.Burn, Vector3.zero, 0, true);
            }

            EffectList.UpdateEffects(Time.deltaTime);
            farmonStateMachine.Tick();
        }
    }

    public void SetHealth(int value)
    {
        health = value;

        if (healthBar)
        {
            healthBar.SetPercent((float)health / maxHealth);
        }

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void ChangeHeath(int amount)
    {
        SetHealth(health + amount);
    }

    public void TakeDamage(int damage, Vector3 damageLocation, float knockBack = 5f, bool undodgeable = false)
    {
        Vector3 toMe = (damageLocation - transform.position).normalized;
        if(!undodgeable && UnityEngine.Random.value < Reflex / 70f)
        {
            //Get the dodge direction
            Vector3 dodgeDirection;
            if(UnityEngine.Random.value > .5f)
            {
                dodgeDirection = Vector3.Cross(toMe, Vector3.up).normalized;
            }
            else
            {
                dodgeDirection = Vector3.Cross(Vector3.up, toMe).normalized;
            }

            rb.AddForce(dodgeDirection * 7f, ForceMode.Impulse);
            FloatingText floatingText = Instantiate(FarmonController.instance.FloatingTextPrefab, transform.position, Quaternion.identity).GetComponent<FloatingText>();
            floatingText.Setup("Dodged!", Color.white);
        }
        else
        {
            rb.AddForce(toMe * knockBack, ForceMode.Impulse);
            ChangeHeath(-damage);

            FloatingText floatingText = Instantiate(FarmonController.instance.FloatingTextPrefab, transform.position, Quaternion.identity).GetComponent<FloatingText>();
            floatingText.Setup(damage.ToString(), Color.red);
        }
    }

    public static float nearPlayerDistance = 6f;

    public void GetClosestEnemyUnit()
    {
        ClosestEnemyUnit = null;
        ClosestEnemyUnitNearPlayer = null;
        float closestEnemyDistance = float.MaxValue;
        float closestEnemyNearPlayerDistance = float.MaxValue;
        foreach (Farmon u in Farmon.unitList)
        {
            if (team == u.team) continue;

            float distanceToUnit = Vector3.Distance(u.transform.position, transform.position);

            if (distanceToUnit < closestEnemyDistance)
            {
                ClosestEnemyUnit = u;
                closestEnemyDistance = distanceToUnit;
            }

            float distanceToPlayer = Vector3.Distance(u.transform.position, Player.instance.transform.position);
            
            if(distanceToPlayer < nearPlayerDistance)
            {
                if (distanceToUnit < closestEnemyNearPlayerDistance)
                {
                    ClosestEnemyUnitNearPlayer = u;
                    closestEnemyNearPlayerDistance = distanceToUnit;
                }
            }

        }
    }

    public void GetClosestFriendlyUnit()
    {
        ClosestFriendlyUnit = null;
        float closestEnemyDistance = float.MaxValue;
        foreach (Farmon u in Farmon.unitList)
        {
            if (team != u.team) continue;

            float distanceToPlayer = Vector3.Distance(u.transform.position, transform.position);

            if (distanceToPlayer < closestEnemyDistance)
            {
                ClosestFriendlyUnit = u;
                closestEnemyDistance = distanceToPlayer;
            }
        }
    }

    public Vector3 GetRandomLocationAround(float radius, float minimumRadius)
    {
        Vector3 targetOffset = Quaternion.Euler(0, UnityEngine.Random.Range(0, 359), 0) * new Vector3(UnityEngine.Random.Range(minimumRadius, minimumRadius + radius), 0, 0);
        return Player.instance.transform.position + targetOffset;
    }

    public Vector3 GetUnitVectorToMe(Vector3 origin)
    {
        return (transform.position - origin).normalized;
    }

    public void SetState(StateMachineState state)
    {
        farmonStateMachine.ChangeState(state);
    }

    public Color debugColor = Color.white;

    protected virtual void OnDrawGizmosSelected()
    {
        if (ClosestEnemyUnit) Debug.DrawLine(transform.position, ClosestEnemyUnit.transform.position, Color.red);

        DebugExtension.DrawCircle(transform.position, Vector3.up, Color.yellow, targetRange);

        DebugExtension.DrawCircle(transform.position, Vector3.up, Color.red, Farmon.alertDistance);
        DebugExtension.DrawCircle(transform.position, Vector3.up, Color.red, Farmon.giveUpDistance);

        DebugExtension.DrawCylinder(transform.position, transform.position + Vector3.up * 1, debugColor, .25f);
    }

    public void LevelUp()
    {
        level++;
        attributePoints++;
        perkPoints++;
        statsChangedEvent.Invoke(this, EventArgs.Empty);
    }

    public int GetModifiedGrit()
    {
        return Grit;
    }

    public int GetModifiedPower()
    {
        return Power;
    }

    public int GetModifiedReflex()
    {
        return Reflex;
    }

    public int GetModifiedFocus()
    {
        return Focus;
    }

    public int GetModifiedSpeed()
    {
        return Speed;
    }

    public void MovementIdle()
    {
        Vector3 softSeperate = SoftSeperate(unitList, sphereCollider.radius);
        Vector3 separate = Seperate(unitList, sphereCollider.radius - AllowedOverlap);

        softSeperate *= 3f;
        separate *= .5f;

        rb.AddForce(softSeperate);
        rb.AddForce(separate);
    }

    public void MovementWander()
    {
        Vector3 wander = Wander();
        Vector3 softSeperate = SoftSeperate(unitList, sphereCollider.radius);
        Vector3 separate = Seperate(unitList, sphereCollider.radius - AllowedOverlap);

        wander *= 1f;
        softSeperate *= 3f;
        separate *= .5f;

        rb.AddForce(wander);
        rb.AddForce(softSeperate);
        rb.AddForce(separate);
    }

    public void StayInRange(Vector3 position, float min, float max)
    {
        Vector3 wander = Wander();
        Vector3 softSeperate = SoftSeperate(unitList, sphereCollider.radius);
        Vector3 separate = Seperate(unitList, sphereCollider.radius - AllowedOverlap);
        Vector3 minDistance = MinDistance(position, min);
        Vector3 maxDistance = MaxDistance(position, max);

        wander *= 1f;
        minDistance *= 2f;
        maxDistance *= 2f;
        softSeperate *= 3f;
        separate *= .5f;

        rb.AddForce(wander);
        rb.AddForce(softSeperate);
        rb.AddForce(separate);
        rb.AddForce(minDistance);
        rb.AddForce(maxDistance);
    }

    public void SeekPosition()
    {
        Vector3 seek = Seek();
        Vector3 softSeperate = SoftSeperate(unitList, sphereCollider.radius);
        Vector3 separate = Seperate(unitList, sphereCollider.radius - AllowedOverlap);

        seek *= 1f;
        softSeperate *= 3f;
        separate *= .5f;

        rb.AddForce(seek);
        rb.AddForce(softSeperate);
        rb.AddForce(separate);
    }

    public void SeekUnit()
    {
        Vehicle targetVehicle = targetTransform.GetComponentInParent<Vehicle>();
        Assert.IsNotNull(targetVehicle);

        Vector3 seek;

        float d = Vector3.Distance(transform.position, targetVehicle.transform.position);
        if (d > sphereCollider.radius + targetVehicle.sphereCollider.radius + 0.15f)
        {
            seek = Seek();
        }
        else
        {
            seek = new Vector3();
        }
        Vector3 softSeperate = SoftSeperate(unitList, sphereCollider.radius);
        Vector3 separate = Seperate(unitList, sphereCollider.radius - AllowedOverlap);

        seek *= 1f;
        softSeperate *= 3f;
        separate *= .5f;

        rb.AddForce(seek);
        rb.AddForce(softSeperate);
        rb.AddForce(separate);
    }

    public void Arrive()
    {
        Vector3 arrive = Arrive(Mathf.Sqrt(maxSpeed) / 2);
        Vector3 softSeperate = SoftSeperate(unitList, sphereCollider.radius);
        Vector3 separate = Seperate(unitList, sphereCollider.radius - AllowedOverlap);

        arrive *= 1f;
        softSeperate *= 3f;
        separate *= .5f;

        rb.AddForce(arrive);
        rb.AddForce(softSeperate);
        rb.AddForce(separate);
    }

    public void PlayerConstraint()
    {
        if (team == TeamEnum.player)
        {
            Vector3 constraint = MaxDistance(Player.instance.transform.position, 5);

            constraint *= 3f;

            rb.AddForce(constraint);
        }
    }

    public abstract void Attack(Farmon targetUnit);
}

public class IdleState : StateMachineState
{
    Farmon unit;
    Transform flag;

    Timer changeTargetLocationTimer;

    public IdleState(Farmon thisUnit)
    {
        unit = thisUnit;

        flag = new GameObject(unit.name + "Flag").transform;
        flag.parent = Player.instance.transform;

        changeTargetLocationTimer = new Timer();
        changeTargetLocationTimer.SetTime(3f + UnityEngine.Random.Range(0f, 2f));
    }

    public override void Enter()
    {
        base.Enter();


        unit.maxSpeed = unit.GetMovementSpeed();

        unit.targetTransform = flag;
        flag.position = unit.GetRandomLocationAround(2f, Player.instance.sphereCollider.radius + unit.sphereCollider.radius);
    }

    public override void Tick()
    {
        unit.maxSpeed = unit.GetMovementSpeed();

        // Get a potential attack target based on whether this is a player's farmon.
        Farmon potentialTarget;
        if (unit.isPlayerFarmon)
        {
            potentialTarget = unit.ClosestEnemyUnitNearPlayer;
        }
        else
        {
            potentialTarget = unit.ClosestEnemyUnit;
        }

        //Get the distance to the potentialTarget.
        float distanceToClosestEnemy;
        if (potentialTarget == null)
        {
            distanceToClosestEnemy = float.MaxValue;
        }
        else
        {
            distanceToClosestEnemy = Vector3.Distance(potentialTarget.transform.position, unit.transform.position);
        }

        // If we have a target, 
        if (unit.attackTarget)
        {
            unit.debugColor = Color.red;
            unit.targetTransform = unit.attackTarget.transform;
            unit.StayInRange(unit.targetTransform.position, unit.targetRange - 1, unit.targetRange + 1);

            if (unit.attackReady)
            {
                unit.Attack(unit.attackTarget);
                unit.attackReady = false;
            }

            if(distanceToClosestEnemy > Farmon.giveUpDistance)
            {
                unit.attackTarget = null;
            }
        }
        else
        {
            unit.debugColor = Color.green;
            if (unit.isPlayerFarmon)
            {
                unit.maxSpeed = Player.instance.GetMovementSpeed() + 1.5f;

                WanderPlayer();
            }
            else
            {
                WanderWild();
            }

            if (distanceToClosestEnemy < Farmon.alertDistance)
            {
                if (unit.isPlayerFarmon)
                {
                    unit.attackTarget = unit.ClosestEnemyUnitNearPlayer;
                }
                else
                {
                    unit.attackTarget = unit.ClosestEnemyUnit;
                }
            }
        }
    }

    public virtual void WanderPlayer()
    {
        unit.targetTransform = flag;

        unit.Arrive();

        if (changeTargetLocationTimer.Tick(Time.deltaTime))
        {
            flag.position = unit.GetRandomLocationAround(2f, Player.instance.sphereCollider.radius + unit.sphereCollider.radius);
            changeTargetLocationTimer.SetTime(3f + UnityEngine.Random.Range(0f, 2f));
        }
    }

    public virtual void WanderWild()
    {
        unit.MovementWander();
    }
}

public class WanderState : StateMachineState
{
    Farmon unit;

    public WanderState(Farmon thisUnit)
    {
        unit = thisUnit;
    }

    public override void Enter()
    {
        base.Enter();

        unit.maxSpeed = unit.GetMovementSpeed();
    }

    public override void Tick()
    {
        base.Tick();

        unit.MovementWander();
    }
}

public class StayInRangeState : StateMachineState
{
    Farmon farmon;

    public StayInRangeState(Farmon farmon)
    {
        this.farmon = farmon;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Tick()
    {
        base.Tick();

        farmon.maxSpeed = farmon.GetMovementSpeed();

        farmon.StayInRange( farmon.targetTransform.position, farmon.targetRange - 1 , farmon.targetRange + 1);
    }
}

public class FollowPlayerState : StateMachineState
{
    Farmon unit;
    Transform flag;

    Timer changeTargetLocationTimer;

    public FollowPlayerState(Farmon thisUnit)
    {
        unit = thisUnit;

        flag = new GameObject(unit.name + "Flag").transform;
        flag.parent = Player.instance.transform;

        changeTargetLocationTimer = new Timer();
        changeTargetLocationTimer.SetTime(3f + UnityEngine.Random.Range(0f, 2f));
    }

    public override void Enter()
    {
        base.Enter();

        unit.maxSpeed = unit.GetMovementSpeed();

        unit.targetTransform = flag;
        flag.position = unit.GetRandomLocationAround(2f, Player.instance.sphereCollider.radius + unit.sphereCollider.radius);
    }

    public override void Tick()
    {
        base.Tick();

        unit.SeekPosition();

        unit.PlayerConstraint();

        if (changeTargetLocationTimer.Tick(Time.deltaTime))
        {
            flag.position = unit.GetRandomLocationAround(2f, Player.instance.sphereCollider.radius + unit.sphereCollider.radius);
            changeTargetLocationTimer.SetTime(3f + UnityEngine.Random.Range(0f, 2f));
        }
    }
}

public class SpawnState : StateMachineState
{
    Farmon unit;
    Timer spawnTimer;

    public SpawnState(Farmon thisUnit)
    {
        unit = thisUnit;
        spawnTimer = new Timer();
        spawnTimer.SetTime(1f);
    }

    public override void Enter()
    {
        base.Enter();

        unit.maxSpeed = 0;
    }

    public override void Tick()
    {
        base.Tick();

        unit.MovementIdle();

        if (spawnTimer.Tick(Time.deltaTime))
        {
            unit.SetState(unit.idleState);
        }
    }
}