using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(HexTiling))]
public class HexTilingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var hexTiler = target as HexTiling;
        if (GUILayout.Button("GENERATE"))
        {
            hexTiler.GenerateTiles();
        }
    }
}
