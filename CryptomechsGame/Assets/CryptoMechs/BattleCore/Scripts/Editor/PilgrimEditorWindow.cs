#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;
    using System.Collections.Generic;
    using System.Collections;
    // 
    // This is the main RPG editor that which exposes everything included in this sample project.
    // 
    // This editor window lets users edit and create characters and items. To achieve this, we inherit from OdinMenuEditorWindow 
    // which quickly lets us add menu items for various objects. Each of these objects are then customized with Odin attributes to make
    // the editor user friendly. 
    // 
    // In order to let the user create items and characters, we don't actually make use of the [CreateAssetMenu] attribute 
    // for any of our scriptable objects, instead we've made a custom ScriptableObjectCreator, which we make use of in the 
    // in the custom toolbar drawn in OnBeginDrawEditors method below.
    // 
    // Go on an adventure in various classes to see how things are achived.
    // 

    public class PilgrimEditorWindow : OdinMenuEditorWindow
    {
        [MenuItem("Tools/CryptoMechs/CryptoMechs Editor")]
        private static void Open()
        {
            var window = GetWindow<PilgrimEditorWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
        }

        //private StoryNode s = null;

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(true);
            tree.DefaultMenuStyle.IconSize = 28.00f;
            tree.Config.DrawSearchToolbar = true;
            
            // Adds the character overview table.
            CharacterOverview.Instance.UpdateCharacterOverview();

            StoriesOverview.Instance.UpdateStoriesOverview();
            //tree.Add("Characters", new CharacterTable(CharacterOverview.Instance.AllCharacters));

            //tree.Add("CharacterSelector", new CharacterTable(CharacterOverview.Instance.AllCharacters));
            tree.AddAllAssetsAtPath("Narrative Trees", "Assets/CryptoMechs/NarrativeTrees", typeof(StoryNarrative), true, true);


            foreach (KeyValuePair<string, List<StoryNode>> kv in StoriesOverview.Instance.AllStoriesDict)
            {
                if (kv.Value != null)
                foreach (StoryNode s in kv.Value)
                {
                    string path = kv.Key + s.storyParentPath;
                    OdinMenuItem n = new OdinMenuItem(tree, s.storyID, s);
                    tree.AddMenuItemAtPath("Narrative Trees/" + path, n);
                }
            }

            //tree.EnumerateTree().Where(x => x.Value as StoryNarrative).ForEach((OdinMenuItem menuItem) => 
            //{
            //    var s = (menuItem.Value as StoryNarrative);
            //    //return;
            //    if (s != null && s.storyBlockData != null && s.storyBlockData.candidateFirstStoryIDs != null && s.storyBlockData.candidateFirstStoryIDs.Count > 0
            //        && s.storyBlockData.candidateFirstStoryIDs[0].nextStory != null)
            //        tree.Add(menuItem.GetFullPath(), (menuItem.Value as StoryNarrative).storyBlockData.candidateFirstStoryIDs[0].nextStory);
            //    //if ((menuItem.Value as StoryNarrative).storyBlockData.candidateFirstStoryIDs[0]) != null  
            //});

            // Adds all characters.
            tree.AddAllAssetsAtPath("Characters", "Assets/CryptoMechs/BattleCore/Data", typeof(Character), true, true);

            // Add all scriptable object items.
            tree.AddAllAssetsAtPath("", "Assets/CryptoMechs/BattleCore/Data/Items", typeof(ItemBase), true)
                .ForEach(this.AddDragHandles);

            // Add drag handles to items, so they can be easily dragged into the inventory if characters etc...
            tree.EnumerateTree().Where(x => x.Value as ItemBase).ForEach(AddDragHandles);

            tree.AddAllAssetsAtPath("MarketPlaces", "Assets/CryptoMechs/BattleCore/Markets", typeof(MarketPlace), true, true);

            // Add all scriptable object items.
            tree.AddAllAssetsAtPath("AI", "Assets/CryptoMechs/BattleCore/Data/AI", typeof(AIBehaviour), true, true);

            tree.AddAllAssetsAtPath("AI", "Assets/CryptoMechs/BattleCore/Data/ActionSequences", typeof(ActionSequenceBehavior), true, true);

            // Add icons to characters and items.
            tree.EnumerateTree().AddIcons<Character>(x => x.Icon);
            tree.EnumerateTree().AddIcons<MarketPlace>(x => x.Icon);
            tree.EnumerateTree().AddIcons<AIBehaviour>(x => x.Icon);
            tree.EnumerateTree().AddIcons<ActionSequenceBehavior>(x => x.Icon);
            tree.EnumerateTree().AddIcons<ItemBase>(x => x.Icon);

            return tree;
        }

        private void AddDragHandles(OdinMenuItem menuItem)
        {
            menuItem.OnDrawItem += x => DragAndDropUtilities.DragZone(menuItem.Rect, menuItem.Value, false, false);
        }

        protected override void OnBeginDrawEditors()
        {
            var selected = this.MenuTree.Selection.FirstOrDefault();
            var toolbarHeight = this.MenuTree.Config.SearchToolbarHeight;

            // Draws a toolbar with the name of the currently selected menu item.
            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
            {
                if (selected != null)
                {
                    GUILayout.Label(selected.Name);
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Narrative Tree")))
                {
                    ScriptableObjectCreator.ShowDialog<StoryNarrative>("Assets/CryptoMechs/BattleCore/NarrativeTrees", obj =>
                    {
                        obj.storyBlockData.NarrativeBlockID = obj.name;
                        base.TrySelectMenuItemWithObject(obj); // Selects the newly created item in the editor
                    });
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Item")))
                {
                    ScriptableObjectCreator.ShowDialog<ItemBase>("Assets/CryptoMechs/BattleCore/Data/Items", obj =>
                    {
                        obj.Name = obj.name;
                        base.TrySelectMenuItemWithObject(obj); // Selects the newly created item in the editor
                    });
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Character")))
                {
                    ScriptableObjectCreator.ShowDialog<Character>("Assets/CryptoMechs/BattleCore/Data/Character", obj =>
                    {
                        obj.Name = obj.name;
                        base.TrySelectMenuItemWithObject(obj); // Selects the newly created item in the editor
                    });
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create AI")))
                {
                    ScriptableObjectCreator.ShowDialog<AIBehaviour>("Assets/CryptoMechs/BattleCore/Data/AI", obj =>
                    {
                        obj.Name = obj.name;
                        base.TrySelectMenuItemWithObject(obj); // Selects the newly created item in the editor
                    });
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Action")))
                {
                    ScriptableObjectCreator.ShowDialog<ActionSequenceBehavior>("Assets/CryptoMechs/BattleCore/Data/ActionSequences", obj =>
                    {
                        obj.Name = obj.name;
                        base.TrySelectMenuItemWithObject(obj); // Selects the newly created item in the editor
                    });
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Market")))
                {
                    ScriptableObjectCreator.ShowDialog<MarketPlace>("Assets/CryptoMechs/BattleCore/Markets", obj =>
                    {
                        obj.Name = obj.name;
                        base.TrySelectMenuItemWithObject(obj); // Selects the newly created item in the editor
                    });
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }
    }
}
#endif
