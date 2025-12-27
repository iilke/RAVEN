using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("UI Groups (States)")]
    public GameObject panelExecution;
    public GameObject groupLoading;      
    public GameObject groupResult;
    public TextMeshProUGUI resultMessageText;

    [Header("Buttons to Hide")]
    public GameObject btnRun;           
    public GameObject groupHumanControls; 
    public GameObject groupCrowControls;

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
    public bool isInputLocked = false;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateStatus("Ready to search");
        OnMainPressed();
        
        if (mapDropdown != null)
            mapDropdown.onValueChanged.AddListener(OnMapChanged);
    }


    public void OnRunPressed()
    {
        SetUIState_Executing();

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

    public void OnGameFinished(bool success)
    {
        
        if (groupLoading != null) groupLoading.SetActive(false);
        if (groupResult != null) groupResult.SetActive(true);

        mapDropdown.interactable = false;
        algoDropdown.interactable = true;

        if (resultMessageText != null)
        {
            if (success)
            {
                resultMessageText.text = "Finished: Target Found";
            }
            else
            {
                resultMessageText.text = "Failed: Target Unreachable";
            }
        }
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

    public void OnRetryPressed()
    {
        StartCoroutine(RetrySequence());
    }

    IEnumerator RetrySequence()
    {
        if (groupResult != null) groupResult.SetActive(false);

        if (groupLoading != null) groupLoading.SetActive(true);
        if (panelExecution != null) panelExecution.SetActive(true);

        gridManager.ClearPathfinding();
        ResetStats();

        gridManager.ResetCharacterPositions();

        UpdateStatus("Retrying in 2 seconds...");
        yield return new WaitForSeconds(2.0f);

        OnRunPressed();
    }


    public void OnMainPressed()
    {
        gridManager.ResetGrid(); 

        mapDropdown.value = 0;
        gridManager.LoadLevel(0);

        gridManager.SetStartNode(0, 0);

        gridManager.SetTargetNode(gridManager.width - 1, gridManager.height - 1);
        
        ResetStats();

        SetUIState_Main();

        UpdateStatus("Reset to Default");
    }
    public void OnSelectCrowClicked() { levelEditor.SetMode(1); UpdateStatus("Place Raven"); }
    public void OnRandomCrowClicked() { gridManager.RandomizeCrowPosition(); gridManager.ClearPathfinding(); }
    public void OnSelectHumanClicked() { levelEditor.SetMode(2); UpdateStatus("Place Human"); }
    public void OnRandomHumanClicked() { gridManager.RandomizeHumanPosition(); gridManager.ClearPathfinding(); }


    public void UpdateStats(int visitedCount, int pathLength, long elapsedMs)
    {
        if (visitedText != null) visitedText.text = $"Visited Nodes: {visitedCount}";
        if (pathCostText != null) pathCostText.text = $"Path Cost: {pathLength}";
        if (timeText != null) timeText.text = elapsedMs > 0 ? $"Time Passed: {elapsedMs} ms" : "Time: ...";
    }

    public void ResetStats()
    {
        if (visitedText != null) visitedText.text = "Nodes: 0";
        if (pathCostText != null) pathCostText.text = "Path: 0";
        if (timeText != null) timeText.text = "Time: 0 ms";
    }

    public void UpdateStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }

    public void SetUIState(bool isRunning)
    {
        //Lock Dropdowns
        algoDropdown.interactable = !isRunning;
        mapDropdown.interactable = !isRunning;

        //Hide main controls
        if (btnRun != null) btnRun.SetActive(!isRunning);
        if (groupHumanControls != null) groupHumanControls.SetActive(!isRunning);
        if (groupCrowControls != null) groupCrowControls.SetActive(!isRunning);

        
        if (panelExecution != null) panelExecution.SetActive(isRunning);
    }

    void SetUIState_Executing()
    {
        isInputLocked = true; 

        if (panelExecution != null) panelExecution.SetActive(true); 
        if (groupLoading != null) groupLoading.SetActive(true);     
        if (groupResult != null) groupResult.SetActive(false);     

        if (btnRun != null) btnRun.SetActive(false);
        if (groupHumanControls != null) groupHumanControls.SetActive(false);
        if (groupCrowControls != null) groupCrowControls.SetActive(false);

        algoDropdown.interactable = false;
        mapDropdown.interactable = false;
    }

    void SetUIState_Main()
    {
        isInputLocked = false;

        if (panelExecution != null) panelExecution.SetActive(false);
        if (groupLoading != null) groupLoading.SetActive(false); 
        if (groupResult != null) groupResult.SetActive(false);   

        if (btnRun != null) btnRun.SetActive(true);
        if (groupHumanControls != null) groupHumanControls.SetActive(true);
        if (groupCrowControls != null) groupCrowControls.SetActive(true);

        algoDropdown.interactable = true;
        mapDropdown.interactable = true;
    }
}