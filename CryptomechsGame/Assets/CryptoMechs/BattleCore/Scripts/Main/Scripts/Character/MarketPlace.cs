//#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    using UnityEngine;

    public class MarketPlace : SerializedScriptableObject
    {
        [HorizontalGroup("Split", 55, LabelWidth = 70)]
        [HideLabel, PreviewField(55, ObjectFieldAlignment.Left)]
        public Texture Icon;

        [VerticalGroup("Split/Meta")]
        public string Name;

        [VerticalGroup("Split/Meta")]
        [TextArea (4, 12)]
        public string Description;

        [BoxGroup("Market Goods")]
        public MarketItemSlot[,] Inventory = new MarketItemSlot[8, 6];

        public void GetAllItems()
        {
            for (int i = 0; i < Inventory.GetLength(0); ++i)
            {
                for (int j = 0; j < Inventory.GetLength(1); ++j)
                {
                    var slot = Inventory[i, j];
                    if (slot.Item != null)
                    {

                    }
                }
            }
        }
    }
}
//#endif
