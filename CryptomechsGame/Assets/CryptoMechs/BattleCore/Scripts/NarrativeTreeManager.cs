using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrativeTreeManager : CharacterComponentManager
{
    public Dictionary<StoryNarrative, List<StoryNode>> completedStories;
    public List<StoryNarrative> completedNarratives;
    public List<StoryNarrative> activeNarratives;
    public Dictionary <StoryNarrative, List<StoryNodeOption>> storiesAwaitingTrigger;

    public StoryNarrative activeStoryTree;
    [HideInInspector]
    public StoryNode activeStory;

    public GameObject storyUIContainer;
    public NarrativeStoryManager storyUI;

    public CharacterAgentController InteractingNPC;

    public bool IsNarrativeInteractionActive;

    public string lastTrigger = "";
    
    public void EvaluateLocationTrigger(LocatorTrigger trigger)
    {
        if (trigger.storyNarrative != null)
        {
            foreach (StoryNodeOption op in trigger.storyNarrative.storyBlockData.candidateFirstStoryIDs)
            {
                if (op.triggerFrom == StoryOptionTriggerMode.LOCATION_TRIGGER && trigger.id == op.triggerID)
                {
                    character.SetState(new NarrativeInteractionState(character));
                    //TriggerNarrative()
                    TriggerNarrativeAtStory(trigger.storyNarrative, op.nextStory);
                    trigger.gameObject.SetActive(false);
                }
            }
        }
        
        if (storiesAwaitingTrigger == null) return;
        foreach (var n in storiesAwaitingTrigger)
        {
            foreach (StoryNodeOption op in n.Value)
            {
                if (op.triggerFrom == StoryOptionTriggerMode.LOCATION_TRIGGER && trigger.id == op.triggerID)
                {
                    character.SetState(new NarrativeInteractionState(character));
                    TriggerNarrativeAtStory(n.Key, op.nextStory);
                    trigger.gameObject.SetActive(false);
                }
            }
        }
        /*
        foreach(StoryNarrative n in activeNarratives)
        {
            StoryNode lastStory = completedStories[n][completedStories[n].Count - 1];
            foreach(StoryNodeOption op in lastStory.storyOptions)
            {
                if (op.triggerFrom == StoryOptionTriggerMode.LOCATION_TRIGGER && trigger.id == op.triggerID)
                {
                    character.SetState(new NarrativeInteractionState(character));
                    TriggerStoryNode(op.nextStory);
                    trigger.gameObject.SetActive(false);
                }
            }
        }
        */
    }

    public NarrativeTreeManager(CharacterAgentController character) : base(character)
    {
        return;
        storyUIContainer = GameObject.Instantiate(Resources.Load("Prefabs/HUD/NarrativeTreeUI") as GameObject);
        storyUI = storyUIContainer.GetComponentInChildren<NarrativeStoryManager>();
        storyUIContainer.SetActive(false);
        InteractingNPC = null;
    }

    public void EndNarrativeTreeInteraction()
    {
        CompleteStory(activeStory);
        CompleteNarrative(activeStoryTree);
        activeStory = null;
        activeStoryTree = null;
        storyUIContainer.SetActive(false);
        BreakNarrativeInteraction();
    }

    public void ListenForTrigger(StoryNodeOption option)
    {
        if (storiesAwaitingTrigger == null) storiesAwaitingTrigger = new Dictionary<StoryNarrative, List<StoryNodeOption>>();
        if (!storiesAwaitingTrigger.ContainsKey(activeStoryTree))
        {
            storiesAwaitingTrigger.Add(activeStoryTree, new List<StoryNodeOption>());
        }
        storiesAwaitingTrigger[activeStoryTree].Add(option);
    }

    public void OnOptionSelected(StoryNodeOption option)
    {
        TriggerStoryNode(option.nextStory);
    }

    public bool CanTriggerStoryOption(StoryNodeOption option)
    {
        if (option.triggerFrom == StoryOptionTriggerMode.LOCATION_TRIGGER) return false;

        if (option.unlockCriteriaCaravan == null || option.unlockCriteriaCaravan.Count == 0)
            return true;
        foreach (UnlockStoryCriteria condition in option.unlockCriteriaCaravan)
        {
            switch (condition.unlockType)
            {
                case UnlockCriteriaType.SLAIN:
                    if (this.character.inventoryManager.UnitsKilled.ContainsKey(condition.character))
                    {
                        if (this.character.inventoryManager.UnitsKilled[condition.character] < condition.amountRequired)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                    break;
                case UnlockCriteriaType.OWNED:
                    if (this.character.inventoryManager.NumberOfItemsHeld(condition.item) < condition.amountRequired)
                    {
                        return false;
                    }
                    break;
            }
        }
        return true;
    }

    public bool CanTriggerNarrative(StoryNarrative storyNarrative, CharacterAgentController interactingNPC)
    {
        foreach (StoryNodeOption o in storyNarrative.storyBlockData.candidateFirstStoryIDs)
        {
            if (CanTriggerStoryOption(o))
            {
                return true;
            }
        }
        return false;
        //StoryNodeOption option = storyNarrative.storyBlockData.candidateFirstStoryIDs[0];
        //return CanTriggerStoryOption(option);
    }

    public void CompleteStory(StoryNode story)
    {
        if (completedStories == null)
            completedStories = new Dictionary<StoryNarrative, List<StoryNode>>();

        if (!completedStories.ContainsKey(activeStoryTree))
        {
            completedStories.Add(activeStoryTree, new List<StoryNode>());
        }
        completedStories[activeStoryTree].Add(story);
    }

    public void TriggerNarrativeAtStory(StoryNarrative storyNarrative, StoryNode story)
    {
        storyUIContainer.gameObject.SetActive(true);
        this.activeStoryTree = storyNarrative;
        TriggerStoryNode(story);
        IsNarrativeInteractionActive = true;
    }

    public void TriggerNarrative(StoryNarrative storyNarrative, CharacterAgentController interactingNPC)
    {
        InteractingNPC = interactingNPC;
        storyUIContainer.gameObject.SetActive(true);
        this.activeStoryTree = storyNarrative;
        StoryNodeOption o = storyNarrative.storyBlockData.candidateFirstStoryIDs.Find(op => CanTriggerStoryOption(op));

        StoryNode story = o.nextStory;
        TriggerStoryNode(story);
        IsNarrativeInteractionActive = true;

        if (activeNarratives == null) activeNarratives = new List<StoryNarrative>();
        activeNarratives.Add(storyNarrative);
    }

    public ActionSequence activeSequence = null;

    public void Tick()
    {
        activeSequence?.Execute();
    }

    public void TriggerStoryNode(StoryNode story)
    {
        if (activeStory != null) CompleteStory(activeStory);

        if (story.triggerActionSequence && story.actionSequence != null)
        {
            this.activeSequence = story.actionSequence.actionSequence;
            story.actionSequence.actionSequence.Trigger();
        }
        else
        {
            activeSequence = null;
        }

        this.activeStory = story;

        storyUI.SetStoryNodeText(story.storyText);
        storyUI.ResetOptionsUI();

        List<StoryNodeOption> validOptions = new List<StoryNodeOption>();
        List<UnityEngine.Events.UnityAction> validOptionActions = new List<UnityEngine.Events.UnityAction>();

        for (int i = 0; i < story.storyOptions.Count; ++i)
        {
            StoryNodeOption option = story.storyOptions[i];
            if (CanTriggerStoryOption(option))
            {
                validOptions.Add(option);
                validOptionActions.Add(delegate { OnOptionSelected(option); });
            }
            else
            {
                if (option.triggerFrom == StoryOptionTriggerMode.LOCATION_TRIGGER)
                {
                    ListenForTrigger(option);
                }
            }
        }

        if (validOptions.Count > 0)
        {
            storyUI.SetStoryNodeText(story.storyText);
            storyUI.SetupOptions(validOptions, validOptionActions);
        }
        else
        {
            EndNarrativeTreeInteraction();
        }
    }
    public void BreakNarrativeInteraction()
    {
        IsNarrativeInteractionActive = false;
    }

    public void CompleteNarrative(StoryNarrative narrative)
    {
        activeNarratives.Remove(narrative);
        completedStories.Remove(narrative);
        if (completedNarratives == null) completedNarratives = new List<StoryNarrative>();
        completedNarratives.Add(narrative);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (storyUI != null)
            GameObject.Destroy(storyUI);
    }
}
