using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Demos.RPGEditor;

public class BuffsManager : CharacterComponentManager
{
    public Dictionary<AbilityImpactType, float> AbilityBuffModifiers;
    public Dictionary<AbilityImpactType, float> AbilityResistanceModifiers;
    public Dictionary<StatType, float> StatModifiers;
    public List<AbilityBuffTemp> appliedBuffs;
    public TimedImpactComponent timedImpacts;

    private readonly List<AbilityImpactType> incapacitationBuffs = new List<AbilityImpactType>() {
        AbilityImpactType.FREEZE,
        AbilityImpactType.STUN
    };

    public void ApplyDelayedImpact(IAbilityCaster caster, AbilityImpactDefinition effect, float abilityFraction = 1f)
    {
        timedImpacts.ApplyDelayedImpact(caster, effect, abilityFraction);
    }

    public void ApplyImpactFX(ImpactFXDefinition impactFX, GameObject targetGO, bool parentToTarget = true, bool useVisuals = true)
    {
        timedImpacts.ApplyImpactFX(impactFX, targetGO, parentToTarget, useVisuals);
        //GameObject.Destroy(impactGO, impactFX.impactDuration);
    }

    public void ApplyImpactFX(ImpactFXDefinition impactFX, IAttackable target, bool parentToTarget = true)
    {
        if (impactFX == null || impactFX.impactFX == null) return;

        var targetGO = target.GetGameObject();
        ApplyImpactFX(impactFX, targetGO);
    }

    public BuffsManager(CharacterAgentController character) : base(character)
    {
        AbilityBuffModifiers = new Dictionary<AbilityImpactType, float>();
        AbilityResistanceModifiers = new Dictionary<AbilityImpactType, float>();
        StatModifiers = new Dictionary<StatType, float>();
        appliedBuffs = new List<AbilityBuffTemp>();
        timedImpacts = new TimedImpactComponent(character);
    }

    public void AddBaseBuffModifiers()
    {
        foreach (AbilityBuffModifier buff in character.data.Skills.StartingBuffModifiers)
        {
            if (AbilityBuffModifiers.ContainsKey(buff.buffType))
            {
                AbilityBuffModifiers[buff.buffType] += buff.buffValue;
            }
            else
            {
                AbilityBuffModifiers.Add(buff.buffType, buff.buffValue);
            }
        }
    }

    public void AddBaseResistanceModifiers()
    {
        foreach (AbilityResistanceModifier resistance in character.data.Skills.StartingResistanceModifiers)
        {
            if (AbilityResistanceModifiers.ContainsKey(resistance.resistanceType))
            {
                AbilityResistanceModifiers[resistance.resistanceType] += resistance.resistanceValue;
            }
            else
            {
                AbilityResistanceModifiers.Add(resistance.resistanceType, resistance.resistanceValue);
            }
        }
    }

    public void AddEquipableItemModifiers(EquipableItem item)
    {
        if (item == null) return;
        if (item.BuffModifiers != null)
        {
            foreach (AbilityBuffModifier buff in item.BuffModifiers)
            {
                if (AbilityBuffModifiers.ContainsKey(buff.buffType))
                {
                    AbilityBuffModifiers[buff.buffType] += buff.buffValue;
                }
                else
                {
                    AbilityBuffModifiers.Add(buff.buffType, buff.buffValue);
                }
            }
            foreach (AbilityResistanceModifier resistance in item.ResistanceModifiers)
            {
                if (AbilityResistanceModifiers.ContainsKey(resistance.resistanceType))
                {
                    AbilityResistanceModifiers[resistance.resistanceType] += resistance.resistanceValue;
                }
                else
                {
                    AbilityResistanceModifiers.Add(resistance.resistanceType, resistance.resistanceValue);
                }
            }

            for (int i = 0; i < item.Modifiers.Count; ++i)
            {
                StatValue stat = item.Modifiers[i];
                if (StatModifiers.ContainsKey(stat.Type))
                {
                    StatModifiers[stat.Type] += stat.Value;
                }
                else
                {
                    StatModifiers.Add(stat.Type, stat.Value);
                }
            }
        }
    }

    public void UpdateDelayedImpacts()
    {
        timedImpacts.UpdateDelayedImpacts();
    }

    public void UpdateImpactFXs()
    {
        timedImpacts.UpdateImpactFXs();
    }

    public void UpdateTimeBasedEffects()
    {
        UpdateDelayedImpacts();
        
        UpdateImpactFXs(); //visual only

        UpdateBuffsAndAfflictions();
    }

    public bool IsIncapacitated()
    {
        foreach (AbilityBuffTemp b in appliedBuffs)
        {
            if (b.applyType == AbilityApplyType.IMPACT)
            {
                if (incapacitationBuffs.Contains(b.buff.buffType))
                    return true;
            }
        }
        return false;
    }

    public void UpdateBuffsAndAfflictions()
    {
        if (appliedBuffs == null) return;
        int move = BattleManager.Instance.moveIndex;
        float time = Time.time;
        for (int i = 0; i < appliedBuffs.Count; ++i)
        {

            AbilityBuffTemp buff = appliedBuffs[i];
            int movesElapsed = move - buff.movestamp;
            float timeElapsed = time - buff.timestamp;
            bool canApply = buff.useDeterministicTime ?
                movesElapsed > buff.moveDuration
                : timeElapsed > buff.duration;

            if (canApply)
            {
                if (buff.applyType == AbilityApplyType.IMPACT)
                {
                    switch (buff.buff.buffType)
                    {
                        case AbilityImpactType.FREEZE:
                            character.ResumeActiveAnimations();
                            break;
                    }
                }
                appliedBuffs.Remove(buff);
                i--;
            }
        }
    }

    public void AddBuff(AbilityBuffTemp buff)
    {
        this.appliedBuffs.Add(buff);
    }

    public float GetAbilityBuffModifier(AbilityImpactType mod)
    {
        float buffVal = AbilityBuffModifiers.ContainsKey(mod) ? AbilityBuffModifiers[mod] : 0;
        List<AbilityBuffTemp> addedBuffs = appliedBuffs.FindAll(b => b.buff.buffType == mod && b.applyType == AbilityApplyType.BUFF);
        if (appliedBuffs != null && appliedBuffs.Count > 0)
        {
            if (addedBuffs != null)
            {
                foreach (AbilityBuffTemp buff in addedBuffs)
                {
                    buffVal += buff.buff.buffValue;
                }
            }
        }
        return buffVal;
    }

    public float GetAbilityResistanceModifier(AbilityImpactType mod)
    {
        float resistanceVal = AbilityResistanceModifiers.ContainsKey(mod) ? AbilityResistanceModifiers[mod] : 0;
        if (appliedBuffs != null && appliedBuffs.Count > 0)
        {
            List<AbilityBuffTemp> addedBuffs = appliedBuffs.FindAll(b => b.buff.buffType == mod && b.applyType == AbilityApplyType.RESISTANCE);
            if (addedBuffs != null)
            {
                foreach (AbilityBuffTemp buff in addedBuffs)
                {
                    resistanceVal += buff.buff.buffValue;
                }
            }
        }
        return resistanceVal;
    }

    public void UpdateCharacterEquipmentModifiers()
    {
        AbilityBuffModifiers.Clear();
        AbilityResistanceModifiers.Clear();
        StatModifiers.Clear();

        AddBaseBuffModifiers();
        AddBaseResistanceModifiers();

        AddEquipableItemModifiers(character.data.StartingEquipment.Body);
        AddEquipableItemModifiers(character.data.StartingEquipment.Head);
        AddEquipableItemModifiers(character.data.StartingEquipment.MainHand);
        AddEquipableItemModifiers(character.data.StartingEquipment.Offhand);
    }
}
