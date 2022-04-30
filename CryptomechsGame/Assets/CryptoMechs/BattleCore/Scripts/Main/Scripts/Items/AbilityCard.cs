//#if UNITY_EDITOR
using UnityEngine;
//using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    public enum AbilityCardType
    {
        ABILITY,
        MOD,
        MOVE,
        CAST_AND_MOVE
    }

    public enum AbilityEvent
    {
        ON_CAST_START,
        ON_PROJECTILE_LAUNCH,
        ON_PROJECTILE_IMPACT
    }

    public enum AbilityCamera
    {
        DEFAULT_BATTLE,
        OPPONENTS_CLOSEUP,
        PLAYERS_CLOSEUP
    }

    public class AbilityCameraDefinition
    {
        public float timeOffset = 0;
        public float blendTime = 1f;
        public AbilityEvent abilityEvent;
        public AbilityCamera abilityCam;
    }

    public class AbilityCard : ItemBase
    {
        [BoxGroup(STATS_BOX_GROUP)]
        [BoxGroup(STATS_BOX_GROUP + "/AbilityCard")]
        public AbilityCardType cardMode;
        [BoxGroup(STATS_BOX_GROUP + "/AbilityCard")]
        public AbilityProjectileItem ability;
        [BoxGroup(STATS_BOX_GROUP + "/AbilityCard")]
        public CastTarget castTarget;
        [BoxGroup(STATS_BOX_GROUP + "/AbilityCard")]
        public ActionSequenceBehavior actionSequence;
        [BoxGroup(STATS_BOX_GROUP + "/AbilityCard")]
        public int FPCost = 1;
        [BoxGroup(STATS_BOX_GROUP + "/AbilityCard")]
        public int range = 3;
        [BoxGroup(STATS_BOX_GROUP + "/AbilityCard")]
        public bool forceStraightLine = false;

        [BoxGroup(STATS_BOX_GROUP + "/AbilityCamera")]
        public List<AbilityCameraDefinition> abilityCams;

        [VerticalGroup(LEFT_VERTICAL_GROUP + "/Modifiers")]
        public StatList Modifiers;

        public override ItemTypes[] SupportedItemTypes
        {
            get
            {
                return new ItemTypes[]
                {
                    ItemTypes.AbilityCard
                };
            }
        }
    }
}
//#endif
