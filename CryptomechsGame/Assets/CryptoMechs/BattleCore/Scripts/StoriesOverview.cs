#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    using Sirenix.Utilities;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    // 
    // This is a scriptable object containing a list of all characters available
    // in the Unity project. When a character is added from the RPG editor, the
    // list then gets automatically updated via UpdateCharacterOverview. 
    //
    // If you inspect the Character Overview in the inspector, you will also notice, that
    // the list is not directly modifiable. Instead, we've customized it so it contains a 
    // refresh button, that scans the project and automatically populates the list.
    //
    // CharacterOverview inherits from GlobalConfig which is just a scriptable 
    // object singleton, used by Odin Inspector for configuration files etc, 
    // but it could easily just be a simple scriptable object instead.
    // 

    [GlobalConfig("CryptoMechs/NarrativeTrees")]
    public class StoriesOverview : GlobalConfig<StoriesOverview> 
    {
        [ReadOnly]
        [ListDrawerSettings(Expanded = true)]

        public string[] AllNarrativeAssetNames;
        public StoryNarrative[] AllNarratives;
        public List<StoryNode> AllStories;
        public Dictionary<string, List<StoryNode>> AllStoriesDict;


#if UNITY_EDITOR
        [Button(ButtonSizes.Medium), PropertyOrder(-1)]
        public void UpdateStoriesOverview()
        {
            AllNarrativeAssetNames = AssetDatabase.FindAssets("t:StoryNarrative");
            // Finds and assigns all scriptable objects of type Character
            this.AllNarratives = AllNarrativeAssetNames
                .Select(guid => AssetDatabase.LoadAssetAtPath<StoryNarrative>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();
            if (AllNarratives == null)
                return;

            AllStories = new List<StoryNode>();
            AllNarratives.ForEach((n) => {
                if (n != null)
                {
                    AllStories?.AddRange(n?.GetAllStoriesRecursive());
                }
            });

            AllStoriesDict = new Dictionary<string, List<StoryNode>>();

            int c = 0;
            foreach(StoryNode s in AllStories)
            {
                s.storyPath = s.storyID;// + " (" + (++c).ToString() + ")";
                s.storyParentPath = "";
            }

            for (int i = 0; i < AllNarratives.Length; ++i)
            {
                string filename = AssetDatabase.GUIDToAssetPath(AllNarrativeAssetNames[i]).Split('/').Last().Replace(".asset", "");
                AllStoriesDict.Add(filename, AllNarratives[i]?.GetAllStoriesRecursive());
            }
        }
#endif
    }
}
#endif
