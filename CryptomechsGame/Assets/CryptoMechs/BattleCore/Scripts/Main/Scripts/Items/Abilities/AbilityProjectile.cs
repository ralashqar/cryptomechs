using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Demos.RPGEditor;

public class AbilityProjectile : Ability
{
    public AbilityProjectileItem data;

    private List<AbilityProjectilePFX> projectiles;

    private float attackTimer = 3.0f;

    //private GameObject pfxInstance;

    public override void ApplyDamage()
    {
        throw new System.NotImplementedException();
    }

    public override void OnImpact()
    {
        throw new System.NotImplementedException();
    }

    public void OnParticleCollisionTriggered(GameObject other)
    {

    }

    public override void TriggerProjectile()
    {
        /*
        GameObject pfxInstance = GameObject.Instantiate(data.pfxPrefab);
        pfxInstance.transform.position = this.transform.position;
        AbilityProjectilePFX projectilePFX = pfxInstance.GetComponentInChildren<AbilityProjectilePFX>();
        projectilePFX.InitializePFX(data, transform.position, transform.forward, data.speed);
        projectilePFX?.TriggerProjectile(transform.position, transform.forward);
        projectiles.Add(projectilePFX);
        */
    }

    // Start is called before the first frame update
    void Start()
    {
        projectiles = new List<AbilityProjectilePFX>();
    }

    // Update is called once per frame
    void Update()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0)
        {
            attackTimer = data.attackCooldown;
            TriggerProjectile();
        }

        for (int i = 0; i < projectiles.Count; ++i)
        {
            AbilityProjectilePFX p = projectiles[i];
            p.Tick();
            if (p.timer <= 0)
            {
                GameObject.Destroy(p.gameObject);
                projectiles.Remove(p);
                i--;
            }
        }
    }
}
