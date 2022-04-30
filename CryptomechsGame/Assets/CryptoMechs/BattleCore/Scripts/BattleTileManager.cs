using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector.Demos.RPGEditor;

public class BattleTileManager : MonoBehaviour
{
    public static BattleTileManager Instance;
    public List<BattleTile> tiles;
    public BattleTile selectedTile;
    private int lastSelectedTileID = -1;
    public AbilityCard moveAction;

    private BattleTile lastPathGenerated;

    public void GeneratePaths(BattleTile tile)
    {
        tile.GeneratePaths();
        lastPathGenerated = tile;
    }

    public List<HexTileBase> GetPath(BattleTile source, BattleTile target)
    {
        List<HexTileBase> path = new List<HexTileBase>();
        GeneratePaths(source);
        HexTileBase current = target.tileBase;
        if (!current.traversible)
        {
            float closestD = float.MaxValue;
            HexTileBase closestN = current;
            foreach(var n in current.adjacentNodes)
            {
                if (n.cachedClosestDistance < closestD)
                {
                    closestD = n.cachedClosestDistance;
                    closestN = n;
                }
            }
            path.Add(current);
            current = closestN;
        }

        while (current != source.tileBase)
        {
            path.Add(current);
            current = current.cachedPathNode;
        }
        path.Reverse();
        return path;
    }

    public bool GetIsStraightClearPath(BattleTile source, List<HexTileBase> path)
    {
        Vector3 line = Vector3.zero;
        for (int i = 0; i < path.Count; ++i)
        {
            var t = path[i];
            if (i == 0)
            {
                line = t.GetPosition() - source.GetPosition();
            }
            else
            {
                Vector3 l = t.GetPosition() - source.GetPosition();
                if (Vector3.Angle(l, line) > 5)
                    return false;
            }
        }
        return true;
    }

    public int GetPathDistance(List<HexTileBase> path)
    {
        return path.Count;
    }

    public int GetDistance(BattleTile source, BattleTile target)
    {
        List<HexTileBase> path = GetPath(source, target);
        return path.Count;
    }

    public BattleTile GetTile(int id)
    {
        return tiles[id];
    }

    public int GetSelectedTileID()
    {
        return lastSelectedTileID;
    }

    public void SetSelectedTile(BattleTile tile)
    {
        this.selectedTile = tile;
        this.lastSelectedTileID = tile.GetID();
    }

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        this.tiles = FindObjectsOfType<BattleTile>()?.ToList();
        if (tiles != null)
        {
            int id = 0;
            foreach (var t in tiles)
            {
                if (t != null)
                    t?.SetID(id++);

                t.neighborTiles = new List<BattleTile>();
                foreach (var n in t.tileBase.adjacentNodes)
                {
                    t.neighborTiles.Add(n.gameObject.GetComponent<BattleTile>());
                }
            }
        }
    }

    public void HandleClick()
    {
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool isUIClicked = EventSystem.current.IsPointerOverGameObject();

            //Physics.queriesHitBackfaces = true;
            if (!isUIClicked && Physics.Raycast(ray, out hit))
            {
                //Debug.Log("Ray has been Cast and hit an Object");
                var targetPos = hit.collider.gameObject.transform.position; //Save the position of the object mouse was over

                //Debug.Log("Target Position: " + hit.collider.gameObject.transform.position);
                var go = hit.collider.gameObject;
                var m = go.GetComponentInParent<BattleTile>();
                if (m == null) return;
                SetSelectedTile(m);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleClick();
    }
}
