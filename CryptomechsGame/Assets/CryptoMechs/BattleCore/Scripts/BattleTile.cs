using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public class BattleTile : MonoBehaviour, IAttackable, ITargettable
{
    public HexTileBase tileBase;
    public TimedImpactComponent timedImpacts;
    public RenderFXComponent meshFX;

    private TurnBasedAgent occupyingAgent;
    public bool isOccupied = false;

    public List<BattleTile> neighborTiles;

    //public List<Vector3> GetPath(BattleTile target)
    //{
        //var path = tileBase.GetClosestPathDjikstra(target.tileBase);
        //return path.Select(p => p.transform.position).ToList();
    //}

    public bool IsTraversible { get { return !isOccupied; } }
    
    public void GeneratePaths()
    {
        tileBase.GeneratePathsDjikstra();
    }

    public void ClearTile()
    {
        this.occupyingAgent = null;
        isOccupied = false;
        tileBase.traversible = IsTraversible;
    }

    public void OccupyTile(TurnBasedAgent agent)
    {
        this.occupyingAgent = agent;
        isOccupied = true;
        tileBase.traversible = IsTraversible;
    }

    //public List<BattleTile> cachedPath;
    public void SetupTileBase()
    {
        tileBase = GetComponent<HexTileBase>();
    }

    public List<BattleTile> GetNeighborTiles()
    {
        return this.neighborTiles;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void SetID(int id)
    {
        if (tileBase == null)
            SetupTileBase();
        if (tileBase == null)
            return;
        tileBase.id = id;

        if (!tileBase.isActive)
        {
            tileBase.traversible = false;
            tileBase.gameObject.SetActive(false);
        }
    }

    public int GetID()
    {
        return tileBase != null ? tileBase.GetID() : -1;
    }

    public void Update()
    {
        Tick();
    }

    public CharacterTeam GetTeam()
    {
        throw new System.NotImplementedException();
    }

    public float GetAbilityResistanceModifier(AbilityImpactType impactType)
    {
        return 1;
    }

    public void ReceiveImpact(IAbilityCaster caster, ImpactDefinition impact, bool applyWithoutVisuals = false)
    {
        switch (impact.impactType)
        {
            case AbilityImpactType.BASH:
                return;
                break;
            case AbilityImpactType.FREEZE:
                AbilityBuffModifier buff = new AbilityBuffModifier();
                buff.buffType = AbilityImpactType.FREEZE;
                buff.buffValue = impact.impactValue;
                var b = new AbilityBuffTemp(buff, impact);
                b.applyType = AbilityApplyType.IMPACT;
                //this.timedImpacts.AddBuff(b);
                break;
            default:
                break;
        }

        if (meshFX == null)
            this.meshFX = new RenderFXComponent(this);

        if (!applyWithoutVisuals && impact.impactFX != null && impact.impactFX.useRenderFx)
        {
            this.meshFX.ReceiveImpact(impact.impactFX);
        }
    }

    public bool GetCollision(Vector3 point, float radius)
    {
        Vector3 delta = point - GetPosition();
        if (delta.magnitude < radius + GetColliderRadius())
        {
            return point.y - radius < GetPosition().y; 
        }
        return false;
    }

    public float GetColliderRadius()
    {
        return 0.3f;
        return this.tileBase.dimension / 2;
    }

    public void ApplyDelayedImpact(IAbilityCaster caster, AbilityImpactDefinition effect, float abilityFraction = 1)
    {
        this.timedImpacts.ApplyDelayedImpact(caster, effect, abilityFraction);
    }

    public void ApplyImpactFX(ImpactFXDefinition impactFX, GameObject target, bool parentToTarget = true)
    {
        if (this.timedImpacts == null) this.timedImpacts = new TimedImpactComponent(this);
        this.timedImpacts.ApplyImpactFX(impactFX, target, parentToTarget);
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }

    public Transform GetTransform()
    {
        return this.transform;
    }

    public Vector3 GetForward()
    {
        return Vector3.up;
    }

    public bool IsAlive()
    {
        return true;
    }

    public void Tick()
    {
        this.timedImpacts?.UpdateTimeBasedEffects();
        this.meshFX?.UpdateImpactFXs();
    }

    public void Initialize()
    {
        
    }

    public TimedImpactComponent GetTimedImpactComponent()
    {
        return timedImpacts;
    }
}
