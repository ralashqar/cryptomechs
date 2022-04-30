using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Demos.RPGEditor;

[System.Serializable]
public class AbilityBuffModifier
{
    public AbilityImpactType buffType;
    public float buffValue;
}

[System.Serializable]
public class AbilityBuffTemp
{
    public AbilityBuffTemp(AbilityBuffModifier buff, ImpactDefinition impact)
    {
        this.buff = buff;
        this.duration = impact.timeOfEffect;
        this.useDeterministicTime = impact.useDeterminsticTime;
        this.moveDuration = impact.timeOfEffectMoves;
        this.movestamp = BattleManager.Instance.moveIndex;
        this.timestamp = Time.time;
    }

    public AbilityBuffModifier buff;
    public AbilityApplyType applyType;
    public float duration;
    public int movestamp = 0;
    public float timestamp = 0;
    public bool useDeterministicTime = false;
    public int moveDuration = 0;
}

[System.Serializable]
public class AbilityResistanceModifier
{
    public AbilityImpactType resistanceType;
    public float resistanceValue;
}

[System.Serializable]
public class ImpactFXDefinition
{
    public string animationTrigger;
    public GameObject impactFX;
    public float impactDuration;
    public bool useDeterministicTime = false;
    public int impactMovesDuration = 0;
    public bool useRenderFx = false;
    public string renderEffect = "";
    public GameObject renderEffectPrefab;
}

[System.Serializable]
public class AbilityImpactDefinition
{
    public AbilityApplyType ApplicationType = AbilityApplyType.IMPACT;
    public AbilityAffectsTeam affectsTeam = AbilityAffectsTeam.ENEMIES;
    public AbilityImpactType impactType;
    public float impactValue;
    public GameObject impactObject;
    public float areaOfEffect;
    public bool useDeterministicTime = false;
    public int timeOfEffectMoves = 0;
    public float timeOfEffect;
    public float impactDelay;
    public bool useDeterministicDelay = false;
    public int movesDelay = 0;
    public bool HasDelay { get { return (!useDeterministicDelay && impactDelay > 0) || (useDeterministicDelay && movesDelay > 0); } }
    public bool CanImpactTarget(IAbilityCaster caster, IAttackable attackable)
    {
        switch (affectsTeam)
        {
            case AbilityAffectsTeam.ALL:
                return true;
            case AbilityAffectsTeam.ALLIES:
                return caster.GetTeam() == attackable.GetTeam();
            case AbilityAffectsTeam.ENEMIES:
            default:
                return caster.GetTeam() != attackable.GetTeam();
        }
    }

    public bool useImpactTargetFX;
    [ShowIf("useImpactTargetFX")]
    public ImpactFXDefinition targetImpactFX;
}

public class ImpactDefinition
{
    public IAbilityCaster caster;
    public IAttackable target; 
    public AbilityImpactType impactType;
    public float impactValue;
    public ImpactFXDefinition impactFX = null;
    public float timeOfEffect = 0;
    public float areaOfEffect = 0;
    public bool useDeterminsticTime = false;
    public int timeOfEffectMoves = 0;

    public ImpactDefinition(AbilityImpactType impactType, float impactValue)
    {
        this.impactType = impactType;
        this.impactValue = impactValue;
    }

    public ImpactDefinition (AbilityImpactDefinition effect)
    {
        impactType = effect.impactType;
        impactValue = effect.impactValue;
        impactFX = effect.useImpactTargetFX ? effect.targetImpactFX : null;
        timeOfEffect = effect.timeOfEffect;
        areaOfEffect = effect.areaOfEffect;
        timeOfEffectMoves = effect.timeOfEffectMoves;
        useDeterminsticTime = effect.useDeterministicTime;
    }
}

public enum AbilityApplyType
{
    IMPACT,
    BUFF,
    RESISTANCE
}

public enum AbilityAimType
{
    LINE,
    POINT,
    CONE,
    INSTANT,
    INSTANT_TARGET
}

public enum AbilityAffectsTeam
{
    ENEMIES,
    ALLIES,
    ALL
}

public enum AbilityImpactType
{
    BASE_MELEE,
    BASE_MAGIC,
    FIRE,
    HEAL,
    STUN,
    BASH,
    MELEE_CRIT_CHANCE,
    MELEE_CRIT_DAMAGE,
    MAGIC_CRIT_CHANCE,
    MAGIC_CRIT_DAMAGE,
    MINE,
    SUMMON,
    FREEZE,
    VISUAL
}

public class AbilitiesManager : MonoBehaviour
{
    private List<AbilityProjectilePFX> projectiles;

    private Dictionary<AbilityImpactType, List<IAttackable>> impactedCharacters;

    public bool HasActiveProjectiles { get { return projectiles != null && projectiles.Count > 0; } }

    public static int GetMaxRounds(AbilityProjectileItem ab)
    {
        int numRounds = 1;
        switch (ab.AbilityMode)
        {
            case AbilityFXType.LASER:
                //numRounds = (int)(ab.Lifetime / ab.ResetTime) + 1;
                numRounds = ab.NumImpacts;
                break;
            case AbilityFXType.BOOMERANG:
                numRounds = 1;
                break;
            case AbilityFXType.PROJECTILE:
                if (ab.ChannelMode == AbilityChannelingMode.SINGLE)
                {
                    break;
                }
                float channelTime = ab.ChannelTime;
                float channelTimer = 1f / ab.ChannelRate;
                //numRounds = (int)(channelTime / channelTimer) + 1;
                numRounds = ab.NumImpacts;
                if (ab.UseProceduralSpread && ab.SpreadType == AbilitySpreadType.BARREL_POINTS)
                {
                    numRounds *= ab.NumberOfRounds;
                }
                break;
            default:
                break;
        }
        return numRounds;
    }

    public AbilitiesManager()
    {
        Initialize();
    }

    public void Initialize()
    {
        impactedCharacters = new Dictionary<AbilityImpactType, List<IAttackable>>();
    }

    public void EndLastAbilityCast()
    {
        impactedCharacters?.Clear();
        EvaluateCancelProjectiles();
    }

    public void CastAbility(IAbilityCaster caster, MechPartCaster part, Vector3 targetPos, AbilityProjectileItem ability, IAttackable targetCharacter = null)
    {
        EndLastAbilityCast();
        
        Vector3 targetDir = part.GetCastPoint().forward;
        Vector3 sourcePos = part.GetCastPoint().position;

        if (ability.MuzzleFX != null)
        {
            var firefx = GameObject.Instantiate(ability.MuzzleFX);
            firefx.transform.position = sourcePos;
            firefx.transform.LookAt(sourcePos + targetDir);
            firefx.transform.parent = caster.GetTransform();
            GameObject.Destroy(firefx, ability.MuzzleFXDuration);
        }

        switch (ability.AbilityMode)
        {
            case AbilityFXType.LASER:
            case AbilityFXType.BOOMERANG:
            case AbilityFXType.PROJECTILE:
                
                CastProjectile(caster, part, ability, targetPos, targetCharacter);
                if (ability.UseProceduralSpread && ability.NumberOfRounds > 0)
                {
                    if (ability.SpreadType == AbilitySpreadType.BARREL_POINTS)
                    {
                        part.CycleFirePoint();
                        int maxRounds = Mathf.Min(part.GetNumFirePoints(), ability.NumberOfRounds);
                        for (int i = 0; i < maxRounds - 1; ++i)
                        {
                            CastProjectile(caster, part, ability, targetPos, targetCharacter);
                            part.CycleFirePoint();
                        }

                    }
                    else
                    {
                        Vector3 forward = targetDir;
                        float angle = (float)ability.SpreadAngle / (float)ability.NumberOfRounds;
                        for (int i = 0; i < ability.NumberOfRounds; ++i)
                        {
                            float a = (1 + i) * angle;
                            Vector3 fr = Quaternion.Euler(0, a, 0) * forward;
                            Vector3 fl = Quaternion.Euler(0, -a, 0) * forward;
                            CastProjectile(caster, part, sourcePos, fr, ability, targetPos, targetCharacter);
                            CastProjectile(caster, part, sourcePos, fl, ability, targetPos, targetCharacter);
                        }
                    }
                }
                break;
            case AbilityFXType.CAST_LINE:
            case AbilityFXType.CAST_TARGET:
            case AbilityFXType.CAST_ONSPOT:
            default:
                switch (ability.AbilityImpactPoint)
                {
                    case AbilityImpactPoint.CHARACTER_CENTER:
                    case AbilityImpactPoint.WEAPON_POINT:
                        CastSpellOnTarget(caster, caster.GetPosition(), ability);
                        if (ability.TrailFX != null)
                        {
                            CastProjectile(caster, part, sourcePos, targetDir, ability, targetPos, targetCharacter);
                        }
                        break;
                    default:
                        CastSpellOnTarget(caster, targetPos, ability);
                        if (ability.TrailFX != null)
                        {
                            CastProjectile(caster, part, sourcePos, targetDir, ability, targetPos, targetCharacter);
                        }
                        break;
                }
                break;
        }
    }

    public void CastAbility(IAbilityCaster caster, MechPartCaster part, Vector3 sourcePos, Vector3 targetDir, Vector3 targetPos, AbilityProjectileItem ability, IAttackable targetCharacter = null)
    {
        EndLastAbilityCast();

        switch (ability.AbilityMode)
        {
            case AbilityFXType.LASER:
            case AbilityFXType.BOOMERANG:
            case AbilityFXType.PROJECTILE:
                CastProjectile(caster, part, sourcePos, targetDir, ability, targetPos, targetCharacter);
                if (ability.UseProceduralSpread && ability.NumberOfRounds > 0)
                {
                    Vector3 forward = targetDir;
                    float angle = (float)ability.SpreadAngle / (float)ability.NumberOfRounds;
                    for (int i = 0; i < ability.NumberOfRounds; ++i)
                    {
                        float a = (1 + i) * angle;
                        Vector3 fr = Quaternion.Euler(0, a, 0) * forward;
                        Vector3 fl = Quaternion.Euler(0, -a, 0) * forward;
                        CastProjectile(caster, part, sourcePos, fr, ability, targetPos, targetCharacter);
                        CastProjectile(caster, part, sourcePos, fl, ability, targetPos, targetCharacter);
                    }
                }
                break;
            case AbilityFXType.CAST_TARGET:
            case AbilityFXType.CAST_ONSPOT:
            default:
                switch (ability.AbilityImpactPoint)
                {
                    case AbilityImpactPoint.CHARACTER_CENTER:
                    case AbilityImpactPoint.WEAPON_POINT:
                        CastSpellOnTarget(caster, caster.GetPosition(), ability);
                        if (ability.TrailFX != null)
                        {
                            CastProjectile(caster, part, sourcePos, targetDir, ability, targetPos, targetCharacter);
                        }
                        break;
                    default:
                        CastSpellOnTarget(caster, targetPos, ability);
                        if (ability.TrailFX != null)
                        {
                            CastProjectile(caster, part, sourcePos, targetDir, ability, targetPos, targetCharacter);
                        }
                        break;
                }
                break;
        }
    }

    public void ChannelAbility(IAbilityCaster caster, Vector3 targetPos, AbilityProjectileItem ability)
    {
        switch (ability.AbilityMode)
        {
            case AbilityFXType.PROJECTILE:
                //CastProjectile(caster, caster.transform.position + Vector3.up * 1f, caster.transform.forward, ability);
                break;
            case AbilityFXType.CAST_TARGET:
            default:
                switch (ability.AbilityImpactPoint)
                {
                    case AbilityImpactPoint.CHARACTER_CENTER:
                    case AbilityImpactPoint.WEAPON_POINT:
                        ApplyAbilityImpact(caster, caster.GetPosition(), ability);
                        break;
                    default:
                        ApplyAbilityImpact(caster, targetPos, ability);
                        break;
                }
                break;
        }

    }
    
    public void CastProjectile(IAbilityCaster caster, MechPartCaster part, AbilityProjectileItem projectile, Vector3 targetPos, IAttackable targetCharacter)
    {
        if (projectiles == null)
            projectiles = new List<AbilityProjectilePFX>();

        var firePos = part.GetCastPoint();
        var fireDir = part.GetCastDir();

        GameObject pfxInstance = projectile.TrailFX != null ? GameObject.Instantiate(projectile.TrailFX) : new GameObject(projectile.Name);
        pfxInstance.transform.position = firePos.position;
        //AbilityProjectilePFX projectilePFX = pfxInstance.GetComponentInChildren<AbilityProjectilePFX>();
        AbilityProjectilePFX projectilePFX = pfxInstance.AddComponent<AbilityProjectilePFX>();
        projectilePFX.damage = projectile.damage;
        projectilePFX.cachedCaster = caster;
        projectilePFX.followsTarget = projectile.FollowsTarget;
        projectilePFX.steerRate = projectile.FollowSteerRate;
        projectilePFX.destroyDelay = projectile.TrailFXDestroyDelay;

        if (projectile.FollowsTarget && targetCharacter != null)
        {
            projectilePFX.SetLockTarget(targetCharacter.GetGameObject());
        }

        projectilePFX.InitializePFX(projectile, firePos, fireDir, projectile.speed);
        projectilePFX?.TriggerProjectile(part, fireDir, targetPos);
        projectiles.Add(projectilePFX);
    }

    public void CastProjectile(IAbilityCaster caster, MechPartCaster part, Vector3 position, Vector3 direction, AbilityProjectileItem projectile, Vector3 targetPos, IAttackable targetCharacter)
    {
        if (projectiles == null)
            projectiles = new List<AbilityProjectilePFX>();

        GameObject pfxInstance = projectile.TrailFX != null ? GameObject.Instantiate(projectile.TrailFX) : new GameObject(projectile.Name);
        pfxInstance.transform.position = position;
        //AbilityProjectilePFX projectilePFX = pfxInstance.GetComponentInChildren<AbilityProjectilePFX>();
        AbilityProjectilePFX projectilePFX = pfxInstance.AddComponent<AbilityProjectilePFX>();
        projectilePFX.damage = projectile.damage;
        projectilePFX.cachedCaster = caster;
        projectilePFX.followsTarget = projectile.FollowsTarget;
        projectilePFX.steerRate = projectile.FollowSteerRate;
        if (projectile.FollowsTarget && targetCharacter != null)
        {
            projectilePFX.SetLockTarget(targetCharacter.GetGameObject());
        }
        projectilePFX.InitializePFX(projectile, position, direction, projectile.speed);
        projectilePFX?.TriggerProjectile(part, position, direction, targetPos);
        projectiles.Add(projectilePFX);
    }

    public IEnumerator ApplyAbilityEffectAfterDelay(IAbilityCaster caster, IAttackable target, AbilityImpactDefinition effect, float abilityFraction = 1f)
    {
        yield return new WaitForSeconds(effect.impactDelay);
        ApplyImpact(caster, target, effect, abilityFraction);
    }

    public IEnumerator ApplyAbilityImpactAfterDelay(IAbilityCaster caster, Vector3 position, AbilityProjectileItem spell)
    {
        yield return new WaitForSeconds(spell.ImpactDelayAfterFX);
        ApplyAbilityImpact(caster, position, spell);
    }

    public bool IsCharacterAlreadyImpacted(AbilityImpactType effect, IAttackable character)
    {
        return effect != AbilityImpactType.BASH && (impactedCharacters.ContainsKey(effect) && impactedCharacters[effect].Contains(character));
    }

    private Coroutine impactFXRoutine;

    public void ApplyAbilityImpact(IAbilityCaster caster, Vector3 position, AbilityProjectileItem spell)
    {
        foreach (AbilityImpactDefinition effect in spell.AbilityEffects)
        {
            
            switch (effect.impactType)
            {
                case AbilityImpactType.MINE:
                    List<IMinable> minableTargets = CharacterAgentsManager.FindMinablesWihtinRange(position, effect.areaOfEffect);
                    if (minableTargets == null || minableTargets.Count == 0)
                    {
                        caster.CancelAbility();
                    }
                    else
                    {
                        foreach (IMinable minable in minableTargets)
                        {
                            float time = spell.ChannelMode != AbilityChannelingMode.CONTINUOUS ? 0.3f : 0;
                            float amount = spell.ChannelMode != AbilityChannelingMode.CONTINUOUS ? 0.1f : Time.deltaTime * 0.15f;
                            caster.Mine(minable, amount, time);
                        }
                    }
                    break;
                case AbilityImpactType.SUMMON:
                    if (effect.impactObject != null)
                    {
                        GameObject summonedObject = GameObject.Instantiate(effect.impactObject, position, Quaternion.identity);
                        CharacterAgentController controller = summonedObject.GetComponent<CharacterAgentController>();
                        if (controller != null)
                        {
                            controller.isBot = true;
                            controller.team = caster.GetTeam();
                        }
                        if (effect.useImpactTargetFX && effect.targetImpactFX.impactFX != null)
                        {
                            caster.ApplyImpactFX(effect.targetImpactFX, summonedObject, false);
                        }
                    }
                    break;
                default:
                    List<IAttackable> impactedTargets = spell.AbilityMode == AbilityFXType.CAST_LINE ?
                        CharacterAgentsManager.FindAttackableEnemiesWithinRangeOfLine(position, position + (caster.GetForward() * spell.range), effect.areaOfEffect, CharacterTeam.ALL)
                            : CharacterAgentsManager.FindClosestEnemiesWithinRange(position, effect.areaOfEffect, CharacterTeam.ALL);
                    
                    foreach (IAttackable target in impactedTargets)
                    {
                        if (effect.CanImpactTarget(caster, target))
                        {
                            if (!IsCharacterAlreadyImpacted(effect.impactType, target))
                            {
                                if (!impactedCharacters.ContainsKey(effect.impactType))
                                {
                                    impactedCharacters.Add(effect.impactType, new List<IAttackable>());
                                }
                                impactedCharacters[effect.impactType].Add(target);

                                if (!effect.HasDelay)
                                    ApplyImpact(caster, target, effect, false);
                                else
                                {
                                    target.ApplyDelayedImpact(caster, effect);
                                    //StartCoroutine(ApplyAbilityEffectAfterDelay(caster, target, effect));
                                }
                            }
                        }
                    }
                    break;
            }
            
        }
    }

    public void CastSpellOnTarget(IAbilityCaster caster, Vector3 position, AbilityProjectileItem spell)
    {
        if (spell.ImpactFX != null && spell.AbilityMode != AbilityFXType.CAST_LINE)
        {
            GameObject impactFX = GameObject.Instantiate(spell.ImpactFX, position, Quaternion.identity);
            GameObject.Destroy(impactFX, spell.ImpactFXDuration);
        }

        if (spell.ImpactDelayAfterFX <= 0)
        {
            ApplyAbilityImpact(caster, position, spell);
        }
        else
        {
            StartCoroutine(ApplyAbilityImpactAfterDelay(caster, position, spell));
        }
    }

    public void TriggerImpact(AbilityProjectilePFX abilityInstance)
    {
        if (abilityInstance.abilityPFX.ImpactFX != null)
        {
            GameObject impactFX = GameObject.Instantiate(abilityInstance.abilityPFX.ImpactFX, abilityInstance.transform.position, abilityInstance.transform.rotation);
            float totalDuration = 0.5f;
            GameObject.Destroy(impactFX, totalDuration);
        }
    }

    public static float GetAbilityModifierBetween(IAbilityCaster caster, IAttackable target, AbilityImpactType impactType)
    {
        return caster.GetAbilityBuffModifier(impactType) - target.GetAbilityResistanceModifier(impactType);
    }

    public static void ApplyImpact(IAbilityCaster caster, IAttackable target, AbilityImpactDefinition effect, bool applyWithoutVisuals = false)
    {
        ApplyImpact(caster, target, new ImpactDefinition(effect), 1, applyWithoutVisuals);
    }

    public static void ApplyImpact(IAbilityCaster caster, IAttackable target, ImpactDefinition impact, float impactFraction = 1, bool applyWithoutVisuals = false)
    {
        if (caster == null || target == null || !target.IsAlive()) return;

        //float impactValue = effect.impactValue * impactFraction;
        float percentDelta = GetAbilityModifierBetween(caster, target, impact.impactType);

        percentDelta /= 100f;
        impact.impactValue = impact.impactValue * (1 + percentDelta);

        switch (impact.impactType)
        {
            case AbilityImpactType.FREEZE:
                target.ReceiveImpact(caster, impact, applyWithoutVisuals);
                break;
            case AbilityImpactType.SUMMON:
                break;
            case AbilityImpactType.BASE_MAGIC:
            case AbilityImpactType.FIRE:
            case AbilityImpactType.BASE_MELEE:
            default:
                float critChance = 0;
                float critDamageMult = 0;
                if (impact.impactType == AbilityImpactType.BASE_MELEE)
                {
                    critChance = GetAbilityModifierBetween(caster, target, AbilityImpactType.MELEE_CRIT_CHANCE);
                    critDamageMult = GetAbilityModifierBetween(caster, target, AbilityImpactType.MELEE_CRIT_DAMAGE);
                }
                else
                {
                    critChance = GetAbilityModifierBetween(caster, target, AbilityImpactType.MAGIC_CRIT_CHANCE);
                    critDamageMult = GetAbilityModifierBetween(caster, target, AbilityImpactType.MAGIC_CRIT_DAMAGE);
                }
                critDamageMult = 1f + (critDamageMult / 100f);
                critDamageMult = Mathf.Max(1f, critDamageMult);

                if (critChance > 0 && critDamageMult > 1f)
                {
                    if (Random.Range(0, 100) > critChance)
                    {
                        impact.impactValue *= critDamageMult;
                    }
                }

                if (!applyWithoutVisuals && impact.impactFX != null)
                    target.ApplyImpactFX(impact.impactFX, target.GetGameObject());
                
                target.ReceiveImpact(caster, impact, applyWithoutVisuals);
                break;
        }
    }

    public void ApplyImpact(IAbilityCaster caster, IAttackable target, AbilityImpactDefinition effect, float impactFraction = 1)
    {
        ImpactDefinition impact = new ImpactDefinition(effect);
        ApplyImpact(caster, target, impact, impactFraction);
        if (effect.areaOfEffect > 0)
        {
            var aoeTargets = CharacterAgentsManager.FindClosestEnemiesWithinRange(target.GetPosition(), effect.areaOfEffect, caster.GetTeam());
            foreach (var t in aoeTargets)
            {
                if (t != target)
                {
                    ApplyImpact(caster, t, impact, impactFraction);
                }
            }
        }
    }

    public static void ApplyImpact(IAbilityCaster caster, IAttackable target, AbilityProjectileItem ability, float impactFraction = 1)
    {
        foreach (AbilityImpactDefinition effect in ability.AbilityEffects)
        {
            ImpactDefinition impact = new ImpactDefinition(effect);
            ApplyImpact(caster, target, impact, impactFraction);
        }
    }

    public void EvaluateCancelProjectiles()
    {
        if (projectiles == null) return;

        for (int i = 0; i < projectiles.Count; ++i)
        {
            AbilityProjectilePFX p = projectiles[i];
            p.OnCancel();
        }
    }

    public void UpdateDynamicCastTarget(Vector3 target)
    {
        if (projectiles == null) return;

        for (int i = 0; i < projectiles.Count; ++i)
        {
            AbilityProjectilePFX p = projectiles[i];
            p.UpdateCastTarget(target);
        }
    }

    // Tick is called once per frame
    public void Tick()
    {
        if (projectiles == null) return;

        for (int i = 0; i < projectiles.Count; ++i)
        {
            AbilityProjectilePFX p = projectiles[i];
            p.Tick();
            if (p.timer <= 0)
            {
                //TriggerImpact(p);
                p.DestroyFX();
                projectiles.Remove(p);
                i--;
            }
        }
    }
}
