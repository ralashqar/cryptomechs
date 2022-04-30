using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Demos.RPGEditor;

public interface IWeaponControl
{
    
}

public class WeaponController : IWeaponControl
{
    public CharacterAgentController character;

    public GameObject root;
    public WeaponItem weapon;
    public GameObject weaponVisuals;

    private XftWeapon.XWeaponTrail weaponTrail;
    Color trailColor = Color.white;
    private float trailAlpha = 0;
    Coroutine trailRoutine;

    private IEnumerator SetWeaponTrail(float amount, float time = 0.25f)
    {
        float timer = time;
        float currentAlpha = weaponTrail.MyColor.a;
        float targetAlpha = amount * trailColor.a;
        float range = targetAlpha - currentAlpha;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            float fraction = (1 - (timer / time));
            Color newCol = trailColor;
            newCol.a = currentAlpha + (fraction * range);
            weaponTrail.MyColor = newCol;
            yield return new WaitForEndOfFrame();
        }
    }

    public WeaponController(CharacterAgentController character, WeaponItem weapon)
    {
        this.character = character;
        this.weapon = weapon;

        EquipWeapon(weapon);
    }

    public void EquipWeapon(WeaponItem weapon)
    {
        this.weapon = weapon;
        LoadWeaponVisuals(weapon);
    }

    public float GetWeaponCooldown()
    {
        return 1f;
        return weapon.CastsAbility ? weapon.castAbility.attackCooldown : 1f;
    }

    public AbilityProjectileItem GetWeaponCastAbility()
    {
        return weapon.castAbility;
    }

    public float GetWeaponMeleeRange()
    {
        return weapon.MeleeRange;
    }


    public float GetWeaponRange()
    {
        return weapon.CastsAbility ? weapon.castAbility.range : weapon.MeleeRange;
    }

    public void LoadWeaponVisuals(WeaponItem weapon)
    {
        if (weapon != null)
        {
            if (this.weaponVisuals != null) GameObject.DestroyImmediate(this.weaponVisuals);
            this.weaponVisuals = GameObject.Instantiate(weapon.prefabObj);

            Transform mainHandTr = TransformDeepChildExtension.FindDeepChild(character.GetCharacterVisuals().transform, "Mainhand");
            
            if (mainHandTr != null && mainHandTr.gameObject != null)
            {
                GameObject mainHand = mainHandTr.gameObject;

                weaponVisuals.transform.SetParent(mainHand.transform);
                weaponVisuals.transform.localPosition = weapon.WeaponPositionOffset;
                weaponVisuals.transform.localRotation = Quaternion.Euler(weapon.WeaponRotationOffsetEulers);
                weaponVisuals.transform.localScale = Vector3.one * weapon.scale;

                weaponTrail = weaponVisuals.GetComponentInChildren<XftWeapon.XWeaponTrail>();
                if (weaponTrail != null)
                {
                    trailColor = weaponTrail.MyColor;
                    trailAlpha = weaponTrail.MyColor.a;
                    Color col = weaponTrail.MyColor;
                    col.a = 0;
                    weaponTrail.MyColor = col;
                }
            }
        }
    }

    public void StrikeTarget(IAttackable targetAgent)
    {
        if (targetAgent == null || !targetAgent.IsAlive()) return;

        AbilitiesManager.ApplyImpact(character, targetAgent, new ImpactDefinition(weapon.BaseDamageType, weapon.BaseAttackDamage));
        //targetAgent.TakeDamage(weapon.BaseDamageType, weapon.BaseAttackDamage);

        //weapon.WeaponImpactFX
        if (weapon.WeaponImpactFX != null)
        {
            Vector3 impactPoint = weaponVisuals.transform.TransformPoint(weapon.WeaponImpactPointOffset);
            impactPoint = targetAgent.GetGameObject().transform.position + Vector3.up * 2.2f;
            GameObject impactFX = GameObject.Instantiate(weapon.WeaponImpactFX, impactPoint, Quaternion.identity);
            float totalDuration = 0.5f;
            GameObject.Destroy(impactFX, totalDuration);
        }

        if (weaponTrail != null)
        {
            //StopAllCoroutines();
            character.StartCoroutine(SetWeaponTrail(0, 0.3f));
        }
    }

    public void BeginWeaponSwing()
    {
        if (weaponTrail != null)
        {
            character.StartCoroutine(SetWeaponTrail(1));
        }
    }

    public void ApplyDamage(CharacterAgentController targetAgent)
    {
        float damage = weapon.BaseAttackDamage;
    }
}
