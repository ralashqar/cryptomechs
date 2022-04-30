//#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    using System.Linq;
    using UnityEngine;

    // 
    // This is the base-class for all items. It contains a lot of layout using various layout group attributes. 
    // We've also defines a few relevant groups in constant variables, which derived classes can utilize.
    // 
    // Also note that each item deriving from this class, needs to specify which Item types are
    // supported via the SupporteItemTypes property. This is then referenced in ValueDropdown attribute  
    // on the Type field, so that when users only can specify supported item-types.  
    // 

    public abstract class Item : ItemBase
    {
        [BoxGroup(STATS_BOX_GROUP)]
        public int ItemStackSize = 1;

        [BoxGroup(STATS_BOX_GROUP)]
        public int CapacityConsumed = 1;

        [BoxGroup(STATS_BOX_GROUP)]
        public float ItemRarity;

        [BoxGroup(STATS_BOX_GROUP + "/Monetary")]
        public bool CanSell;
        [BoxGroup(STATS_BOX_GROUP + "/Monetary")]
        public int GoldValue = 0;
        [BoxGroup(STATS_BOX_GROUP + "/Monetary")]

        public int DirhamValue = 0;

        private bool IsSupportedType(ItemTypes type)
        {
            return this.SupportedItemTypes.Contains(type);
        }
    }
}
//#endif
