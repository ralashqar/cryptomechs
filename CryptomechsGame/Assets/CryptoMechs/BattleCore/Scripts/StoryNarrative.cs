using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Sirenix.Utilities.Editor;
#endif

using Sirenix.Utilities;


public enum StoryNarrativeType
{
    CHANCE_ENCOUNTER,
    MAINSTORY
}

[System.Serializable]
public class StoryNarrative : SerializedScriptableObject
{
    [HideInInspector]
    public MapNodeBase mapNode;

    public StoryNarrativeBlockData storyBlockData;

    [HideInInspector]
    public StoryNode firstStoryNode;

    public StoryNode GetRootStory()
    {
        if (storyBlockData == null || storyBlockData.candidateFirstStoryIDs == null || storyBlockData.candidateFirstStoryIDs.Count == 0)
            return null;

        return storyBlockData.candidateFirstStoryIDs[0].nextStory;
    }
    //public void TriggerStoryNarrative()
    //{
    //    firstStoryNode = GetFirstStoryNodeFromPlayerMapState();
    //}

    //public StoryNode GetFirstStoryNodeFromPlayerMapState()
    //{
    ///    return candidateFirstStoryNodes.Count == 1 ? candidateFirstStoryNodes[0] : candidateFirstStoryNodes[0];
    //}

    public List<StoryNode> GetAllStoriesRecursive()
    {
        List<StoryNode> stories = new List<StoryNode>();
        if (storyBlockData.candidateFirstStoryIDs == null)
            storyBlockData.candidateFirstStoryIDs = new List<StoryNodeOption>();
        foreach (StoryNodeOption s in storyBlockData.candidateFirstStoryIDs)
        {
            stories.Add(s.nextStory);
            s.nextStory?.GetAllStoriesRecursive(ref stories, s.nextStory.storyPath);
        }
        return stories;
    }

}

[System.Serializable]
public class StoryNarrativeBlockData
{
    public string NarrativeBlockID;
    public List<StoryNodeOption> candidateFirstStoryIDs;
}

#if UNITY_EDITOR
public enum SelectedNarrativePage
{
    OPTIONS = 0,
    REWARDS_AND_PENALTIES
}

#if UNITY_EDITOR
[CustomEditor(typeof(StoryNarrative))]
public class StoryNarrativeEditor : OdinEditor
{
    public StoryNode selectedStory = null;
    public StoryNodeOption selectedOption = null;
    public SelectedNarrativePage selectedPage = SelectedNarrativePage.OPTIONS;
    public static StoryNarrative editedNarrative = null;

    public void ForceRebuildTree()
    {
        var obj = this.target as StoryNarrative;
        editedNarrative = obj;
        //EditorUtility.SetDirty(editedNarrative);
        //AssetDatabase.SaveAssets();
        OdinMenuEditorWindow menuWindow = (GUIHelper.CurrentWindow as OdinMenuEditorWindow);
        menuWindow?.ForceMenuTreeRebuild();
    }

    public void DrawRootStories()
    {
        var tree = this.Tree;
        var obj = this.target as StoryNarrative;

        Color oCol = GUI.backgroundColor;

        GUILayout.Space(10f);

        Vector4 fadeMultiplier = new Vector4(1, 1, 1, 0.1f);

        SirenixEditorGUI.BeginBox("OPTIONS");
        SirenixEditorGUI.BeginHorizontalToolbar();

        if (obj.storyBlockData.candidateFirstStoryIDs == null)
        {
            obj.storyBlockData.candidateFirstStoryIDs = new List<StoryNodeOption>();
        }

        var candidateOptions = obj.storyBlockData.candidateFirstStoryIDs;

        foreach (StoryNodeOption o in candidateOptions)
        {
            bool isSelected = selectedOption != null && selectedOption == o;
            GUI.backgroundColor = isSelected ? Color.green : Color.red * fadeMultiplier;
            if (isSelected)
            {
                SirenixEditorGUI.BeginVerticalList(false, false, GUILayoutOptions.Width(120f));
                if (GUILayout.Button(o.optionID, GUILayout.Width(120), GUILayout.Height(60)))
                {
                    selectedOption = o;
                }
                GUI.backgroundColor = Color.blue;
                if (GUILayout.Button("GOTO", GUILayout.Width(120), GUILayout.Height(30)))
                {
                    selectedOption = o;
                    OdinMenuEditorWindow menuWindow = (GUIHelper.CurrentWindow as OdinMenuEditorWindow);
                    menuWindow.TrySelectMenuItemWithObject(o.nextStory);
                }
                SirenixEditorGUI.EndVerticalList();
            }
            else
            {
                if (GUILayout.Button(o.optionID, GUILayout.Width(120), GUILayout.Height(90)))
                {
                    selectedOption = o;
                }
            }
            GUILayout.Space(10f);
        }
        GUILayout.Space(20f);
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("ADD NEW", GUILayout.Width(90), GUILayout.Height(90)))
        {
            candidateOptions.Add(selectedOption = new StoryNodeOption("Story " + candidateOptions.Count.ToString()));
            selectedOption.nextStory.storyID = "Story " + candidateOptions.Count.ToString();
            ForceRebuildTree();
        }

        SirenixEditorGUI.EndHorizontalToolbar();
        SirenixEditorGUI.EndBox();

        GUILayout.Space(10f);
        GUI.backgroundColor = oCol;

        if (GUILayout.Button("DELETE"))
        {
            candidateOptions.Remove(selectedOption);
            selectedOption = null;
            ForceRebuildTree();
        }

        DrawRootOptionConditions(selectedOption);
    }

    public bool selected = false;

    public override void OnInspectorGUI()
    {
        var tree = this.Tree;
        var obj = this.target as StoryNarrative;

        InspectorUtilities.BeginDrawPropertyTree(tree, true);
        var headerProp = tree.GetPropertyAtPath("storyBlockData.NarrativeBlockID");
        SirenixEditorGUI.BeginBoxHeader();
        headerProp.Draw(new GUIContent("Tree ID"));
        SirenixEditorGUI.EndBoxHeader();

        DrawRootStories();
        InspectorUtilities.EndDrawPropertyTree(tree);


        if (!selected)
        {
            EditorUtility.SetDirty(obj);
            selected = true;
        }

        return;

        if (obj.storyBlockData.candidateFirstStoryIDs == null || obj.storyBlockData.candidateFirstStoryIDs.Count == 0)
        {
            StoryNodeOption rootOption = new StoryNodeOption();
            rootOption.nextStory = new StoryNode("");
            obj.storyBlockData.candidateFirstStoryIDs = new List<StoryNodeOption>();
            obj.storyBlockData.candidateFirstStoryIDs.Add(new StoryNodeOption());
        }
        if (selectedStory == null)
        {
            selectedStory = obj.storyBlockData.candidateFirstStoryIDs[0].nextStory;
        }

        var rootPath = tree.GetPropertyAtPath("storyBlockData.candidateFirstStoryIDs").Children[0].Path;
        var storyProp = tree.GetPropertyAtPath(rootPath + ".nextStory");
        storyProp.Draw();

        InspectorUtilities.EndDrawPropertyTree(tree);

        return;

        SirenixEditorGUI.BeginHorizontalToolbar();
        //var rootPath = tree.GetPropertyAtPath("storyBlockData.candidateFirstStoryIDs").Children[0].Path;
        SirenixEditorGUI.BeginBox("Story");
        //SirenixEditorGUI.BeginHorizontalToolbar();
        var storyID = tree.GetPropertyAtPath(rootPath + ".nextStory.storyID");
        storyID.Draw(new GUIContent("ID"));
        var journeyTime = tree.GetPropertyAtPath(rootPath + ".nextStory.addedJourneyTime");
        journeyTime.Draw(new GUIContent("Journey Time (Days)"));
        //SirenixEditorGUI.EndHorizontalToolbar();
        var storyText = tree.GetPropertyAtPath(rootPath + ".nextStory.storyText");
        storyText.Draw(new GUIContent("Body Text"));
        SirenixEditorGUI.EndBox();

        SirenixEditorGUI.BeginBox("Rewards & Penalties");
        var rewardsAndPenalties = tree.GetPropertyAtPath(rootPath + ".nextStory.rewardsAndPenalties");
        rewardsAndPenalties.Draw();
        SirenixEditorGUI.EndBox();

        SirenixEditorGUI.EndHorizontalToolbar();

        selectedPage = SelectedNarrativePage.OPTIONS;
        //DrawButtons();
        DrawOptions();
        //DrawTabs();

        InspectorUtilities. EndDrawPropertyTree(tree);

        // You can also call base.OnInspectorGUI(); instead if you simply want to prepend or append GUI to the editor.
    }

    //public string GetStoryPropertyPath(StoryNarrative narrative, string property)
    //{

    //}

    public void DrawRootOptionConditions(StoryNodeOption option)
    {
        if (option == null) return;

        var tree = this.Tree;
        var obj = this.target as StoryNarrative;

        int index =  obj.storyBlockData.candidateFirstStoryIDs.IndexOf(selectedOption);

        if (index >= 0 && index < tree.GetPropertyAtPath("storyBlockData.candidateFirstStoryIDs").Children.Count)
        {
            //var p = tree.GetPropertyAtPath("storyBlockData.candidateFirstStoryIDs").Children[0].Path;
            var p = tree.GetPropertyAtPath("storyBlockData.candidateFirstStoryIDs").Children[index].Path;

            var optionID = tree.GetPropertyAtPath(p + ".optionID");
            var optionText = tree.GetPropertyAtPath(p + ".optionText");

            var optionUnlockConditions = tree.GetPropertyAtPath(p + ".unlockCriteriaCaravan");
            var foreshadowedLands = tree.GetPropertyAtPath(p + ".unlockCriteriaCaravan");

            var statRequirements = tree.GetPropertyAtPath(p + ".statRequirements");
            var baseUnlockConditions = tree.GetPropertyAtPath(p + ".baseUnlockConditions");

            var storyUnlockConditions = tree.GetPropertyAtPath(p + ".storyUnlockConditions");

            SirenixEditorGUI.BeginHorizontalToolbar();

            /*
            SirenixEditorGUI.BeginBox("Option");
            optionID.Draw();
            optionText.Draw();

            if (GUILayout.Button("DELETE"))
            {
                selectedStory.storyOptions.Remove(selectedOption);
                selectedOption = null;
                ForceRebuildTree();
            }
            SirenixEditorGUI.EndBox();
            */

            SirenixEditorGUI.BeginBox("Base Unlock Conditions");
            storyUnlockConditions.Draw();
            baseUnlockConditions.Draw();
            statRequirements.Draw();

            SirenixEditorGUI.EndBox();

            SirenixEditorGUI.BeginBox("Inventory Unlock Conditions");
            optionUnlockConditions.Draw();
            SirenixEditorGUI.EndBox();

            SirenixEditorGUI.EndHorizontalToolbar();

        }
    }


    public void DrawOptionConditions(StoryNodeOption option)
    {
        if (option == null) return;

        var tree = this.Tree;
        var obj = this.target as StoryNarrative;

        int index = selectedStory.storyOptions.IndexOf(selectedOption);
        if (index >= 0)
        {
            var r = tree.GetPropertyAtPath("storyBlockData.candidateFirstStoryIDs").Children[0].Path;
            var p = tree.GetPropertyAtPath(r + ".nextStory.storyOptions").Children[index].Path;

            var optionID = tree.GetPropertyAtPath(p + ".optionID");
            var optionText = tree.GetPropertyAtPath(p + ".optionText");

            var optionUnlockConditions = tree.GetPropertyAtPath(p + ".unlockCriteriaCaravan");
            var foreshadowedLands = tree.GetPropertyAtPath(p + ".unlockCriteriaCaravan");

            SirenixEditorGUI.BeginHorizontalToolbar();
            SirenixEditorGUI.BeginBox("Option");
            optionID.Draw();
            optionText.Draw();

            if (selectedOption != null)
            {
                if (GUILayout.Button("DELETE"))
                {
                    selectedStory.storyOptions.Remove(selectedOption);
                    selectedOption = null;
                }
            }

            SirenixEditorGUI.EndBox();

            SirenixEditorGUI.BeginBox("Unlock Conditions");
            optionUnlockConditions.Draw();
            SirenixEditorGUI.EndBox();
            SirenixEditorGUI.EndHorizontalToolbar();
        }
    }

    public void DrawOptions()
    {
        if (selectedPage != SelectedNarrativePage.OPTIONS || selectedStory == null)
            return;

        var tree = this.Tree;
        var obj = this.target as StoryNarrative;

        Color oCol = GUI.backgroundColor;

        GUILayout.Space(10f);

        Vector4 fadeMultiplier = new Vector4(1, 1, 1, 0.1f);

        SirenixEditorGUI.BeginBox("CANDIDATE FIRST STORIES");
        SirenixEditorGUI.BeginHorizontalToolbar();

        if (selectedStory.storyOptions == null)
        {
            selectedStory.storyOptions = new List<StoryNodeOption>();
        }

        foreach (StoryNodeOption o in selectedStory.storyOptions)
        {
            bool isSelected = selectedOption != null && selectedOption == o;
            GUI.backgroundColor = isSelected ? Color.green : Color.red * fadeMultiplier;
            if (isSelected)
            {
                SirenixEditorGUI.BeginVerticalList(false, false, GUILayoutOptions.Width(120f));
                if (GUILayout.Button(o.optionID, GUILayout.Width(120), GUILayout.Height(60)))
                {
                    selectedOption = o;
                }
                GUI.backgroundColor = Color.blue;
                if (GUILayout.Button("GOTO", GUILayout.Width(120), GUILayout.Height(30)))
                {
                    selectedOption = o;
                }
                SirenixEditorGUI.EndVerticalList();
            }
            else
            {
                if (GUILayout.Button(o.optionID, GUILayout.Width(120), GUILayout.Height(90)))
                {
                    selectedOption = o;
                }
            }
            GUILayout.Space(10f);
        }
        GUILayout.Space(20f);
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("ADD NEW", GUILayout.Width(90), GUILayout.Height(90)))
        {
            selectedStory.storyOptions.Add(selectedOption = new StoryNodeOption("Option " + selectedStory.storyOptions.Count.ToString()));
        }

        SirenixEditorGUI.EndHorizontalToolbar();
        SirenixEditorGUI.EndBox();

        GUILayout.Space(10f);
        GUI.backgroundColor = oCol;

        DrawOptionConditions(selectedOption);
    }

    public void DrawButtons()
    {
        Color oCol = GUI.backgroundColor;
        SirenixEditorGUI.BeginHorizontalToolbar();
        GUI.backgroundColor = selectedPage == SelectedNarrativePage.OPTIONS ? Color.green : Color.gray;
        if(GUILayout.Button("OPTIONS", GUILayout.Width(200), GUILayout.Height(60)))
        {
            selectedPage = SelectedNarrativePage.OPTIONS;
        }
        GUILayout.Space(15f);
        GUI.backgroundColor = selectedPage == SelectedNarrativePage.REWARDS_AND_PENALTIES ? Color.green : Color.gray;
        if (GUILayout.Button("REWARDS & PENALTIES", GUILayout.Width(200), GUILayout.Height(60)))
        {
            selectedPage = SelectedNarrativePage.REWARDS_AND_PENALTIES;
            selectedOption = null;
        }
        SirenixEditorGUI.EndHorizontalToolbar();
        GUI.backgroundColor = oCol;
    }

    GUITabGroup tabGroup = null;
    public void DrawTabs()
    {
        var tree = this.Tree;
        var obj = this.target as StoryNarrative;

        tabGroup = SirenixEditorGUI.CreateAnimatedTabGroup(obj);
        
        // Register your tabs before starting BeginGroup.
        var tab1 = tabGroup.RegisterTab("Story Options");
        var tab2 = tabGroup.RegisterTab("Rewards & Penalties");

        tabGroup.BeginGroup(drawToolbar: true);
        {
            if (tab1.BeginPage())
            {
                var headerProp = tree.GetPropertyAtPath("storyBlockData.NarrativeBlockID");
                SirenixEditorGUI.BeginBox("Header");
                headerProp.Draw(new GUIContent("Tree ID"));
                SirenixEditorGUI.EndBox();
            }
            tab1.EndPage();

            if (tab2.BeginPage())
            {
                var headerProp = tree.GetPropertyAtPath("storyBlockData.NarrativeBlockID");
                SirenixEditorGUI.BeginBox("Header");
                headerProp.Draw(new GUIContent("Tree ID"));
                SirenixEditorGUI.EndBox();
            }
            tab2.EndPage();
        }
        tabGroup.EndGroup();

        // Control the animation speed.
        tabGroup.AnimationSpeed = 0.2f;

        // If true, the tab group will have the height equal to the biggest page. Otherwise the tab group will animate in height as well when changing page.
        tabGroup.FixedHeight = true;

        // You can change page by calling:
        //tabGroup.GoToNextPage();
        //tabGroup.GoToPreviousPage();
        
    }
}
#endif

#endif