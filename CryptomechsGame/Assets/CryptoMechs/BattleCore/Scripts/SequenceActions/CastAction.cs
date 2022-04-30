using Sirenix.OdinInspector.Demos.RPGEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CastTarget
{
    SELECTED_TARGET,
    MULTI_TARGET,
    OPPONENT_CENTER,
    EMPTY_TILE,
    CUSTOM,
}

public class CastAction : CharacterActionBase
{
    public AbilityProjectileItem selectedAbility;

    public CastTarget castTargetType;
    public string castTarget;
    Transform targetTr;
    Vector3 targetTrOffset = Vector3.zero;
    Vector3 targetPos;
    public GameObject castTargetUnit { get; private set; }

    private MechPartCaster caster;
    public float aimTime = 1f;
    private float aimTimer = 1f;
    public float channelTime = 1f;
    private float channelTimer = 1f;

    private CharacterAgentController AICharacterAgent { get { return caster.character; } }

    public override ICharacterAction Clone()
    {
        CastAction clone = new CastAction();
        clone.targetCharacter = this.targetCharacter;
        clone.targetSlot = this.targetSlot;
        clone.characterID = this.characterID;
        clone.waitMode = this.waitMode;
        clone.waitTime = this.waitTime;

        clone.selectedAbility = this.selectedAbility;
        clone.castTargetType = this.castTargetType;
        clone.castTarget = this.castTarget;
        clone.aimTime = this.aimTime;
        clone.channelTime = this.channelTime;

        return clone;
    }

    public override void Complete()
    {
    }

    public override bool IsComplete()
    {
        if (!base.IsTriggered() || !base.IsActionCommited()) return false;
        return channelTimer <= 0 && !caster.IsCasting && !caster.HasActiveProjectiles;
    }

    /*
    public bool CanExecute()
    {
        cachedAttackTarget = CharacterAgentsManager.FindClosestAttackableEnemy(AICharacterAgent.GetPosition(), AICharacterAgent.GetTeam()) as IAttackable;
        //AICharacterAgent.cachedClosestEnemy = cachedAttackTarget;

        if (IsCasting() || AICharacterAgent.IsCasting) return true;

        return cachedAttackTarget != null &&
            cachedAttackTarget.IsAlive() &&
            Vector3.Distance(AICharacterAgent.GetPosition(), cachedAttackTarget.GetPosition()) < AICharacterAgent.attackRange;
    }
    */

    public void SetTarget(GameObject target, Vector3 offset)
    {
        this.castTargetUnit = target;
        this.targetTr = target.transform;
        this.targetTrOffset = offset;
    }

    public void SetTarget(Vector3 target)
    {
        this.targetPos = target;
        this.castTargetUnit = null;
        this.targetTr = null;
    }

    public void CommitAbility()
    {
        caster?.CommitAbility(selectedAbility);
    }

    public void BeginRandomAbilityCast()
    {
        AICharacterAgent.ForceStandStill();

        //selectedCaster = abilityIndex == 0 ? AICharacterAgent.mech.GetCasterBySlot(MechSlot.RIGHT_WEAPON_SHIELD) : AICharacterAgent.mech.casters[0];
        caster.BeginChannelAbility(selectedAbility);
        //AICharacterAgent.BeginChannelAbility(selectedAbility);
        aimTimer = aimTime;
    }

    public void ChannelAbilityUpdate()
    {
        if (aimTimer > 0) aimTimer -= Time.deltaTime;
        //AICharacterAgent.ForceStandStill();

        //if (cachedAttackTarget != null)
        //    AICharacterAgent.RotateTowardsTarget(cachedAttackTarget.GetPosition(), Time.deltaTime * 6f);

        bool hasLockedTarget = false;
        switch(castTargetType)
        {
            case CastTarget.SELECTED_TARGET:
            case CastTarget.EMPTY_TILE:
                if (targetTr != null)
                    hasLockedTarget = caster.AimSpellAtTarget(targetTr.TransformPoint(targetTrOffset));
                else
                    hasLockedTarget = caster.AimSpellAtTarget(AICharacterAgent.GetPosition() + AICharacterAgent.GetForward() * 3f);
                break;
            default:
                hasLockedTarget = caster.AimSpellAtTarget(targetPos);
                break;
        }

        if (aimTimer <= 0 && hasLockedTarget)
        {
            CommitAbility();
            hasCommittedAbility = true;
        }
    }

    public bool IsCasting()
    {
        return isCastRoutineActive;
    }

    Coroutine castRoutine = null;

    private bool isCastRoutineActive = false;

    public IEnumerator CastRoutine()
    {
        isCastRoutineActive = true;
        BeginRandomAbilityCast();
        while (caster.IsAimingAbility)
        {
            if (!AICharacterAgent.IsIncapacitated())
            {
                ChannelAbilityUpdate();
            }
            yield return null;
        }
        while (channelTimer > 0)
        {
            channelTimer -= Time.deltaTime;
            yield return null;
        }
        AICharacterAgent.ClearActiveAbility();
        isCastRoutineActive = false;
    }

    private Vector3 GetTargetPos()
    {
        if (castTargetType == CastTarget.SELECTED_TARGET && targetTr != null)
            return targetTr.TransformPoint(targetTrOffset);
        else 
            return targetPos;
    }

    public override void ExecuteFrame()
    {

        if (caster != null && caster.selectedAbility != null)
        {
            var target = GetTargetPos();
            if (selectedAbility.AbilityMode == AbilityFXType.LASER)
            {
                caster.OnUpdateCastTarget(target);
                caster.lastAbilityPoint = target;
                caster.AimSpellAtTarget(target);
            }
            if (selectedAbility.ChannelMode == AbilityChannelingMode.REPEATED_INTERVALS)
            {
                caster.AimSpellAtTarget(target);
                caster.lastAbilityPoint = target;
            }
        }

        //if (caster == null || (!caster.IsChannelingAbility && !caster.IsAimingAbility))
        //{
            //if (taskCooldown > 0) taskCooldown -= Time.deltaTime;
            //if (taskCooldown <= 0)
            //{
            //    AICharacterAgent?.StartCoroutine(CastRoutine());
            //}
        //}
        if (AICharacterAgent.IsAimingAbility)
        {
            ChannelAbilityUpdate();
        }

    }

    public override void CommitAction()
    {
        if (this.selectedAbility == null)
        {
            Complete();
            return;
        }

        aimTimer = aimTime;
        channelTimer = channelTime;

        caster = base.GetTargetCaster();

        GameObject target = GameObject.Find(castTarget);
        
        if (castTargetUnit != null)
            target = castTargetUnit;

        if (target != null)
            targetTr = target.transform;

        if (castTargetType == CastTarget.SELECTED_TARGET && target == null)
        {
            Complete();
            hasCommittedAbility = true;
        }
        else
        {
            AICharacterAgent?.StartCoroutine(CastRoutine());
        }

    }

    private bool hasCommittedAbility = false;

    /*
    public void CommitAbility()
    {
        caster.CommitAbility(selectedAbility);
        hasCommittedAbility = true;
        
    }

    public void BeginAbilityCast()
    {
        caster.BeginChannelAbility(selectedAbility);
    }

    public void ChannelAbilityUpdate()
    {
        caster?.AimSpellAtTarget(targetTr.position);
        aimTimer -= Time.deltaTime;
        if (aimTimer <= 0)
        {
            CommitAbility();
        }
    }

    public override void ExecuteFrame()
    {
        if (!hasCommittedAbility && !caster.IsChannelingAbility && !caster.IsAimingAbility)
        {
            BeginAbilityCast();
        }
        else if (caster.IsAimingAbility)
        {
            ChannelAbilityUpdate();
        }

        if (hasCommittedAbility)
            channelTimer -= Time.deltaTime;
    }
    */
}
