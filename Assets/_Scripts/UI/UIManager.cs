using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Script References")]
    public Pathfinding pathfindingScript;
    public GridManager gridManager;
    public LevelEditor levelEditor;

    [Header("Dropdowns")]
    public TMP_Dropdown algoDropdown;
    public TMP_Dropdown mapDropdown;

    [Header("Status Text (Optional)")]
    public TextMeshProUGUI statusText;

    [Header("Stats Texts (Optional)")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI visitedText;
    public TextMeshProUGUI pathCostText;

    public static UIManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateStatus("Ready to search");

        // Dropdown olayýný baðla
        if (mapDropdown != null)
            mapDropdown.onValueChanged.AddListener(OnMapChanged);
    }


    public void OnRunPressed()
    {
        gridManager.ClearPathfinding();

        int selectedAlgo = algoDropdown.value;

        switch (selectedAlgo)
        {
            case 0:
                UpdateStatus("Running BFS...");
                StartCoroutine(pathfindingScript.RunBFS());
                break;
            case 1:
                UpdateStatus("Running DFS...");
                StartCoroutine(pathfindingScript.RunDFS());
                break;
            case 2:
                UpdateStatus("Running A*...");
                StartCoroutine(pathfindingScript.RunAStar());
                break;
        }
    }


    public void OnResetPressed()
    {
        gridManager.ClearPathfinding();
        UpdateStatus("Path Cleared");
    }

  
    public void OnMapChanged(int index)
    {
        
        if (index >= 0 && index <= 2) //preset maps chosen
        {
            gridManager.LoadLevel(index);
            UpdateStatus("Preset Map Loaded");
        }
     
        else if (index == 3)//clear map option
        {
            gridManager.ResetGrid();
            UpdateStatus("Map Cleared - Draw Mode");
        }
    }

    // POZÝSYON BUTONLARI
    public void OnSelectCrowClicked() { levelEditor.SetMode(1); UpdateStatus("Place Raven"); }
    public void OnRandomCrowClicked() { gridManager.RandomizeCrowPosition(); gridManager.ClearPathfinding(); }
    public void OnSelectHumanClicked() { levelEditor.SetMode(2); UpdateStatus("Place Human"); }
    public void OnRandomHumanClicked() { gridManager.RandomizeHumanPosition(); gridManager.ClearPathfinding(); }


    public void UpdateStats(int visitedCount, int pathLength, long elapsedMs)
    {
        if (visitedText != null) visitedText.text = $"Nodes: {visitedCount}";
        if (pathCostText != null) pathCostText.text = $"Path: {pathLength}";
        if (timeText != null) timeText.text = elapsedMs > 0 ? $"Time: {elapsedMs} ms" : "Time: ...";
    }

    public void UpdateStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}