using Assets.Scripts.States;
using Assets.Scripts.Timer;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using DG.Tweening;

public abstract class Farmon : Vehicle
{
    public static List<Farmon> farmonList = new List<Farmon>();
    public static Dictionary<uint, Farmon> loadedFarmonMap = new Dictionary<uint, Farmon>();
    private static uint loadedFarmonMapCurrentIndex = 1;
    private static float followRange = 3;

    public static int StatMax = 40;

    public enum TeamEnum
    {
        team1,
        team2
    }

    //Enum used to filter for specific lists of farmon.
    public enum FarmonFilterEnum
    {
        any,
        myTeam,
        enemyTeam,
        team1,
        team2
    }

    //Enum used to sort a list of farmon.
    public enum FarmonSortEnum
    {
        none,
        nearest,
        furthest,
        lowestHealth,
        mostHealth
    }

    public string DebugString;
    public uint uniqueID = 0;

    [HideInInspector]
    public FarmonHud Hud;

    [HideInInspector]
    public SpriteRenderer mySpriteRenderer;
    [HideInInspector]
    public Bar healthBar;

    private StateMachine farmonStateMachine;

    public uint loadedFarmonMapId = 0;

    public string farmonName = "Unit";
    public string nickname = "";
    public string Description = "Default description.";
    public int level = 1;
    public int experience = 0;
    public bool canJump = false;

    public Dictionary<string, int> perkList = new Dictionary<string, int>();

    [HideInInspector]
    public int attributePoints = 1;
    [HideInInspector]
    public int perkPoints = 1;

    [HideInInspector]
    public int MaxHealth = 3;
    private int health = 3;

    [HideInInspector]
    public bool Selected = false;

    [HideInInspector]
    public bool dead = false;

    [HideInInspector]
    public bool attackReady = false;
    Timer attackTimer = new Timer();

    public float targetRange = 5f;

    public uint attackTarget;
    public uint protectTarget;

    //Hover Highlight
    Highlight _hoverHighlight;
    Highlight _selectedHighlight;
    private HighlightList _highlightList;

    //States
    public StateMachineState mainState;
    public StateMachineState attackState;
    public StateMachineState spawnState;

    //Effects
    public EffectList EffectList = new EffectList();
    private Timer burnTimer = new Timer();

    GameObject shadow;

    public TeamEnum team;

    public EventHandler statsChangedEvent;

    [HideInInspector]
    public Farmon LastFarmonToDamageMe = null;

    [HideInInspector]
    public UnityEvent GridSpaceChangedEvent = new UnityEvent();
    [HideInInspector]
    public Vector3Int GridSpaceIndex;

    Timer hitStopTimer = new Timer();

    [HideInInspector]
    private bool immuneToHitStop = false;
    public bool ImmuneToHitStop { get => immuneToHitStop; set => immuneToHitStop = value; }
    public static GameObject ConstructFarmon(FarmonSaveData data, bool playerFarmon = false, Farmon farmonToReplace = null)
    {
        if (Debug.isDebugBuild)
        {
            Debug.Log("Loading data:" +
                      "\nNickname: " + data.Nickname +
                      "\nFarmon Name: " + data.FarmonName +
                      "\n\nLevel: " + data.Level +
                      "\nExperience: " + data.experience +
                      "\nPerk Points: " + data.perkPoints +
                      "\nAttribute Points: " + data.attributePoints +
                      "\n\nGrit Bonus: " + data.GritBonus +
                      "\nPower Bonus: " + data.PowerBonus +
                      "\nAgility Bonus: " + data.AgilityBonus +
                      "\nFocus Bonus: " + data.FocusBonus +
                      "\nLuck Bonus: " + data.LuckBonus);
        }

        GameObject farmonPrefab = Resources.Load("Farmon/" + data.FarmonName) as GameObject;
        GameObject farmonGameObject = GameObject.Instantiate(farmonPrefab);

        Farmon farmon = farmonGameObject.GetComponent<Farmon>();

        farmon.uniqueID = data.uniqueID;

        farmon.farmonName = data.FarmonName;
        farmon.nickname = data.Nickname;

        farmon.GritBonus = data.GritBonus;
        farmon.PowerBonus = data.PowerBonus;
        farmon.AgilityBonus = data.AgilityBonus;
        farmon.FocusBonus = data.FocusBonus;
        farmon.LuckBonus = data.LuckBonus;

        farmon.level = data.Level;
        farmon.experience = data.experience;
        farmon.perkPoints = data.perkPoints;
        farmon.attributePoints = data.attributePoints;

        farmon.mainState = new IdleState(farmon);

        foreach (string perkString in data.perks)
        {
            string[] perkStringSplit = perkString.Split(":");

            farmon.perkList[perkStringSplit[0]] = int.Parse(perkStringSplit[1]);
        }

        if (farmon.nickname == "")
        {
            farmonGameObject.name = farmon.farmonName;
        }
        else
        {
            farmonGameObject.name = farmon.nickname;
        }

        //Lastly, add the farmon to the farmon map. If we are replacing a farmon, replace its entry in the loadedFarmonMap
        if (farmonToReplace)
        {
            loadedFarmonMap[farmonToReplace.loadedFarmonMapId] = farmon;
            farmon.loadedFarmonMapId = farmonToReplace.loadedFarmonMapId;
        }
        else
        {
            loadedFarmonMap[loadedFarmonMapCurrentIndex] = farmon;
            farmon.loadedFarmonMapId = loadedFarmonMapCurrentIndex;
            loadedFarmonMapCurrentIndex++;
        }

        if(playerFarmon) Player.instance.LoadedFarmon.Add(farmon.loadedFarmonMapId);

        return farmonGameObject;
    }

    public static void UnloadFarmon()
    {
        Player.instance.LoadedFarmon.Clear();
        foreach(var tuple in loadedFarmonMap)
        {
            Destroy((tuple.Value as Farmon).gameObject);
        }

        loadedFarmonMap.Clear();
        loadedFarmonMapCurrentIndex = 1;
    }

    #region Grit
    public int Grit {
        get => GritBase + gritBonus;
    }

    public int GritBase = 10;

    [SerializeField, HideInInspector]
    private int gritBonus = 0;
    public int GritBonus {
        get => gritBonus;
        set
        {
            if (GritBase + value > StatMax)
            {
                int dif = GritBase + value - StatMax;
                gritBonus = value - dif;

                attributePoints += dif;
            }
            else
            {
                gritBonus = value;
            }


            // Update max health
            float healthPercent = (float)health / (float)MaxHealth;
            MaxHealth = 10 + Grit * 5;

            // Adjust remaining health after the maxHealthChange
            SetHealth((int)Mathf.Ceil((float)MaxHealth * healthPercent));

            // Set the size of the farmon
            float scaleIncrease = (float)Grit / ((float)StatMax * 2f);
            SetSize(scaleIncrease);
            if(Hud) Hud.SpriteQuad.transform.localScale = (1f + scaleIncrease) * Vector3.one;
            if (shadow)
            {
                shadow.transform.localScale = sphereCollider.radius * 2f * Vector3.one;
            }

            statsChangedEvent?.Invoke(this, EventArgs.Empty);
        }
    }
    #endregion

    #region Power
    public int Power
    {
        get => PowerBase + PowerBonus;
    }

    public int PowerBase = 10;

    [SerializeField, HideInInspector]
    int powerBonus = 0;
    public int PowerBonus
    {
        get => powerBonus;
        set
        {
            if (PowerBase + value > StatMax)
            {
                int dif = PowerBase + value - StatMax;
                powerBonus = value - dif;

                attributePoints += dif;
            }
            else
            {
                powerBonus = value;
            }

            maxForce = 50;
            statsChangedEvent?.Invoke(this, EventArgs.Empty);
        }
    }
    #endregion

    #region Agility
    public int Agility
    {
        get => AgilityBase + AgilityBonus;
    }

    public int AgilityBase = 10;

    [SerializeField, HideInInspector]
    int agilityBonus = 0;
    public int AgilityBonus
    {
        get => agilityBonus;
        set
        {
            if (AgilityBase + value > StatMax)
            {
                int dif = AgilityBase + value - StatMax;
                agilityBonus = value - dif;

                attributePoints += dif;
            }
            else
            {
                agilityBonus = value;
            }

            statsChangedEvent?.Invoke(this, EventArgs.Empty);
        }
    }
    #endregion

    #region Focus
    public int Focus
    {
        get => FocusBase + FocusBonus;
    }

    public int FocusBase = 10;

    [SerializeField, HideInInspector]
    int focusBonus = 0;
    public int FocusBonus
    {
        get => focusBonus;
        set
        {
            if (FocusBase + value > StatMax)
            {
                int dif = FocusBase + value - StatMax;
                focusBonus = value - dif;

                attributePoints += dif;
            }
            else
            {
                focusBonus = value;
            }

            statsChangedEvent?.Invoke(this, EventArgs.Empty);
        }
    }
    #endregion

    #region Luck
    public int Luck
    {
        get => LuckBase + LuckBonus;
    }

    public int LuckBase = 10;

    [SerializeField, HideInInspector]
    int luckBonus = 0;
    public int LuckBonus
    {
        get => luckBonus;
        set
        {
            if (LuckBase + value > StatMax)
            {
                int dif = LuckBase + value - StatMax;
                luckBonus = value - dif;

                attributePoints += dif;
            }
            else
            {
                luckBonus = value;
            }

            statsChangedEvent?.Invoke(this, EventArgs.Empty);
        }
    }
    #endregion

    public abstract float AttackTime();

    public override void Initialize()
    {
        if (initialized) return;

        base.Initialize();

        Hud = GetComponentInChildren<FarmonHud>();
        Assert.IsNotNull(Hud);
        Hud.TargetFarmon = this;

        mySpriteRenderer = Hud.SpriteQuad.GetComponentInChildren<SpriteRenderer>();
        Assert.IsNotNull(mySpriteRenderer);

        farmonStateMachine = new StateMachine();
        spawnState = new SpawnState(this);

        mainState = new IdleState(this);

        farmonStateMachine.InitializeStateMachine(spawnState);

        shadow = Instantiate(FarmonController.instance.ShadowPrefab, transform.position + sphereCollider.center + ((sphereCollider.radius - 0.01f) * Vector3.down), Quaternion.Euler(90, 0, 0), this.transform);

        _highlightList = mySpriteRenderer.GetComponentInParent<HighlightList>();

        if (team == Player.instance.playerTeam)
        {
            _highlightList.AddHighlight(Color.white, 100);
        }
        else
        {
            _highlightList.AddHighlight(Color.red, 100);
        }

        GridSpaceIndex = H.Vector3ToGridPosition(transform.position, LevelController.Instance.gridSize);

        RefreshStats();

        SetHealth(MaxHealth);

        attackTimer.autoReset = false;
        attackTimer.SetTime(AttackTime());

        burnTimer.autoReset = true;
        burnTimer.SetTime(1f);
    }

    private void OnValidate()
    {
        RefreshStats();
    }

    void RefreshStats()
    {
        GritBonus = gritBonus;
        PowerBonus = powerBonus;
        AgilityBonus = agilityBonus;
        FocusBonus = focusBonus;
        LuckBonus = luckBonus;
    }

    public virtual float GetMovementSpeed()
    {
        return 3 + Agility / 6;
    }

    public int GetArmor()
    {
        //Add up all armor sources and return;
        return EffectList.TortorrentShield;
    }

    protected override void Awake()
    {
        base.Awake();
        farmonList.Add(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        farmonList.Remove(this);
    }

    private void OnMouseEnter()
    {
        _hoverHighlight = _highlightList.AddHighlight(Color.gray, 5);
    }

    private void OnMouseExit()
    {
        _highlightList.RemoveHighlight(_hoverHighlight);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        DebugString = "";
        if (Debug.isDebugBuild)
        {
            DebugString += "\n" + farmonStateMachine.CurrentState.ToString();
        }

        if (!FarmonController.Paused)
        {
            if (attackReady == false && attackTimer.Tick(Time.deltaTime))
            {
                attackReady = true;
            }

            if (EffectList.Burn > 0 && burnTimer.Tick(Time.deltaTime))
            {
                AttackData burnDamageData = new AttackData((int)EffectList.Burn, 0, 0, true);
                TakeDamage(burnDamageData, transform.position, Vector3.zero, null);
            }

            if (hitStopTimer.Tick(Time.deltaTime))
            {
                //DOTween.Play(Hud.SpriteQuad.transform);
                rb.constraints = RigidbodyConstraints.FreezeRotation;

                Hud.Animator.speed = 1;
            }

            EffectList.UpdateEffects(Time.deltaTime);
        }

        // only tick if the game isn't paused or we are in the die state.
        if(!FarmonController.Paused && !hitStopTimer.running || farmonStateMachine.CurrentState.GetType() == typeof(DieState))
        {
            farmonStateMachine.Tick();
        }
        else
        {
            MovementIdle();
        }

        Vector3Int currentGridSpaceIndex = H.Vector3ToGridPosition(transform.position, LevelController.Instance.gridSize);

        if(GridSpaceIndex != currentGridSpaceIndex)
        {
            GridSpaceIndex = currentGridSpaceIndex;
            GridSpaceChangedEvent.Invoke();
        }

        if (Physics.BoxCast(transform.position, sphereCollider.radius/2 * Vector3.one, Vector3.down, out RaycastHit hitInfo, Quaternion.identity, 100, LayerMask.GetMask("Default")))
        {
            shadow.transform.position = transform.position + ((hitInfo.distance + (sphereCollider.radius / 2) - 0.01f) * Vector3.down);
        }
    }

    private void LateUpdate()
    {
        Hud.debugText.text = DebugString;
    }

    public void Select()
    {
        Selected = true;

        _selectedHighlight = _highlightList.AddHighlight(Color.green, 4);
    }

    public void Deselect()
    {
        Selected = false;

        _highlightList.RemoveHighlight(_selectedHighlight);
    }

    public void EnterAttackState(Farmon target)
    {
        protectTarget = 0;
        attackTarget = target.loadedFarmonMapId;
        mainState = new AttackState(this);
        SetState(mainState);
    }

    public void EnterDefendState(Farmon target)
    {
        protectTarget = target.loadedFarmonMapId;
        attackTarget = 0;
        mainState = new DefendState(this);
        SetState(mainState);
    }

    public virtual void AttackComplete()
    {
        attackTimer.SetTime(AttackTime());
        SetState(mainState);
    }

    public Farmon GetAttackTargetFarmon()
    {
        Farmon.loadedFarmonMap.TryGetValue(attackTarget, out Farmon retValue);
        return retValue;
    }

    public Farmon GetProtectTargetFarmon()
    {
        Farmon.loadedFarmonMap.TryGetValue(protectTarget, out Farmon retValue);
        return retValue;
    }


    public static List<Farmon> SearchFarmon(Farmon originFarmon, FarmonFilterEnum farmonFilter = FarmonFilterEnum.any, FarmonSortEnum farmonSort = FarmonSortEnum.nearest, float maxRange = 10000, List<Farmon> exclusions = default, bool excludeDead = true)
    {
        List<Farmon> searchList = new List<Farmon>(farmonList);

        //filter the farmon based on teams.
        searchList = FarmonFilter(searchList, farmonFilter, originFarmon);

        //sort the farmon based on different criteria.
        searchList = FarmonSort(searchList, farmonSort, originFarmon);

        // Remove farmon that are too far away.
        searchList = searchList.FindAll((farmon) => { return Vector3.Distance(farmon.transform.position, originFarmon.transform.position) < maxRange; });

        // Remove excluded farmon.
        searchList = searchList.FindAll((farmon) => { return !exclusions.Contains(farmon); });

        if (excludeDead)
        {
            // Remove dead farmon.
            searchList = searchList.FindAll((farmon) => { return !farmon.dead; });
        }

        return searchList;
    }

    public static List<Farmon> FarmonFilter(List<Farmon> listToFilter, FarmonFilterEnum farmonFilter, Farmon originFarmon = null)
    {
        switch(farmonFilter)
        {
            case FarmonFilterEnum.any:
                return listToFilter;
            case FarmonFilterEnum.enemyTeam:
                return listToFilter.FindAll((farmon) => { return farmon.team != originFarmon.team; });
            case FarmonFilterEnum.myTeam:
                return listToFilter.FindAll((farmon) => { return farmon.team == originFarmon.team; });
            case FarmonFilterEnum.team1:
                return listToFilter.FindAll((farmon) => { return farmon.team == TeamEnum.team1; });
            case FarmonFilterEnum.team2:
                return listToFilter.FindAll((farmon) => { return farmon.team == TeamEnum.team2; });
        }

        return listToFilter;
    }

    public static List<Farmon> FarmonSort(List<Farmon> listToSort, FarmonSortEnum farmonSort, Farmon originFarmon = null)
    {
        switch (farmonSort)
        {
            case FarmonSortEnum.none:
                break;
            case FarmonSortEnum.furthest:
                listToSort.Sort((f1, f2) => { return Vector3.Distance(f2.transform.position, originFarmon.transform.position).CompareTo(Vector3.Distance(f1.transform.position, originFarmon.transform.position)); });
                break;
            case FarmonSortEnum.nearest:
                listToSort.Sort((f1, f2) => { return Vector3.Distance(f1.transform.position, originFarmon.transform.position).CompareTo(Vector3.Distance(f2.transform.position, originFarmon.transform.position)); });
                break;
            case FarmonSortEnum.mostHealth:
                listToSort.Sort((f1, f2) => { return f2.health.CompareTo(f1.health); });
                break;
            case FarmonSortEnum.lowestHealth:
                listToSort.Sort((f1, f2) => { return f1.health.CompareTo(f2.health); });
                break;
        }

        return listToSort;
    }


    public virtual bool IsInAttackPosition()
    {
        Farmon attackFarmon = GetAttackTargetFarmon();

        //Can we see the enemy?
        Vector3 targetFeet = attackFarmon.transform.position + (attackFarmon.sphereCollider.radius - .01f) * Vector3.down;
        Vector3 myFeet = transform.position + (sphereCollider.radius - .01f) * Vector3.down;

        Vector3 toEnemy = targetFeet - myFeet;

        Ray r = new Ray(myFeet, toEnemy.normalized);

        bool wallIsBlockingTarget = Physics.Raycast(r, toEnemy.magnitude, LayerMask.GetMask("Default"));

        bool isWithinAttackRange = toEnemy.magnitude < targetRange;

        return !wallIsBlockingTarget && isWithinAttackRange;
    }

    public virtual bool IsInProtectPosition()
    {
        Farmon protectFarmon = GetProtectTargetFarmon();

        //Can we see the freindly?
        Vector3 targetFeet = protectFarmon.transform.position + (protectFarmon.sphereCollider.radius - .01f) * Vector3.down;
        Vector3 myFeet = transform.position + (sphereCollider.radius - .01f) * Vector3.down;

        Vector3 toFriendly = targetFeet - myFeet;

        Ray r = new Ray(myFeet, toFriendly.normalized);

        bool wallIsBlockingTarget = Physics.Raycast(r, toFriendly.magnitude, LayerMask.GetMask("Default"));

        bool isWithinAttackRange = toFriendly.magnitude < followRange;

        return !wallIsBlockingTarget && isWithinAttackRange;
    }

    public int GetHealth()
    {
        return health;
    }

    public void SetHealth(int value)
    {
        if (dead)
        {
            return;
        }

        if(value > MaxHealth)
        {
            health = MaxHealth;
        }
        else if (value <= 0)
        {
            health = 0;
            Die();
        }
        else
        {
            health = value;
        }

        if (healthBar)
        {
            healthBar.SetPercent((float)health / MaxHealth);
        }
    }

    public void ChangeHeath(int amount)
    {
        SetHealth(health + amount);
    }

    public UnityEvent HealEvent;
    public void Heal(int amount)
    {
        if (dead) return;
        ChangeHeath(amount);

        HealEvent.Invoke();
    }

    public UnityEvent DamageEvent;
    public bool TakeDamage(AttackData attackData, Vector3 damageOrigin, Vector3 knockBackDirection, Farmon owner)
    {        
        if(!attackData.Undodgeable && UnityEngine.Random.value < Agility / 100f)
        {
            //Get the dodge direction
            Vector3 dodgeDirection;
            if(UnityEngine.Random.value > .5f)
            {
                dodgeDirection = Vector3.Cross(knockBackDirection, Vector3.up).normalized;
            }
            else
            {
                dodgeDirection = Vector3.Cross(Vector3.up, knockBackDirection).normalized;
            }

            rb.AddForce(dodgeDirection * 7f, ForceMode.Impulse);
            FloatingText floatingText = Instantiate(FarmonController.instance.FloatingTextPrefab, transform.position, Quaternion.identity).GetComponent<FloatingText>();
            floatingText.Setup("Dodged!", Color.white);

            Hud.AudioSource.clip = FarmonController.instance.DodgeSound;
            Hud.AudioSource.volume = .3f;
            Hud.AudioSource.Play();

            return false;
        }
        else
        {
            if(owner) LastFarmonToDamageMe = owner;
            if (!ImmuneToHitStop && attackData.HitStopTime > 0)
            {
                SetState(new HitStopState(this, attackData, damageOrigin, knockBackDirection));
            }
            else
            {
                ChangeHeath(-attackData.Damage);

                MakeDamageNumber(attackData);

                MakeHitEffect(attackData, damageOrigin);

                DamageEvent.Invoke();
            }

            return true;
        }
    }

    public void MakeDamageNumber(AttackData attackData)
    {
        FloatingText floatingText = Instantiate(FarmonController.instance.FloatingTextPrefab, transform.position, Quaternion.identity).GetComponent<FloatingText>();
        float severityPercent = Mathf.Min(attackData.Damage / (StatMax / 2), 1f);
        floatingText.transform.localScale = (0.7f + 0.4f * severityPercent) * Vector3.one;

        Color color1 = new Color(1.0f, 0.549f, 0.004f);
        Color color2 = new Color(0.929f, 0.161f, 0.220f);
        Color damageColor = Color.Lerp(color1, color2, severityPercent);

        floatingText.Setup(attackData.Damage.ToString(), damageColor);
    }

    public void MakeHitEffect(AttackData attackData, Vector3 damageOrigin)
    {
        //Spawn a hit effect.
        Vector3 meToDamageOrigin = damageOrigin - (sphereCollider.transform.position + sphereCollider.center);
        GameObject hitEffect = Instantiate(FarmonController.instance.HitEffectPrefab, transform);
        hitEffect.transform.position = transform.position + meToDamageOrigin.normalized * sphereCollider.radius;
        hitEffect.transform.localScale = (.2f + 2.5f * attackData.HitStopTime) * Vector3.one;
    }

    public void Die()
    {
        SetState(new DieState(this));

        if(team == TeamEnum.team1)
        {
            GameController.SlowMo(2.5f, .3f);
        }
    }

    public void HitStopSelf(float stopTime)
    {
        //DOTween.Pause(Hud.SpriteQuad.transform);
        rb.constraints = RigidbodyConstraints.FreezeAll;

        hitStopTimer.SetTime(stopTime);

        Hud.Animator.speed = 0;
    }

    public void AddPerk(Perk perk)
    {
        perkList.TryGetValue(perk.PerkName, out int perkCurrentValue);

        perkList[perk.PerkName] = perkCurrentValue + 1;

        statsChangedEvent?.Invoke(this, EventArgs.Empty);
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
        if (dead) return;

        farmonStateMachine.ChangeState(state);
    }

    public Color debugColor = Color.white;

    protected virtual void OnDrawGizmosSelected()
    {
        DebugExtension.DrawCircle(transform.position, Vector3.up, Color.yellow, targetRange);
        Gizmos.color = Color.green;
        if(sphereCollider) Gizmos.DrawWireSphere(transform.position + sphereCollider.center, sphereCollider.radius);
    }

    public int GetXpRequiredForNextLevel()
    {
        return 100;
    }

    internal void GiveXp(int xp)
    {
        experience += xp;

        if (experience >= GetXpRequiredForNextLevel())
        {
            experience -= GetXpRequiredForNextLevel();
            LevelUp();
        }
    }

    public virtual void LevelUp()
    {
        level++;
        perkPoints++;
        statsChangedEvent?.Invoke(this, EventArgs.Empty);

        GetLevelUpBonusStats(out int gritPlus, out int powerPlus, out int agilityPlus, out int focusPlus, out int luckPlus, out int pointsPlus);


        int gritPrev = Grit;
        int powerPrev = Power;
        int agilityPrev = Agility;
        int focusPrev = Focus;
        int luckPrev = Luck;
        int pointsPrev = attributePoints;

        GritBonus += gritPlus;
        PowerBonus += powerPlus;
        AgilityBonus += agilityPlus;
        FocusBonus += focusPlus;
        LuckBonus += luckPlus;
        attributePoints += pointsPlus;

        LevelUpScreen.instance.Popup(this, gritPrev, powerPrev, agilityPrev, focusPrev, luckPrev, pointsPrev);

        FloatingText floatingText = Instantiate(FarmonController.instance.FloatingTextPrefab, transform.position, Quaternion.identity).GetComponent<FloatingText>();
        floatingText.Setup("LEVEL UP!", Color.yellow);
    }

    public abstract void OnLevelUp();

    public Farmon Evolve(string newFarmonName)
    {
        farmonName = newFarmonName;

        //Save this farmon with the new farmon name then immediately load it and replace this farmon instance with it.
        SaveController.SaveFarmonPlayer(this);
        GameObject evolutionGameObject = ConstructFarmon(SaveController.LoadFarmonPlayer(uniqueID), true, this);
        Farmon evolution = evolutionGameObject.GetComponent<Farmon>();
        evolution.Initialize();

        // Move the evolution to where we are
        evolution.transform.position = transform.position;
        FixPosition();

        // Set the evolution's state to the farmon's state. DANGEROUS?
        //evolution.farmonStateMachine.ChangeState(farmonStateMachine.CurrentState);
        evolution.attackTarget = attackTarget;
        evolution.protectTarget = protectTarget;

        // Copy all stats over to the new prefab instance
        evolution.uniqueID = uniqueID;

        evolution.farmonName = farmonName;
        evolution.nickname = nickname;

        evolution.GritBonus = GritBonus;
        evolution.PowerBonus = PowerBonus;
        evolution.AgilityBonus = AgilityBonus;
        evolution.FocusBonus = FocusBonus;
        evolution.LuckBonus = LuckBonus;

        evolution.level = level;
        evolution.experience = experience;
        evolution.perkPoints = perkPoints;
        evolution.attributePoints = attributePoints;

        foreach (var tuple in perkList)
        {
            evolution.perkList[tuple.Key] = tuple.Value;
        }

        if (evolution.nickname == "")
        {
            evolutionGameObject.name = evolution.farmonName;
        }
        else
        {
            evolutionGameObject.name = evolution.nickname;
        }

        Destroy(gameObject);

        //Return the newly evolved farmon.
        return evolution;
    }

    public virtual void DistributeEvolvePerks()
    {

    }

    protected virtual void GetLevelUpBonusStats(out int gritPlus, out int powerPlus, out int agilityPlus, out int focusPlus, out int luckPlus, out int pointsPlus)
    {
        gritPlus = 0;
        powerPlus = 0;
        agilityPlus = 0;
        focusPlus = 0;
        luckPlus = 0;
        pointsPlus = 0;

        switch ((int)UnityEngine.Random.Range(0, 4.999f))
        {
            case 0:
                gritPlus++;
                break;
            case 1:
                powerPlus++;
                break;
            case 2:
                agilityPlus++;
                break;
            case 3:
                focusPlus++;
                break;
            case 4:
                luckPlus++;
                break;
        }

        pointsPlus++;
    }

    public int GetModifiedGrit()
    {
        return Grit;
    }

    public int GetModifiedPower()
    {
        return Power;
    }

    public int GetModifiedAgility()
    {
        return Agility;
    }

    public int GetModifiedFocus()
    {
        return Focus;
    }

    public int GetModifiedLuck()
    {
        return Luck;
    }

    public void MovementIdle()
    {
        Vector3 softSeperate = SoftSeperate(vehicleList, sphereCollider.radius);
        Vector3 separate = Seperate(vehicleList, sphereCollider.radius - AllowedOverlap);
        

        softSeperate *= 3f;
        separate *= .5f;

        rb.AddForce(softSeperate);
        rb.AddForce(separate);

        Vector3 friction = 0.25f * Friction(softSeperate + separate);
        rb.AddForce(friction);
    }

    public void MovementWander()
    {
        Vector3 wander = Wander();
        Vector3 softSeperate = SoftSeperate(vehicleList, sphereCollider.radius);
        Vector3 separate = Seperate(vehicleList, sphereCollider.radius - AllowedOverlap);

        wander *= 1f;
        softSeperate *= 3f;
        separate *= .5f;

        rb.AddForce(wander);
        rb.AddForce(softSeperate);
        rb.AddForce(separate);

        Vector3 friction = Friction(wander + softSeperate + separate);
        rb.AddForce(friction);
    }

    public void StayInRange(Vector3 position, float min, float max)
    {
        Vector3 wander = Wander();
        Vector3 softSeperate = SoftSeperate(vehicleList, sphereCollider.radius);
        Vector3 separate = Seperate(vehicleList, sphereCollider.radius - AllowedOverlap);
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

        Vector3 friction = Friction(wander + softSeperate + separate + minDistance + maxDistance);
        rb.AddForce(friction);
    }

    public void SeekPosition(bool localAvoidance = true)
    {
        Vector3 seek = Seek(localAvoidance);
        Vector3 softSeperate = SoftSeperate(vehicleList, sphereCollider.radius);
        Vector3 separate = Seperate(vehicleList, sphereCollider.radius - AllowedOverlap);

        seek *= 1f;
        softSeperate *= 3f;
        separate *= .5f;

        rb.AddForce(seek);
        rb.AddForce(softSeperate);
        rb.AddForce(separate);

        Vector3 friction = Friction(seek + softSeperate + separate);
        rb.AddForce(friction);
    }

    public void SeekUnit(bool localAvoidance = true)
    {
        Vehicle targetVehicle = targetTransform.GetComponentInChildren<Vehicle>();

        if (!targetVehicle)
        {
            return;
        }

        Vector3 seek;

        float d = Vector3.Distance(transform.position, targetVehicle.transform.position);
        if (d > sphereCollider.radius + targetVehicle.sphereCollider.radius + 0.15f)
        {
            seek = Seek(localAvoidance);
        }
        else
        {
            seek = Vector3.zero;
        }
        Vector3 softSeperate = SoftSeperate(vehicleList, sphereCollider.radius);
        Vector3 separate = Seperate(vehicleList, sphereCollider.radius - AllowedOverlap);

        seek *= 1f;
        softSeperate *= 3f;
        separate *= .5f;

        rb.AddForce(seek);
        rb.AddForce(softSeperate);
        rb.AddForce(separate);

        Vector3 friction = Friction(seek + softSeperate + separate);
        rb.AddForce(friction);
    }

    public void FollowPath(Path path, bool localAvoidance = true)
    {
        Vector3 seekPath = SeekPath(path, localAvoidance);
        Vector3 softSeperate = SoftSeperate(vehicleList, sphereCollider.radius);
        Vector3 separate = Seperate(vehicleList, sphereCollider.radius - AllowedOverlap);

        seekPath *= 1f;
        softSeperate *= 3f;
        separate *= .5f;

        rb.AddForce(seekPath);
        rb.AddForce(softSeperate);
        rb.AddForce(separate);

        Vector3 friction = Friction(seekPath + softSeperate + separate);
        rb.AddForce(friction);
    }

    public void Arrive(bool localAvoidance = true)
    {
        Vector3 arrive = Arrive(localAvoidance, Mathf.Sqrt(maxSpeed) / 2);
        Vector3 softSeperate = SoftSeperate(vehicleList, sphereCollider.radius);
        Vector3 separate = Seperate(vehicleList, sphereCollider.radius - AllowedOverlap);

        arrive *= 1f;
        softSeperate *= 3f;
        separate *= .5f;

        rb.AddForce(arrive);
        rb.AddForce(softSeperate);
        rb.AddForce(separate);

        Vector3 friction = Friction(arrive + softSeperate + separate);
        rb.AddForce(friction);
    }

    public abstract void Attack(Farmon targetUnit);
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


public class FollowPathState : StateMachineState
{
    Farmon farmon;
    Path path;

    Timer randomPathUpdateTimer = new Timer();

    bool jump = false;

    public FollowPathState(Farmon thisUnit)
    {
        farmon = thisUnit;
    }

    public override void Enter()
    {
        base.Enter();

        farmon.PathNodeReachedEvent.AddListener(NodeReached);
        farmon.GridSpaceChangedEvent.AddListener(UpdatePath);

        farmon.maxSpeed = farmon.GetMovementSpeed();

        randomPathUpdateTimer.SetTime(UnityEngine.Random.Range(.5f, 1f));

        UpdatePath();
    }

    public override void Exit()
    {
        base.Exit();

        farmon.PathNodeReachedEvent.RemoveListener(NodeReached);
        farmon.GridSpaceChangedEvent.RemoveListener(UpdatePath);
    }

    private void UpdatePath()
    {
        if (farmon.targetTransform)
        {
            Vector3 myPos = farmon.transform.position;
            Vector3 targetPos = farmon.targetTransform.position;
            float gridSize = LevelController.Instance.gridSize;

            Vector3 myRaycastPosition = myPos - new Vector3(myPos.x % gridSize, 0, myPos.z % gridSize) + new Vector3(gridSize / 2, 0, gridSize / 2);
            Vector3 targetRaycastPosition = targetPos - new Vector3(targetPos.x % gridSize, 0, targetPos.z % gridSize) + new Vector3(gridSize / 2, 0, gridSize / 2);

            Physics.Raycast(new Ray(myRaycastPosition, Vector3.down), out RaycastHit myHitInfo, 100f, LayerMask.GetMask("Default"));
            Physics.Raycast(new Ray(targetRaycastPosition, Vector3.down), out RaycastHit targetHitInfo, 100f, LayerMask.GetMask("Default"));

            Vector3 myPoint = myHitInfo.point + .1f * Vector3.up;
            Vector3 targetPoint = targetHitInfo.point + .1f * Vector3.up;

            GridSpace myGridSpace = NavMesh.instance.GetGridSpaceArray(H.Vector3ToGridPosition(myPoint, gridSize));
            GridSpace targetGridSpace = NavMesh.instance.GetGridSpaceArray(H.Vector3ToGridPosition(targetPoint, gridSize));

            farmon.perkList.TryGetValue(new PerkJump().PerkName, out int jumpAbility);
            path = NavMesh.instance.GetPath(myGridSpace, targetGridSpace, jumpAbility, (x) => { return Vector3.Distance(x.Center, targetGridSpace.Center); });

            jump = ShouldJumpForNextLink();
        }
        else
        {
            path = new Path();
        }
    }

    private void NodeReached(PathNode previousNode)
    {
        if(jump == true)
        {
            BlockLink nextLink = previousNode.OutputBlockLink;

            float jumpHeight = nextLink.HeightDifference / 2 + 1f;

            Vector3 centerOfBlock = nextLink.ToGridSpace.HitCenter.point;

            JumpState jumpState = new JumpState(farmon, farmon.transform.position, centerOfBlock + farmon.sphereCollider.radius * Vector3.up, jumpHeight);
            farmon.SetState(jumpState);

            //return since we have exited this state.
            return;
        }

        jump = ShouldJumpForNextLink();
    }

    private bool ShouldJumpForNextLink()
    {
        if (path != null && path.nodeList.Count > 0)
        {
            BlockLink nextLink = path.PeekNode().OutputBlockLink;

            if (nextLink != null && !nextLink.walkable && nextLink.jumpable)
            {
                return true;
            }
        }

        return false;
    }

    public override void Tick()
    {
        base.Tick();

        if (!farmon.targetTransform){
            farmon.mainState = new IdleState(farmon);
            farmon.SetState(farmon.mainState);
            return;
        }

        farmon.maxSpeed = farmon.GetMovementSpeed();

        if (randomPathUpdateTimer.Tick(Time.deltaTime))
        {
            randomPathUpdateTimer.SetTime(UnityEngine.Random.Range(2f, 3f));
            UpdatePath();
        }

        farmon.FollowPath(path);
    }
}

public class AttackState : StateMachineState
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
}

public class DefendState : StateMachineState
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
}

public class BattleState : StateMachineState
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
}

public class ProtectState : StateMachineState
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

        if(potentialTarget && IsValidTarget(potentialTarget))
        {
            // The last target to damage our protectTarget is valid and can be attacked.
            return potentialTarget;
        }
        else
        {
            //Loop through all farmon and find a valid target
            foreach(Farmon currentTarget in Farmon.farmonList)
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
                //subStateMachine.ChangeState(battleState);


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
}

public class AttackData{            
    public int Damage;
    public float Knockback;
    public float HitStopTime;
    public bool Undodgeable;
    public AudioClip InitialSound;
    public AudioClip HitSound;
    public AttackData(int damage = 10, float knockback = 5f, float hitStopTime = 0.3f, bool undodgeable = false, AudioClip initialSound = null, AudioClip hitSound = null)
    {
        Damage = damage;
        Knockback = knockback;
        HitStopTime = hitStopTime;
        Undodgeable = undodgeable;
        InitialSound = initialSound;
        HitSound = hitSound;
    }
}

// In the melee attack state, farmon moves towards the attackTarget farmon and, after getting within attack range, lunges at the attackTarget farmon.
public class MeleeAttackState : StateMachineState
{
    protected AttackData _attackData;

    protected Farmon _farmon;
    protected float _lungeRange;
    protected bool _selfHitStun;

    protected uint _targetFarmonInstanceID;

    private bool jumping = false;

    public MeleeAttackState(Farmon farmon, uint targetFarmonInstanceID, AttackData attackData, bool selfHitstun = true)
    {
        _farmon = farmon;
        _selfHitStun = selfHitstun;
        _attackData = attackData;

        _lungeRange = LevelController.Instance.gridSize * .1f;

        _targetFarmonInstanceID = targetFarmonInstanceID;
    }

    public override void Enter()
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

        if(!jumping) _farmon.SeekUnit(false);

        Vector3 toTargetEnemy = attackFarmon.Hud.SpriteQuad.transform.position - _farmon.Hud.SpriteQuad.transform.position;
        float distanceToEnemy = Vector3.Distance(_farmon.transform.position, attackFarmon.transform.position);

        //If we have the jump perk and a flying enemy is less than 1.5 blocks away, jump at them.
        _farmon.perkList.TryGetValue(new PerkJump().PerkName, out int jumpAbility);
        if (jumpAbility > 0 && distanceToEnemy < 1.5f * LevelController.Instance.gridSize && attackFarmon.Flying && _farmon.Grounded)
        {
            _farmon.rb.velocity = Vector3.zero;
            _farmon.rb.AddForce((toTargetEnemy/LevelController.Instance.gridSize) * 1.4f, ForceMode.Impulse);
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
    }

    public virtual void OnAttack()
    {
        Farmon attackFarmon = Farmon.loadedFarmonMap[_targetFarmonInstanceID];

        SphereCollider sc = _farmon.sphereCollider;

        bool hit = attackFarmon.TakeDamage(_attackData, sc.transform.position + sc.center, (attackFarmon.transform.position - _farmon.transform.position).normalized, _farmon);

        if (hit)
        {
            //Spawn a hit effect.
            Vector3 farmonToMe = (_farmon.transform.position - attackFarmon.transform.position).normalized;
            GameObject hitEffect = GameObject.Instantiate(FarmonController.instance.HitEffectPrefab, attackFarmon.transform);
            hitEffect.transform.position = attackFarmon.transform.position + farmonToMe * attackFarmon.sphereCollider.radius;
            hitEffect.transform.localScale = (.2f + 2.5f * _attackData.HitStopTime) * Vector3.one;

            _farmon.Hud.AudioSource.clip = _attackData.HitSound;
            _farmon.Hud.AudioSource.volume = .2f;
            _farmon.Hud.AudioSource.Play();
        }
    }
}

public class HitStopState : StateMachineState
{
    readonly Farmon farmon;
    readonly Timer hitStopTimer = new Timer();
    readonly Timer flashTimer = new Timer();

    private Vector3 spriteQuadOrigin;
    private Vector3 spriteHealthBarOrigin;

    HitStopState2 hitStopState2;
    SpriteRenderer spriteRenderer;

    bool flashFlag;

    public HitStopState(Farmon thisUnit, AttackData attackData, Vector3 damageOrigin, Vector3 knockbackDirection)
    {
        farmon = thisUnit;

        hitStopTimer.SetTime(attackData.HitStopTime);

        flashTimer.SetTime(.1f);
        flashTimer.autoReset = true;

        Vector3 bounceVector = knockbackDirection * attackData.Knockback;

        if(bounceVector.magnitude > .01f)
        {
            hitStopState2 = new HitStopState2(farmon, attackData, damageOrigin, bounceVector);
        }

        spriteRenderer = farmon.Hud.SpriteQuad.GetComponentInChildren<SpriteRenderer>();
    }

    public override void Enter()
    {
        base.Enter();

        farmon.rb.isKinematic = true;

        farmon.Hud.Animator.speed = 0;
        farmon.Hud.PositionQuad.enabled = false;

        spriteQuadOrigin = farmon.Hud.SpriteQuad.transform.position;
        spriteHealthBarOrigin = farmon.Hud.HealthBar.transform.position;

        spriteRenderer.color = new Color(.6f, .6f, .6f, 1);

        farmon.Hud.AudioSource.clip = FarmonController.instance.HitSound;
        farmon.Hud.AudioSource.volume = .3f;
        farmon.Hud.AudioSource.Play();
    }

    public override void Exit()
    {
        base.Exit();

        farmon.rb.isKinematic = false;

        farmon.Hud.Animator.speed = 1;
        farmon.Hud.PositionQuad.enabled = true;

        spriteRenderer.color = new Color(1, 1, 1, 1);

        farmon.Hud.SpriteQuad.transform.position = spriteQuadOrigin;
        farmon.Hud.HealthBar.transform.position = spriteHealthBarOrigin;
    }

    public override void Tick()
    {
        base.Tick();

        Vector3 cameraRight = Camera.main.transform.right;
        Vector3 cameraUp = Camera.main.transform.up;

        float noise1X = Mathf.PerlinNoise(Time.time * 20, 0) * 2 - 1;
        float noise1Y = Mathf.PerlinNoise(0, Time.time * 20) * 2 - 1;
        float noise2X = Mathf.PerlinNoise(100 + Time.time * 20, 0) * 2 - 1;
        float noise2Y = Mathf.PerlinNoise(0, 100 + Time.time * 20) * 2 - 1;

        float diminish = .75f * (1 - hitStopTimer.Percent);

        farmon.Hud.SpriteQuad.transform.position = spriteQuadOrigin + (0.3f - diminish) * noise1X * cameraRight + (0.2f - diminish) * noise1Y * cameraUp;
        farmon.Hud.HealthBar.transform.position = spriteHealthBarOrigin + (0.3f - diminish) * noise2X * cameraRight + (0.2f - diminish) * noise2Y * cameraUp;

        if (flashTimer.Tick(Time.deltaTime))
        {
            if (flashFlag)
            {
                spriteRenderer.color = new Color(.6f, .6f, .6f, 1);
            }
            else
            {
                spriteRenderer.color = new Color(1, 1, 1, 1);
            }
            flashFlag = !flashFlag;
        }

        if (hitStopTimer.Tick(Time.deltaTime))
        {
            if (hitStopState2 != null)
            {
                farmon.SetState(hitStopState2);
            }
            else
            {
                farmon.SetState(farmon.mainState);
            }
        }

    }
}

public class HitStopState2 : StateMachineState
{
    readonly Farmon farmon;
    readonly Vector3 _damageOrigin;
    readonly Vector3 _bounceVector;
    readonly AttackData _attackData;

    readonly Timer hitStopTimer = new Timer();


    public HitStopState2(Farmon thisUnit, AttackData attackData, Vector3 damageOrigin, Vector3 bounceVector)
    {
        farmon = thisUnit;
        _bounceVector = bounceVector;
        _damageOrigin = damageOrigin;

        hitStopTimer.SetTime(0.15f);

        _attackData = attackData;
    }

    public override void Enter()
    {
        base.Enter();

        if (_attackData.Damage > farmon.MaxHealth / 10)
        {
            farmon.Hud.AudioSource.clip = FarmonController.instance.HitSound2;
            farmon.Hud.AudioSource.volume = .3f;
            farmon.Hud.AudioSource.Play();
        }

        farmon.ChangeHeath(-_attackData.Damage);
        farmon.MakeDamageNumber(_attackData);
        farmon.MakeHitEffect(_attackData, _damageOrigin);
        farmon.DamageEvent.Invoke();

        farmon.rb.AddForce(farmon.dead ? _bounceVector * 3 : _bounceVector, ForceMode.Impulse);
    }

    public override void Tick()
    {
        base.Tick();

        if (hitStopTimer.Tick(Time.deltaTime))
        {
            farmon.SetState(farmon.mainState);
        }

    }
}

public class DieState : StateMachineState
{
    Farmon farmon;

    SpriteRenderer spriteRenderer;

    readonly Timer dieTimer = new Timer();
    
    readonly Timer flashTimer = new Timer();
    bool flashFlag;

    public DieState(Farmon _farmon)
    {
        farmon = _farmon;

        dieTimer.SetTime(1f);

        flashTimer.SetTime(0.1f);
        flashTimer.autoReset = true;

        spriteRenderer = farmon.Hud.SpriteQuad.GetComponentInChildren<SpriteRenderer>();
    }

    public override void Enter()
    {
        farmon.dead = true;

        spriteRenderer.color = new Color(.6f, .6f, .6f, 1);

        foreach(LookAtCamera lac in farmon.GetComponentsInChildren<LookAtCamera>())
        {
            lac.enabled = false;
            lac.transform.localEulerAngles = new Vector3(5, lac.transform.localEulerAngles.y, 0);
        }

        farmon.Hud.AudioSource.clip = FarmonController.instance.DieSound;
        farmon.Hud.AudioSource.volume = .3f;
        farmon.Hud.AudioSource.Play();

        farmon.rb.AddForce(Vector3.up * 3, ForceMode.Impulse);
    }

    public override void Tick()
    {
        base.Tick();

        farmon.Hud.SpriteQuad.transform.Rotate(new Vector3(0, 1, 0), 400f * Time.deltaTime);

        farmon.maxSpeed = 0;
        farmon.MovementIdle();
         
        farmon.transform.localScale -= .35f * Time.deltaTime * Vector3.one;


        if (dieTimer.Tick(Time.deltaTime))
        {
            farmon.gameObject.SetActive(false);
            return;
        }

        if (flashTimer.Tick(Time.deltaTime))
        {
            if (flashFlag)
            {
                spriteRenderer.color = new Color(.6f, .6f, .6f, 1);
            }
            else
            {
                spriteRenderer.color = new Color(1, 1, 1, 1);
            }
            flashFlag = !flashFlag;
        }
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

public class SpawnState : StateMachineState
{
    Farmon farmon;
    Timer spawnTimer;

    public SpawnState(Farmon thisUnit)
    {
        farmon = thisUnit;
        spawnTimer = new Timer();
        spawnTimer.SetTime(1f);
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

        if (spawnTimer.Tick(Time.deltaTime))
        {
            farmon.SetState(farmon.mainState);
        }
    }
}

public class JumpState : StateMachineState
{
    Farmon unit;

    Vector3 startingPosition, endingPosition;

    float jumpTime = 1;

    float h;

    Timer jumpTimer = new Timer();

    public JumpState(Farmon thisUnit, Vector3 startingPos, Vector3 endingPos, float height, float duration = 1f)
    {
        unit = thisUnit;
        startingPosition = startingPos;

        Vector2 randomFlatVector = UnityEngine.Random.insideUnitCircle;
        Vector3 slightOffset = new Vector3(randomFlatVector.x, 0, randomFlatVector.y) * .01f;
        endingPosition = endingPos + slightOffset;

        jumpTime = duration;
        h = height;
    }

    public override void Enter()
    {
        base.Enter();
        jumpTimer.SetTime(jumpTime);

        unit.maxSpeed = 0;
        unit.rb.isKinematic = true;
    }

    public override void Tick()
    {
        base.Tick();

        if (jumpTimer.Tick(Time.deltaTime))
        {
            unit.SetState(unit.mainState);
            unit.rb.MovePosition(MathParabola.Parabola(startingPosition, endingPosition, h, .9f));
            return;
        }

        float jumpPercent = Mathf.Max(0, 0.9f - jumpTimer.Percent);

        unit.rb.MovePosition(MathParabola.Parabola(startingPosition, endingPosition, h, jumpPercent));
    }

    public override void Exit()
    {
        base.Exit();
        unit.rb.isKinematic = false;
    }
}