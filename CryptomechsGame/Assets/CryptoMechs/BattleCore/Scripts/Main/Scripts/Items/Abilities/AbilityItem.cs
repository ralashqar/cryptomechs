//#if UNITY_EDITOR
using UnityEngine;

namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    public abstract class AbilityItem : Item
    {
        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public AbilityFXType AbilityMode;

        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public ParticleSystem ChannelOnSpotFX;

        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public ParticleSystem ImpactPFX;

        [ShowIf("AbilityMode", AbilityFXType.PROJECTILE)]
        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public ParticleSystem TrailFX;

        [BoxGroup(STATS_BOX_GROUP + "/AbilityFX")]
        public float ChannelTime = 0;

    }
}
//#endif
