#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    using UnityEngine;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector.Editor.Drawers;
    using Sirenix.Utilities.Editor;
    using Sirenix.Utilities;
    using UnityEditor;

    // 
    // In Character.cs we have a two dimention array of ItemSlots which is our inventory.
    // And instead of using the the TableMatrix attribute to customize it there, we in this case 
    // instead create a custom drawer that will work for all two-dimentional ItemSlot arrays,
    // so we don't have to make the same CustomDrawer via the TableMatrix attribute again and again.
    // 

    internal sealed class MarketItemSlotCellDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, MarketItemSlot>
        where TArray : System.Collections.IList
    {
        protected override TableMatrixAttribute GetDefaultTableMatrixAttributeSettings()
        {
            return new TableMatrixAttribute()
            {
                SquareCells = true,
                HideColumnIndices = true,
                HideRowIndices = true,
                ResizableColumns = false
            };
        }

        OdinEditorWindow w = null;
        MarketItemSlot selectedItem = null;

        protected override MarketItemSlot DrawElement(Rect rect, MarketItemSlot value)
        {
            var id = DragAndDropUtilities.GetDragAndDropId(rect);
            DragAndDropUtilities.DrawDropZone(rect, value.Item ? value.Item.Icon : null, null, id); // Draws the drop-zone using the items icon.

            if (value.Item != null)
            {
                // Item count
                var selectRect = rect.Padding(2).AlignTop(16);
                if (GUI.Button(selectRect, "Select"))
                {
                    w = OdinEditorWindow.InspectObject(value);
                    w.Show();
                };
                var countRect = rect.Padding(2).AlignBottom(16);
                value.ItemCount = EditorGUI.IntField(countRect, Mathf.Max(1, value.ItemCount));
                GUI.Label(countRect, "/ " + value.Item.ItemStackSize, SirenixGUIStyles.RightAlignedGreyMiniLabel);
            }
            if (w == null)
            {
                value = DragAndDropUtilities.DropZone(rect, value);                                     // Drop zone for ItemSlot structs.
                value.Item = DragAndDropUtilities.DropZone<Item>(rect, value.Item);
                // Drop zone for Item types.
                value = DragAndDropUtilities.DragZone(rect, value, true, true);                         // Enables dragging of the ItemSlot
            }
            return value;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var t = base.Property;
            base.DrawPropertyLayout(label);

            // Draws a drop-zone where we can destroy items.
            var rect = GUILayoutUtility.GetRect(0, 40).Padding(2);
            var id = DragAndDropUtilities.GetDragAndDropId(rect);
            DragAndDropUtilities.DrawDropZone(rect, null as UnityEngine.Object, null, id);
            DragAndDropUtilities.DropZone<MarketItemSlot>(rect, new MarketItemSlot(), false, id);

        }
    }

}
#endif
