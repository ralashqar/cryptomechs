//#if UNITY_EDITOR
using System.Collections.Generic;
namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    using System;
    using UnityEngine;

    // 
    // CharacterStats is simply a StatList, that expose the relevant stats for a character.
    // Also note that the StatList might look like a dictionary, in how it's used, 
    // but it's actually just a regular list, serialized by Unity. Take a look at the StatList to learn more.
    // 

    [Serializable]
    public class CharacterStats
    {
        [HideInInspector]
        public StatList Stats = new StatList();

        [ProgressBar(0, 100), ShowInInspector]
        public float Shooting
        {
            get { return this.Stats[StatType.Shooting]; }
            set { this.Stats[StatType.Shooting] = value; }
        }

        [ProgressBar(0, 100), ShowInInspector]
        public float Melee
        {
            get { return this.Stats[StatType.Melee]; }
            set { this.Stats[StatType.Melee] = value; }
        }

        [ProgressBar(0, 100), ShowInInspector]
        public float Animals
        {
            get { return this.Stats[StatType.Animals]; }
            set { this.Stats[StatType.Animals] = value; }
        }

        [ProgressBar(0, 100), ShowInInspector]
        public float Medicine
        {
            get { return this.Stats[StatType.Medicine]; }
            set { this.Stats[StatType.Medicine] = value; }
        }

        [ProgressBar(0, 100), ShowInInspector]
        public float Crafting
        {
            get { return this.Stats[StatType.Crafting]; }
            set { this.Stats[StatType.Crafting] = value; }
        }

        [ProgressBar(0, 100), ShowInInspector]
        public float Prestige
        {
            get { return this.Stats[StatType.Prestige]; }
            set { this.Stats[StatType.Prestige] = value; }
        }

        [ProgressBar(0, 100), ShowInInspector]
        public float Defense
        {
            get { return this.Stats[StatType.Defense]; }
            set { this.Stats[StatType.Defense] = value; }
        }

        public List<AbilityBuffModifier> StartingBuffModifiers;
        public List<AbilityResistanceModifier> StartingResistanceModifiers;
    }
}
//#endif
