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

    public abstract class ItemBase : UnitBase3D
    {
        [PropertyOrder(-1)]
        [BoxGroup("Split/Right/Description")]
        [HideLabel, TextArea(4, 14)]
        public string Description;

        [HorizontalGroup("Split", 0.5f, MarginLeft = 5, LabelWidth = 130)]
        [BoxGroup("Split/Right/Notes")]
        [HideLabel, TextArea(4, 9)]
        public string Notes;

        [VerticalGroup(GENERAL_SETTINGS_VERTICAL_GROUP)]
        [ValueDropdown("SupportedItemTypes")]
        [ValidateInput("IsSupportedType")]
        public ItemTypes Type;

        [VerticalGroup("Split/Right")]
        public StatList Requirements;

        //[AssetsOnly]
        //[VerticalGroup(GENERAL_SETTINGS_VERTICAL_GROUP)]
        //public GameObject Prefab;

        public abstract ItemTypes[] SupportedItemTypes { get; }

        private bool IsSupportedType(ItemTypes type)
        {
            return this.SupportedItemTypes.Contains(type);
        }

        public virtual void OnReceive(float timestamp)
        {

        }
    }
}
//#endif
