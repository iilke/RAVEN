using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("UI Settings")]
    public float mapOffsetX = -5.0f;

    [Header("Grid Settings")]
    public int width = 20;
    public int height = 10;
    public float cellSize = 1.1f;
    [Range(0, 5)] public float padding = 1.0f;

    [Header("References")]
    public GameObject tilePrefab;
    public Node[,] grid;

    [Header("Agent & Target")]
    public GameObject crowPrefab;   
    public GameObject humanPrefab;

    private Color colorWall = Color.black;
    private Color colorRoad = Color.white;
    public Color crowTrailColor = Color.cyan;
    public Color humanAreaColor = Color.magenta;

    private GameObject activeCrow;
    private GameObject activeHuman;

    //Algorithm references:
    public Node startNode;  // Raven
    public Node targetNode; // Human

    [Header("Fine Tuning Prefab Position")]
    public Vector3 crowOffset;  
    public Vector3 humanOffset;

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
        LoadLevel(0);      // Start with first level

        // Default fixed positions (left bottom and right top)
        SetStartNode(0, 0);
        SetTargetNode(width - 1, height - 1);
    }

    // --- NODE ASSIGNING FUNCTIONS ---

    public void SetStartNode(int x, int y)
    {
        if (!IsCoordinateValid(x, y)) return;

        if (startNode != null)
        {
            startNode.tileRef.GetComponent<SpriteRenderer>().color = colorRoad;
        }

        startNode = grid[x, y];
        startNode.isWall = false;

        // Color raven's trail
        startNode.tileRef.GetComponent<SpriteRenderer>().color = crowTrailColor;

        // Move or create object
        if (activeCrow != null)
        {
            activeCrow.transform.position = startNode.worldPosition;
        }
        else
        {
            activeCrow = Instantiate(crowPrefab, startNode.worldPosition, Quaternion.identity);
            activeCrow.name = "The Crow";
        }
    }

    public void SetTargetNode(int x, int y)
    {
        if (!IsCoordinateValid(x, y)) return;

        if (targetNode != null)
        {
            targetNode.tileRef.GetComponent<SpriteRenderer>().color = colorRoad;
        }

        targetNode = grid[x, y];
        targetNode.isWall = false;

        // Color target tile
        targetNode.tileRef.GetComponent<SpriteRenderer>().color = humanAreaColor;

        Vector3 finalPos = targetNode.worldPosition + humanOffset;

        // Move or create object
        if (activeHuman != null)
        {
            activeHuman.transform.position = finalPos;
        }
        else
        {
            activeHuman = Instantiate(humanPrefab, finalPos, Quaternion.identity);
            activeHuman.name = "The Target";
        }
    }


    public void RandomizePositions()
    {
        //counter to avoid infinite loops
        int attempts = 0;
        int maxAttempts = 100;

        //find a random start that's not a wall
        int rx, ry;
        do
        {
            rx = Random.Range(0, width);
            ry = Random.Range(0, height);
            attempts++;
        } while (grid[rx, ry].isWall && attempts < maxAttempts);

        SetStartNode(rx, ry);

        //find a random target that's not a wall, not the start
        attempts = 0;
        do
        {
            rx = Random.Range(0, width);
            ry = Random.Range(0, height);
            attempts++;
        } while ((grid[rx, ry].isWall || grid[rx, ry] == startNode) && attempts < maxAttempts);

        SetTargetNode(rx, ry);
    }

    // --- HELPERS ---

    void ResetNodeColor(Node node)
    {
        // If node is wall, make it black, else white
        Color targetColor = node.isWall ? colorWall : colorRoad;
        node.tileRef.GetComponent<SpriteRenderer>().color = targetColor;
    }

    bool IsCoordinateValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    // Drawing preset levels with matrices
    void InitializeLevels()
    {
        predefinedLevels.Clear();
        predefinedLevels.Add(new string[] {
            "000000000000000",
            "000000000000000",
            "000000000000000",
            "000001111100000",
            "000001111100000",
            "000001111100000",
            "000001111100000",
            "000000000000000",
            "000000000000000",
            "000000000000000"
        });
        predefinedLevels.Add(new string[] {
            "000000000000000",
            "111111111111000",
            "000000000000000",
            "000111111111111",
            "000000000000000",
            "111111111111000",
            "000000000000000",
            "000111111111111",
            "000000000000000",
            "000000000000000"
        });
        predefinedLevels.Add(new string[] {
            "000000000000000",
            "000000000000000",
            "000011111110000",
            "000010000010000",
            "000010000010000",
            "000010000010000",
            "000011111110000",
            "000000000000000",
            "000000000000000",
            "000000000000000"
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

 
    public void RandomizeCrowPosition()
    {
        int attempts = 0;
        while (attempts < 100)
        {
            int rx = Random.Range(0, width);
            int ry = Random.Range(0, height);

            Node node = grid[rx, ry];

            
            if (!node.isWall && node != targetNode)
            {
                SetStartNode(rx, ry);
                break;
            }
            attempts++;
        }
    }

  
    public void RandomizeHumanPosition()
    {
        int attempts = 0;
        while (attempts < 100)
        {
            int rx = Random.Range(0, width);
            int ry = Random.Range(0, height);

            Node node = grid[rx, ry];

            if (!node.isWall && node != startNode)
            {
                SetTargetNode(rx, ry);
                break;
            }
            attempts++;
        }
    }

    void AdjustCamera()
    {
        float gridTotalWidth = width * cellSize;
        float gridTotalHeight = height * cellSize;
        Vector3 centerPos = new Vector3((gridTotalWidth / 2) - (cellSize / 2), (gridTotalHeight / 2) - (cellSize / 2), -10);

        centerPos.x += mapOffsetX;

        Camera.main.transform.position = centerPos;

        float targetHeight = gridTotalHeight / 2.0f;
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetWidth = (gridTotalWidth / 2.0f) / screenRatio;
        float requiredSize = Mathf.Max(targetHeight, targetWidth);
        Camera.main.orthographicSize = requiredSize + padding;
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        // 4 directions movement: R, L, U, D

        int[] xDir = { 0, 0, 1, -1 };
        int[] yDir = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int checkX = node.x + xDir[i];
            int checkY = node.y + yDir[i];

            if (IsCoordinateValid(checkX, checkY))
            {
                neighbors.Add(grid[checkX, checkY]);
            }
        }

        return neighbors;
    }

    // GridManager.cs içine ekleyin:

    public void ClearPathfinding()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // reset cost calculations 
                grid[x, y].gCost = 0;
                grid[x, y].hCost = 0;
                grid[x, y].parentNode = null;

                // don't reset walls
                if (grid[x, y].isWall)
                {
                    continue; 
                }

                //if that tile isn't target or start, make it white to clean traces
                if (grid[x, y] != startNode && grid[x, y] != targetNode)
                {
                    grid[x, y].tileRef.GetComponent<SpriteRenderer>().color = colorRoad;
                }
            }
        }
    }
}