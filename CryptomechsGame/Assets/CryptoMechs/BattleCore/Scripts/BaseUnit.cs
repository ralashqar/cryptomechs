using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterTeam
{
    PLAYER,
    OPPONENT,
    NEUTRAL,
    ALL
}

public interface ISummonable
{

}

public interface IBaseUnit
{
    GameObject GetGameObject();
    Transform GetTransform();
    Vector3 GetPosition();
    Vector3 GetForward();
    bool IsAlive();
    void Tick();
    void Initialize();
}

public interface ITargettable : IBaseUnit
{

}

public interface IAttackable : ITargettable, IBaseUnit
{
    CharacterTeam GetTeam();
    float GetAbilityResistanceModifier(AbilityImpactType impactType);
    //void ReceiveImpact(IAbilityCaster caster, AbilityImpactType impactType, float impactValue, ImpactFXDefinition impactFX, float timeOfImpact = 0, float areaOfImpact = 0, bool applyWithoutVisuals = false);
    void ReceiveImpact(IAbilityCaster caster, ImpactDefinition impact, bool applyWithoutVisuals = false);
    bool GetCollision(Vector3 point, float radius);
    float GetColliderRadius();
    TimedImpactComponent GetTimedImpactComponent();
    void ApplyDelayedImpact(IAbilityCaster caster, AbilityImpactDefinition effect, float abilityFraction = 1f);
    void ApplyImpactFX(ImpactFXDefinition impactFX, GameObject target, bool parentToTarget = true);
}

//public interface IAbilityCasterBase
//{

//}

public interface IAbilityCaster : IBaseUnit
{
    float GetAbilityBuffModifier(AbilityImpactType mod);
    CharacterTeam GetTeam();
    AbilitiesManager GetAbilitiesManager();
    void Mine(IMinable minable, float amount, float time = 0);
    void CancelAbility();
    void OnUpdateCastTarget(Vector3 target);
    void ClaimKill(ITargettable target);
    void ApplyImpactFX(ImpactFXDefinition impactFX, GameObject targetGO, bool parentToTarget = true);
}

public class BaseUnit : MonoBehaviour, ITargettable
{
    public bool useOverrideVisuals = false;
    public GameObject visualsPrefabOverride;
    
    public GameObject visuals;

    public virtual void SetVisualsScale(float scale)
    {
        visuals.transform.localScale = Vector3.one * scale;
    }

    public virtual void LoadVisuals(GameObject prefabObj)
    {
        if (prefabObj == null) return;

        DestroyVisuals();
        visuals = Instantiate(prefabObj);
        visuals.gameObject.layer = this.gameObject.layer;
        visuals.transform.SetParent(this.transform);
        visuals.transform.localPosition = Vector3.zero;
        visuals.transform.rotation = Quaternion.identity;
    }

    public virtual void DestroyVisuals()
    {
        foreach (Transform child in this.transform)
        {
            SafeDestroy(child.gameObject);
        }
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }
    public Transform GetTransform()
    {
        return this.transform;
    }

    public Vector3 GetPosition()
    {
        return this.transform.position;
    }

    public Vector3 GetForward()
    {
        return this.transform.forward;
    }

    public virtual void Initialize()
    {

    }

    public virtual void Tick()
    {

    }

    public virtual bool IsAlive()
    {
        return true;
    }

    public static T SafeDestroy<T>(T obj) where T : Object
    {
        if (Application.isEditor)
            Object.DestroyImmediate(obj);
        else
            Object.Destroy(obj);

        return null;
    }
    public static T SafeDestroyGameObject<T>(T component) where T : Component
    {
        if (component != null)
            SafeDestroy(component.gameObject);
        return null;
    }
}
