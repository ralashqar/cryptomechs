using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Demos.RPGEditor;

public enum NodeVisualType
{
    VILLAGE,
    CITY
}


public enum NodeType
{
    NEUTRAL,
    VILLAGE,
    CITY
}

[ExecuteInEditMode]
public class MapNodeBase : MonoBehaviour
{
    public MapLocation locationData;

    public string ID = "";
    public Vector2 MapCoord;
    public Sprite MapGraphic;
    public float localMapScale = 1;

    public List<NodePath> paths;
    public List<MapNodeBase> neighborNodes;

    public NodeType nodeType = NodeType.VILLAGE;

    //public TerrainType terrainType = TerrainType.NEUTRAL;

    public float banditChance = 0.2f;
    public float huntingChance = 0.4f;

    public StoryNarrativeBlockData story;
    public string narrativeStoryID = "";

    public void Initialize()
    {
        switch (nodeType)
        {
            case NodeType.VILLAGE:
                break;
            case NodeType.CITY:
                break;
        }
    }

    public void AddPath(NodePath path)
    {
        MapNodeBase newNeigbor = path.NodeB != this ? path.NodeB : path.NodeA;
        if (!neighborNodes.Contains(newNeigbor))
        {
            paths.Add(path);
            neighborNodes.Add(newNeigbor);
        }
    }

    public void RemovePath(NodePath path)
    {
        MapNodeBase newNeigbor = path.NodeB != this ? path.NodeB : path.NodeA;
        //if (!neighborNodes.Contains(newNeigbor))
        //{
        paths.Remove(path);
        neighborNodes.Remove(newNeigbor);
        //}
    }

    private void OnDestroy()
    {
        //DestroyNode();
    }

    public void DestroyNode()
    {
        foreach (NodePath path in paths)
        {
            if (path.NodeA != this)
            {
                path.NodeA.RemovePath(path);
            }
            if (path.NodeB != this)
            {
                path.NodeB.RemovePath(path);
            }

            SafeDestroy(path.gameObject);
        }
    }

    public static T SafeDestroy<T>(T obj) where T : Object
    {
        if (Application.isEditor)
            Object.DestroyImmediate(obj);
        else
            Object.Destroy(obj);

        return null;
    }
    public static T SafeDestroyGameObject<T>(T component) where T : Component
    {
        if (component != null)
            SafeDestroy(component.gameObject);
        return null;
    }

}
