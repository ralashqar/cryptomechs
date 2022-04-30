#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapHexagonalGrid))]
[CanEditMultipleObjects]
public class HexagonalMapInspector : Editor
{
    
    private MapHexagonalGrid script { get { return target as MapHexagonalGrid; } }
    private MapHexagonalGrid clone;
    private int selectedPoint = -1, deletePoint = -1, addPoint = -1;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(10f);
        if (GUILayout.Button("Re-Initialize"))
        {
            script.Initialize();
        }
        if (GUILayout.Button("ReCenter"))
        {
            script.xOffset = -(script.numRows * script.Dimension)/2f;
            script.zOffset = -(script.numColumns * script.Dimension) / 2f;
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Generate Tiles"))
        {
            script.GenerateTiles();
        }
        if (GUILayout.Button("Clear Tiles"))
        {
            script.ClearTiles();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Expand"))
        {
            script.ExpandActiveCells();
            SceneView.RepaintAll();
        }

        GUILayout.Label(lastRow.ToString() + " : " + lastColumn.ToString());
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += CustomOnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= CustomOnSceneGUI;
    }

    public void SetDefaultHandleColor()
    {
        Color col = Color.red;
        col.a = 0.25f;
        //Handles.color = Color.red;
        Handles.color = col;
    }

    int lastRow = 0;
    int lastColumn = 0;
    void CustomOnSceneGUI(SceneView sceneview)
    {
        if (!script.gameObject.activeInHierarchy) return;

        if (script.isActive == null) script.Initialize();
        GUI.changed = false;
        Handles.color = Color.red;
        //if (script.isActive == null) return;

        Vector3 origin = script.transform.position;
        for (int i = 0; i < script.numRows; ++i)
            for (int j = 0; j < script.numColumns; ++j)
            {
                int cellIndex = script.GetCellIndexFromRowColumn(i, j);
                Vector3 pos = script.GetPosition(i, j);

                SetDefaultHandleColor();
                if (Inspector.DotButton(pos, Quaternion.identity, script.Dimension * 0.3f, script.Dimension * 0.5f))
                {
                    selectedPoint = cellIndex;
                    script.ToggleCell(cellIndex);
                    lastRow = i;
                    lastColumn = j;
                }

                if (script.IsCellActive(cellIndex))
                {
                    Handles.color = Color.green;
                }
                else
                {
                    SetDefaultHandleColor();
                }
                //Handles.DrawSolidRectangleWithOutline(new Rect(pos, Vector2.one * script.dimension), Handles.color, Color.white);
                //Handles.DrawWireCube(pos, Vector3.one * script.dimension * 0.85f);
                Handles.DrawSolidDisc(pos, Vector3.up, script.Dimension / 2 * 0.85f);
            }
    }
    /*
    private void OnSceneGUI()
    {
        GUI.changed = false;
        Handles.color = Color.red;
        //if (script.isActive == null) return;

        Vector3 origin = script.transform.position;
        for (int i = 0; i < script.numRows; ++ i)
            for (int j = 0; j < script.numColumns; ++j)
            {
                Vector3 xOffset = Vector3.zero;
                if (i % 2 == 0) xOffset = Vector3.forward * script.dimension / 2;
                Vector3 pos = origin + Vector3.right * (i * script.dimension) + Vector3.forward * (j * script.dimension) + xOffset;

                int cellIndex = script.GetCellIndexFromRowColumn(i, j);
                if (Inspector.DotButton(pos, Quaternion.identity, script.dimension * 0.05f, script.dimension * 0.5f))
                {
                    selectedPoint = cellIndex;
                    script.ToggleCell(cellIndex);
                }

                if (script.IsCellActive(cellIndex))
                {
                    Handles.color = Color.green;
                }
                else
                {
                    Color col = Color.red;
                    col.a = 0.25f;
                    //Handles.color = Color.red;
                    Handles.color = col;
                }
                //Handles.DrawSolidRectangleWithOutline(new Rect(pos, Vector2.one * script.dimension), Handles.color, Color.white);
                //Handles.DrawWireCube(pos, Vector3.one * script.dimension * 0.85f);
                Handles.DrawSolidDisc(pos, Vector3.up, script.dimension/2 * 0.85f);
            }

    }
    */
}
#endif
