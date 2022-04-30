using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Demos.RPGEditor;
using Sirenix.OdinInspector;
using System.Linq;

[System.Serializable]
public class MechCinematicAbility
{
    public AbilityProjectileItem ability;
    public ActionSequenceBehavior actionSequence;
    //public List<SequenceActionDef> sequenceActions;
}

public enum SequenceActionType
{
    WAIT,
    AIM,
    CAST,
    CHANNEL,
    MOVE
}

[System.Serializable]
public class SequenceActionDef
{
    //MoveAction move;
    //ICharacterAction action;
    public SequenceActionType actionType;
    public string strVal;
    public float numVal;
}

public interface IAbilityCardCaster
{
    ActionSequence SetupActionSequence(AbilityCard ab, TurnBasedAgent target, int tileID = -1);
    void CastAbilityCard(AbilityCard card, TurnBasedAgent target, int tileID = -1);
    void Tick();
    List<AbilityCard> GetAbilities();
    bool IsActionSequenceActive { get; }
    bool AllowPitchRotation { get; }
    bool ForceLocalAim { get; }

}

public class MechPartAbility : MonoBehaviour, IAbilityCardCaster
{
    //[HorizontalGroup("Stats", 0.5f, MarginLeft = 5, LabelWidth = 130)]

    [VerticalGroup("Modifiers")]
    public StatList StatModifiers;

    [VerticalGroup("Buffs")]
    public List<AbilityBuffModifier> BuffsModifiers;
    [VerticalGroup("Resistances")]
    public List<AbilityResistanceModifier> ResistanceModifiers;

    [VerticalGroup("Ability")]
    public List<AbilityCard> abilities;

    public bool forceLocalAim = false;
    public bool allowPitchRotation = true;
    //public ActionSequenceBehavior actionSequence;

    //[VerticalGroup("Abilities/Sequence")]
    //[BoxGroup(STATS_BOX_GROUP)]

    //[BoxGroup("ActionSequence")]
    //public ActionSequenceBehavior actionSequence;
    //public ActionSequence actionSequence;

    //[VerticalGroup("Sequence")]
    //public MechCinematicAbility cinematicAbilities;

    public ActionSequence SetupActionSequence(AbilityCard ab, TurnBasedAgent target, int tileID = -1)
    {
        ActionSequence seq = new ActionSequence();
        seq.actions = new List<ICharacterAction>();
        foreach (var action in ab.actionSequence.actionSequence.actions)
        {
            seq.actions.Add(action.Clone());
        }
        
        foreach (var action in seq.actions)
        {
            var caster = GetComponent<MechPartCaster>();
            (action as CharacterActionBase)?.SetCaster(GetComponent<MechPartCaster>());

            if (tileID >= 0)
            {
                var moveAction = (action as MoveAction);
                if (moveAction != null)
                {
                    var tile = BattleTileManager.Instance.GetTile(tileID);
                    var fromTile = caster.character.battleTurnManager.GetOccupiedTile();
                    fromTile.ClearTile();
                    var path = BattleTileManager.Instance.GetPath(fromTile, tile);
                    var positions = path.Select(p => p.GetPosition()).ToList();
                    //var path = fromTile.GetPath(tile);
                    moveAction.SetTarget(positions);
                    //tile.OccupyTile(caster.character.battleTurnManager);
                    caster.character.battleTurnManager.SetOccupiedTile(tile);
                    //moveAction.SetTarget(BattleTileManager.Instance.GetTile(tileID).transform);
                }
            }

            var castAction = (action as CastAction);
            if (castAction != null)
            {
                castAction.castTargetType = ab.castTarget;
                switch(ab.castTarget)
                {
                    case CastTarget.OPPONENT_CENTER:
                        castAction.SetTarget(caster.character.battleTurnManager.GetOpponentCenter());
                        break;
                    case CastTarget.EMPTY_TILE:
                    case CastTarget.SELECTED_TARGET:
                    default:
                        if (target != null)
                        {
                            var mech = (target as BattleTurnBasedAgent);
                            Vector3 delta = mech.GetHeadVisuals().transform.position - mech.GetGameObject().transform.position;
                            castAction.SetTarget((target as BattleTurnBasedAgent).GetGameObject(), delta / 2);
                        }
                        else if (tileID >= 0)
                        {
                            var tile = BattleTileManager.Instance.GetTile(tileID);
                            castAction.SetTarget(tile.gameObject, Vector3.zero);
                            if (ab.cardMode == AbilityCardType.CAST_AND_MOVE)
                            {
                                caster.character.battleTurnManager.SetOccupiedTile(tile);
                            }
                        }
                        break;
                }
                castAction.selectedAbility = ab.ability;
                castAction.channelTime = ab.ability.ChannelTime;
            }
        }

        return seq;
    }

    public void CastAbilityCard(AbilityCard card, TurnBasedAgent target, int tileID = -1)
    {
        activeSequence = SetupActionSequence(card, target, tileID);
        //activeSequence = card.actionSequence.actionSequence;
        activeSequence?.Trigger();
        sequenceActive = true;

        var sequence = this.gameObject.GetComponent<MechPartCameraSequence>();
        if (sequence != null)
        {
            CustomisationCameraSystem.Instance.PlaySequence(sequence);
        }
    }

    public void PreviewAction()
    {
        //foreach (var seq in abilities)
        //    SetupActionSequence(seq);

        //CastAbilityCard(abilities[0]);
    }

    private ActionSequence activeSequence = null;
    public bool IsActionSequenceActive { get { return sequenceActive; } }

    public bool AllowPitchRotation { get { return allowPitchRotation; } }

    public bool ForceLocalAim { get { return forceLocalAim; } }

    private bool sequenceActive = false;

    public void Tick()
    {
        if (activeSequence != null && activeSequence.Execute())
        {
            activeSequence = null;
            sequenceActive = false;
        }
    }

    public void Update()
    {
        Tick();    
    }

    public List<AbilityCard> GetAbilities()
    {
        return this.abilities;
    }
}

