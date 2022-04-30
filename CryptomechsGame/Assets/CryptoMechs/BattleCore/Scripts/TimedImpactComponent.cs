using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Demos.RPGEditor;

public class ImpactFXInstance
{
    public GameObject fx;
    public float timestamp;
    public int moveStamp;
    public float duration = 1f;
    public int moveDuration = 0;
    public bool useDeterministicTime = false;

    public ImpactFXInstance(GameObject fx, float duration, int moveDuration = 0, bool useDeterministicTime = false)
    {
        this.fx = fx;
        this.timestamp = Time.time;
        this.duration = duration;
        this.moveStamp = BattleManager.Instance.moveIndex;
        this.useDeterministicTime = useDeterministicTime;
        this.moveDuration = moveDuration;
    }
}

public class DelayedImpact
{
    public IAbilityCaster caster;
    public AbilityImpactDefinition effect;
    public float abilityFraction = 1f;
    public float timestamp;
    public int turnStamp;
    public int moveStamp;

    public DelayedImpact(IAbilityCaster caster, AbilityImpactDefinition effect, float abilityFraction = 1f)
    {
        this.caster = caster;
        this.effect = effect;
        this.abilityFraction = abilityFraction;
        this.timestamp = Time.time;
        this.moveStamp = BattleManager.Instance.moveIndex;
        this.turnStamp = BattleManager.Instance.currentTurnNumber;
    }
}

public class TimedImpactComponent
{
    public List<DelayedImpact> delayedImpacts;
    public List<AbilityBuffTemp> appliedBuffs;
    public List<ImpactFXInstance> impactFXs;
    private IAttackable targetCharacter;

    public TimedImpactComponent()
    {

    }

    public TimedImpactComponent(IAttackable target)
    {
        this.targetCharacter = target;
    }

    public void SetTarget(IAttackable target)
    {
        this.targetCharacter = target;
    }

    public void ApplyDelayedImpact(IAbilityCaster caster, AbilityImpactDefinition effect, float abilityFraction = 1f)
    {
        if (delayedImpacts == null)
            delayedImpacts = new List<DelayedImpact>();

        delayedImpacts.Add(new DelayedImpact(caster, effect, abilityFraction));
    }

    public void ApplyImpactFX(ImpactFXDefinition impactFX, GameObject targetGO, bool parentToTarget = true, bool useVisuals = true)
    {
        if (impactFX == null || impactFX.impactFX == null) return;

        GameObject impactGO = GameObject.Instantiate(impactFX.impactFX);
        impactGO.transform.position = targetGO.transform.position;
        var poi = impactGO.GetComponent<POI_Target>();
        if (poi != null)
        {
            poi.LatchToPOITarget(targetGO);
        }
        //impactGO.transform.SetParent(targetGO.transform, true);
        if (parentToTarget) impactGO.transform.parent = targetGO.transform;

        AddImpactFX(impactGO, impactFX);
        //GameObject.Destroy(impactGO, impactFX.impactDuration);
    }

    public void AddImpactFX(GameObject impactGO, ImpactFXDefinition impactFX)
    {
        if (impactFXs == null)
            impactFXs = new List<ImpactFXInstance>();
        impactFXs.Add(new ImpactFXInstance(impactGO, impactFX.impactDuration, impactFX.impactMovesDuration, impactFX.useDeterministicTime));
    }

    public void ApplyImpactFX(ImpactFXDefinition impactFX, IAttackable target, bool parentToTarget = true)
    {
        if (impactFX == null || impactFX.impactFX == null) return;

        var targetGO = target.GetGameObject();
        ApplyImpactFX(impactFX, targetGO);
    }

    public void UpdateDelayedImpacts()
    {
        if (delayedImpacts == null) return;
        float time = Time.time;
        int move = BattleManager.Instance.moveIndex;

        for (int i = 0; i < delayedImpacts.Count; ++i)
        {
            var impact = delayedImpacts[i];
            int movesElapsed = move - impact.moveStamp;
            float timeElapsed = time - impact.timestamp;
            bool canApply = impact.effect.useDeterministicDelay ?
                movesElapsed > impact.effect.movesDelay
                : timeElapsed > impact.effect.impactDelay;

            if (canApply)
            {
                //impact.caster.GetAbilitiesManager()?.ApplyImpact(impact.caster, this.character, impact.effect, impact.abilityFraction);
                AbilitiesManager.ApplyImpact(impact.caster, this.targetCharacter, impact.effect);

                if (impact.effect.areaOfEffect > 0)
                {
                    var aoeTargets = CharacterAgentsManager.FindClosestEnemiesWithinRange(this.targetCharacter.GetPosition(), impact.effect.areaOfEffect, impact.caster.GetTeam());
                    foreach (var t in aoeTargets)
                    {
                        AbilitiesManager.ApplyImpact(impact.caster, t, impact.effect);
                    }
                }

                if (this.targetCharacter is CharacterAgentController)
                    (this.targetCharacter as CharacterAgentController).battleTurnManager.SaveToNetworkState();
                delayedImpacts.Remove(impact);
                i--;
            }
        }
    }

    public void UpdateImpactFXs()
    {
        if (impactFXs == null) return;
        
        float time = Time.time;
        int move = BattleManager.Instance.moveIndex;

        for (int i = 0; i < impactFXs.Count; ++i)
        {
            var impact = impactFXs[i];
            int movesElapsed = move - impact.moveStamp;
            float timeElapsed = time - impact.timestamp;
            bool canApply = impact.useDeterministicTime ?
                movesElapsed > impact.moveDuration
                : timeElapsed > impact.duration;

            if (canApply)
            {
                //impact.caster.GetAbilitiesManager()?.ApplyImpact(impact.caster, this.character, impact.effect, impact.abilityFraction);
                GameObject.Destroy(impact.fx);
                impactFXs.Remove(impact);
                i--;
            }
        }
    }

    public void UpdateTimeBasedEffects()
    {
        UpdateDelayedImpacts();
        
        UpdateImpactFXs(); //visual only
    }
}
