//#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    public class MinableItem : Item
    {
        [BoxGroup(STATS_BOX_GROUP + "/Minable")]
        public float MinableValue = 0;
        
        [VerticalGroup(LEFT_VERTICAL_GROUP)]
        public StatList Modifiers;

        public override ItemTypes[] SupportedItemTypes
        {
            get
            {
                return new ItemTypes[]
                {
                    ItemTypes.Minable
                };
            }
        }
    }
}
//#endif
