using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NarrativeStoryManager : MonoBehaviour
{
    public static NarrativeStoryManager Instance;

    public Image CharacterNarratorImage;
    public TextMeshProUGUI narrationText;
    public GameObject OptionsContainer;
    public StoryNarrative activeStoryTree;
    [HideInInspector]
    public StoryNode activeStory;

    public void OnOptionSelected(StoryNodeOption option)
    {
        //TriggerStoryNode(option.nextStory);
    }

    public void TriggerNarrative(StoryNarrative storyNarrative, CharacterAgentController playerProfile = null)
    {
        this.activeStoryTree = storyNarrative;
        StoryNode story = storyNarrative.storyBlockData.candidateFirstStoryIDs[0].nextStory;
        //TriggerStoryNode(story);
    }

    public void ResetOptionsUI()
    {
        for (int i = 0; i < OptionsContainer.transform.childCount; ++i)
        {
            OptionsContainer.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void SetStoryNodeText(string narrativeText)
    {
        this.narrationText.SetText(narrativeText);
    }

    public void SetupOptions(List <StoryNodeOption> options, List<UnityEngine.Events.UnityAction> optionActions)
    {
        ResetOptionsUI();
        for (int i = 0; i < options.Count; ++i)
        {
            if (i >= OptionsContainer.transform.childCount) break;
            StoryNodeOption option = options[i];
            GameObject optionGO = OptionsContainer.transform.GetChild(i).gameObject;
            optionGO.SetActive(true);
            Button optionButton = optionGO.GetComponentInChildren<Button>();
            optionGO.GetComponentInChildren<Text>().text = option.optionText;
            optionButton.onClick.RemoveAllListeners();
            optionButton.onClick.AddListener(optionActions[i]);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        //if (activeStoryTree != null)
        //    TriggerNarrative(activeStoryTree);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
