//#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    [System.Serializable]
    public class PathNarrativeEncounter
    {
        public StoryNarrative narrative;
        [Range(0,1)]
        public float encounterProbability;
        [Range(0, 1)]
        public float fractionAlongPath;

        [BoxGroup("NarrativeEncounter")]
        [BoxGroup("NarrativeEncounter" + "/PathPin")]
        //[VerticalGroup(STATS_BOX_GROUP + "/LocationPin")]
        public bool usePathSprite;
        [ShowIf("usePathSprite")]
        [PreviewField(55)]
        [HorizontalGroup("NarrativeEncounter" + "/PathPin/LEFT", 55, LabelWidth = 87)]
        public Texture pathSprite;
        //public Sprite mapSprite;
        [ShowIf("usePathSprite")]
        [BoxGroup("NarrativeEncounter" + "/PathPin")]
        public float pinScale;
        [ShowIf("usePathSprite")]
        [BoxGroup("NarrativeEncounter" + "/PathPin")]
        public Vector3 pinOffset;
    }

    public class MapConnectorPath: ItemBase
    {
        [BoxGroup(STATS_BOX_GROUP)]
        [BoxGroup(STATS_BOX_GROUP + "/PathSettings")]
        public TerrainPreset terrainSettings;
        [BoxGroup(STATS_BOX_GROUP + "/PathPin")]
        //[VerticalGroup(STATS_BOX_GROUP + "/LocationPin")]
        public bool usePathSprite;
        [ShowIf("usePathSprite")]
        [PreviewField(55)]
        [HorizontalGroup(STATS_BOX_GROUP + "/PathPin/LEFT", 55, LabelWidth = 87)]
        public Texture pathSprite;
        [ShowIf("usePathSprite")]
        [BoxGroup(STATS_BOX_GROUP + "/PathPin")]
        public float pathRepeatScale;

        [BoxGroup(STATS_BOX_GROUP + "/PathNarratives")]
        public List<PathNarrativeEncounter> narrativeEncounters;

        public override ItemTypes[] SupportedItemTypes
        {
            get
            {
                return new ItemTypes[]
                {
                    ItemTypes.MapPath
                };
            }
        }
    }
}
//#endif
