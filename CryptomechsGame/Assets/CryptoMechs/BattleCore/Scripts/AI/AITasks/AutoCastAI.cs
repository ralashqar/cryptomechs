using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Demos.RPGEditor;

[System.Serializable]
public class AutoCastAI : IBehaviourTask
{
    public CharacterAgentController AICharacterAgent { get; set; }

    public float taskCooldownTimer = 4f;
    private float taskCooldown = 1f;

    public float aimTimer = 2f;
    private float aimCooldown = 3f;

    private IAttackable cachedAttackTarget;

    private MechPartCaster selectedCaster;

    public AutoCastAI(CharacterAgentController character, MechPartCaster caster)
    {
        this.AICharacterAgent = character;
        this.selectedCaster = caster;

        this.taskCooldownTimer = 1f;
        this.aimTimer = 1f;
    }

    private AbilityProjectileItem selectedAbility = null;

    public bool IsOnCooldown { get { return taskCooldown <= 0; } }
    
    public IBehaviourTask Clone()
    {
        AutoCastAI clone = new AutoCastAI(this.AICharacterAgent, this.selectedCaster);
        clone.taskCooldown = this.taskCooldown;
        clone.taskCooldownTimer = this.taskCooldownTimer;
        clone.aimTimer = this.aimTimer;
        clone.aimCooldown = this.aimCooldown;
        clone.selectedAbility = this.selectedAbility;

        return clone;
    }

    public bool CanExecute()
    {
        cachedAttackTarget = CharacterAgentsManager.FindClosestAttackableEnemy(AICharacterAgent.GetPosition(), AICharacterAgent.GetTeam()) as IAttackable;
        //AICharacterAgent.cachedClosestEnemy = cachedAttackTarget;

        if (IsCasting() || AICharacterAgent.IsCasting) return true;

        return cachedAttackTarget != null &&
            cachedAttackTarget.IsAlive() &&
            Vector3.Distance(AICharacterAgent.GetPosition(), cachedAttackTarget.GetPosition()) < AICharacterAgent.attackRange;
    }

    public void TriggerTask()
    {
        cachedAttackTarget = CharacterAgentsManager.FindClosestAttackableEnemy(AICharacterAgent.GetPosition(), AICharacterAgent.GetTeam()) as IAttackable;
        //AICharacterAgent.ForceStandStill();
    }

    public bool CanExecuteAbility(AbilityProjectileItem ability)
    {
        return true;
    }

    public void CommitAbility()
    {
        selectedCaster?.CommitAbility(selectedAbility);
        taskCooldown = taskCooldownTimer;
    }

    public void BeginRandomAbilityCast()
    {
        if (selectedCaster == null) return;
        
        int abilityIndex = Random.Range(0, AICharacterAgent.data.StartingAbilities.Count);
        
        //if (abilityIndex == 1)
            abilityIndex = 0;
        
        selectedAbility = AICharacterAgent.data.StartingAbilities[abilityIndex];
        //AICharacterAgent.ForceStandStill();

        //selectedCaster = AICharacterAgent.mech.GetCasterBySlot(MechSlot.HEAD_WEAPON_SHIELD);
        //selectedCaster = abilityIndex == 0 ? AICharacterAgent.mech.GetCasterBySlot(MechSlot.RIGHT_WEAPON_SHIELD) : AICharacterAgent.mech.casters[0];
        selectedCaster.BeginChannelAbility(selectedAbility);
        //AICharacterAgent.BeginChannelAbility(selectedAbility);
        aimCooldown = aimTimer;
    }

    public void ChannelAbilityUpdate()
    {
        if (aimCooldown > 0) aimCooldown -= Time.deltaTime;
        //AICharacterAgent.ForceStandStill();

        //if (cachedAttackTarget != null)
        //    AICharacterAgent.RotateTowardsTarget(cachedAttackTarget.GetPosition(), Time.deltaTime * 6f);

        if (cachedAttackTarget != null)
            selectedCaster?.AimSpellAtTarget(cachedAttackTarget.GetPosition());
        else
            selectedCaster?.AimSpellAtTarget(AICharacterAgent.GetPosition() + AICharacterAgent.GetForward() * 3f);

        if (aimCooldown <= 0)
        {
            CommitAbility();
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
        while (selectedCaster.IsAimingAbility)
        {
            if (!AICharacterAgent.IsIncapacitated())
            {
                ChannelAbilityUpdate();
            }
            yield return null;
        }
        isCastRoutineActive = false;
    }

    public void Execute()
    {
        if (GameManager.Instance.mode != GameMode.REALTIME_BATTLE) return;

        //return;
        if (selectedCaster.IsManuallyCasting) return;
        cachedAttackTarget = CharacterAgentsManager.FindClosestAttackableEnemy(AICharacterAgent.GetPosition(), AICharacterAgent.GetTeam()) as IAttackable;

        if (cachedAttackTarget == null || !cachedAttackTarget.IsAlive()) return;

        if (selectedCaster != null && selectedCaster.selectedAbility != null)
        {
            if (selectedAbility.AbilityMode == AbilityFXType.LASER || selectedAbility.ChannelMode == AbilityChannelingMode.REPEATED_INTERVALS)
            {
                var target = cachedAttackTarget.GetPosition();
                selectedCaster.AimSpellAtTarget(target);
                if (selectedAbility.AbilityMode == AbilityFXType.LASER)
                    selectedCaster.OnUpdateCastTarget(target);
                selectedCaster.lastAbilityPoint = target;
            }
        }

        if (selectedCaster == null || (!selectedCaster.IsChannelingAbility && !selectedCaster.IsAimingAbility))
        {
            if (taskCooldown > 0) taskCooldown -= Time.deltaTime;
            if (taskCooldown <= 0)
            {
                AICharacterAgent?.StartCoroutine(CastRoutine());
            }
        }
        else if (!selectedCaster.IsChannelingAbility && selectedCaster.IsAimingAbility)
        {
            ChannelAbilityUpdate();
        }

    }

    public bool IsCompleted()
    {
        return false;
    }

    public void OnSuccess()
    {
        throw new System.NotImplementedException();
    }
}
