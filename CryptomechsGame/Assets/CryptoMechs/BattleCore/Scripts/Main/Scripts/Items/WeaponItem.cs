//#if UNITY_EDITOR
using UnityEngine;

namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    public class WeaponItem : EquipableItem
    {
        [BoxGroup(STATS_BOX_GROUP)]
        public AbilityImpactType BaseDamageType;

        [BoxGroup(STATS_BOX_GROUP)]
        public float BaseAttackDamage;

        [BoxGroup(STATS_BOX_GROUP)]
        public float BaseAttackSpeed;

        [BoxGroup(STATS_BOX_GROUP)]
        public float BaseCritChance;

        [BoxGroup(STATS_BOX_GROUP)]
        public float MeleeRange;

        [BoxGroup(STATS_BOX_GROUP + "/Visuals")]
        public GameObject WeaponPrefab;
        [BoxGroup(STATS_BOX_GROUP + "/Visuals")]
        public float WeaponScale = 1;
        [BoxGroup(STATS_BOX_GROUP + "/Visuals")]
        public Vector3 WeaponRotationOffsetEulers;
        [BoxGroup(STATS_BOX_GROUP + "/Visuals")]
        public Vector3 WeaponPositionOffset;

        [BoxGroup(STATS_BOX_GROUP + "/Visuals")]
        public GameObject WeaponImpactFX;
        [BoxGroup(STATS_BOX_GROUP + "/Visuals")]
        public Vector3 WeaponImpactPointOffset;

        [BoxGroup(STATS_BOX_GROUP + "/Casting")]
        public bool CastsAbility;
        [ShowIf ("CastsAbility")]
        [BoxGroup(STATS_BOX_GROUP + "/Casting")]
        public AbilityProjectileItem castAbility;

        public override ItemTypes[] SupportedItemTypes
        {
            get
            {
                return new ItemTypes[]
                {
                    ItemTypes.MainHand,
                    ItemTypes.OffHand
                };
            }
        }
    }
}
//#endif
