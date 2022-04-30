//#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    public class Affliction : ItemBase, IExpireable
    {
        [SuffixLabel("days ", true)]
        [BoxGroup(STATS_BOX_GROUP)]
        public float Cooldown;

        [BoxGroup(STATS_BOX_GROUP + "/Affliction")]
        public float ExpiryTime;

        private float receiveTimestamp;

        [BoxGroup(STATS_BOX_GROUP + "/Consumption")]
        public bool ConsumeOverTime;
        [HideLabel]
        [BoxGroup(STATS_BOX_GROUP + "/Consumption")]
        [SuffixLabel("days ", true), EnableIf("ConsumeOverTime")]
        [LabelWidth(20)]
        public float Duration;

        [VerticalGroup(LEFT_VERTICAL_GROUP)]
        public StatList Modifiers;


        public override ItemTypes[] SupportedItemTypes
        {
            get
            {
                return new ItemTypes[]
                {
                    ItemTypes.Affliction
                };
            }
        }

        public float GetReceivedTime()
        {
            return receiveTimestamp;
        }

        public float GetExpiryTime()
        {
            return ExpiryTime;
        }

        public override void OnReceive(float timestamp)
        {
            receiveTimestamp = timestamp;
        }
    }
}
//#endif
