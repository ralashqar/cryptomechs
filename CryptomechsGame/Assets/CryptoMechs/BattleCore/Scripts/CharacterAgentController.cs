using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Werewolf.StatusIndicators.Components;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Demos.RPGEditor;

[RequireComponent(typeof(NavMeshAgent))]
public class CharacterAgentController : CharacterStateMachine, IAttackable, IAbilityCaster
{
    public string uniqueID;

    public NavMeshAgent agent;

    [OnValueChanged("LoadCharacter")]
    [InlineEditor]
    public Character data;

    public FullMech mech;
    public int mechID = 1;

    public AgentMover mover;
    //public AgentAnimationControllerRuntime animationController;
    public MechAnimationController animationController;
    public WeaponController weaponController;

    public bool useOverrideHealth = false;
    public int healthPoints = 500;
    public bool useOverrideNarrative = false;
    public StoryNarrative interactionNarrative;
    public NarrativeTreeManager narrativesManager;
    
    public bool useOverrideAI = false;
    public AIBehaviour aiOverride;
    [HideInInspector]
    public AIBehaviour aiBehavior;

    public InventoryManager inventoryManager;
    public BuffsManager buffsManager;
    public AbilitiesManager abilitiesManager;

    public CharacterTeam team = CharacterTeam.PLAYER;

    private WeaponItem mainHandWeapon = null;

    private Animator animator;

    public ITargettable targetObject;

    public CharacterHealthManager healthManager;

    public CharacterFXManager fxManager;

    public BattleTurnBasedAgent battleTurnManager;

    public AbilityProjectileItem selectedAbility = null;

    public bool IsInitialized { get; private set; } = false;

    public SplatManager Splats { get; set; } = null;
    public bool IsAimingAbility { get { return mech != null && this.mech.casters.Exists(c => c.IsAimingAbility); } }
    public bool CanMove { get { return mech == null || !this.mech.casters.Exists(c => c.IsBlockingCasterMovement); } }

    public bool IsActionSequenceActive { get { return mech != null && this.mech.casters.Exists(c => c.IsActionSequenceActive); } }
    public bool IsCasting { get { return (IsAimingAbility || IsChannelingAbility || animationController.IsCasting()); } }
    public bool HasActiveProjectiles { get { return mech != null && this.mech.casters.Exists(c => c.HasActiveProjectiles); } }
    public Vector3 lastAbilityPoint = Vector3.zero;
    public bool IsChannelingAbility { get { return mech != false && this.mech.casters.Exists(c => c.IsChannelingAbility); } }
    public Vector3 positionAtCastTime;
    public float lastCastTime = 0;

    public bool isDead = false;
    public float attackRangeMelee = 0.75f;
    public float attackRange = 0.75f;
    public float attackCooldown = 0;
    public float attackCooldownTimer = 1;
    public bool manualControl = true;
    public bool remoteControl = false;

    public bool isImmovable = false;
    public float idleSpeedThreshold = 0.1f;

    private float cachedTargetDistance = 0;

    public bool IsWeaponStriking = false;
    public IAttackable cachedClosestEnemy;

    public delegate void OnCastAbilityDelegate();
    public OnCastAbilityDelegate OnCastAbility;

    public delegate void OnCancelPartAbilityDelegate(MechPartCaster part);
    public OnCancelPartAbilityDelegate OnCancelPartAbility;
    
    public bool isBot = false;


    public void OnCancelAbility(MechPartCaster part)
    {
        OnCancelPartAbility?.Invoke(part);
    }

    public bool GetCollision(Vector3 point, float radius)
    {
        point.y = 0;
        Vector3 delta = point - GetPosition();
        delta.y = 0;
        return (delta.magnitude < radius + GetColliderRadius());
    }

    public float GetColliderRadius()
    {
        return this.agent.radius;
    }

    public IAttackable GetCurrentAttackTarget()
    {
        return cachedClosestEnemy;
    }

    public void SetMoveToTarget(ITargettable target)
    {
        if (target != null)
        {
            targetObject = target;
            cachedClosestEnemy = target as IAttackable;
            agent.stoppingDistance = attackRange * 0.75f;
            agent.destination = target.GetPosition();
            cachedTargetDistance = Vector3.Distance(this.GetPosition(), target.GetPosition());
        }
        else
        {
            targetObject = null;
            cachedClosestEnemy = null;
            agent.stoppingDistance = 1.0f;
            agent.destination = GetPosition();
            cachedTargetDistance = 0;
        }
    }

    public void SetMoveToTarget(Vector3 target)
    {
        targetObject = null;
        cachedClosestEnemy = null;
        agent.stoppingDistance = 1.0f;
        agent.destination = target;
        cachedTargetDistance = 0;
    }

    public void TriggerDeath()
    {
        TerminateStateMachine();
        ClearActiveAbility();
        
        mech.casters.ForEach(c => c.TriggerDeath());

        battleTurnManager?.TriggerDeath();
        //animationController.TriggerDeath();
        CharacterAgentsManager.RemoveCharacterAgent(this);
        //GameObject.Destroy(this.gameObject, 2.5f);
    }

    public GameObject GetHeadVisuals()
    {
        return mech.GetSlotGameObject(MechSlot.HEAD_WEAPON_SHIELD);
    }

    public GameObject GetCharacterVisuals()
    {
        return visuals;
    }

    public void InitializeFXManager()
    {
        fxManager = new CharacterFXManager(this);
    }

    public void InitializeHealthManager()
    {
        healthManager = new CharacterHealthManager(this);
        if (useOverrideHealth)
            healthManager.healthPoints = healthManager.healthPointsTotal = healthPoints;
        healthManager?.UpdateHealth();
    }

    public float GetHP()
    {
        return this.healthManager.healthPoints;
    }

    public void Mine(IMinable minable, float amount, float time = 0)
    {
        minable.Mine(amount, time);
    }

    public void OnItemEquip()
    {
        buffsManager.UpdateCharacterEquipmentModifiers();
    }

    public void EquipWeapon(WeaponItem weapon)
    {
        return;
        if (this.weaponController == null)
        {
            this.weaponController = new WeaponController(this, this.mainHandWeapon);
        }
        else
        {
            weaponController.EquipWeapon(weapon);
        }
        this.attackRangeMelee = weaponController.GetWeaponMeleeRange() * data.scale;
        this.attackRange = weaponController.GetWeaponRange() * data.scale;
        this.attackCooldownTimer = weaponController.GetWeaponCooldown();

        OnItemEquip();
    }

    public void LoadSplats()
    {
        if (this.Splats != null)
            return;

        GameObject splatsObj = Instantiate(Resources.Load("Prefabs/SplatManager")) as GameObject;
        splatsObj.transform.position = this.transform.position;
        splatsObj.transform.SetParent(this.transform);
        this.Splats = splatsObj.GetComponent<SplatManager>();
    }

    public void CancelAllSplats()
    {
        Splats?.CancelSpellIndicator();
        Splats?.CancelRangeIndicator();
        Splats?.CancelStatusIndicator();
    }

    public void BeginCastAim(AbilityProjectileItem nextSpell)
    {
        //IsAimingAbility = true;
        return;
        if (Splats != null)
        {
            switch (nextSpell.AimType)
            {
                case AbilityAimType.INSTANT:
                case AbilityAimType.INSTANT_TARGET:
                    break;
                case AbilityAimType.LINE:
                    Splats.SelectSpellIndicator("Line");
                    break;
                case AbilityAimType.POINT:
                default:
                    Splats.SelectSpellIndicator("Point");
                    break;
            }
        }
    }

    public void ForceStandStill()
    {
        mover.SetTarget(transform.position);
        UpdateLocomotion(forceStandstill: true);
    }

    public AbilitiesManager GetAbilitiesManager()
    {
        return abilitiesManager;
    }

    public void AimSpellAtTarget(Vector3 targetPos)
    {
        if (IsAimingAbility)
        {
            ForceStandStill();
            lastAbilityPoint = targetPos;
            switch (selectedAbility.AimType)
            {
                //case AbilityAimType.INSTANT:
                //case AbilityAimType.INSTANT_TARGET:
                //    break;
                case AbilityAimType.LINE:
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, 
                        Splats != null ? Splats.transform.rotation 
                        : Quaternion.LookRotation((targetPos - GetPosition()).normalized, Vector3.up), Time.deltaTime * 600f);
                    break;
                case AbilityAimType.POINT:
                default:
                    Vector3 forward = Vector3.RotateTowards(transform.forward, (targetPos - GetPosition()).normalized, 0.1f, 0.1f);
                    forward.y = 0;
                    transform.forward = forward;
                    break;
            }
        }
    }

    public void CommitAbility(AbilityProjectileItem ability, bool triggerAnimation = true)
    {
        //IsAimingAbility = false;
        targetObject = null;
        cachedClosestEnemy = null;
        positionAtCastTime = transform.position;
        lastCastTime = Time.time;
        
        //if (triggerAnimation)
        //    animationController.TriggerCast(ability);

        AddState(new ChannelingAbilityState(this, null));
    }

    public void EvaluateAbilityCommit()
    {
        if (selectedAbility == null) return;
        switch (selectedAbility.ChannelMode)
        {
            case AbilityChannelingMode.CONTINUOUS:
                CastAbility(false);
                if (selectedAbility != null)
                    ChannelLoopActiveAnimations();
                break;
            case AbilityChannelingMode.REPEATED_INTERVALS:
                CastAbility(false);
                CommitAbility(selectedAbility);
                //Invoke("TriggerCast", 1f);
                break;
            case AbilityChannelingMode.SINGLE:
            default:
                CastAbility(true);
                break;
        }
    }

    public void SetupAgentMover()
    {
        if (agent == null)
            agent = this.GetComponent<NavMeshAgent>();

        this.mover = new AgentMover();
        this.mover.agent = this.agent;
        this.mover.cachedCharacter = this;
    }

    public override void SetVisualsScale(float scale)
    {
        base.SetVisualsScale(scale);
        agent.speed *= scale;
        agent.acceleration *= scale;
    }

    private void SetupAnimators()
    {
        this.animationController = new MechAnimationController();
        this.animationController.Setup(this);
        return;
        /*
        if (visuals.GetComponent<CharacterAnimationEventHandler>() == null)
            visuals.AddComponent<CharacterAnimationEventHandler>();
        this.animator = visuals.GetComponent<Animator>();
        if (animator == null)
            animator = visuals.AddComponent<Animator>();
        this.animationController = new AgentAnimationControllerRuntime(data.AnimationController);
        this.animationController.Setup(this, this.animator);

#if UNITY_EDITOR
        string controllerName = data.Name + "_Controller";
        if (!Application.isPlaying)
        {
            var controller = data.AnimationController.CreateController(controllerName);
            animator.runtimeAnimatorController = controller as RuntimeAnimatorController;
        }
#endif
        if (Application.isPlaying)
        {
            animator.runtimeAnimatorController = Resources.Load("AnimationControllers/" + controllerName) as RuntimeAnimatorController;
        }
        */
    }

    public void LoadBuffsManager()
    {
        this.buffsManager = new BuffsManager(this);
    }

    public void LoadInventory()
    {
        this.inventoryManager = new InventoryManager(this);
    }

    public void LoadNarrativesManager()
    {
        if (!useOverrideNarrative) interactionNarrative = data.interactionNarrative;
        this.narrativesManager = new NarrativeTreeManager(this);
    }

    public bool HasAI { get { return data.aiBehaviour != null; } }

    public void LoadAI()
    {
        //if (!useOverrideAI) aiBehavior = data.aiBehaviour;
        aiBehavior = new AIBehaviour();
        aiBehavior.AISequences = new List<IBehaviourSequence>();
        
        if (!useOverrideAI)
            aiOverride = data.aiBehaviour;

        foreach (IBehaviourSequence seq in aiOverride.AISequences)
        {
            aiBehavior.AISequences.Add(seq.Clone());
        }

        if (aiBehavior != null)
            aiBehavior.AISequences?.ForEach(x => { x.SetCharacterAgentController(this); });

    }

    public void InitializeControllerState()
    {
        TerminateStateMachine();

        if (manualControl && GameManager.Instance.mode == GameMode.REALTIME_BATTLE)
        {
            CharacterAgentsManager.SetPlayerCharacter(this);
            SetState(new PlayerControlState(this));
            mech.GetCasterBySlot(MechSlot.HEAD_WEAPON_SHIELD).canAutoCast = true;
            mech.GetCasterBySlot(MechSlot.HEAD_WEAPON_SHIELD).hasIndependentAim = true;
            AddState(new AutoCastState(this, mech.GetCasterBySlot(MechSlot.HEAD_WEAPON_SHIELD)));
        }
        else
        {
            if (GetTeam() == CharacterTeam.NEUTRAL)
            {
                SetState(new AINeutralState(this));
            }
            else
            {
                if (!remoteControl)
                    SetState(new AICombatState(this));
            }
        }
    }

    public override void LoadVisuals(GameObject prefabObj)
    {
        //if (prefabObj == null) return;

        this.mech = GetComponent<FullMech>();
        if (this.mech == null)
        {
            visuals = new GameObject("Mech" + mechID.ToString());
            DestroyVisuals();

            this.mech = visuals.AddComponent<FullMech>();
            MechAssembler assembler = GameObject.FindObjectOfType<MechAssembler>();
            
            if (this.useMechDefOverride)
                this.mech.mechDef = this.mechDefOverride;
            
            assembler?.BuildFullyRandomMech(this.mech);
            visuals.transform.SetParent(this.transform);
        }
        else
        {
            this.mech.AssignSlots();
            this.visuals = transform.GetChild(0).gameObject;
        }

        var parentNode = GetComponentInParent<MechPlacementNode>();
        if (parentNode != null)
            this.placement = parentNode.placement;

        this.mech.enableCustomizationMode = false;
        mech.SetupMechPartCasters(this);

        var legs = mech.GetCasterBySlot(MechSlot.LEGS);
        if (legs != null && legs.animator != null)
            mech.GetCasterBySlot(MechSlot.LEGS).animator?.SetTrigger("Idle");

        visuals.gameObject.layer = this.gameObject.layer;
        visuals.transform.localPosition = Vector3.zero;
        visuals.transform.localRotation = Quaternion.identity;
    }

    private bool useMechDefOverride = false;
    public List<int> mechDefOverride;
    public MechPlacement placement = MechPlacement.NONE;

    public void SetMechDefOverride(List<int> mechDef)
    {
        useMechDefOverride = true;
        mechDefOverride = mechDef;
    }

    private void LoadCharacter(Character _data)
    {
        //DestroyVisuals();

        abilitiesManager = this.gameObject.GetComponent<AbilitiesManager>();
        if (abilitiesManager == null) abilitiesManager = this.gameObject.AddComponent<AbilitiesManager>();

        // load current character visuals
        if (!useOverrideVisuals)
            LoadVisuals(data.prefabObj);
        else LoadVisuals(visualsPrefabOverride);

        // load aiming graphics
        //LoadSplats();

        // load AI
        LoadAI();

        LoadInventory();
        LoadNarrativesManager();
        LoadBuffsManager();
        InitializeFXManager();
        InitializeHealthManager();

        // load weapon
        this.mainHandWeapon = data.StartingEquipment.MainHand as WeaponItem;
        EquipWeapon(this.mainHandWeapon);
        SetupAgentMover();
        SetupAnimators();
        SetVisualsScale(data.scale);

        InitializeControllerState();
        InitializeTurnBasedAgent();

        CharacterAgentsManager.AddCharacterAgent(this, this.uniqueID);
    }

    public void InitializeTurnBasedAgent()
    {
        battleTurnManager = new BattleTurnBasedAgent(this);
    }

    public override void Initialize()
    {
        if (IsInitialized) return;
        if (agent == null)
            agent = this.GetComponent<NavMeshAgent>();

        if (data != null)
            LoadCharacter(data);

        IsInitialized = true;
    }

    public void Start()
    {
        Initialize();
    }

    private void UpdateTimeBasedEffects()
    {
        buffsManager?.UpdateTimeBasedEffects();
    }

    private void UpdateAbilities()
    {
        abilitiesManager?.Tick();
    }

    public void MoveTo(GameObject target)
    {

    }

    public void MoveToTargetAtPoint(GameObject target)
    {

    }

    public void TriggerWeaponCast()
    {
        selectedAbility = weaponController.GetWeaponCastAbility();
        //animationController.TriggerCast(selectedAbility);
    }

    public void TriggerMeleeStrike()
    {
        IsWeaponStriking = true;
        //animationController.TriggerMelee();
        weaponController.BeginWeaponSwing();
    }
    public bool RotateTowardsTarget(Vector3 targetPos, float rate = 4f, float angleTolerance = 2f)
    {
        Vector3 delta = targetPos - this.GetPosition();
        delta.y = 0;
        Vector3 newDir = Vector3.RotateTowards(this.GetForward(), delta.normalized, Time.deltaTime * rate, 1f);
        transform.rotation = Quaternion.LookRotation(newDir);

        return (Vector3.Angle(delta.normalized, this.GetForward()) < angleTolerance);
    }

    public void TryWeaponAttack()
    {
        return;
        if (IsChannelingAbility) return;

        if (
            //!animationController.IsAttackingTarget() &&
            attackCooldown <= 0
            //&& !animationController.IsRunning()
            && !IsChannelingAbility
            && cachedTargetDistance <= attackRange)
        {
            agent.destination = transform.position;
            attackCooldown = attackCooldownTimer;
            IsWeaponStriking = true;
            if (weaponController.weapon.CastsAbility && cachedTargetDistance > attackRangeMelee)
            {
                TriggerWeaponCast();
            }
            else
            {
                TriggerMeleeStrike();
            }
        }
    }

    public void UpdateCooldowns()
    {
        if (attackCooldown > 0) { attackCooldown -= Time.deltaTime; }
    }

    private void UpdateLocomotion(bool forceStandstill = false)
    {
        animationController.UpdateMoveAnimationState(forceStandstill ? 0 : mover.GetSpeed(), mover.GetCanMove(), idleSpeedThreshold);
    }

    public override void Tick()
    {
        if (!IsAlive()) return;

        if (mech != null)
            mech.casters?.ForEach(c => c.Tick());

        UpdateTimeBasedEffects();
        UpdateAbilities();

        if (!IsIncapacitated())
        {
            //if (!IsAimingAbility && !IsChannelingAbility)
            if (CanMove)
            {
                UpdateLocomotion();
            }

            UpdateCooldowns();
        }
    }

    public void ReceiveImpact(IAbilityCaster caster, ImpactDefinition impact, bool applyWithoutVisuals = false)
    {
        if (healthManager.IsDead) return;

        switch (impact.impactType)
        { 
            case AbilityImpactType.BASH:
                //if (applyWithoutVisuals) break;
                if (this.battleTurnManager.Bash(caster, impact))
                    this.mover.BashToTarget(this.battleTurnManager.GetOccupiedTile().GetPosition(), impact.timeOfEffect);
                break;
            case AbilityImpactType.FREEZE:
                AbilityBuffModifier buff = new AbilityBuffModifier();
                buff.buffType = AbilityImpactType.FREEZE;
                buff.buffValue = impact.impactValue;
                var b = new AbilityBuffTemp(buff, impact);
                b.applyType = AbilityApplyType.IMPACT;
                this.buffsManager.AddBuff(b);
                if (!applyWithoutVisuals)
                {
                    ForceStandStill();
                    PauseActiveAnimations();
                }
                break;
            default:
                healthManager.TakeDamage(impact.impactValue);
                if (healthManager.IsDead)
                {
                    caster.ClaimKill(this);
                }
                break;
        }

        if (!applyWithoutVisuals && impact.impactFX != null && impact.impactFX.useRenderFx)
        {
            fxManager.ReceiveImpact(impact.impactFX);
        }
    }

    public TimedImpactComponent GetTimedImpactComponent()
    {
        return buffsManager.timedImpacts;
    }

    void IAttackable.ApplyImpactFX(ImpactFXDefinition impactFX, GameObject targetGO, bool parentToTarget = true)
    {
        this.buffsManager.ApplyImpactFX(impactFX, targetGO, parentToTarget);
    }

    void IAbilityCaster.ApplyImpactFX(ImpactFXDefinition impactFX, GameObject targetGO, bool parentToTarget = true)
    {
        this.buffsManager.ApplyImpactFX(impactFX, targetGO, parentToTarget);
    }

    public void ApplyImpactFX(ImpactFXDefinition impactFX, IAttackable target, bool parentToTarget = true)
    {
        this.buffsManager.ApplyImpactFX(impactFX, target, parentToTarget);
    }

    public void ApplyDelayedImpact(IAbilityCaster caster, AbilityImpactDefinition effect, float abilityFraction = 1f)
    {
        this.buffsManager.ApplyDelayedImpact(caster, effect, abilityFraction);
    }

    public void ChannelLoopActiveAnimations()
    {
        foreach (var caster in mech.casters)
        {
            caster.ChannelLoopActiveAnimation();
        }
    }

    public void ResumeActiveAnimations()
    {
        foreach (var caster in mech.casters)
        {
            caster.ResumeActiveAnimation();
        }
    }

    public void PauseActiveAnimations()
    {
        foreach (var caster in mech.casters)
        {
            caster.PauseActiveAnimation();
        }
    }

    public bool IsIncapacitated()
    {
        return buffsManager.IsIncapacitated();
    }

    /*
    public void TakeDamage(AbilityImpactType impactType, float damage)
    {
        if (healthManager.IsDead) return;
        healthManager.TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        if (healthManager.IsDead) return;
        healthManager.TakeDamage(damage);
    }
    */

    public void MeleeStrikeImpact()
    {
        if (selectedAbility != null && !IsAimingAbility)
        {
            TriggerCast();
        }
        else
        {
            weaponController.StrikeTarget(cachedClosestEnemy);
        }

        IsWeaponStriking = false;
    }

    public void ClearActiveAbility()
    {
        selectedAbility = null;
        //IsAimingAbility = false;

        //IsCasting = false;
        ResumeActiveAnimations();
        abilitiesManager?.EndLastAbilityCast();
    }

    public void ChannelingAbilityUpdate()
    {
        //bool isChanneling = selectedAbility != null && animationController.IsCurrentAnimationTaggedAs(selectedAbility.CustomChannelAnimationTrigger);

        if (IsAimingAbility) return;

        if (selectedAbility.MoveWhileChanneling)// && animationController.IsCurrentAnimationTaggedAs(selectedAbility.CustomChannelAnimationTrigger))
        {
            if (selectedAbility.AimType == AbilityAimType.POINT)
            {
                float distance = Vector3.Distance(positionAtCastTime, lastAbilityPoint);
                transform.position = Vector3.MoveTowards(transform.position, lastAbilityPoint, Time.deltaTime * selectedAbility.ChannelRate * distance);
                targetObject = null;
                mover.SetTarget(GetPosition());
                transform.forward = Vector3.RotateTowards(transform.forward, (lastAbilityPoint - transform.position).normalized, 0.1f, 0.1f);
            }
            else
            {
                float distance = selectedAbility.MoveDistance;
                Vector3 forwardDelta = selectedAbility.ChannelRate * transform.forward * Time.deltaTime * distance;
                transform.position += forwardDelta;
                mover.SetTarget(GetPosition());
                if (targetObject != null)
                {
                    RotateTowardsTarget(targetObject.GetPosition(), 4f);
                }
            }
        }

        if (selectedAbility.ApplyImpactWhileChanneling || selectedAbility.ChannelMode == AbilityChannelingMode.CONTINUOUS)
        {
            abilitiesManager.ChannelAbility(this, lastAbilityPoint, selectedAbility);
        }
    }

    public void RotateTowardsTargetPoint(GameObject target, float rate = 1)
    {
        RotateTowardsTargetPoint(target.transform.position, rate);
    }

    public void RotateTowardsTargetPoint(Vector3 target, float rate = 1)
    {
        Vector3 newForward = Vector3.RotateTowards(transform.forward, (target - transform.position).normalized, rate, 100f);
        newForward.y = 0;
        transform.forward = newForward;
    }

    public void CastAbility(bool clearAbility = true)
    {
        if (IsChannelingAbility)
        {
            Vector3 sourcePos = GetPosition() + Vector3.up * 1f + GetForward() * 2f;
            abilitiesManager?.CastAbility(this, null, sourcePos, this.GetForward(), lastAbilityPoint, selectedAbility, cachedClosestEnemy);
            if (clearAbility) ClearActiveAbility();
        }

        //battleTurnManager?.CommitAbility(selectedAbility);
        OnCastAbility();
    }

    public void TriggerCast()
    {
        if (selectedAbility == null)
        {
            ClearActiveAbility();
            return;
        }

        EvaluateAbilityCommit();
    }

    public void BeginChannelAbility(AbilityProjectileItem ability)
    {
        if (IsChannelingAbility) return;

        this.selectedAbility = ability;
        if (ability.AimType == AbilityAimType.INSTANT)
        {
            CommitAbility(ability);
        }
        else
        {
            BeginCastAim(ability);
        }
    }

    //Player Control
    public void PlayerAbilityInput(AbilityProjectileItem ability)
    {
        BeginChannelAbility(ability);
    }

    public override bool IsAlive()
    {
        return this.GetHP() > 0;
    }

    public CharacterTeam GetTeam()
    {
        return this.team;
    }

    public void Update()
    {
        //if (IsCasting) return;
        
        if (!manualControl) return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            var ability = this.data.StartingAbilities[2];
            var caster = this.mech.GetCasterBySlot(MechSlot.RIGHT_WEAPON_SHIELD);// casters[7];
            caster.BeginChannelAbility(ability, true);
            //this.mech.casters[6].BeginChannelAbility(ability);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            var ability = this.data.StartingAbilities[1];
            var caster = this.mech.GetCasterBySlot(MechSlot.LEGS);
            caster.BeginChannelAbility(ability);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            var ability = this.data.StartingAbilities[2];
            var caster = this.mech.GetCasterBySlot(MechSlot.RIGHT_WEAPON_SHIELD);
            caster.BeginChannelAbility(ability);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            var ability = this.data.StartingAbilities[3];
            var caster = this.mech.GetCasterBySlot(MechSlot.LEFT_WEAPON_SHIELD);// casters[7];
            caster.BeginChannelAbility(ability);
        }
    }

    public void CancelAbility()
    {
        ClearActiveAbility();
    }

    public float GetAbilityBuffModifier(AbilityImpactType mod)
    {
        return buffsManager.GetAbilityBuffModifier(mod);
    }

    public float GetAbilityResistanceModifier(AbilityImpactType impactType)
    {
        return buffsManager.GetAbilityResistanceModifier(impactType);
    }

    void OnDestroy()
    {
        this.narrativesManager?.OnDestroy();
        this.buffsManager?.OnDestroy();
    }

    public void OnUpdateCastTarget(Vector3 castTarget)
    {
        this.abilitiesManager.UpdateDynamicCastTarget(castTarget);
    }

    public void ClaimKill(ITargettable target)
    {
        CharacterAgentController killedTarget = target as CharacterAgentController;
        if (killedTarget != null)
        {
            this.inventoryManager.AddUnitKilled(killedTarget.data);
            //killedTarget.data.Name
            //InventoryManager inventory = killedTarget.inventoryManager;
            //inventory.InventoryItems
        }
    }

    public void TriggerLocation(LocatorTrigger trigger)
    {
        this.narrativesManager.EvaluateLocationTrigger(trigger);
    }

    public void ApplyRenderFX(ImpactFXDefinition impactFX)
    {
        
    }

}