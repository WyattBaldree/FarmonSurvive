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

    public static int StatMax = 40;

    /// <summary>
    /// This value represents this farmon's position in the loadedFarmonMap
    /// This value is used to constantly track this farmon, even if it evolves or
    /// otherwise switches gameobjects.
    /// </summary>
    public uint loadedFarmonMapId = 0;

    /// <summary>
    /// This value represents this farmon's location in the player save data.
    /// When a farmon is first saved, this value is set. This value is then used to
    /// load and save that farmon in the future.
    /// </summary>
    public uint uniqueID = 0;

    /// <summary>
    /// This value scales this farmon's attack speed. A value of 2 will make the farmon attack half as frequently.
    /// </summary>
    public float attackSpeedAdjust = 1;

    public enum TeamEnum
    {
        team1,
        team2
    }

    public enum AttackType
    {
        melee,
        ranged
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
        nearestFlat,
        furthest,
        lowestHealth,
        mostHealth
    }

    public enum FormationTypeEnum
    {
        none,
        frontline,
        backline
    }

    //Track the farmon's position in the formation
    public FormationTypeEnum formationType = FormationTypeEnum.none;
    public int formationPosition = 0;

    public string DebugString;

    [HideInInspector]
    public FarmonHud Hud;

    [HideInInspector]
    public SpriteRenderer mySpriteRenderer;
    [HideInInspector]
    public Bar healthBar;

    private StateMachine farmonStateMachine;

    private StateMachine controlStateMachine;

    public string farmonName = "Unit";
    public string nickname = "";
    public string Description = "Default description.";
    public int level = 1;
    public int experience = 0;
    public bool canJump = false;

    public AttackType attackType = AttackType.melee;

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

    public float attackRangeBase = 5f;

    public override bool Flying
    {
        get => base.Flying;
        set{
            base.Flying = value;
            rb.useGravity = !value;
        }

    }

    //Hover Highlight
    Highlight _hoverHighlight;
    Highlight _selectedHighlight;
    private HighlightList _highlightList;

    //States
    public NewBattleState mainBattleState;
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

    public virtual float AttackTime(float scale = 1f)
    {
        float baseAttackTime = 4 * scale;

        float finalAttackTime = baseAttackTime;

        finalAttackTime -= GetModifiedFocus() / 60f;

        finalAttackTime -= GetModifiedAgility() / 30f;

        perkList.TryGetValue(new PerkFrenzy().PerkName, out int frenzyAbility);
        finalAttackTime *= 1 + (0.2f * frenzyAbility);

        return finalAttackTime;
    }

    public override void Initialize()
    {
        if (initialized) return;

        base.Initialize();

        Hud = GetComponentInChildren<FarmonHud>();
        Assert.IsNotNull(Hud);
        Hud.Initialize(this);

        mySpriteRenderer = Hud.SpriteQuad.GetComponentInChildren<SpriteRenderer>();
        Assert.IsNotNull(mySpriteRenderer);

        farmonStateMachine = new StateMachine();
        controlStateMachine = new StateMachine();

        //spawnState = new SpawnState(this);

        //mainState = new IdleState(this);

        controlStateMachine.InitializeStateMachine(new MainState(this));

        farmonStateMachine.InitializeStateMachine(new NewIdleState(this));

        shadow = Instantiate(FarmonController.instance.ShadowPrefab, transform.position + sphereCollider.center + ((sphereCollider.radius - 0.01f) * Vector3.down), Quaternion.Euler(90, 0, 0), this.transform);

        _highlightList = mySpriteRenderer.GetComponentInParent<HighlightList>();

        if (team == Player.instance.playerTeam)
        {
            _highlightList.AddHighlight(Color.blue, 100);
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

    public override float GetMovementSpeed()
    {
        return 3 + Agility / 6;
    }

    public int GetArmor()
    {
        //Add up all armor sources and return;
        return EffectList.TortorrentShield.Value;
    }

    protected override void Awake()
    {
        base.Awake();
        farmonList.Add(this);

        EffectList.Initialize();

        statsChangedEvent += StatsChanged;
        PathNodeReachedEvent.AddListener(PathNodeReached);
    }

    private void StatsChanged(object sender, EventArgs e)
    {
        Flying = IsFlying();

        //Any time our stats change, force a path update in case something like the ability to jump or fly has effected our ability to navigate.
        forcePathUpdate = true;
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
            DebugString += "\n" + controlStateMachine.CurrentState.ToString();
            DebugString += "\n" + farmonStateMachine.CurrentState.ToString();
        }

        if (!FarmonController.Paused)
        {
            //tick the farmon's control state machine. This state machine is resposible for ticking the farmonStateMachine
            controlStateMachine.Tick();
        }

        

        UpdateGridSpace();

        PositionShadow();
    }

    public void UpdateEffects()
    {
        if (EffectList.Burn.Value > 0 && burnTimer.Tick(Time.deltaTime))
        {
            AttackData burnDamageData = new AttackData((int)EffectList.Burn.Value, 0, true);
            AttemptDamage(burnDamageData, 0, transform.position, Vector3.zero, null);
        }

        EffectList.UpdateEffects(Time.deltaTime);
    }

    public void UpdateTimers()
    {
        //If we don't have an attack ready, count down the attack timer and ready an attack.
        if (attackReady == false && attackTimer.Tick(Time.deltaTime))
        {
            attackReady = true;
        }
    }

    /// <summary>
    /// Update the Farmon's grid location and if the location has changed, invoke GridSpaceChangedEvent
    /// </summary>
    private void UpdateGridSpace()
    {
        Vector3Int currentGridSpaceIndex = H.Vector3ToGridPosition(transform.position, LevelController.Instance.gridSize);

        if (GridSpaceIndex != currentGridSpaceIndex)
        {
            GridSpaceIndex = currentGridSpaceIndex;
            GridSpaceChangedEvent.Invoke();
        }
    }

    /// <summary>
    /// Position the farmon's shadow directly below it.
    /// </summary>
    private void PositionShadow()
    {
        if (Physics.BoxCast(transform.position, sphereCollider.radius / 2 * Vector3.one, Vector3.down, out RaycastHit hitInfo, Quaternion.identity, 100, LayerMask.GetMask("Default")))
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

    public void SetFormation(int position, FormationTypeEnum type)
    {
        formationType = type;
        formationPosition = position;
    }

    public void GetNextAction()
    {
        List<Farmon> closestEnemies = Farmon.SearchFarmon(this, Farmon.FarmonFilterEnum.enemyTeam, Farmon.FarmonSortEnum.nearestFlat);
        WeightedRandomBag<Farmon> enemyFarmonBag = new WeightedRandomBag<Farmon>();

        foreach (Farmon enemyFarmon in closestEnemies)
        {
            float weight = enemyFarmon.formationType == FormationTypeEnum.frontline ? 1 : 0.5f;
            enemyFarmonBag.AddEntry(enemyFarmon, weight);
        }

        Farmon attackTarget = enemyFarmonBag.GetRandom();

        if (attackTarget)
        {
            mainBattleState = new NewAttackState(this, attackTarget.loadedFarmonMapId);
            SetState(mainBattleState);
        }
        else
        {
            mainBattleState = new NewIdleState(this);
            SetState(mainBattleState);
        }
    }

    public virtual void AttackComplete()
    {
        attackTimer.SetTime(AttackTime());
    }

    /// <summary>
    /// Provide a 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ignoreDead"></param>
    /// <returns></returns>
    public static Farmon GetFarmonInstanceFromLoadedID(uint id, bool ignoreDead)
    {
        if(Farmon.loadedFarmonMap.TryGetValue(id, out Farmon retValue))
        {
            if (ignoreDead && retValue.dead) return null;
            return retValue;
        }
        return null;
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
        if(exclusions != null) searchList = searchList.FindAll((farmon) => { return !exclusions.Contains(farmon); });

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
            case FarmonSortEnum.nearestFlat:

                listToSort.Sort((f1, f2) => {
                    Vector3 f1Flat = H.Flatten(f1.transform.position);
                    Vector3 f2Flat = H.Flatten(f2.transform.position);
                    Vector3 originFlat = H.Flatten(f1.transform.position);
                    return Vector3.Distance(f1Flat, originFlat).CompareTo(Vector3.Distance(f2Flat, originFlat)); 
                });
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


    /*public virtual bool IsInAttackPosition()
    {
        Farmon attackFarmon = GetAttackTargetFarmon();

        //Can we see the enemy?
        Vector3 targetFeet = attackFarmon.transform.position + (attackFarmon.sphereCollider.radius - .01f) * Vector3.down;
        Vector3 myFeet = transform.position + (sphereCollider.radius - .01f) * Vector3.down;

        Vector3 toEnemy = targetFeet - myFeet;

        Ray r = new Ray(myFeet, toEnemy.normalized);

        bool wallIsBlockingTarget = Physics.Raycast(r, toEnemy.magnitude, LayerMask.GetMask("Default"));

        bool isWithinAttackRange = toEnemy.magnitude < GetAttackRange();

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
    }*/

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

    /// <summary>
    /// Can be called every frame to constantly attempt attacking the closest enemy.
    /// </summary>
    /// <returns></returns>
    public bool AttemptAttackOnNearestEnemyUnit()
    {
        List<Farmon> closestEnemies = Farmon.SearchFarmon(this, Farmon.FarmonFilterEnum.enemyTeam, Farmon.FarmonSortEnum.nearestFlat);

        foreach (Farmon potentialTarget in closestEnemies)
        {
            if (AttemptAttack(potentialTarget))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Can be called every frame to constantly attempt attacking the supplied target.
    /// </summary>
    public bool AttemptAttack(Farmon farmonToAttack)
    {
        //if an attack is ready and we are in range of the target farmon, enter this farmon's unique attack state.
        if (CanSeeFarmon(farmonToAttack) && InAttackRange(farmonToAttack))
        {
            if (attackType == Farmon.AttackType.melee)
            {
                //For melee farmon, check if we are able to reach the opposing farmon vertically
                perkList.TryGetValue(new PerkJump().PerkName, out int jumpAbility);
                float verticalDistance = Mathf.Abs((transform.position - farmonToAttack.transform.position).y);

                //The ability to fly or jump can assist in hitting flying or high up enemies.
                if (Flying || verticalDistance < LevelController.Instance.gridSize * .75f)
                {
                    Attack(farmonToAttack);
                    return true;
                }
                else if (jumpAbility > 0 && verticalDistance < LevelController.Instance.gridSize * 1.5f)
                {
                    Attack(farmonToAttack);
                    return true;
                }
                else if (jumpAbility > 1 && verticalDistance < LevelController.Instance.gridSize * 3f)
                {
                    Attack(farmonToAttack);
                    return true;
                }
            }
            else
            {
                Attack(farmonToAttack);
                return true;
            }
        }

        return false;
    }

    public UnityEvent DamageEvent;
    public bool AttemptDamage(AttackData attackData, float hitStopTime, Vector3 damageOrigin, Vector3 knockBackDirection, Farmon owner)
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
            if (!ImmuneToHitStop && hitStopTime > 0)
            {
                SetControlState(new HitStopState(this, hitStopTime, attackData, damageOrigin, knockBackDirection));
            }
            else
            {
                TakeDamage(attackData, damageOrigin);
            }

            return true;
        }
    }

    public void TakeDamage(AttackData attackData, Vector3 damageOrigin)
    {
        //First damage armor and overhealth

        int remainingDamage = attackData.Damage;

        remainingDamage = EffectList.TortorrentShield.SubtractFromTotal(remainingDamage);

        MakeHitEffect(attackData, damageOrigin);

        if (remainingDamage == 0)
        {
            Hud.AudioSource.clip = FarmonController.instance.DeflectSound;
            Hud.AudioSource.volume = .3f;
            Hud.AudioSource.Play();

            MakeDamageNumber(new AttackData(0, 0));

            return;
        }

        if (attackData.Damage > MaxHealth / 5)
        {
            Hud.AudioSource.clip = FarmonController.instance.HitSound2;
            Hud.AudioSource.volume = .3f;
            Hud.AudioSource.Play();
        }

        ChangeHeath(-remainingDamage);

        MakeDamageNumber(attackData);

        DamageEvent.Invoke();
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
        hitEffect.transform.localScale = (.2f + 2.5f * attackData.Knockback/5f) * Vector3.one;
    }

    private bool IsFlying()
    {
        if (dead) return false;

        perkList.TryGetValue(new PerkFly().PerkName, out int flyAbility);
        return flyAbility > 0;
    }

    public void Die()
    {
        SetControlState(new DieState(this));

        if (team == TeamEnum.team1)
        {
            GameController.SlowMo(2.5f, .3f);
        }
    }

    public void HitStopSelf(float stopTime)
    {
        SetControlState(new HitStopState(this, stopTime, null, Vector3.zero, Vector3.zero));
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
        farmonStateMachine.ChangeState(state);
    }

    public void SetControlState(StateMachineState state)
    {
        controlStateMachine.ChangeState(state);
    }

    public Color debugColor = Color.white;

    protected virtual void OnDrawGizmosSelected()
    {
        DebugExtension.DrawCircle(transform.position, Vector3.up, Color.yellow, GetAttackRange());
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

        string levelUpMessage = nickname + " has reached level <color.yellow>" + level + "<default>!";

        LevelUpScreen.instance.Popup(this, levelUpMessage, gritPrev, powerPrev, agilityPrev, focusPrev, luckPrev, pointsPrev, true);

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
        Vector3 avoidEdges = AvoidEdges();

        wander *= 1f;
        minDistance *= 2f;
        maxDistance *= 2f;
        softSeperate *= 3f;
        separate *= .5f;
        avoidEdges *= 1;

        rb.AddForce(wander);
        rb.AddForce(softSeperate);
        rb.AddForce(separate);
        rb.AddForce(minDistance);
        rb.AddForce(maxDistance);

        rb.AddForce(avoidEdges);

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

    public void SeekUnit(bool localAvoidance = true, bool ignoreFlightHeight = false)
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
            seek = Seek(localAvoidance, ignoreFlightHeight);
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

    internal void FlyTowardsPosition(Vector3 position, float speed = 10f)
    {
        //rb.AddForce((position - transform.position).normalized * speed, ForceMode.VelocityChange);
        rb.velocity = (position - transform.position).normalized * speed;
    }

    public virtual void Attack(Farmon targetUnit)
    {
        attackReady = false;
    }

    internal void UpdateFarmonStateMachine()
    {
        farmonStateMachine.Tick();
    }

    //Return true if this farmon can see farmonToSee
    public bool CanSeeFarmon(Farmon farmonToSee)
    {
        Vector3 toEnemy = farmonToSee.transform.position - transform.position;

        return CanSeePosition(farmonToSee.transform.position);
    }

    public bool CanSeePosition(Vector3 position)
    {
        Vector3 toPosition = position - transform.position;

        return !Physics.Raycast(transform.position, toPosition.normalized, toPosition.magnitude, LayerMask.GetMask("Default")); ;
    }

    public float GetAttackRange()
    {
        return attackRangeBase;
    }

    internal bool InAttackRange(Farmon farmonToAttack)
    {
        return H.Flatten(farmonToAttack.transform.position - transform.position).magnitude < GetAttackRange();
    }

    public float GetAttackWanderDistance()
    {
        return GetAttackRange()*.9f;
    }
    public float GetPotectWanderDistance()
    {
        return GetAttackRange()/2f * .9f;
    }

    //PATHFINDING
    #region Pathfinding

    //Navigate to the farmonToNavigateTo via pathfinding and seeking
    public void NavigateToFarmon(Farmon farmonToNavigateTo)
    {
        NavigateToPosition(farmonToNavigateTo.transform.position);
    }

    public void NavigateToPosition(Vector3 targetPosition)
    {
        //First update the path if it needs to be updated
        UpdatePathAsNecessary(targetPosition);

        if(myPath == null)
        {
            Debug.LogError("No valid path was found.", this);
            return;
        }

        //Then follow the path every frame.
        FollowPath(myPath);
    }

    //Called each time this farmon reaches a node in the path
    private void PathNodeReached()
    {
        PathNode pathNode = myPath.PeekNode();
        if (pathNode != null)
        {
            BlockLink nextLink = pathNode.OutputBlockLink;

            //if the next node in the path is a jump link, enter the jump state and consume the jump link.
            if (nextLink != null && !nextLink.walkable && nextLink.jumpable)
            {
                float jumpHeight = nextLink.HeightDifference / 2 + 1f;

                Vector3 centerOfBlock = nextLink.ToGridSpace.HitCenter.point;

                JumpState jumpState = new JumpState(this, transform.position, centerOfBlock + sphereCollider.radius * Vector3.up, jumpHeight);
                SetControlState(jumpState);
            }
        }
    }

    // Where we are currently.
    Vector3Int PathStartPosition = Vector3Int.zero;
    
    // Where we want to be.
    Vector3Int PathEndPosition = Vector3Int.zero;

    // The path this farmon is currently following.
    Path myPath = null;

    // This value can be set to true to force a new path to generate in the event that we know something has changed that will
    // necessitate updating the path (like the farmon losing the ability to fly or jump).
    bool forcePathUpdate = false;

    //Check if anything has changed that would effect our path and update values and the path if necessary.
    public void UpdatePathAsNecessary(Vector3 targetPosition)
    {
        bool necessaryToGeneratePath = false;

        //First get the gridSpace that this farmon will be navigating from (first valid terrain beneath this farmon)
        Vector3Int myNavigationSpace = H.GetNavigationPosition(transform.position);

        //Now check if this position is different from our current path start position
        //If so, we need to generate a new path.
        if(myNavigationSpace != PathStartPosition)
        {
            PathStartPosition = myNavigationSpace;
            necessaryToGeneratePath = true;
        }

        //Next do the same thing for the targetPosition
        Vector3Int targetNavigationSpace = H.GetNavigationPosition(targetPosition);

        //Now check if this position is different from our current path start position
        //If so, we need to generate a new path.
        if (targetNavigationSpace != PathEndPosition)
        {
            PathEndPosition = targetNavigationSpace;
            necessaryToGeneratePath = true;
        }

        //If updating the path is necessary, do it.
        if (necessaryToGeneratePath || forcePathUpdate)
        {
            perkList.TryGetValue(new PerkJump().PerkName, out int jumpAbility);

            GridSpace myGridSpace = NavMesh.instance.GetGridSpaceArray(myNavigationSpace);
            GridSpace targetGridSpace = NavMesh.instance.GetGridSpaceArray(targetNavigationSpace);
            myPath = NavMesh.instance.GetPath(myGridSpace, targetGridSpace, jumpAbility, Flying, (x) => { return Vector3.Distance(x.Center, targetGridSpace.Center); });

            //if we were forced to update the path, set the value to false.
            forcePathUpdate = false;

            PathNodeReachedEvent.Invoke();
        }
    }
    #endregion
}