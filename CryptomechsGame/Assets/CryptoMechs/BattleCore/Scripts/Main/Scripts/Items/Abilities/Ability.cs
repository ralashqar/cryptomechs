using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AbilityType
{
    PROJECTILE,
    CAST_ON_SPOT,
    MELEE
}

public abstract class Ability : MonoBehaviour
{
    public float damage;
    public float castTime;
    public Vector3 target;
    public float areaOfEffect;

    public abstract void TriggerProjectile();

    public abstract void OnImpact();

    public abstract void ApplyDamage();

}
