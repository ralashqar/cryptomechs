//#if UNITY_EDITOR
//using Sirenix.OdinInspector.Demos.RPGEditor;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class UnitBase3D : UnitBase
{
    [BoxGroup(STATS_BOX_GROUP)]
    //[("CharacterPrefab"), 75]
    [BoxGroup(STATS_BOX_GROUP + "/Visuals")]
    public bool Has3DRepresentation = false;

    [ShowIf("Has3DRepresentation")]
    [BoxGroup(STATS_BOX_GROUP + "/Visuals")]
    [HideLabel]
    [PreviewField(75, ObjectFieldAlignment.Left)]
    public GameObject prefabObj;

    [ShowIf("Has3DRepresentation")]
    [BoxGroup(STATS_BOX_GROUP + "/Visuals")]
    public float scale = 1f;
}

public class UnitBase : SerializedScriptableObject
{
    protected const string LEFT_VERTICAL_GROUP = "Split/Left";
    protected const string STATS_BOX_GROUP = "Split/Left/Stats";
    protected const string RIGHT_VERTICAL_GROUP = "Split/Right";
    protected const string GENERAL_SETTINGS_VERTICAL_GROUP = "Split/Left/General Settings/Split/Right";

    [PropertyOrder(-1)]
    [HideLabel, PreviewField(55)]
    [VerticalGroup(LEFT_VERTICAL_GROUP)]
    [HorizontalGroup(LEFT_VERTICAL_GROUP + "/General Settings/Split", 55, LabelWidth = 67)]
    public Texture Icon;

    [PropertyOrder(-1)]
    [BoxGroup(LEFT_VERTICAL_GROUP + "/General Settings")]
    [VerticalGroup(GENERAL_SETTINGS_VERTICAL_GROUP)]
    public string Name;
}

//#endif
