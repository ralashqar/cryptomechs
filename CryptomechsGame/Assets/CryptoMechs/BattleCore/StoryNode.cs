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
using System.Linq;
using Sirenix.OdinInspector.Demos.RPGEditor;

public enum StoryType
{
    NARRATIVE,
    PLAYEROPTIONS
}

public enum NarratorMode
{
    NARRATOR,
    TARGET_NPC,
    PLAYER,
    CUSTOM_CHARACTER
}

public enum StoryOptionTriggerMode
{
    NARRATIVE_DEFAULT,
    LOCATION_TRIGGER
}

public enum UnlockCriteriaType
{
    OWNED,
    CONSUMED,
    SLAIN
}

[System.Serializable]
public class RewardsAndPenalties
{
    public ItemBase item;
    public int amountChanged;
}

[System.Serializable]
public class BaseRewardsAndPenalties
{
    public int goldAmountChanged = 0;
    public int dirhamAmountChanged = 0;
    public float journeyTimeAdded = 0;
}

[System.Serializable]
public class BasePathUnlockConditions
{
    public int goldRequired = 0;
    public int dirhamRequired = 0;
}

[System.Serializable]
public class StoryReference
{
    public StoryNarrative storyAsset;
    public StoryNode storyNode;
    public string storyID;
}

[System.Serializable]
public class UnlockStoryCriteria
{
    public UnlockCriteriaType unlockType;
    [ShowIf("unlockType", UnlockCriteriaType.SLAIN)]
    public Character character;
    [ShowIf("unlockType", UnlockCriteriaType.OWNED)]
    //[ShowIf("unlockType", UnlockCriteriaType.CONSUMED)]
    public ItemBase item;
    public int amountRequired;
}


[System.Serializable]
public class StoryNodeOption
{
    [LabelWidth (80)]
    public string optionID = "";

    [TextArea(4, 14)]
    public string optionText;
    //[HideInInspector]

    //[HideLabel]
    //[System.NonSerialized, OdinSerialize]
    public StoryNode nextStory;

    public StoryOptionTriggerMode triggerFrom;
    public string triggerID = "";
    public List<UnlockStoryCriteria> unlockCriteriaCaravan;
    public StatList statRequirements;
    public BasePathUnlockConditions baseUnlockConditions;
    public List<StoryReference> storyUnlockConditions;

    public List<string> completeStoryIdsToUnlock;

    public bool isRootStory = false;

    public StoryNodeOption(string optionText = "")
    {
        this.optionID = optionText;
        this.nextStory = new StoryNode(optionText);
    }
}

[System.Serializable]
public class StoryNode 
{
    public StoryNode(string id)
    {
        this.storyID = id;
        this.storyOptions = new List<StoryNodeOption>();
    }

    //[BoxGroup("Story")]
    [LabelWidth(80)]
    public string storyID;

    //Tree Hierarchy paths
    public string storyParentPath = "";
    public string storyPath = "";

    //[BoxGroup("Story")]
    //[HideLabel, TextArea(4, 14)]
    //[ShowInInspector]
    [TextArea(12, 34)]
    public string storyText;

    public List<StoryNodeOption> storyOptions;

    [LabelWidth(120)]
    public NarratorMode narratedBy = NarratorMode.NARRATOR;
    [LabelWidth(120)]
    public Character narratingCharacter;

    [HideInInspector]
    public string nextStoryID;

    //public List<StoryNode> branchNewStories;

    public float addedJourneyTime = 0;
    public List<string> ForeshadowsLandIds;
    public List<RewardsAndPenalties> rewardsAndPenalties;
    public BaseRewardsAndPenalties baseRewardsAndPenalties;

    public bool triggerActionSequence = false;
    public ActionSequenceBehavior actionSequence;

    //[VerticalGroup("Split/Right")]
    //public StatList statRequirements;

    public bool isFinalStory = false;

    public void GetAllStoriesRecursive(ref List<StoryNode> nodes, string storyPath = "")
    {
        if (nodes == null) return;
        if (storyOptions == null)
        {
            return;
        }
        foreach(StoryNodeOption o in storyOptions)
        {
            if (o.nextStory == null)
                o.nextStory = new StoryNode("");
            if (!string.IsNullOrWhiteSpace(storyPath))
                o.nextStory.storyParentPath = "/" + storyPath;
            nodes.Add(o.nextStory);
            string nextParentPath = storyPath + "/" + o.nextStory.storyPath;
            o.nextStory.GetAllStoriesRecursive(ref nodes, nextParentPath);
        }
    }
}

#if UNITY_EDITOR
public class StoryReferenceDrawer : OdinValueDrawer<StoryReference>
{
    private int selectedStoryIndex = -1;
    protected override void DrawPropertyLayout(GUIContent label)
    {
        var selectedReference = this.ValueEntry.SmartValue as StoryReference;
        selectedReference.storyAsset = EditorGUILayout.ObjectField(selectedReference.storyAsset, typeof(StoryNarrative), true) as StoryNarrative;

        if (selectedReference.storyAsset != null)
        {
            List<StoryNode> stories = selectedReference.storyAsset.GetAllStoriesRecursive();
            if (stories == null || stories.Count == 0) return;

            selectedStoryIndex = stories.FindIndex(s => s == selectedReference.storyNode);
            if (selectedStoryIndex == -1) selectedStoryIndex = 0;

            int newSelectedIndex = EditorGUILayout.Popup(selectedStoryIndex, stories.Select(x => x.storyID).ToArray());
            if (newSelectedIndex != selectedStoryIndex)
            {
                selectedStoryIndex = newSelectedIndex;
                selectedReference.storyNode = stories[selectedStoryIndex];
            }
        }
    }
}

public class StoryNodeDrawer : OdinValueDrawer<StoryNode>
{
    public StoryNode selectedStory = null;
    public StoryNodeOption selectedOption = null;
    public SelectedNarrativePage selectedPage = SelectedNarrativePage.OPTIONS;

    private string rootPath = "";

    protected override void DrawPropertyLayout(GUIContent label)
    {
        selectedStory = this.ValueEntry.SmartValue as StoryNode;
        //var rootPath = this.Property.Path + ".StoryNode";
        var tree = this.Property.Tree;
        rootPath = "";// + ".StoryNode";
        if (tree.GetPropertyAtPath("storyBlockData") != null)
        {
            rootPath = tree.GetPropertyAtPath("storyBlockData.candidateFirstStoryIDs").Children[0].Path + ".nextStory.";
        }
        //InspectorUtilities.BeginDrawPropertyTree(tree, true);

        SirenixEditorGUI.BeginHorizontalToolbar();
        //var rootPath = tree.GetPropertyAtPath("storyBlockData.candidateFirstStoryIDs").Children[0].Path;
        SirenixEditorGUI.BeginBox(GUILayoutOptions.MinHeight(150f));
        //SirenixEditorGUI.BeginHorizontalToolbar();
        var storyID = tree.GetPropertyAtPath(rootPath + "storyID");
        storyID.Draw(new GUIContent("ID"));
        var journeyTime = tree.GetPropertyAtPath(rootPath + "addedJourneyTime");
        journeyTime.Draw(new GUIContent("Journey Time (Days)"));
        //SirenixEditorGUI.EndHorizontalToolbar();
        var storyText = tree.GetPropertyAtPath(rootPath + "storyText");
        storyText.Draw(new GUIContent("Body Text"));

        var narratedBy = tree.GetPropertyAtPath(rootPath + "narratedBy");
        narratedBy.Draw(new GUIContent("Narrated By"));
        if (selectedStory.narratedBy == NarratorMode.CUSTOM_CHARACTER)
        {
            var narratingCharacter = tree.GetPropertyAtPath(rootPath + "narratingCharacter");
            narratingCharacter.Draw(new GUIContent("Narrating Character"));
        }

        SirenixEditorGUI.EndBox();

        SirenixEditorGUI.BeginBox();// GUILayoutOptions.MaxWidth(110f));
        var baseRewardsAndPenalties = tree.GetPropertyAtPath(rootPath + "baseRewardsAndPenalties");
        baseRewardsAndPenalties.Draw(new GUIContent("Base Rewards & Penalties"));
        var rewardsAndPenalties = tree.GetPropertyAtPath(rootPath + "rewardsAndPenalties");
        rewardsAndPenalties.Draw(new GUIContent("Specific Rewards & Penalties"));
        SirenixEditorGUI.EndBox();

        SirenixEditorGUI.EndHorizontalToolbar();

        selectedPage = SelectedNarrativePage.OPTIONS;
        //DrawButtons();
        DrawOptions();
        //DrawTabs();

        //InspectorUtilities.EndDrawPropertyTree(tree);

        // You can also call base.OnInspectorGUI(); instead if you simply want to prepend or append GUI to the editor.
        var triggerActionSequence = tree.GetPropertyAtPath(rootPath + "triggerActionSequence");
        triggerActionSequence.Draw(new GUIContent("Trigger In-game Action Sequence"));
        if (selectedStory.triggerActionSequence)
        {
            var actionSequence = tree.GetPropertyAtPath(rootPath + "actionSequence");
            actionSequence.Draw(new GUIContent("Action Sequence"));
        }
    }

    //public string GetStoryPropertyPath(StoryNarrative narrative, string property)
    //{

    //}
    public void ForceRebuildTree()
    {
        OdinMenuEditorWindow menuWindow = (GUIHelper.CurrentWindow as OdinMenuEditorWindow);
        menuWindow?.ForceMenuTreeRebuild();
    }

    public void DrawOptions()
    {
        if (selectedPage != SelectedNarrativePage.OPTIONS || selectedStory == null)
            return;

        //var rootPath = this.Property.Path;
        var tree = this.Property.Tree;
        var obj = this.ValueEntry.SmartValue as StoryNode;

        Color oCol = GUI.backgroundColor;

        GUILayout.Space(10f);

        Vector4 fadeMultiplier = new Vector4(1, 1, 1, 0.1f);

        SirenixEditorGUI.BeginBox("OPTIONS");
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
            selectedStory.storyOptions.Add(selectedOption = new StoryNodeOption("Option " + selectedStory.storyOptions.Count.ToString()));
            ForceRebuildTree();
        }

        SirenixEditorGUI.EndHorizontalToolbar();
        SirenixEditorGUI.EndBox();

        GUILayout.Space(10f);
        GUI.backgroundColor = oCol;

        if (selectedOption == null) return;

        int index = selectedStory.storyOptions.IndexOf(selectedOption);
        if (index >= 0 && index < tree.GetPropertyAtPath(rootPath + "storyOptions").Children.Count)
        {
            //var p = tree.GetPropertyAtPath("storyBlockData.candidateFirstStoryIDs").Children[0].Path;
            var p = tree.GetPropertyAtPath(rootPath + "storyOptions").Children[index].Path;

            var optionID = tree.GetPropertyAtPath(p + ".optionID");
            var optionText = tree.GetPropertyAtPath(p + ".optionText");

            var optionTriggerFrom = tree.GetPropertyAtPath(p + ".triggerFrom");
            var optionTriggerID = tree.GetPropertyAtPath(p + ".triggerID");

            var optionUnlockConditions = tree.GetPropertyAtPath(p + ".unlockCriteriaCaravan");
            var foreshadowedLands = tree.GetPropertyAtPath(p + ".unlockCriteriaCaravan");

            var statRequirements = tree.GetPropertyAtPath(p + ".statRequirements");
            var baseUnlockConditions = tree.GetPropertyAtPath(p + ".baseUnlockConditions");

            var storyUnlockConditions = tree.GetPropertyAtPath(p + ".storyUnlockConditions");

            SirenixEditorGUI.BeginHorizontalToolbar();
            SirenixEditorGUI.BeginBox("Option");
            optionID.Draw();
            optionText.Draw();
            
            optionTriggerFrom.Draw();
            if (selectedOption.triggerFrom == StoryOptionTriggerMode.LOCATION_TRIGGER)
            {
                optionTriggerID.Draw();
            }

            if (GUILayout.Button("DELETE"))
            {
                selectedStory.storyOptions.Remove(selectedOption);
                selectedOption = null;
                ForceRebuildTree();
            }

            SirenixEditorGUI.EndBox();

            SirenixEditorGUI.BeginBox("Unlock Conditions");
            baseUnlockConditions.Draw();
            optionUnlockConditions.Draw();
            statRequirements.Draw();
            storyUnlockConditions.Draw(new GUIContent("Completed Story Conditions"));
            SirenixEditorGUI.EndBox();
            SirenixEditorGUI.EndHorizontalToolbar();

            //SirenixEditorGUI.DrawThickHorizontalSeparator();// new GUIContent("Option"));
            //SirenixEditorGUI.EndHorizontalPropertyLayout();
        }
    }

    public void DrawButtons()
    {
        Color oCol = GUI.backgroundColor;
        SirenixEditorGUI.BeginHorizontalToolbar();
        GUI.backgroundColor = selectedPage == SelectedNarrativePage.OPTIONS ? Color.green : Color.gray;
        if (GUILayout.Button("OPTIONS", GUILayout.Width(200), GUILayout.Height(60)))
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
}
#endif


