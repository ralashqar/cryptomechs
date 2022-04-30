//#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    public abstract class EquipableItem : Item
    {
        [BoxGroup(STATS_BOX_GROUP)]
        public float Durability;

        [VerticalGroup(LEFT_VERTICAL_GROUP + "/Modifiers")]
        public StatList Modifiers;

        [VerticalGroup(LEFT_VERTICAL_GROUP + "/Modifiers")]
        public List<AbilityBuffModifier> BuffModifiers;

        [VerticalGroup(LEFT_VERTICAL_GROUP + "/Modifiers")]
        public List<AbilityResistanceModifier> ResistanceModifiers;
    }
}
//#endif
