using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class StoryNarrativeO : SerializedScriptableObject
{
    public MapNodeBase mapNode;

    public StoryNarrativeBlockData storyBlockData;

    public StoryNode firstStoryNode;

    //public void TriggerStoryNarrative()
    //{
    //    firstStoryNode = GetFirstStoryNodeFromPlayerMapState();
    //}

    //public StoryNode GetFirstStoryNodeFromPlayerMapState()
    //{
    ///    return candidateFirstStoryNodes.Count == 1 ? candidateFirstStoryNodes[0] : candidateFirstStoryNodes[0];
    //}
}

[System.Serializable]
public class StoryNarrativeBlockDataO
{
    public string NarrativeBlockID;
    public List<StoryNodeOption> candidateFirstStoryIDs;
}