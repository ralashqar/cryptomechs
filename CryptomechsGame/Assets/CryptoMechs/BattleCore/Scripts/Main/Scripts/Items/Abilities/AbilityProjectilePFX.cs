using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Demos.RPGEditor;
using System.Linq;

public class AbilityProjectilePFX : MonoBehaviour
{
    public float damage;
    public float fireRate;
    public float cooldown;
    public int numRounds = 1;

    public IAbilityCaster cachedCaster;
    public MechPartCaster partCaster;
    public AbilityProjectileItem abilityPFX;
    public bool followsTarget = false;
    public float steerRate = 0;
    public Vector3 currentPosition;
    public float destroyDelay = 0;
    public int maxRounds = 1;
    private int roundsUsed = 0;

    public GameObject lockTarget;
    public Transform muzzleGO;

    [Header("Motion")]
    public Vector3 direction;
    public float speed;

    //public ParticleSystem[] ParticleSystemsEmitters;
    //public ParticleSystem[] ParticleSystemsEmitters;
    public float timer = 3.0f;
    private float resetTime = 0;
    //public ParticleSystem ParticleSystemsCollider;

    //public GameObject ParticleSystemPrefab;
    //private GameObject particleInstance;

    private System.Action<Vector3> OnCollision;

    public void SetLockTarget(GameObject target)
    {
        this.lockTarget = target;
    }

    private Vector3 targetPos;
    private Transform sourceTr;
    public AbilityVisualFXBehavior customFX;

    public void TriggerProjectile(MechPartCaster part, Vector3 direction, Vector3 targetPos)
    {
        this.targetPos = targetPos;
        this.partCaster = part;
        this.sourceTr = part.firePoint != null ? part.firePoint : part.partGO.transform;
        var targetRot = Quaternion.LookRotation(direction);
        transform.position = sourceTr.position;
        transform.rotation = targetRot;

        currentPosition = sourceTr.position;
        customFX = transform.GetComponentInChildren<AbilityVisualFXBehavior>();
        //SetCollisions(false);
        if (customFX != null)
        {
        }
        customFX?.OnFire(cachedCaster, sourceTr, abilityPFX);
        this.timer = abilityPFX.Lifetime;
        this.maxRounds = AbilitiesManager.GetMaxRounds(abilityPFX);
    }

    public void TriggerProjectile(MechPartCaster part, Vector3 position, Vector3 direction, Vector3 targetPos)
    {
        //lockTarget = GameObject.FindGameObjectWithTag("Opponent");
        this.targetPos = targetPos;
        this.partCaster = part;
        var targetRot = Quaternion.LookRotation(direction);
        transform.position = position;
        transform.rotation = targetRot;

        currentPosition = position;
        customFX = transform.GetComponentInChildren<AbilityVisualFXBehavior>();
        //SetCollisions(false);
        if (customFX != null)
        {
        }
        customFX?.OnFire(cachedCaster, null, abilityPFX);
        this.timer = abilityPFX.Lifetime;
    }

    /*
    public void SetCollisions(bool enableCollision)
    {
        foreach (var ps in ParticleSystemsEmitters)
        {
            var collision = ps.collision;
            collision.enabled = enableCollision;
        }
    }
    */
    public bool IsAlive = true;

    public AbilitiesManager GetAbilitiesManager()
    {
        return partCaster != null ? partCaster.GetAbilitiesManager() : cachedCaster.GetAbilitiesManager();

    }
    public void Tick()
    {
        switch(abilityPFX.AbilityMode)
        {
            case AbilityFXType.LASER:
                float y = currentPosition.y;
                currentPosition = this.targetPos;
                currentPosition.y = y;
                Vector3 sourcePos = sourceTr != null ? sourceTr.position : transform.position;
                Vector3 dir = currentPosition - sourcePos;
                dir.y = 0;
                if (dir.magnitude > abilityPFX.range)
                    dir = dir.normalized * abilityPFX.range;
                RaycastHit hit;
                //currentPosition = CharacterAgentsManager.FindNearestRaycastTarget(sourcePos, currentPosition, 0, cachedCaster.GetTeam());
                currentPosition = sourcePos + dir;
                int layermask = cachedCaster.GetTeam() == CharacterTeam.PLAYER ? LayerMask.GetMask("Opponent") : LayerMask.GetMask("Player");
                if (Physics.Raycast(sourcePos, dir.normalized, out hit, abilityPFX.range, layermask))
                    currentPosition = hit.point - (dir.normalized);
                currentPosition.y = 2f;
                break;
            case AbilityFXType.BOOMERANG:
                currentPosition = customFX != null ? customFX.GetTargetPosition() : currentPosition;
                break;
            case AbilityFXType.PROJECTILE:
            default:
                currentPosition += this.transform.forward * speed * Time.deltaTime;
                break;
        }

        if (customFX == null)
        {
            this.transform.position = currentPosition;
        }
        else
        {
            customFX.OnChannel(currentPosition);
        }

        if (lockTarget != null)
        {
            float desiredAngle = Vector3.SignedAngle(this.transform.forward, (lockTarget.transform.position - currentPosition).normalized, Vector3.up);
            float maxAngle = steerRate * Time.deltaTime;

            float a = Mathf.Sign(desiredAngle) * Mathf.Min(Mathf.Abs(desiredAngle), maxAngle);
            this.transform.Rotate(Vector3.up, a);
        }

        timer -= Time.deltaTime;
        if (resetTime > 0)
            resetTime -= Time.deltaTime;

        if (IsAlive && resetTime <= 0 && roundsUsed < maxRounds)
        {
            int layermask = cachedCaster.GetTeam() == CharacterTeam.PLAYER ? LayerMask.GetMask("Opponent") : LayerMask.GetMask("Player");
            Ray ray = new Ray(currentPosition, this.transform.forward);
            RaycastHit hit;
            float radius = 1f;
            
            var opponentTeam = cachedCaster.GetTeam() == CharacterTeam.PLAYER ? CharacterTeam.OPPONENT : CharacterTeam.PLAYER;
            List<ITargettable> characters = CharacterAgentsManager.Instance.characters.ContainsKey(opponentTeam) ? CharacterAgentsManager.Instance.characters[opponentTeam]
                : null;
            
            if (characters != null && BattleTileManager.Instance.selectedTile != null)
            {
                if (!characters.Contains(BattleTileManager.Instance.selectedTile))
                    characters.Add(BattleTileManager.Instance.selectedTile);
            }

            //List<ITargettable> characters = CharacterAgentsManager.Instance.allCharacters;

            if (characters != null)
            foreach (IAttackable c in characters)
            {
                if (c == null) continue;
                    IAttackable targetCharacter = c;// as CharacterAgentController;

                if (targetCharacter != null && targetCharacter != (cachedCaster as IAttackable))
                {
                    Vector3 delta = c.GetPosition() - currentPosition;
                    delta.y = 0;
                    if (targetCharacter.GetCollision(currentPosition, radius))
                    //if (delta.magnitude < radius + targetCharacter.GetColliderRadius())
                    {
                        float abilityFraction = 1;
                        //if (abilityPFX.AbilityMode == AbilityFXType.LASER)
                        //{
                        //    abilityFraction = Time.deltaTime;
                        //}

                        if (abilityPFX.AbilityMode == AbilityFXType.BOOMERANG)
                        {
                                if (--numRounds >= 0)
                                {
                                    OnCollision?.Invoke(currentPosition);
                                    //break;
                                }
                                else
                                {
                                    break;
                                }
                        }

                        if (abilityPFX.AbilityMode == AbilityFXType.PROJECTILE)
                        {
                            OnCollision?.Invoke(currentPosition + this.transform.forward * speed * 0.1f);

                            if (--numRounds <= 0)
                            {
                                timer = 0.1f;
                                IsAlive = false;
                            }
                        }

                        resetTime = abilityPFX.ResetTime;
                        resetTime = abilityPFX.Lifetime / (float)abilityPFX.NumImpacts;
                        roundsUsed++;
                        
                        //Debug.Log(roundsUsed.ToString() + "/" + maxRounds.ToString());

                        foreach (AbilityImpactDefinition effect in abilityPFX.AbilityEffects)
                        {
                            if (!effect.HasDelay)
                            {
                                GetAbilitiesManager().ApplyImpact(cachedCaster, targetCharacter, effect, abilityFraction);
                            }
                            else
                            {
                                targetCharacter.ApplyDelayedImpact(cachedCaster, effect);
                                //GetAbilitiesManager()?.StartCoroutine(cachedCaster.GetAbilitiesManager().ApplyAbilityEffectAfterDelay(cachedCaster, targetCharacter, effect));
                            }
                        }
                            //return;
                    }
                }
            }
        }
    }

    public void UpdateCastTarget(Vector3 target)
    {
        target.y = this.targetPos.y;
        this.targetPos = target;
    }

    public void OnCancel()
    {
        if (abilityPFX.CanCancelAbility)
        {
            timer = 0.1f;
        }
    }

    public void DestroyFX()
    {
        IsAlive = false;
        customFX?.DestroyFX(this.gameObject);
        if (customFX == null)
            GameObject.Destroy(this.gameObject, destroyDelay);
    }

    public void InitializePFX(AbilityProjectileItem abilityPFX, Vector3 position, Vector3 direction, float speed)
    {
        transform.position = position;
        transform.forward = direction;
        this.speed = speed;
        this.direction = direction;
        this.abilityPFX = abilityPFX;
        this.numRounds = abilityPFX.numRounds;

        OnCollision = delegate (Vector3 pos)
        {
            if (abilityPFX.ImpactFX != null)
            {
                GameObject impactFX = GameObject.Instantiate(abilityPFX.ImpactFX, pos, transform.rotation);
                float totalDuration = 0.5f;
                GameObject.Destroy(impactFX, totalDuration);
            }
        };
    }

    public void InitializePFX(AbilityProjectileItem abilityPFX, Transform sourceTr, Vector3 direction, float speed)
    {
        transform.position = sourceTr.position;
        this.muzzleGO = sourceTr;
        transform.forward = direction;
        this.speed = speed;
        this.direction = direction;
        this.abilityPFX = abilityPFX;
        this.numRounds = abilityPFX.numRounds;
        OnCollision = delegate (Vector3 pos)
        {
            if (abilityPFX.ImpactFX != null)
            {
                GameObject impactFX = GameObject.Instantiate(abilityPFX.ImpactFX, pos, transform.rotation);
                float totalDuration = 0.5f;
                GameObject.Destroy(impactFX, totalDuration);
            }
        };
    }

    // Start is called before the first frame update
    //void Start()
    //{
    //    InitializePFX();    
    //}
}
