//#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    public class ConsumableItem : Item
    {
        [SuffixLabel("days ", true)]
        [BoxGroup(STATS_BOX_GROUP)]
        public float Cooldown;

        [BoxGroup(STATS_BOX_GROUP + "/Consumption")]
        public int WaterGranted = 0;
        [BoxGroup(STATS_BOX_GROUP + "/Consumption")]
        public int NutritionGranted = 0;

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
                    ItemTypes.Consumable
                };
            }
        }
    }
}
//#endif
