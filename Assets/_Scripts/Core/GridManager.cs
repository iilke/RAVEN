using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [Range(0, 5)]
    public float padding = 0.5f; 
    public int width = 20;  // Number of columns
    public int height = 10; // Number of rows
    public float cellSize = 1.1f; // Spacing between tiles

    [Header("References")]
    public GameObject tilePrefab; // Assign the Square Prefab here

    // The logical representation of the grid (2D Array)
    // All algorithms will read data from this array.
    public Node[,] grid;

    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        // Initialize the 2D array
        grid = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 1. Calculate the world position for the tile
                Vector3 worldPoint = new Vector3(x * cellSize, y * cellSize, 0);

                // 2. Instantiate the visual GameObject
                GameObject newTile = Instantiate(tilePrefab, worldPoint, Quaternion.identity);
                newTile.name = $"Tile {x},{y}"; // Naming for Hierarchy clarity
                newTile.transform.parent = this.transform; // Keep Hierarchy clean

                // 3. Create the logical Node and store it in the array
                // By default, isWall is false. We will change this later.
                grid[x, y] = new Node(x, y, false, worldPoint, newTile);
            }
        }

        //Center the camera to view the whole grid
        AdjustCamera();
    }

    void AdjustCamera()
    {

        float gridTotalWidth = width * cellSize;
        float gridTotalHeight = height * cellSize;

        Vector3 centerPos = new Vector3((gridTotalWidth / 2) - (cellSize / 2), (gridTotalHeight / 2) - (cellSize / 2), -10);

        Camera.main.transform.position = centerPos;

        float targetHeight = gridTotalHeight / 2.0f;

        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetWidth = (gridTotalWidth / 2.0f) / screenRatio;

        float requiredSize = Mathf.Max(targetHeight, targetWidth);

        Camera.main.orthographicSize = requiredSize + padding;
    }
}