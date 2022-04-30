//#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    public class Personnel : Item, IGrantCapacity, IConsumesFoodAndWater
    {
        [SuffixLabel("days ", true)]
        [BoxGroup(STATS_BOX_GROUP)]
        public float Cooldown;

        [BoxGroup(STATS_BOX_GROUP + "/Grants")]
        public int CapacityGranted = 0;
        [BoxGroup(STATS_BOX_GROUP + "/Grants")]
        public int DefenseGranted = 0;

        [BoxGroup(STATS_BOX_GROUP + "/Needs")]
        public int GoldConsumedPerDay = 0;
        [BoxGroup(STATS_BOX_GROUP + "/Needs")]
        public int DirhamConsumedPerDay = 0;
        [BoxGroup(STATS_BOX_GROUP + "/Needs")]
        public int WaterConsumedPerDay = 0;
        [BoxGroup(STATS_BOX_GROUP + "/Needs")]
        public int NutritionConsumedPerDay = 0;

        [VerticalGroup(LEFT_VERTICAL_GROUP)]
        public StatList Modifiers;

        public override ItemTypes[] SupportedItemTypes
        {
            get
            {
                return new ItemTypes[]
                {
                    ItemTypes.Personnel
                };
            }
        }

        int IGrantCapacity.GrantCapacity
        {
            get { return CapacityGranted; }
            set { value = CapacityGranted; }
        }

        int IConsumesFoodAndWater.WaterConsumption
        {
            get { return WaterConsumedPerDay; }
            set { value = WaterConsumedPerDay; }
        }

        int IConsumesFoodAndWater.NutritionConsumption
        {
            get { return NutritionConsumedPerDay; }
            set { value = NutritionConsumedPerDay; }
        }
    }
}
//#endif
