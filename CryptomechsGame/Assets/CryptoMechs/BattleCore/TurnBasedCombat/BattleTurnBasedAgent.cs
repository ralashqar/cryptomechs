using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Demos.RPGEditor;

public class BattleTurnBasedAgent : TurnBasedAgent
{
    private BattleTeam team;
    private BattleManager battleManager;
    private CharacterAgentController character;
    private Vector3 initPos;
    private Quaternion initRot;
    private int tileID = -1;
    private BattleTile occupiedTile;
    private bool isBot = false;

    public int playerIndex = 0;
    public CharacterNetworkState networkState;

    public GameObject GetHeadVisuals()
    {
        return character.GetHeadVisuals();
    }

    public GameObject GetGameObject()
    {
        return character.GetGameObject();
    }

    public void SaveToNetworkState()
    {
        if (battleManager == null)
            battleManager = BattleManager.Instance;
        
        if (battleManager == null) return;

        if (this.networkState == null)
        {
            this.networkState = new CharacterNetworkState(battleManager, character);
        }
        this.networkState.turnNum = battleManager.currentTurnNumber;
        this.networkState.moveNumber = battleManager.moveIndex;
        this.networkState.health = (int)Mathf.Floor(character.GetHP());
        this.networkState.appliedBuffs = character.buffsManager.appliedBuffs;
        this.networkState.delayedImpacts = character.buffsManager.timedImpacts.delayedImpacts;
        this.networkState.occupiedTileID = GetOccupiedTileID();
        Debug.Log("SaveToNetworkState " + this.playerIndex.ToString() + " : " + GetOccupiedTileID().ToString());
    }

    public Vector3 GetOpponentCenter()
    {
        return battleManager.GetOpponentCenter(this);
    }

    public Vector3 GetCastFromPoint()
    {
        return initPos + initRot * Vector3.forward * 3f;
    }

    public Vector3 GetBasePoint()
    {
        return initPos;
    }

    public BattleTurnBasedAgent(CharacterAgentController character)
    {
        this.character = character;
        this.initPos = character.GetPosition();
        this.initRot = character.GetTransform().rotation;
        this.isBot = character.isBot;
        InitializeTile();
        SaveToNetworkState();
    }

    public BattleTeam GetTeam()
    {
        return team;
    }

    public float GetHP()
    {
        return character.GetHP();
    }

    public float GetFP()
    {
        return 1;
    }

    public bool IsBot()
    {
        return isBot;
    }

    public void TriggerDeath()
    {
        this.occupiedTile?.ClearTile();
        this.occupiedTile = null;
        this.battleManager.RemoveAgentFromBattle(this);
    }

    public bool PlayCardBot()
    {
        var cards = GetAbilityCards();
        int tileID = -1;

        bool foundCard = false;

        if (cards.Count > 0)
        {
            foundCard = true;
            var card = cards[Random.Range(0, cards.Count)];
            var enemy = GetTarget(card);
            bool isSummon = card.ability.AbilityEffects.Exists(im => im.impactType == AbilityImpactType.SUMMON);
            if (isSummon || card.cardMode == AbilityCardType.CAST_AND_MOVE)
            {
                foundCard = false;
                GetOccupiedTile()?.GeneratePaths();
                var enemyTile = (enemy as BattleTurnBasedAgent).GetOccupiedTile();
                foreach (var t in enemyTile.GetNeighborTiles())
                {
                    if (!t.isOccupied)
                    {
                        var path = BattleTileManager.Instance.GetPath(GetOccupiedTile(), t);
                        int distance = path.Count;
                        bool canTraverse = !card.forceStraightLine || (card.forceStraightLine && BattleTileManager.Instance.GetIsStraightClearPath(GetOccupiedTile(), path));
                        if (canTraverse && distance <= card.range)
                        {
                            tileID = t.GetID();
                            enemy = null;
                            foundCard = true;
                            break;
                        }
                    }
                }
            }
            //battleManager.SetLastSelectedTarget(enemy);
            
            if (foundCard)
                PlayCard(card, enemy, tileID);
        }

        return foundCard;
    }

    public bool IsMovable()
    {
        return !this.character.isImmovable;
    }

    public bool Bash(IAbilityCaster caster, ImpactDefinition impact)
    {
        if (!IsMovable()) return false;

        Vector3 direction = this.character.GetPosition() - caster.GetPosition();
        direction.y = 0;
        var neigbors = this.GetOccupiedTile().GetNeighborTiles();
        var freeNeighbors = neigbors.FindAll(n => !n.isOccupied);
        if (freeNeighbors == null || freeNeighbors.Count == 0)
        {
            return false;
        }
        
        var targetTile = freeNeighbors[0];
        
        var closestTile = freeNeighbors[0];
        float closestD = float.MinValue;
        foreach (var t in freeNeighbors)
        {
            float d = Vector3.Distance(t.GetPosition(), caster.GetPosition());
            if (d > closestD)
            {
                closestD = d;
                closestTile = t;
            }
        }

        if (closestTile == GetOccupiedTile())
            return false;

        Debug.Log("Bash " + this.playerIndex.ToString() + " from tile: " + GetOccupiedTileID().ToString() +" to tile: " + closestTile.tileBase.GetID().ToString());
        this.SetOccupiedTile(closestTile);
        return true;
    }

    public void CommitAbility(AbilityCard card, TurnBasedAgent target, int tileID = -1)
    {
        battleManager?.CommitAbility(this, card, target, tileID);
    }

    public void CommitTurn()
    {

    }

    public void RegisterBattle(BattleManager battle, BattleTeam team)
    {
        this.team = team;
        this.battleManager = battle;
    }

    public void InitializeTile()
    {
        BattleTile closestTile = null;
        float closestD = float.MaxValue;
        foreach (var t in BattleTileManager.Instance.tiles)
        {
            float d = Vector3.Distance(this.character.GetPosition(), t.GetPosition());
            if (d < closestD)
            {
                closestD = d;
                closestTile = t;
            }
        }

        occupiedTile = closestTile;
        this.tileID = closestTile.GetID();
        closestTile?.OccupyTile(this);
        this.character.mover.ForcePosition(closestTile.transform.position);
    }

    public List<AbilityCard> GetAbilityCards()
    {
        List<AbilityCard> cards = new List<AbilityCard>();
        foreach (var c in character.mech.casters)
        {
            IAbilityCardCaster ma = c.partAbilities != null ? c.partAbilities : c.gameObject.GetComponent<IAbilityCardCaster>();
            if (ma != null)
            {
                cards.AddRange(ma.GetAbilities());
            }
        }

        return cards;
    }

    public TurnBasedAgent GetTarget(AbilityCard card)
    {
        var agent = CharacterAgentsManager.FindClosestTargettableEnemy(this.character.GetPosition(), this.character.GetTeam()) as CharacterAgentController;
        
        /*
        if (card.cardMode == AbilityCardType.CAST_AND_MOVE)
        {
            //return null;
            this.GetOccupiedTile()?.GeneratePaths();
            var enemyTile = agent.battleTurnManager.GetOccupiedTile();
            foreach(var t in enemyTile.GetNeighborTiles())
            {
                if (!t.isOccupied)
                {
                    int distance = BattleTileManager.Instance.GetDistance(GetOccupiedTile(), t);
                    if (distance <= card.range)
                    {

                    }
                }
            }
        }
        */
        if (agent != null)
            return agent.battleTurnManager;
        return null;
    }

    public bool ValidateMove(AbilityCard card, TurnBasedAgent target, int tileID = -1)
    {
        this.GetOccupiedTile()?.GeneratePaths();
        if (card.cardMode == AbilityCardType.MOVE)
        {
            if (tileID < 0) return false;
            var targetTile = BattleTileManager.Instance.GetTile(tileID);
            int distance = BattleTileManager.Instance.GetDistance(GetOccupiedTile(), targetTile);
            if (!targetTile.IsTraversible) return false;
            if (distance <= card.range)
            {
                return true;
            }
        }
        else
        {
            if (target == null && tileID < 0) return false;
            var targetTile = target != null ? target.GetOccupiedTile() : BattleTileManager.Instance.GetTile(tileID);
            //int distance = BattleTileManager.Instance.GetDistance(GetOccupiedTile(), targetTile);
            var path = BattleTileManager.Instance.GetPath(GetOccupiedTile(), targetTile);
            int distance = path.Count;
            bool canTraverse = !card.forceStraightLine || (card.forceStraightLine && BattleTileManager.Instance.GetIsStraightClearPath(GetOccupiedTile(), path));
            bool canTargetTile = !targetTile.isOccupied || (card.castTarget == CastTarget.SELECTED_TARGET && targetTile.isOccupied);
            if (distance <= card.range && canTraverse && canTargetTile)
            {
                return true;
            }
        }

        return false;
    }

    public bool PlayCardPlayer(AbilityCard card, TurnBasedAgent target, int tileID = -1)
    {
        if (!character.IsAlive())
            return false;

        if (!battleManager.CanPlayMove(this.GetTeam()))
            return false;

        if (!ValidateMove(card, target, tileID))
        {
            Debug.Log("INVALID MOVE");
            return false;
        }

        if (card.cardMode == AbilityCardType.MOVE)
        {
            var legs = character.mech.GetCasterBySlot(MechSlot.LEGS);
            legs.CastAbilityCard(card, target, tileID);
            int casterIndex = character.battleTurnManager.GetPlayerIndex();
            MechTeamNetworkController.SendMove(casterIndex, (tileID));
            //this.battleManager.SetLastSelectedTarget(target, tileID);
            return true;
        }

        foreach (var c in character.mech.casters)
        {
            if (c.partAbilities != null && c.partAbilities.GetAbilities().Contains(card))
            {
                c.CastAbilityCard(card, target, tileID);

                int casterIndex = -1;
                int targetIndex = -1;
                int abilityIndex = c.partAbilities.GetAbilities().IndexOf(card);

                //MechPlacementNode casterNode = character.GetComponentInParent<MechPlacementNode>();
                //casterIndex = (int)casterNode.placement + 1;
                casterIndex = character.battleTurnManager.GetPlayerIndex();

                if (target != null)
                {
                    //MechPlacementNode targetNode = (target as BattleTurnBasedAgent).character.GetComponentInParent<MechPlacementNode>();
                    //targetIndex = (int)targetNode.placement + 1;
                    targetIndex = target.GetPlayerIndex();
                }

                MechTeamNetworkController.SendNetworkAbility(casterIndex, (int)c.slot, abilityIndex, targetIndex, tileID);
                return true;
            }
        }
        return false;
    }

    public int GetPlayerIndex()
    {
        return this.playerIndex;
    }

    public bool PlayCard(AbilityCard card, TurnBasedAgent target, int tileID = -1)
    {
        if (character.IsIncapacitated()) return false;
        
        //this.battleManager.SetLastSelectedTarget(target, tileID);
        foreach (var c in character.mech.casters)
        {
            if (c.partAbilities != null && c.partAbilities.GetAbilities().Contains(card))
            {
                c.CastAbilityCard(card, target, tileID);
                return true;
            }
        }
        return false;
    }

    public void ResetForNextTurn()
    {
        this.character.AddState(new IdleState(this.character));
        //if (occupiedTile != null)
        //    this.character.mover.SetTarget(this.occupiedTile.transform.position, MoveMode.DEFAULT);
        //this.character.mover.SetTarget(GetBasePoint(), MoveMode.BACKWARDS);
    }

    public Vector3 GetPosition()
    {
        return character.GetPosition();
    }

    public IAbilityCaster GetCaster()
    {
        return this.character;
    }

    public bool IsCasting()
    {
        return character.IsActionSequenceActive || character.IsCasting || character.HasActiveProjectiles;
    }

    public void ApplyCardImpactDeterministic(TurnBasedAgent caster, BattleManager battle, AbilityCard card, TurnBasedAgent target, int tileID = -1)
    {
        //Debug.Log("Deterministic health before: " + networkState.health);
        LoadFromNetworkState();

        int damage = 0;
        AbilityProjectileItem ab = card.ability;

        int numRounds = AbilitiesManager.GetMaxRounds(ab);

        Vector3 position = GetOccupiedTile().GetPosition();
        Vector3 prevPos = GetPosition();
        this.character.mover.ForcePosition(position);
        //Debug.Log("Deterministic num rounds: " + numRounds);

        for (int i = 0; i < numRounds; ++i)
        {
            foreach (AbilityImpactDefinition impact in ab.AbilityEffects)
            {
                bool canApplyImpact = (tileID < 0 && target != null && (target == this || Vector3.Distance(this.GetPosition(), target.GetPosition()) <= impact.areaOfEffect))
                    || (tileID >= 0 && Vector3.Distance(this.GetPosition(), BattleTileManager.Instance.GetTile(tileID).GetPosition()) <= impact.areaOfEffect);
                
                if (canApplyImpact)
                {
                    if (!impact.HasDelay)
                    {
                        AbilitiesManager.ApplyImpact(caster.GetCaster(), character, impact, true);
                    }
                    else
                    {
                        character.ApplyDelayedImpact(caster.GetCaster(), impact);
                    }
                }
            }
        }

        this.character.mover.ForcePosition(prevPos);

        SaveToNetworkState();
        //Debug.Log("Deterministic health after: " + networkState.health);

    }

    public void LoadFromNetworkState()
    {
        character.healthManager.healthPoints = networkState.health;
        character.buffsManager.appliedBuffs = networkState.appliedBuffs;
        character.buffsManager.timedImpacts.delayedImpacts = networkState.delayedImpacts;
        SetOccupiedTile(BattleTileManager.Instance.GetTile(networkState.occupiedTileID));
        Debug.Log("LoadFromNetworkState " + this.playerIndex.ToString() + " : " + networkState.occupiedTileID.ToString());
    }

    public int GetTileID()
    {
        return tileID;
    }

    public BattleTile GetOccupiedTile()
    {
        return occupiedTile;
    }

    public int GetOccupiedTileID()
    {
        return occupiedTile != null ? occupiedTile.GetID() : -1;
    }

    public void SetOccupiedTile(BattleTile tile)
    {
        this.occupiedTile?.ClearTile();
        this.occupiedTile = tile;
        tile.OccupyTile(this);
    }

    public void SetOccupiedTileID(int tileID)
    {
        if (this.tileID >= 0)
        {
            BattleTileManager.Instance.GetTile(this.tileID).ClearTile();
        }
        this.tileID = tileID;
        var tile = BattleTileManager.Instance.GetTile(tileID);
        SetOccupiedTile(tile);
        //BattleTileManager.Instance.GetTile(tileID).OccupyTile(this);
    }

    public bool CanPlayMove()
    {
        return !this.character.IsIncapacitated();
    }
}
