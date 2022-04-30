using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Demos.RPGEditor;
using System.Linq;

public enum MatchState
{
    START,
    TURN_IN_PROGRESS
}

public enum BattleTeam
{
    PLAYER,
    OPPONENT
}

[System.Serializable]
public class MechColorScheme
{
    public float primaryHue = 0;
    public float secondaryHue = 0;
}

public interface BattleTeamController
{
    void RegisterBattle(BattleManager battle, BattleTeam team);
    List<TurnBasedAgent> GetTeamMembers();
    void SelectTeamMember(TurnBasedAgent agent);
    void PlayMove();
    void SetTurn();
    bool IsReady { get; }
}

public interface TurnBasedAgent
{
    BattleTeam GetTeam();
    BattleTile GetOccupiedTile();
    int GetOccupiedTileID();
    bool CanPlayMove();
    int GetPlayerIndex();
    void SetOccupiedTileID(int tileID);
    bool IsBot();
    float GetHP();
    bool IsMovable();
    float GetFP();
    List<AbilityCard> GetAbilityCards();
    bool PlayCardBot();
    bool PlayCard(AbilityCard card, TurnBasedAgent target, int tileID = -1);
    TurnBasedAgent GetTarget(AbilityCard card);
    IAbilityCaster GetCaster();

    void CommitAbility(AbilityCard ability, TurnBasedAgent target, int tileID = -1);

    void CommitTurn();
    void ResetForNextTurn();
    void RegisterBattle(BattleManager battle, BattleTeam team);
    void SaveToNetworkState();
    void LoadFromNetworkState();
    Vector3 GetCastFromPoint();
    Vector3 GetBasePoint();
    Vector3 GetPosition();
    void ApplyCardImpactDeterministic(TurnBasedAgent caster, BattleManager battle, AbilityCard card, TurnBasedAgent target, int tileID = -1);
    bool IsCasting();
}

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    public delegate void OnTurnCommitted();
    public OnTurnCommitted OnTurnPlayed;

    public MatchState matchState;
    public int maxFPPerTurn = 4;

    public int playerIndex = 0;
    public int opponentIndex = 0;

    public BattleTeam currentTurn = BattleTeam.PLAYER;

    public bool autoAssignColorSchemes = false;
    public MechColorScheme playerColorScheme;
    public MechColorScheme opponentColorScheme;

    public List<TurnBasedAgent> players;
    public List<TurnBasedAgent> opponents;

    private TurnBasedAgent lastSelectedTarget;
    private int lastSelectedTileID = -1;
    private BattleTile lastSelectedTile;

    public int moveIndex = 0;

    private int currentPlayerFP = 4;
    public int currentTurnNumber = 0;
    private BattleTeamController CurrentController { get { return  currentTurn == BattleTeam.OPPONENT ? opponentController : playerController;} }
    private BattleTeamController playerController;
    private BattleTeamController opponentController;
    public bool IsAbilityCardCasting { get; private set; }

    public void RemoveAgentFromBattle(TurnBasedAgent agent)
    {
        this.players.Remove(agent);
        this.opponents.Remove(agent);
    }

    private void ReplenishFP()
    {
        currentPlayerFP = maxFPPerTurn;
    }

    Coroutine playCardRoutine;
    public Vector3 GetOpponentCenter(TurnBasedAgent agent)
    {
        Vector3 center = Vector3.zero;

        if (agent.GetTeam() == BattleTeam.PLAYER)
        {
            foreach(var c in opponents)
            {
                center += c.GetCastFromPoint();
            }
            center /= opponents.Count;
        }
        else
        {
            foreach (var c in players)
            {
                center += c.GetCastFromPoint();
            }
            center /= opponents.Count;
        }
        return center;
    }

    public void SetLastSelectedTarget(TurnBasedAgent target, int tileID = -1)
    {
        this.lastSelectedTarget = target;
        this.lastSelectedTileID = tileID;
    }

    public IEnumerator PlayAbilityCardRoutine(TurnBasedAgent caster, AbilityCard card, TurnBasedAgent target, int tileID = -1)
    {
        IsAbilityCardCasting = true;
        if (!caster.IsBot())
            currentPlayerFP -= card.FPCost;
        //yield return new WaitForSeconds(card.castTime);
        yield return new WaitForSeconds(1f);

        while (caster.IsCasting())
        {
            yield return null;
        }
        yield return new WaitForSeconds(1f);

        //CustomisationCameraSystem.Instance.TriggerCamByID("default", 1f);

        EnforceDeterministicOutputs(caster, card, target, tileID);

        ResetAllAgentForNextMove();

        if (!caster.IsBot() && currentPlayerFP <= 0)
        {
            yield return CommitTurnRoutine();
        }

        IsAbilityCardCasting = false;

        yield return EvaluateNextMoveRoutine();
        //EvaluateNextMove();
    }

    public void EnforceDeterministicOutputs(TurnBasedAgent caster, AbilityCard card, TurnBasedAgent target, int tileID = -1)
    {
        caster.LoadFromNetworkState();

        if ((card.cardMode == AbilityCardType.MOVE || card.cardMode == AbilityCardType.CAST_AND_MOVE) && tileID >= 0)
        {
            //caster.LoadFromNetworkState();
            caster.SetOccupiedTileID(tileID);
            //caster.SaveToNetworkState();
            if (card.cardMode == AbilityCardType.MOVE)
            {
                caster.SaveToNetworkState();
                return;
            }
        }

        switch (card.castTarget)
        {
            /*
            case CastTarget.OPPONENT_CENTER:
            case CastTarget.MULTI_TARGET:
                if (caster.GetTeam() == BattleTeam.PLAYER)
                {
                    foreach (var opp in opponents)
                    {
                        opp.ApplyCardImpactDeterministic(caster, this, card, target);
                    }
                }
                else
                {
                    foreach (var pl in players)
                    {
                        pl.ApplyCardImpactDeterministic(caster, this, card, target);
                    }
                }
                break;
            case CastTarget.EMPTY_TILE:
                if (caster.GetTeam() == BattleTeam.PLAYER)
                {
                    foreach (var opp in opponents)
                    {
                        opp.ApplyCardImpactDeterministic(caster, this, card, target);
                    }
                }
                else
                {
                    foreach (var pl in players)
                    {
                        pl.ApplyCardImpactDeterministic(caster, this, card, target);
                    }
                }
                break;
            */
            case CastTarget.EMPTY_TILE:
            case CastTarget.SELECTED_TARGET:
            default:
                if (card.castTarget == CastTarget.SELECTED_TARGET)
                {
                    tileID = -1;
                }
                if (card.castTarget == CastTarget.EMPTY_TILE)
                {
                    target = null;
                }

                if (caster.GetTeam() == BattleTeam.PLAYER)
                {
                    foreach (var opp in opponents)
                    {
                        opp.ApplyCardImpactDeterministic(caster, this, card, target, tileID);
                    }
                }
                else
                {
                    foreach (var pl in players)
                    {
                        pl.ApplyCardImpactDeterministic(caster, this, card, target, tileID);
                    }
                }
                //target?.ApplyCardImpactDeterministic(caster, this, card, target);
                break;
        }
        caster.SaveToNetworkState();
    }

    public void ResetAllAgentForNextMove()
    {
        foreach (var a in players)
        {
            a.ResetForNextTurn();
        }

        foreach (var a in opponents)
        {
            a.ResetForNextTurn();
        }
    }

    public IEnumerator EvaluateNextMoveRoutine()
    {
        yield return null;

        bool botCasting = true;
        while (botCasting)
        {
            botCasting = false;
            var mechs = GameObject.FindObjectsOfType<CharacterAgentController>().ToList();
            mechs.Find(m => m.GetTeam() == CharacterTeam.PLAYER && m.battleTurnManager.IsBot() && m.IsAlive());
            if (mechs != null && mechs.Count > 0)
            {
                foreach(var m in mechs)
                {
                    if (m.IsCasting)
                    {
                        botCasting = true;
                        //break;
                    }
                }
            }
            yield return null;
        }

        if (currentPlayerFP > 0)
        {
            CurrentController?.PlayMove();
        }
        moveIndex++;
    }

    Coroutine nextMoveRoutine;
    public void EvaluateNextMove()
    {
        //nextMoveRoutine = StartCoroutine(EvaluateNextMoveRoutine());
        
        if (currentPlayerFP > 0)
        {
            CurrentController?.PlayMove();
        }
        moveIndex++;
        
    }

    public IEnumerator CommitNoAbilityRoutine()
    {
        IsAbilityCardCasting = false;

        currentPlayerFP -= 1;

        ResetAllAgentForNextMove();

        if (currentPlayerFP <= 0)
        {
            yield return CommitTurnRoutine();
        }

        yield return EvaluateNextMoveRoutine();
    }

    public void CommitNoAbility()
    {
        StartCoroutine(CommitNoAbilityRoutine());
        /*
        IsAbilityCardCasting = false;
        
        currentPlayerFP -= 1;

        ResetAllAgentForNextMove();

        if (currentPlayerFP <= 0)
        {
            CommitTurn();
        }

        StartCoroutine(EvaluateNextMoveRoutine());
        */
    }

    public void CommitAbility(TurnBasedAgent caster, AbilityCard card, TurnBasedAgent target, int tileID = -1)
    {
        //SetLastSelectedTarget(target, tileID);
        playCardRoutine = StartCoroutine(PlayAbilityCardRoutine(caster, card, target, tileID));
        //currentPlayerFP -= card.FPCost;
        //if (currentPlayerFP <= 0)
        //{
        //    CommitTurn();
        //}
    }

    bool isTeamReady = false;
    public IEnumerator PlayBotsRoutine(CharacterTeam team)
    {
        isTeamReady = false;
        var mechs = GameObject.FindObjectsOfType<CharacterAgentController>().ToList();
        if (mechs != null && mechs.Count > 0)
        {
            var bots = mechs.FindAll(m => m.GetTeam() == team && m.battleTurnManager.IsBot() && m.IsAlive());
            foreach (var bot in bots)
            {
                bot.battleTurnManager.PlayCardBot();
                while (bot.battleTurnManager.IsCasting())
                {
                    yield return null;
                }
            }
        }

        isTeamReady = true;
    }

    public bool CanPlayMove(BattleTeam team)
    {
        return currentTurn == team && currentPlayerFP > 0 && !IsAbilityCardCasting;
    }

    public IEnumerator CommitTurnRoutine()
    {
        if (currentTurn == BattleTeam.PLAYER)
        {
            yield return PlayBotsRoutine(CharacterTeam.PLAYER);

            currentTurn = BattleTeam.OPPONENT;
            playerController.SetTurn();
            //while (!playerController.IsReady)
            //{
            //    yield return null;
            //}

        }
        else
        {
            yield return PlayBotsRoutine(CharacterTeam.OPPONENT);

            currentTurn = BattleTeam.PLAYER;
            opponentController.SetTurn();
            //while (!opponentController.IsReady)
            //{
            //    yield return null;
            //}
        }
        currentTurnNumber++;
        ReplenishFP();
    }

    public void CommitTurn()
    {
        //StartCoroutine(CommitTurnRoutine());
        {
            if (currentTurn == BattleTeam.PLAYER)
            {
                currentTurn = BattleTeam.OPPONENT;
                playerController.SetTurn();
            }
            else
            {
                currentTurn = BattleTeam.PLAYER;
                opponentController.SetTurn();
            }
            currentTurnNumber++;
            ReplenishFP();
        }
    }

    public void StartMatch(bool aiOpponent = true)
    {
        //GameManager.Instance.SetGameMode(GameMode.BATTLE);
        matchState = MatchState.TURN_IN_PROGRESS;
        currentPlayerFP = maxFPPerTurn;

        playerController = new PlayerTeamController(this, BattleTeam.PLAYER);
        
        if (aiOpponent)
            opponentController = new OpponentTeamAIController(this, BattleTeam.OPPONENT);
        else
            opponentController = new PlayerTeamController(this, BattleTeam.OPPONENT);


        foreach (var mech in players)
        {
            mech.RegisterBattle(this, BattleTeam.PLAYER);
            mech.SaveToNetworkState();
        }

        foreach (var mech in opponents)
        {
            mech.RegisterBattle(this, BattleTeam.OPPONENT);
            mech.SaveToNetworkState();
        }
    }

    public void InitializeFromScene()
    {
        players = new List<TurnBasedAgent>();
        foreach (var c in CharacterAgentsManager.Instance.characters[CharacterTeam.PLAYER])
        {
            var agent = c as CharacterAgentController;
            if (agent != null)
            {
                players.Add(agent.battleTurnManager);
                if (autoAssignColorSchemes)
                    agent.mech.ApplyColorScheme(this.playerColorScheme);
            }
        }

        opponents = new List<TurnBasedAgent>();
        foreach (var c in CharacterAgentsManager.Instance.characters[CharacterTeam.OPPONENT])
        {
            var agent = c as CharacterAgentController;
            if (agent != null)
            {
                opponents.Add(agent.battleTurnManager);
                if (autoAssignColorSchemes)
                    agent.mech.ApplyColorScheme(this.opponentColorScheme);
            }
        }

        //StartMatch();
    }

    public void SetupAndStartMatch(bool aiOpponent = true)
    {
        InitializeFromScene();
        StartMatch(aiOpponent);
    }

    public void ForceStartMatch()
    {
        InitializeFromScene();
        StartMatch(true);
    }

    public void Awake()
    {
        Instance = this;
        //Invoke("ForceStartMatch", 1f);
        //InitializeFromScene();
    }

}
