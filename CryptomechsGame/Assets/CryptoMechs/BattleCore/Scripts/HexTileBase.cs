using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public interface HexTile
{
    int GetID();
    int GetRow();
    int GetColumn();
    void Setup(int id, int row, int column, float dimension, bool isActive);
}

public class HexTileBase : MonoBehaviour, HexTile
{
    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        if (adjacentNodes != null)
        {
            foreach (var t in adjacentNodes)
            {
                Gizmos.DrawLine(this.GetPosition(), t.GetPosition());
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 1);
    }


    public int id = -1;
    public int mirrorId = -1;
    public int row = 0;
    public int column = 0;
    public bool traversible = true;
    public float dimension = 1f;
    public bool isActive = true;
    public float cachedClosestDistance = 0;
    private bool cachedExplored = false;
    public HexTileBase cachedPathNode;

    public List<HexTileBase> adjacentNodes;

    public HexTileBase tr;
    public HexTileBase tl;
    public HexTileBase r;
    public HexTileBase l;
    public HexTileBase br;
    public HexTileBase bl;


    public bool IsOddRow { get { return this.row % 2 == 1; } }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public int GetColumn()
    {
        return column;
    }

    public int GetRow()
    {
        return row;
    }

    public int GetID()
    {
        return id;
    }

    public void Setup(int id, int row, int column, float dimension, bool isActive)
    {
        this.row = row;
        this.column = column;
        this.id = id;
        this.dimension = dimension;
        this.isActive = isActive;
    }

    public void SetupAdjacencies()
    {
        this.adjacentNodes = new List<HexTileBase>();
        if (tr != null)
            this.adjacentNodes.Add(tr);
        if (tl != null)
            this.adjacentNodes.Add(tl);
        if (l != null)
            this.adjacentNodes.Add(l);
        if (r != null)
            this.adjacentNodes.Add(r);
        if (bl != null)
            this.adjacentNodes.Add(bl);
        if (br != null)
            this.adjacentNodes.Add(br);
    }

    public void GeneratePathsDjikstra()
    {
        List<HexTileBase> tiles = FindObjectsOfType<HexTileBase>().ToList();
        tiles.ForEach(t =>
        {
            t.cachedExplored = false;
            t.cachedClosestDistance = float.MaxValue;
            t.cachedPathNode = null;
        }
        );
        
        this.cachedClosestDistance = 0;

        List<HexTileBase> path = new List<HexTileBase>();

        var unvisited = tiles.OrderBy(node => node.cachedClosestDistance).ToList();

        while (unvisited.Count > 0)
        {
            // Ordering the unvisited list by distance, smallest distance at start and largest at end
            unvisited = unvisited.OrderBy(node => node.cachedClosestDistance).ToList();

            // Getting the Node with smallest distance
            var current = unvisited[0];

            // Remove the current node from unvisisted list
            unvisited.Remove(current);
            
            if (current != this && !current.traversible)
            {
                continue;
            }

            /*
            // When the current node is equal to the end node, then we can break and return the path
            if (current == target)
            {
                while (current.cachedPathNode != this)
                {
                    path.Insert(0, current);
                    current = current.cachedPathNode;
                }
                path.Insert(0, current);
                break;
            }
            */

            // Looping through the Node connections (neighbors) and where the connection (neighbor) is available at unvisited list
            for (int i = 0; i < current.adjacentNodes.Count; i++)
            {

                var neighbor = current.adjacentNodes[i];
                
                if (!neighbor.traversible) continue;

                // Getting the distance between the current node and the connection (neighbor)
                float length = Vector3.Distance(this.transform.position, neighbor.transform.position);


                // The distance from start node to this connection (neighbor) of current node
                float fullLength = current.cachedClosestDistance + length;

                // A shorter path to the connection (neighbor) has been found
                if (fullLength < neighbor.cachedClosestDistance)
                {
                    neighbor.cachedClosestDistance = fullLength;
                    neighbor.cachedPathNode = current;
                }
            }
        }
        //return path;
    }

    /*
    public bool CanReachStraightLine(BattleTile tile)
    {
        int deltaX = tile.row - this.row;
        int deltaY = tile.column - this.column;
        if (tile.row == this.row) return true;
        return false;
    }
    */


}
