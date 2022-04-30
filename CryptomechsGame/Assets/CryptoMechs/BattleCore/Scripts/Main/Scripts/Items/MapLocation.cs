//#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    public enum MapNodeType
    {
        NEUTRAL,
        VILLAGE,
        CITY
    }

    public class MapLocation : ItemBase
    {
        [BoxGroup(STATS_BOX_GROUP)]
        [PreviewField(55)]
        [BoxGroup(STATS_BOX_GROUP + "/LocationPin")]
        //[VerticalGroup(STATS_BOX_GROUP + "/LocationPin")]
        [HorizontalGroup(STATS_BOX_GROUP + "/LocationPin/LEFT", 55, LabelWidth = 87)]
        public Texture mapSprite;
        //public Sprite mapSprite;

        [BoxGroup(STATS_BOX_GROUP + "/LocationPin")]
        public float pinScale;
        [BoxGroup(STATS_BOX_GROUP + "/LocationPin")]
        public Vector3 pinOffset;
        [BoxGroup(STATS_BOX_GROUP + "/LocationPin")]
        public MapNodeType locationType;

        [BoxGroup(STATS_BOX_GROUP + "/LocationNarratives")]
        public List<StoryNarrative> narratives;

        [BoxGroup(STATS_BOX_GROUP + "/LocationMarket")]
        public bool HasMarketplace;
        [BoxGroup(STATS_BOX_GROUP + "/LocationMarket")]
        [ShowIf("HasMarketplace")]
        public MarketPlace market;

        public override ItemTypes[] SupportedItemTypes
        {
            get
            {
                return new ItemTypes[]
                {
                    ItemTypes.MapLocation
                };
            }
        }
    }
}
//#endif
