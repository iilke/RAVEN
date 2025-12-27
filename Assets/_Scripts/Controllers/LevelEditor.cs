using UnityEngine;

public class LevelEditor : MonoBehaviour
{
    public GridManager gridManager;

    // 0:wall edit, 1:put raven, 2: put human
    public int currentMode = 0;

    void Update()
    {
        if (UIManager.Instance != null && UIManager.Instance.isInputLocked) return;
       

        if (gridManager == null || gridManager.grid == null) return;

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        //mouse position turns into node
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        int x = Mathf.RoundToInt(worldPos.x / gridManager.cellSize);
        int y = Mathf.RoundToInt(worldPos.y / gridManager.cellSize);

        //border control
        if (x >= 0 && x < gridManager.width && y >= 0 && y < gridManager.height)
        {
            
            switch (currentMode)
            {
                case 0: //place wall
                    //can't put wall on raven or human
                    if (gridManager.grid[x, y] == gridManager.startNode || gridManager.grid[x, y] == gridManager.targetNode) return;

                    bool makeWall = Input.GetMouseButton(0);
                    gridManager.grid[x, y].isWall = makeWall;
                    gridManager.grid[x, y].tileRef.GetComponent<SpriteRenderer>().color = makeWall ? Color.black : Color.white;
                    break;

                case 1: //place raven
                    if (Input.GetMouseButtonDown(0))
                    {
                        gridManager.SetStartNode(x, y);
                        currentMode = 0; //go back to wall mode when done
                        Debug.Log("Raven placed, back to wall mode");
                    }
                    break;

                case 2: // Ýnsan Koyma Modu
                    if (Input.GetMouseButtonDown(0))
                    {
                        gridManager.SetTargetNode(x, y);
                        currentMode = 0; 
                        Debug.Log("Human placed, back to wall mode");
                    }
                    break;
            }
        }
    }

   
    public void SetMode(int modeIndex)
    {
        currentMode = modeIndex;
    }
}