using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Demos.RPGEditor;
using System.Linq;

public class PlayerTeamController : BattleTeamController
{
    private List<TurnBasedAgent> teamMembers;

    private BattleManager battleManager;

    private TurnBasedAgent selectedAgent;
    private bool isPlayingTurn = false;

    private bool isReady = false;
    public bool IsReady { get { return isReady; } }

    public PlayerTeamController(BattleManager battle, BattleTeam team)
    {
        this.RegisterBattle(battle, team);
    }

    public void RegisterBattle(BattleManager battle, BattleTeam team)
    {
        this.battleManager = battle;
        this.teamMembers = team == BattleTeam.PLAYER ? battle.players : battle.opponents;
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
    }

    public IEnumerator PlayBotsRoutine()
    {
        isReady = false;
        var mechs = GameObject.FindObjectsOfType<CharacterAgentController>().ToList();
        if (mechs != null && mechs.Count > 0)
        {
            var bots = mechs.FindAll(m => m.GetTeam() == CharacterTeam.PLAYER && m.battleTurnManager.IsBot());
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
        var mechs = GameObject.FindObjectsOfType<CharacterAgentController>().ToList();
        var mech = mechs.Find(m => m.GetTeam() == CharacterTeam.PLAYER && m.battleTurnManager.IsBot());
        if (mech != null)
        {
            mech.battleTurnManager.PlayCardBot();
        }
        */
    }
}
