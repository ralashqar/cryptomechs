//#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    public class SpecialItem : Item
    {
        [SuffixLabel("days ", true)]
        [BoxGroup(STATS_BOX_GROUP)]
        public float Cooldown;

        [BoxGroup(STATS_BOX_GROUP + "/Grants")]
        public int CapacityGranted = 0;
        [BoxGroup(STATS_BOX_GROUP + "/Grants")]
        public int DefenseGranted = 0;

        [VerticalGroup(LEFT_VERTICAL_GROUP)]
        public StatList Modifiers;

        public override ItemTypes[] SupportedItemTypes
        {
            get
            {
                return new ItemTypes[]
                {
                    ItemTypes.SpecialItem
                };
            }
        }
    }
}
//#endif
