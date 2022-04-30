//#if UNITY_EDITOR
using UnityEngine;
//using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    public enum AbilityFXType
    {
        PROJECTILE,
        CAST_TARGET,
        CAST_ONSPOT,
        LASER,
        CAST_LINE,
        BOOMERANG
    }

    public enum AbilityChannelingMode
    {
        SINGLE,
        CONTINUOUS,
        REPEATED_INTERVALS
    }

    public enum AbilitySpreadType
    {
        BARREL_POINTS,
        CONE,
        RADIAL
    }

    public enum AbilityImpactPoint
    {
        DEFAULT_TARGET,
        CHARACTER_CENTER,
        WEAPON_POINT
    }

    public interface AbilityVisualFXBehavior
    {
        void OnFire(IAbilityCaster caster, Transform sourceMuzzle, AbilityProjectileItem ability);
        void OnChannel(Vector3 position);
        void OnHitTarget(Vector3 target);
        void DestroyFX(GameObject go);
        Vector3 GetTargetPosition();
    }

    [System.Serializable]
    public class AnimationSegmentLooper
    {
        public float timeOffsetAmplitudeNormalized = 0.06f;
        public float timeOffsetFrequency = 5f;
    }

    public class AbilityProjectileItem : ItemBase
    {
        [BoxGroup(STATS_BOX_GROUP)]
        [BoxGroup(STATS_BOX_GROUP + "/Ability")]
        public bool applyImpactAsDPS = false;
        [BoxGroup(STATS_BOX_GROUP + "/Ability")]
        public float damage = 5f;
        [BoxGroup(STATS_BOX_GROUP + "/Ability")]
        public float speed = 5f;
        [BoxGroup(STATS_BOX_GROUP + "/Ability")]
        public float attackCooldown = 3.0f;
        [BoxGroup(STATS_BOX_GROUP + "/Ability")]
        public float range = 15.0f;
        [BoxGroup(STATS_BOX_GROUP + "/Ability")]
        public int numRounds = 1;
        [BoxGroup(STATS_BOX_GROUP + "/Ability")]
        public AbilityAimType AimType = AbilityAimType.LINE;
        [BoxGroup(STATS_BOX_GROUP + "/Ability")]
        public float TriggerCastTime = 0.5f;

        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public AbilityImpactPoint AbilityImpactPoint;

        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public AbilityFXType AbilityMode;

        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public GameObject ChannelFX;

        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public GameObject MuzzleFX;
        
        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public float MuzzleFXDuration = 0.1f;

        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public GameObject ImpactFX;

        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public float ImpactDelayAfterFX = 0f;

        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public float ImpactFXDuration = 1f;

        //[ShowIf("AbilityMode", AbilityFXType.PROJECTILE)]
        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public GameObject TrailFX;
        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public float TrailFXDestroyDelay = 0f;

        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public float Lifetime = 3f;
        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public float ResetTime = 0.1f;
        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public int NumImpacts = 1;

        [ShowIf("AbilityMode", AbilityFXType.PROJECTILE)]
        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public bool FollowsTarget;
        [ShowIf("AbilityMode", AbilityFXType.PROJECTILE)]
        [ShowIf("FollowsTarget")]
        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        [Range (1, 360)]
        public float FollowSteerRate;

        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public bool AffectsSingleTarget;

        [BoxGroup(RIGHT_VERTICAL_GROUP + "/AbilityImpact")]
        public List<AbilityImpactDefinition> AbilityEffects;

        [BoxGroup(RIGHT_VERTICAL_GROUP + "/AbilitySpread")]
        public bool UseProceduralSpread;
        [BoxGroup(RIGHT_VERTICAL_GROUP + "/AbilitySpread")]
        public AbilitySpreadType SpreadType;
        [BoxGroup(RIGHT_VERTICAL_GROUP + "/AbilitySpread")]
        public float SpreadLength;
        [BoxGroup(RIGHT_VERTICAL_GROUP + "/AbilitySpread")]
        [Range(0,360)]
        public int SpreadAngle;
        [BoxGroup(RIGHT_VERTICAL_GROUP + "/AbilitySpread")]
        public int NumberOfRounds;

        [BoxGroup(STATS_BOX_GROUP + "/AbilityAnimation")]
        public bool UseCustomChannelAnimation = false;
        [ShowIf("UseCustomChannelAnimation")]
        [BoxGroup(STATS_BOX_GROUP + "/AbilityAnimation")]
        public string CustomChannelAnimationTrigger = "";

        [BoxGroup(STATS_BOX_GROUP + "/AbilityChanneling")]
        public AbilityChannelingMode ChannelMode;
        
        [ShowIf("ChannelMode", AbilityChannelingMode.CONTINUOUS)]
        [BoxGroup(STATS_BOX_GROUP + "/AbilityChanneling")]
        public AnimationSegmentLooper channelAnimLooping;
        
        [BoxGroup(STATS_BOX_GROUP + "/AbilityChanneling")]
        public bool CanCancelAbility;
        [BoxGroup(STATS_BOX_GROUP + "/AbilityChanneling")]
        public float ChannelTime = 0;
        [BoxGroup(STATS_BOX_GROUP + "/AbilityChanneling")]
        public float ChannelRate = 1f;
        [BoxGroup(STATS_BOX_GROUP + "/AbilityChanneling")]
        public bool ApplyImpactWhileChanneling = false;
        [BoxGroup(STATS_BOX_GROUP + "/AbilityChanneling")]
        public bool MoveWhileChanneling = false;
        [ShowIf("MoveWhileChanneling")]
        [BoxGroup(STATS_BOX_GROUP + "/AbilityChanneling")]
        public float MoveDelay;
        [BoxGroup(STATS_BOX_GROUP + "/AbilityChanneling")]
        public bool MoveToSelectedTarget;
        [BoxGroup(STATS_BOX_GROUP + "/AbilityChanneling")]
        [ShowIf("MoveWhileChanneling")]
        [HideIf("MoveToSelectedTarget")]
        public Vector3 MoveDirection;
        [ShowIf("MoveWhileChanneling")]
        [BoxGroup(STATS_BOX_GROUP + "/AbilityChanneling")]
        public float MoveDistance;

        [VerticalGroup(LEFT_VERTICAL_GROUP + "/Modifiers")]
        public StatList Modifiers;

        public override ItemTypes[] SupportedItemTypes
        {
            get
            {
                return new ItemTypes[]
                {
                    ItemTypes.Ability
                };
            }
        }
    }
}
//#endif
