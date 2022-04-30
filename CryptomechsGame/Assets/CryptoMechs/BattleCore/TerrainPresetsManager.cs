using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class TerrainPresetsData
{
    //employees is case sensitive and must match the string "employees" in the JSON.
    public TerrainPreset[] TerrainPresets;
}

public class TerrainPresetsManager : MonoBehaviour
{
    public static TerrainPresetsManager Instance = null;

    public TerrainPresetsData presetData;

    //public List<TerrainPreset> TerrainPresets;
    Dictionary<string, TerrainPreset> TerrainPresetDict = new Dictionary<string, TerrainPreset>();

    string filename = "Metadata/TerrainPresets.json";

    public TextAsset jsonFile;

    //*********************************************************************************
    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("TerrainPresetsManager is already instantiated");
            return;
        }

        Instance = this;

        //TerrainPresets = new List<TerrainPreset>();

        LoadPresets();
    }

    //*********************************************************************************
    void OnDestroy()
    {
        Instance = null;
    }

    public void LoadPresets()
    {
        presetData = JsonUtility.FromJson<TerrainPresetsData>(jsonFile.text);

        //foreach (TerrainPreset preset in terainPresets.TerrainPresets)
        //{
        //    Debug.Log("Found terrain preset: " + preset.ID);
            //TerrainPresets.Add(preset);
        //}
    }

#if UNITY_EDITOR
    public void SavePresets()
    {
        if (presetData != null && presetData.TerrainPresets != null)
        File.WriteAllText(AssetDatabase.GetAssetPath(jsonFile), PrettyStringHack(JsonUtility.ToJson(presetData)));
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


