using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 20;
    public int height = 10;
    public float cellSize = 1.1f;
    [Range(0, 5)] public float padding = 1.0f;

    [Header("References")]
    public GameObject tilePrefab;
    public Node[,] grid;


    // --- PRESET LEVELS ---
    // 0: TILE, 1: WALL
    private List<string[]> predefinedLevels = new List<string[]>();

    void Awake() 
    {
        InitializeLevels();
    }

    void Start()
    {
        CreateGrid();      // Empty, all tile grid creation first
        LoadLevel(1);      // Start with first level
    }

    // Drawing preset levels with matrices
    void InitializeLevels()
    {
        
        predefinedLevels.Add(new string[] {
            "00000000000000000000",
            "00000000000000000000",
            "00000000000000000000",
            "00000001111110000000",
            "00000001111110000000",
            "00000001111110000000",
            "00000001111110000000",
            "00000000000000000000",
            "00000000000000000000",
            "00000000000000000000"
        });

        
        predefinedLevels.Add(new string[] {
            "00000000000000000000",
            "11111111111111111100",
            "00000000000000000000",
            "00111111111111111111",
            "00000000000000000000",
            "11111111111111111100",
            "00000000000000000000",
            "00111111111111111111",
            "00000000000000000000",
            "00000000000000000000"
        });

        
        predefinedLevels.Add(new string[] {
            "00000000000000000000",
            "00000000000000000000",
            "00000111111111100000",
            "00000100000000100000",
            "00000100000000100000",
            "00000100000000100000",
            "00000111111111100000",
            "00000000000000000000",
            "00000000000000000000",
            "00000000000000000000"
        });
    }

    void CreateGrid()
    {
        grid = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPoint = new Vector3(x * cellSize, y * cellSize, 0);
                GameObject newTile = Instantiate(tilePrefab, worldPoint, Quaternion.identity);
                newTile.name = $"Tile {x},{y}";
                newTile.transform.parent = this.transform;

                grid[x, y] = new Node(x, y, false, worldPoint, newTile);
            }
        }
        AdjustCamera();
    }

    //Function to call from outside
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= predefinedLevels.Count)
        {
            Debug.LogError("Geçersiz Level ID!");
            return;
        }

        ResetGrid(); //Clean first

        string[] mapData = predefinedLevels[levelIndex];

        // Harita verisini Grid'e iþle
        // Not: String array yukarýdan aþaðý (Height-1 -> 0) okunur, ama grid x,y tabanlýdýr.
        // Bu yüzden döngüde dikkatli olmalýyýz.

        for (int y = 0; y < height; y++)
        {
            // Reading in reverse to match with visually
            // Top row of map is index 0. grid's highest Y value (height-1).
            string row = mapData[height - 1 - y];

            for (int x = 0; x < width; x++)
            {
                if (x < row.Length)
                {
                    char tileType = row[x];
                    if (tileType == '1')
                    {
                        // Duvar Yap
                        grid[x, y].isWall = true;
                        grid[x, y].tileRef.GetComponent<SpriteRenderer>().color = Color.black; 
                    }
                }
            }
        }
    }

    public void ResetGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y].isWall = false;
                grid[x, y].tileRef.GetComponent<SpriteRenderer>().color = Color.white; 
                // Cost reset to be added here
            }
        }
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