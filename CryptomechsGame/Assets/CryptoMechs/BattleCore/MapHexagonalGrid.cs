using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapHexagonalGrid : MonoBehaviour
{

    public int numRows = 20;
    public int numColumns = 20;
    public float dimension = 3f;
    public float Dimension { get { return dimension * scale; } }
    public float xOffset = 0;
    public float zOffset = 0;
    public float scale = 1f;

    public List<HexTileBase> tilesObjs;
    public GameObject tilePrefab;

    public List<bool> isActive;

    public List<List<HexTileBase>> tiles;
    public void GenerateTiles()
    {
        if (tilesObjs != null)
        {
            foreach (var tile in tilesObjs)
            {
                if (tile != null && tile.gameObject != null)
                    DestroyImmediate(tile.gameObject);
            }
            tilesObjs.Clear();
        }
        else
        {
            tilesObjs = new List<HexTileBase>();
        }

        tiles = new List<List<HexTileBase>>();

        Vector3 origin = transform.position;
        int id = 0;
        for (int i = 0; i < numRows; ++i)
        {
            List<HexTileBase> tileRow = new List<HexTileBase>();
            for (int j = 0; j < numColumns; ++j)
            {
                int cellIndex = GetCellIndexFromRowColumn(i, j);
                //if (!isActive[cellIndex]) continue;
                Vector3 pos = GetPosition(i, j);
                GameObject newTile = Instantiate(tilePrefab, pos, transform.rotation, this.transform);
                newTile.transform.localScale *= this.scale;
                var tileBase = newTile.AddComponent<HexTileBase>();
                tileBase.Setup(id++, i, j, dimension, isActive[cellIndex]);
                tilesObjs.Add(tileBase);
                tileRow.Add(tileBase);
            }
            tiles.Add(tileRow);
        }

        //set adjacencies
        foreach (var tile in tilesObjs)
        {
            int r = tile.GetRow();
            int c = tile.GetColumn();

            if (c - 1 >= 0)
                tile.l = tiles[r][c - 1];
            if (c + 1 < tiles[r].Count)
                tile.r = tiles[r][c + 1];

            if (tile.IsOddRow)
            {
                if (r - 1 >= 0)
                    tile.tr = tiles[r - 1][c];
                if (r - 1 >= 0 && c - 1 >= 0)
                    tile.tl = tiles[r - 1][c - 1];
                if (r + 1 < tiles.Count)
                    tile.br = tiles[r + 1][c];
                if (r + 1 < tiles.Count && c - 1 >= 0)
                    tile.bl = tiles[r + 1][c - 1];
            }

            else
            {
                if (r - 1 >= 0 && c + 1 < tiles[r - 1].Count)
                    tile.tr = tiles[r - 1][c + 1];
                if (r - 1 >= 0)
                    tile.tl = tiles[r - 1][c];
                if (r + 1 < tiles.Count)
                    tile.bl = tiles[r + 1][c];
                if (r + 1 < tiles.Count && c + 1 < tiles[r + 1].Count)
                    tile.br = tiles[r + 1][c + 1];
            }

            //tile.SetupAdjacencies();
        }

        for (int i = 0; i < tilesObjs.Count; ++i)
        {
            var tile = tilesObjs[i];
            if (!tile.isActive)
            {
                DestroyImmediate(tile.gameObject);
                tilesObjs.Remove(tile);
                i--;
            }
        }

        foreach (var tile in tilesObjs)
        {
            tile.SetupAdjacencies();
        }
    }

    public List<(int, int)> GetAdjacent(int r, int c)
    {
        List<(int, int)> adj = new List<(int, int)>();

        bool isOdd = r % 2 == 1;

        adj.Add((r, c - 1));
        adj.Add((r, c + 1));
         
        if (isOdd)
        {
            adj.Add((r - 1, c));
            adj.Add((r - 1, c - 1));
            adj.Add((r + 1, c));
            adj.Add((r + 1, c - 1));
        }

        else
        {
            adj.Add((r - 1, c + 1));
            adj.Add((r - 1, c));
            adj.Add((r + 1, c));
            adj.Add((r + 1, c + 1));
        }
        
        return adj;
    }


    public void ClearTiles()
    {
        if (tilesObjs != null)
        {
            foreach (var tile in tilesObjs)
            {
                if (tile != null && tile.gameObject != null)
                    DestroyImmediate(tile.gameObject);
            }
            tilesObjs.Clear();
        }
        else
        {
            tilesObjs = new List<HexTileBase>();
        }

        for (int i = 0; i < numRows; ++i)
        {
            for (int j = 0; j < numColumns; ++j)
            {
                int cellIndex = GetCellIndexFromRowColumn(i, j);
                isActive[cellIndex] = false;
            }
        }
    }

    public void ExpandActiveCells()
    {
        List<(int, int)> cellsToExpand = new List<(int, int)>();
        for (int i = 0; i < numRows; ++i)
        {
            for (int j = 0; j < numColumns; ++j)
            {
                int cellIndex = GetCellIndexFromRowColumn(i, j);
                if (isActive[cellIndex])
                {
                    cellsToExpand.Add((i, j));
                }
            }
        }
        foreach(var c in cellsToExpand)
        {
            ExpandCellPropertiesToNeighbors(c.Item1, c.Item2);
        }
    }

    public void ExpandCellPropertiesToNeighbors(int row, int column)
    {
        int cellIndex = GetCellIndexFromRowColumn(row, column);
        bool a = isActive[cellIndex];

        var adj = GetAdjacent(row, column);
        foreach (var n in adj)
        {
            if (n.Item1 >= 0 && n.Item2 >= 0 && n.Item1 < numRows && n.Item2 < numColumns)
            {
                int id = GetCellIndexFromRowColumn(n.Item1, n.Item2);
                isActive[id] = a;
            }
        }
    }

    public Vector3 GetPosition(int row, int column)
    {
        Vector3 alternatingShift = Vector3.zero;
        if (row % 2 == 0) alternatingShift = transform.rotation * Vector3.forward * Dimension / 2;
        Vector3 xVec = transform.rotation * Vector3.right * (row * Dimension + xOffset);
        Vector3 zVec = transform.rotation * Vector3.forward * (column * Dimension + zOffset);

        return transform.position + xVec + zVec + alternatingShift;
    }

    public int GetCellIndexFromRowColumn(int row, int column)
    {
        return row * numColumns + column;
    }

    public void Initialize()
    {
        isActive = new List<bool>();
        for (int i = 0; i < numRows * numColumns; ++i)
            isActive.Add(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    public bool IsCellActive(int cellIndex)
    {
        if (isActive == null || cellIndex < 0) return false;
        if (isActive != null && isActive.Count <= cellIndex) return false;
        return isActive[cellIndex];
    }

    public void ToggleCell(int cellIndex)
    {
        //return;
        if (isActive != null && cellIndex < isActive.Count)
            isActive[cellIndex] = ! isActive[cellIndex];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
