using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Demos.RPGEditor;
using System.Linq;

public class OpponentTeamAIController : BattleTeamController
{
    private List<TurnBasedAgent> teamMembers { get { return team == BattleTeam.PLAYER ? battleManager.players : battleManager.opponents; } }

    private BattleManager battleManager;
    private BattleTeam team;

    private TurnBasedAgent selectedAgent;
    private bool isPlayingTurn = false;
    private bool isReady = false;
    public bool IsReady { get { return isReady; } }

    public OpponentTeamAIController(BattleManager battle, BattleTeam team)
    {
        this.RegisterBattle(battle, team);
    }

    public void RegisterBattle(BattleManager battle, BattleTeam team)
    {
        this.battleManager = battle;
        this.team = team;
        //this.teamMembers = team == BattleTeam.PLAYER ? battle.players : battle.opponents;
    }

    public List<TurnBasedAgent> GetTeamMembers()
    {
        return teamMembers;
    }

    public void SelectTeamMember(TurnBasedAgent agent)
    {
        this.selectedAgent = agent;
    }

    public void PlayMove()
    {
        isPlayingTurn = true;
        List<AbilityCard> cards = new List<AbilityCard>();

        List<TurnBasedAgent> availablePlayers = teamMembers.FindAll(t => t.CanPlayMove() && !t.IsBot());
        if (availablePlayers == null ||availablePlayers.Count == 0)
        {
            battleManager.CommitNoAbility();
            return;
        }

        int selectedMech = Random.Range(0, availablePlayers.Count);
        SelectTeamMember(availablePlayers[selectedMech]);
        
        //selectedAgent.PlayCardBot();

        if (!selectedAgent.PlayCardBot())
        {
            battleManager.CommitNoAbility();
        }
    }

    public void Update()
    {
        if (this.battleManager != null && this.battleManager.currentTurn == BattleTeam.OPPONENT && !isPlayingTurn)
        {

        }
    }

    public IEnumerator PlayBotsRoutine()
    {
        isReady = false;
        var mechs = GameObject.FindObjectsOfType<CharacterAgentController>().ToList();
        if (mechs != null && mechs.Count > 0)
        {
            var bots = mechs.FindAll(m => m.GetTeam() == CharacterTeam.OPPONENT && m.battleTurnManager.IsBot());
            foreach (var bot in bots)
            {
                bot.battleTurnManager.PlayCardBot();
                while (bot.battleTurnManager.IsCasting())
                {
                    yield return null;
                }
            }
        }

        isReady = true;
    }

    public void SetTurn()
    {
        //var mechs = GameObject.FindObjectsOfType<CharacterAgentController>().ToList();
        //battleManager.StartCoroutine(PlayBotsRoutine());
        /*
        if (mechs != null && mechs.Count > 0)
        {
            var mech = mechs.Find(m => m.GetTeam() == CharacterTeam.OPPONENT && m.battleTurnManager.IsBot());
            if (mech != null)
            {
                mech.battleTurnManager.PlayCardBot();
            }
        }
        */
    }
}
