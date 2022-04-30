using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Demos.RPGEditor;

[System.Serializable]
public class CharacterNetworkState : MonoBehaviour
{
    public int turnNum;
    public int moveNumber;
    public int health;
    public List<AbilityBuffTemp> appliedBuffs;
    public List<DelayedImpact> delayedImpacts;
    public int occupiedTileID = 0;

    public static void ApplyAbilityCardToTargetDeterministic(CharacterNetworkState initialState, IAbilityCaster caster, BattleManager battle, CharacterAgentController agent, AbilityCard card)
    {
        SetCharacterState(agent, initialState);

        int damage = 0;
        AbilityProjectileItem ab = card.ability;

        int numRounds = 1;
        switch (ab.AbilityMode)
        {
            case AbilityFXType.LASER:
                numRounds = (int)(ab.Lifetime / ab.ResetTime);
                break;
            case AbilityFXType.BOOMERANG:
                numRounds = 1;
                break;
            case AbilityFXType.PROJECTILE:
                float channelTime = ab.ChannelTime;
                float channelTimer = 1f / ab.ChannelRate;
                numRounds = (int)(channelTime / channelTimer);
                if (ab.UseProceduralSpread && ab.SpreadType == AbilitySpreadType.BARREL_POINTS)
                {
                    numRounds *= ab.NumberOfRounds;
                }
                break;
            default:
                break;
        }

        for (int i = 0; i < numRounds; ++i)
        {
            foreach (AbilityImpactDefinition impact in ab.AbilityEffects)
            {
                AbilitiesManager.ApplyImpact(caster, agent, impact, true);
            }
        }

    }

    public static void SetCharacterState(CharacterAgentController agent, CharacterNetworkState state)
    {
        agent.healthManager.healthPoints = state.health;
        agent.buffsManager.appliedBuffs = state.appliedBuffs;
        agent.buffsManager.timedImpacts.delayedImpacts = state.delayedImpacts;
        agent.battleTurnManager.SetOccupiedTile(BattleTileManager.Instance.GetTile(state.occupiedTileID));
    }

    public CharacterNetworkState(BattleManager battle, CharacterAgentController agent)
    {
        this.turnNum = battle.currentTurnNumber;
        this.health = (int)Mathf.Floor(agent.GetHP());
    }

}
