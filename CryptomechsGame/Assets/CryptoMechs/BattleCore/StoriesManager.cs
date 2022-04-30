using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class StoriesBlocksData
{
    public StoryNarrativeBlockData[] StoriesBlocks;
}

public class StoriesManager : MonoBehaviour
{
    public static StoriesManager Instance = null;

    public StoriesBlocksData storyBlocksData;

    string filename = "Metadata/StoryBlocks.json";

    public TextAsset jsonFile;

    //*********************************************************************************
    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("StoriesManager is already instantiated");
            return;
        }

        Instance = this;

        //TerrainPresets = new List<TerrainPreset>();

        LoadStoryBlocks();
    }

    //*********************************************************************************
    void OnDestroy()
    {
        Instance = null;
    }

    public void LoadStoryBlocks()
    {
        storyBlocksData = JsonUtility.FromJson<StoriesBlocksData>(jsonFile.text);
    }

#if UNITY_EDITOR
    public void SaveStoryBlocks()
    {
        if (storyBlocksData != null && storyBlocksData.StoriesBlocks != null)
            File.WriteAllText(AssetDatabase.GetAssetPath(jsonFile), PrettyStringHack(JsonUtility.ToJson(storyBlocksData)));
        EditorUtility.SetDirty(jsonFile);
    }
#endif

    public string PrettyStringHack(string json)
    {
        //RA: MANUAL PRETTY PRINT HACK - align to existing metadata format to make it SVN / diff friendly
        json = json.Replace("[{", "[\n  {\n    ");
        json = json.Replace("},{", "\n  },\n  {\n    ");
        json = json.Replace(",\"", ",\n    \"");
        json = json.Replace("[", "\n[");
        json = json.Replace("]", "\n]");
        json = json.Replace(":", ": ");
        json = json.Replace("{\"", "{\n  \"");
        json = json.Replace("}]}}", "\n  }\n]\n}\n}");

        return json;
    }
}


