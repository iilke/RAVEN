using UnityEngine;

public class LevelEditor : MonoBehaviour
{
    [Header("References")]
    public GridManager gridManager; //Reach Grid

    [Header("Settings")]
    public Color wallColor = Color.black;
    public Color floorColor = Color.white;

    void Update()
    {
        //Don't do anything if can't acces grid
        if (gridManager == null || gridManager.grid == null) return;

        //Left click: add wall
        if (Input.GetMouseButton(0))
        {
            PaintTile(true);
        }
        //Right Click: remove wall
        else if (Input.GetMouseButton(1))
        {
            PaintTile(false);
        }
    }

    void PaintTile(bool makeWall)
    {
        
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10; 

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        // GridManager x * cellSize formula used. Reverse of it is division
        // Mathf.RoundToInt to find closest tile
        int x = Mathf.RoundToInt(worldPos.x / gridManager.cellSize);
        int y = Mathf.RoundToInt(worldPos.y / gridManager.cellSize);

        // check are these indexes in map
        if (x >= 0 && x < gridManager.width && y >= 0 && y < gridManager.height)
        {
            Node node = gridManager.grid[x, y];

            //Don't do anything if it's already a wall
            if (node.isWall == makeWall) return;

            //Update visually
            node.isWall = makeWall;
            node.tileRef.GetComponent<SpriteRenderer>().color = makeWall ? wallColor : floorColor;
        }
    }
}