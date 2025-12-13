using UnityEngine;

// This class is the id of each tile.
public class Node
{
    public int x; // x coordinate on grid
    public int y; // y coordinate on grid

    public bool isWall; 
    public Vector3 worldPosition; // real position in unity world

    // Variables for pathfinding algorithms
    public Node parentNode; //previous node
    public int gCost; //cost to here from the start node
    public int hCost; //heuristic from here to destination

    // F Cost = G + H (A* total cost)
    public int FCost { get { return gCost + hCost; } }

    // visual reference of tile to adjust its color later
    public GameObject tileRef;

    public Node(int _x, int _y, bool _isWall, Vector3 _worldPos, GameObject _tileRef)
    {
        x = _x;
        y = _y;
        isWall = _isWall;
        worldPosition = _worldPos;
        tileRef = _tileRef;
    }
}