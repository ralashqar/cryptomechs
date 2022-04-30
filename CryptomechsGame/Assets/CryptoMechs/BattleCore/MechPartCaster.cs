using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Demos.RPGEditor;
using static CharacterAgentController;

public interface IPartCaster
{
    bool AutoCasting { get; }
    bool IsManuallyCasting { get; }

    //public MechPartAbility partAbilities;

    bool IsBlockingCasterMovement { get; }
    bool HasActiveProjectiles { get; }
    bool IsAimingAbility { get; }
    bool IsActionSequenceActive { get; }
    bool IsCasting { get; }
    bool IsChannelingAbility { get; }
    bool IsChannelStateActive { get; }
}

[System.Serializable]
public class MechPartCaster : MonoBehaviour, IPartCaster
{
    public CharacterAgentController character;
    public MechSlot slot;
    public AbilitiesManager abilitiesManager;
    public MechPartComponent part;
    public GameObject partGO;
    public Vector3 defaultPosition;
    public Quaternion defaultOrientation;

    public AbilityProjectileItem selectedAbility = null;
    public AbilityCard selectedCard;
    public Animator animator;

    public Transform firePoint;
    public bool IsBoomeranging = false;
    public bool hasIndependentAim = false;
    public bool hasParentRotator = false;
    private Quaternion initRotation;
    Transform rotatorTr;
    public MechSlot parentRotator = MechSlot.LEGS;

    public bool canAutoCast = false;
    public bool AutoCasting { get { return canAutoCast && !IsManuallyCasting; } }
    private bool _isManuallyCasting = true;
    public bool IsManuallyCasting { get { return _isManuallyCasting; } set { _isManuallyCasting = value; } }

    public IAbilityCardCaster partAbilities;

    public bool IsBlockingCasterMovement { get { 
            return ((IsAimingAbility || IsChannelingAbility) && !hasIndependentAim) || IsManuallyCasting; 
        } 
    }
    public bool HasActiveProjectiles { get { return abilitiesManager.HasActiveProjectiles; } }
    public bool IsAimingAbility { get; private set; }
    public bool IsActionSequenceActive { get { return partAbilities == null ? false : partAbilities.IsActionSequenceActive; } }
    public bool IsCasting { get { return (IsAimingAbility || IsChannelingAbility || IsBoomeranging); } }
    public Vector3 lastAbilityPoint = Vector3.zero;
    public bool IsChannelingAbility { get { return selectedAbility != null; } set { } }
    private bool _isChannelStateActive = false;
    public bool IsChannelStateActive { get { return _isChannelStateActive; } set { _isChannelStateActive = value; } }

    public Vector3 positionAtCastTime;
    public float lastCastTime = 0;
    public IAttackable cachedClosestEnemy;
    public ITargettable targetObject;

    public delegate void OnCommitAbilityDelegate(AbilityCard card);
    public OnCastAbilityDelegate OnCastAbility;
    public OnCommitAbilityDelegate OnCommitAbility;

    public List<Transform> firePoints;
    private int roundNum = 0;

    public IAttackable GetCurrentAttackTarget()
    {
        return cachedClosestEnemy;
    }


    public void Setup(MechPartComponent part, MechSlot slot, GameObject partGO, CharacterAgentController character)
    {
        this.part = part;
        this.slot = slot;
        this.partGO = partGO;
        this.character = character;
        //this.abilitiesManager = partGO.AddComponent<AbilitiesManager>();
        abilitiesManager = this.partGO.GetComponent<AbilitiesManager>();
        if (abilitiesManager == null) abilitiesManager = this.partGO.AddComponent<AbilitiesManager>();

        //this.abilitiesManager = new AbilitiesManager();
        this.IsAimingAbility = false;
        this.animator = this.partGO.GetComponent<Animator>();
        if (animator != null)
            cachedPausedAnimSpeed = animator.speed;

        firePoints = new List<Transform>();
        firePoints = TransformDeepChildExtension.FindDeepChildrenContainingName(partGO.transform, "barrel_end");
        if (firePoints.Count == 0) firePoints.Add(partGO.transform);

        defaultPosition = this.partGO.transform.localPosition;
        defaultOrientation = this.partGO.transform.localRotation;

        this.partAbilities = partGO.GetComponent<MechPartAbility>();

        this.rotatorTr = this.transform.parent;

        if (slot == MechSlot.LEFT_WEAPON_SHIELD || slot == MechSlot.RIGHT_WEAPON_SHIELD)
        {
            this.hasParentRotator = true;
            this.parentRotator = MechSlot.SHOULDERS;
        }
        this.initRotation = this.transform.parent.localRotation;
    }

    public void RotateToDefaultOrientation(float rate = 4f)
    {
        this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, Quaternion.identity, Time.deltaTime * rate);
        this.transform.parent.localRotation = Quaternion.Lerp(this.transform.parent.localRotation, initRotation, Time.deltaTime * rate);
    }

    public void SetupCardCams(AbilityCard card)
    {
        if (card == null) return;

        if (card.abilityCams != null && card.abilityCams.Count > 0)
        {
            var cam = card.abilityCams[0];
            CustomisationCameraSystem.Instance.TriggerCamByID(ConvertCamTargetToID(cam.abilityCam), cam.blendTime, cam.timeOffset);
        }
        //this.OnCommitAbility = null;
    }

    public string cardName = "";
    public void CastAbilityCard(AbilityCard card, TurnBasedAgent target, int tileID = -1)
    {
        this.OnCommitAbility += SetupCardCams;
        this.selectedCard = card;
        this.cardName = card.name;

        if (this.partAbilities == null)
            this.partAbilities = this.gameObject.AddComponent<MechPartAbility>();
        this.partAbilities?.CastAbilityCard(card, target, tileID);
        this.character.battleTurnManager?.CommitAbility(card, target, tileID);
    }

    public string ConvertCamTargetToID(AbilityCamera cam)
    {
        switch (cam)
        {
            case AbilityCamera.OPPONENTS_CLOSEUP:
                return character.GetTeam() == CharacterTeam.PLAYER ? "opponents" : "players";
            case AbilityCamera.PLAYERS_CLOSEUP:
                return character.GetTeam() == CharacterTeam.PLAYER ? "players" : "opponents";
            case AbilityCamera.DEFAULT_BATTLE:
            default:
                return "default";
        }
    }

    public void CastPreview(AbilityCard card, TurnBasedAgent target, int tileID = -1)
    {
        if (card.cardMode != AbilityCardType.MOVE && this.partAbilities == null) return;
        this.character.battleTurnManager.PlayCardPlayer(card, target, tileID);
    }

    public void CastPreview(TurnBasedAgent target)
    {
        if (this.partAbilities == null) return;
        if (character.GetTeam() != CharacterTeam.PLAYER) return;
        this.character.battleTurnManager.PlayCardPlayer(this.partAbilities.GetAbilities()[0], target, BattleTileManager.Instance.GetSelectedTileID());
        //CastAbilityCard(this.partAbilities.abilities[0], target);
        //this.partAbilities?.CastAbilityCard(this.partAbilities.abilities[0]);
        //this.partAbilities?.PreviewAction();
    }

    public MechPartCaster(MechPartComponent part, MechSlot slot, GameObject partGO, CharacterAgentController character)
    {
        Setup(part, slot, partGO, character);
    }

    public void MeleeStrikeImpact()
    {
        if (selectedAbility != null && !IsAimingAbility)
        {
            TriggerCast();
        }
    }

    public void ClearActiveAbility()
    {
        selectedAbility = null;
        selectedCard = null;
        OnCommitAbility = null;
        IsAimingAbility = false;
        //IsCasting = false;
        //animationController.ResumeActiveAnimation();
        abilitiesManager?.EndLastAbilityCast();
        IsManuallyCasting = false;
        IsChannelStateActive = false;
    }

    public void Setup(MechPartComponent part, CharacterAgentController character)
    {
        this.part = part;
        this.character = character;
    }

    public AbilitiesManager GetAbilitiesManager()
    {
        return this.abilitiesManager;
    }

    float updateTargetCooldown = 1f;
    float updateTargetTimer = 1f;

    private void UpdateAbilities()
    {
        abilitiesManager?.Tick();
    }

    private float cachedPausedAnimSpeed = 1;
    Coroutine loopingAnim = null;

    public IEnumerator LoopAnimationSegment(int state, float time, float amplitude, float frequency)
    {
        float startLoopTime = Time.time;
        while (true)
        {
            float t = Time.time - startLoopTime;
            animator.Play(state, 0, time + Mathf.Sin(t * frequency) * amplitude);
            yield return null;
        }
    }
    public IEnumerator LoopAnimationSegment(int state, float time, AnimationSegmentLooper loopSettings)
    {
        if (animator == null) yield break;
        float startLoopTime = Time.time;
        while (true)
        {
            float t = Time.time - startLoopTime;
            animator.Play(state, 0, time + Mathf.Sin(t * loopSettings.timeOffsetFrequency) * loopSettings.timeOffsetAmplitudeNormalized);
            yield return null;
        }
    }

    public bool IsAnimationPaused { get { return animator != null ? animator.speed == 0 : false; } }
    public void PauseActiveAnimation()
    {
        IAnimatorPausable[] pausableAnimators = this.transform.GetComponentsInChildren<IAnimatorPausable>();
        if (pausableAnimators != null)
        {
            foreach (var a in pausableAnimators)
            {
                a.PauseAnimations();
            }
        }
        if (animator == null) return;
        if (!IsAnimationPaused)
        {
            cachedPausedAnimSpeed = animator.speed;
            animator.speed = 0;
        }
    }

    public void ChannelLoopActiveAnimation()
    {
        if (animator == null) return;
        cachedPausedAnimSpeed = animator.speed;
        animator.speed = 0;

        var clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        var s = animator.GetCurrentAnimatorStateInfo(0);
        var time = s.normalizedTime;
        var clipName = clipInfo[0].clip.name;
        loopingAnim = character.StartCoroutine(LoopAnimationSegment(s.fullPathHash, time, this.selectedAbility.channelAnimLooping));
    }

    public void ResumeActiveAnimation()
    {
        IAnimatorPausable[] pausableAnimators = this.transform.GetComponentsInChildren<IAnimatorPausable>();
        if (pausableAnimators != null)
        {
            foreach (var a in pausableAnimators)
            {
                a.ResumeAnimations();
            }
        }

        if (animator == null) return;
        if (loopingAnim != null)
            character.StopCoroutine(loopingAnim);

        animator.speed = cachedPausedAnimSpeed;
    }


    public void Tick()
    {
        //if (GameManager.Instance.mode == GameMode.CUSTOMIZATION) return;

        UpdateAbilities();

        updateTargetCooldown -= Time.deltaTime;
        if (updateTargetCooldown <= 0)
        {
            updateTargetCooldown = updateTargetTimer;
            if (hasIndependentAim)
            {
                //if (cachedClosestEnemy == null)
                cachedClosestEnemy = CharacterAgentsManager.FindClosestTargettable(this.character, this.character.GetPosition()) as IAttackable;

                if (cachedClosestEnemy != null && !IsChannelingAbility && !IsAimingAbility && !IsCasting)
                {
                    AimSpellAtTarget(cachedClosestEnemy.GetPosition());
                    //this.partGO.transform.localRotation = Quaternion.Lerp(this.partGO.transform.localRotation, defaultOrientation, Time.deltaTime * 5f);
                }
            }
        }

        if (GameManager.Instance.mode == GameMode.REALTIME_BATTLE && !IsCasting && !hasIndependentAim && slot != MechSlot.LEGS)
        //if (slot != MechSlot.LEGS && !IsAimingAbility && !IsCasting && !IsChannelStateActive && !HasActiveProjectiles)
        {
            RotateToDefaultOrientation();
        }
    }

    public bool AimSpellAtTarget(Vector3 targetPos, bool forcePartRotator = false, float angleTolerance = 2f)
    {
        bool hasReachedTarget = false;

        //if (IsAimingAbility || selectedAbility.AbilityMode == AbilityFXType.LASER)
        {
            lastAbilityPoint = targetPos;

            //Transform rotator = hasIndependentAim ? this.partGO.transform : this.character.transform;
            var pRotator = hasIndependentAim ? this : (hasParentRotator ? this.character.mech.GetCasterBySlot(parentRotator) : this.character.mech.GetCasterBySlot(MechSlot.LEGS));
            bool forceLocalAim = false;
            bool allowPitchRotation = true;

            if (partAbilities != null)
            {
                allowPitchRotation = partAbilities.AllowPitchRotation;
                forceLocalAim = partAbilities.ForceLocalAim;
                if (forceLocalAim)
                    pRotator = this;
            }
            if (partAbilities != null && (partAbilities as MechPartAbility).allowPitchRotation)
            {
            }

            float rotRate = this.character.manualControl ? 120f : 120f;
            rotRate = 0.3f;
            //Vector3 forward = Vector3.RotateTowards(rotator.forward, (targetPos - rotator.position).normalized, 60f * Time.deltaTime, 0.1f);
            //forward.y = 0;
            //Vector3 up = Quaternion.FromToRotation(rotator.forward, forward) * rotator.up;

            //if (hasIndependentAim && slot != MechSlot.LEGS)
            //    rotator.rotation = Quaternion.LookRotation(forward, up);
            //else
            if (GameManager.Instance.mode == GameMode.REALTIME_BATTLE)
            {
                rotRate *= 4f;
            }

            if (pRotator.slot == MechSlot.LEGS)
            {
                hasReachedTarget = character.RotateTowardsTarget(targetPos, rotRate, angleTolerance);
            }
            else
            {
                //if (!hasIndependentAim && GameManager.Instance.mode == GameMode.REALTIME_BATTLE)
                if (!hasIndependentAim && !forceLocalAim)
                {
                    character.RotateTowardsTarget(targetPos, rotRate, angleTolerance);
                }
                hasReachedTarget = pRotator.RotateChildTowardsTarget(this, targetPos, rotRate, angleTolerance);
            }

            if (allowPitchRotation && (slot == MechSlot.LEFT_WEAPON_SHIELD || slot == MechSlot.RIGHT_WEAPON_SHIELD
                || slot == MechSlot.HEAD_WEAPON_SHIELD))
            {
                Vector3 delta = targetPos - this.character.GetPosition();
                delta.y = 0;
                float angle = Vector3.Angle(delta.normalized, character.GetForward());
                //if (angle < 30)
                RotatePitch(targetPos, rotRate);
            }
            /*
            if (!hasIndependentAim && slot != MechSlot.LEGS)
            {
                rotator = this.partGO.transform;
                forward = Vector3.RotateTowards(rotator.forward, (targetPos - rotator.position).normalized, 0.1f, 0.1f);
                forward.y = 0;
                up = Quaternion.FromToRotation(rotator.forward, forward) * rotator.up;
                rotator.rotation = Quaternion.LookRotation(forward, up);
            }
            */

        }
        return hasReachedTarget;
    }

    public bool RotateChildTowardsTarget(MechPartCaster child, Vector3 targetPos, float rate = 4f, float angleTolerance = 2f)
    {
        if (rotatorTr == null) rotatorTr = this.transform.parent;
        Vector3 delta = targetPos - child.GetPosition();
        delta.y = 0;
        Vector3 newDir = Vector3.RotateTowards(child.GetForward(), delta.normalized, Time.deltaTime * rate, 1f);

        //var rot = Quaternion.FromToRotation(child.GetForward(), newDir);
        var rot = Quaternion.LookRotation(newDir, rotatorTr.up);
        rotatorTr.rotation = rot;

        return (Vector3.Angle(child.GetForward(), delta.normalized) <= angleTolerance);
        //rotatorTr.rotation *= rot;
    }

    public void RotatePitch(Vector3 targetPos, float rate = 4f)
    {
        var rotator = this.transform;
        Vector3 normal = Vector3.Cross(rotator.forward, Vector3.up);
        Vector3 proj = Vector3.ProjectOnPlane(targetPos - rotator.position, normal.normalized);
        Vector3 newDir = Vector3.RotateTowards(this.transform.forward, proj.normalized, Time.deltaTime * rate, 1f);
        //rotator.LookAt(targetPos, Vector3.up);
        rotator.rotation = Quaternion.LookRotation(newDir, rotator.up);
    }

    public void RotateTowardsTarget(Vector3 targetPos, float rate = 4f)
    {
        if (rotatorTr == null) rotatorTr = this.transform.parent;
        Vector3 delta = targetPos - this.GetPosition();
        delta.y = 0;
        Vector3 newDir = Vector3.RotateTowards(this.GetForward(), delta.normalized, Time.deltaTime * rate, 1f);
        rotatorTr.rotation = Quaternion.LookRotation(newDir);
    }

    public Vector3 GetPosition()
    {
        if (rotatorTr == null) rotatorTr = this.transform.parent;
        return rotatorTr.position;
    }

    public IEnumerator TriggerCastAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EvaluateAbilityCommit();
    }

    public void CommitAbility(AbilityProjectileItem ability, bool triggerAnimation = true)
    {
        IsAimingAbility = false;
        targetObject = null;
        cachedClosestEnemy = null;
        positionAtCastTime = partGO.transform.position;
        lastCastTime = Time.time;

        StartCoroutine(TriggerCastAfterDelay(ability.TriggerCastTime));

        //Invoke("EvaluateAbilityCommit", ability.TriggerCastTime);

        if (triggerAnimation)
            TriggerCastAnimations(ability);

        //character.selectedAbility = this.selectedAbility;
        
        character.AddState(new ChannelingAbilityState(character, this));

        //SetupCardCams(selectedCard);
        OnCommitAbility?.Invoke(selectedCard);
    }

    public void TriggerCastAnimations(AbilityProjectileItem ability)
    {
        if (animator != null)
        {
            animator.SetFloat("speed", 1);
            animator?.SetTrigger(ability.CustomChannelAnimationTrigger);
        }
    }

    public void OnUpdateCastTarget(Vector3 castTarget)
    {
        this.abilitiesManager.UpdateDynamicCastTarget(castTarget);
    }

    public void EvaluateAbilityCommit()
    {
        if (selectedAbility == null) return;
        switch (selectedAbility.ChannelMode)
        {
            case AbilityChannelingMode.CONTINUOUS:
                CastAbility(false);
                //if (selectedAbility != null)
                //    animationController.ChannelLoopActiveAnimation();
                break;
            case AbilityChannelingMode.REPEATED_INTERVALS:
                CastAbility(false);
                //CommitAbility(selectedAbility);
                //Invoke("TriggerCast", 1f);
                break;
            case AbilityChannelingMode.SINGLE:
            default:
                CastAbility(true);
                break;
        }
    }

    public void TriggerDeath()
    {
        if (animator != null)
            animator.SetTrigger("Death");

    }

    public void BeginChannelAbility(AbilityProjectileItem ability, bool manualOverride = false)
    {
        if (manualOverride)
        {
            ClearActiveAbility();
            IsManuallyCasting = true;
            character.OnCancelAbility(this);
            character.ForceStandStill();
        }
        if (IsBoomeranging || IsChannelingAbility) 
            return;

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

    public void BeginCastAim(AbilityProjectileItem nextSpell)
    {
        IsAimingAbility = true;
        
        if (!hasIndependentAim) character.ForceStandStill();
        
        if (character.Splats != null)
        {
            switch (nextSpell.AimType)
            {
                case AbilityAimType.INSTANT:
                case AbilityAimType.INSTANT_TARGET:
                    break;
                case AbilityAimType.LINE:
                    character.Splats.SelectSpellIndicator("Line");
                    break;
                case AbilityAimType.POINT:
                default:
                    character.Splats.SelectSpellIndicator("Point");
                    break;
            }
        }
    }

    public int GetNumFirePoints()
    {
        return firePoints != null && firePoints.Count > 0 ? firePoints.Count : 1;
    }

    public Vector3 GetForward()
    {
        if (rotatorTr == null) rotatorTr = this.transform.parent;
        return rotatorTr.forward;
    }

    public void CycleFirePoint()
    {
        if (firePoints != null && firePoints.Count > 0)
        {
            roundNum++;
            if (roundNum >= firePoints.Count) roundNum = 0;
        }
    }
    public Transform GetCastPoint()
    {
        firePoint = firePoints != null && firePoints.Count > 0 ? firePoints[roundNum] : null;
        Transform firePos = firePoint != null ? firePoint  : this.partGO.transform;
        return firePos;
    }

    public Vector3 GetCastDir()
    {
        firePoint = firePoints != null && firePoints.Count > 0 ? firePoints[roundNum] : null;
        Vector3 fireDir = firePoint != null ? firePoint.forward : GetForward();
        return fireDir;
    }

    public void CastAbility(bool clearAbility = true)
    {
        if (IsChannelingAbility)
        {
    
            abilitiesManager?.CastAbility(this.character, this, lastAbilityPoint, selectedAbility, character.cachedClosestEnemy);

            //if (firePoint != null)
            //    abilitiesManager?.CastAbility(this.character, firePoint, GetForward(),  lastAbilityPoint, selectedAbility, cachedClosestEnemy);
            //else
            //    abilitiesManager?.CastAbility(this.character, firePos, GetForward(), lastAbilityPoint, selectedAbility, cachedClosestEnemy);
            
            if (clearAbility) ClearActiveAbility();
        }

        OnCastAbility?.Invoke();

        //character.battleTurnManager?.CommitAbility(selectedAbility);
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
}
