using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    [Header("References")]
    public GridManager gridManager;

    [Header("Settings")]
    public float searchDelay = 0.05f;  
    public float moveSpeed = 5f;     

    [Header("Colors")]
    public Color visitedColor = new Color(1, 0.9f, 0.5f, 0.5f); //nodes expanded
    public Color pathColor = Color.green; //path found

    private bool isRunning = false; //Lock for not running twice when algo is already running

    void Update()
    {
        //TEST: with space, start algorithm
        if (Input.GetKeyDown(KeyCode.Space) && !isRunning)
        {
            StartCoroutine(RunBFS());
        }

        //TEST: D for DFS start
        if (Input.GetKeyDown(KeyCode.D) && !isRunning)
        {
            StartCoroutine(RunDFS());
        }

        //TEST: A for A* srch
        if (Input.GetKeyDown(KeyCode.A) && !isRunning) StartCoroutine(RunAStar());
    }

    // BFS (Breadth First Search) ---
    IEnumerator RunBFS()
    {
        isRunning = true;
        gridManager.ClearPathfinding(); //Clean previous coloring

        Node startNode = gridManager.startNode;
        Node targetNode = gridManager.targetNode;

        // BFS FIFO queue
        Queue<Node> queue = new Queue<Node>();
        HashSet<Node> visited = new HashSet<Node>(); 

        queue.Enqueue(startNode);
        visited.Add(startNode);

        bool pathFound = false;

        while (queue.Count > 0)
        {
            Node currentNode = queue.Dequeue();

            //target reached?
            if (currentNode == targetNode)
            {
                pathFound = true;
                break;
            }

            // visualize node currently visited
            if (currentNode != startNode)
            {
                currentNode.tileRef.GetComponent<SpriteRenderer>().color = visitedColor;
            }

            //look at neighbours
            foreach (Node neighbor in gridManager.GetNeighbors(currentNode))
            {
                //if it's not a wall and not visited efore
                if (!neighbor.isWall && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    neighbor.parentNode = currentNode; //where did we came from? -for tracing
                    queue.Enqueue(neighbor);
                }
            }

            //Animation wait
            yield return new WaitForSeconds(searchDelay);
        }

        if (pathFound)
        {
            Debug.Log("Target found! Drawing the path now...");
            List<Node> path = RetracePath(startNode, targetNode);

            //color the path to green
            foreach (Node n in path)
            {
                if (n != startNode && n != targetNode)
                    n.tileRef.GetComponent<SpriteRenderer>().color = pathColor;
            }

            //move the raven
            yield return StartCoroutine(MoveCrow(path));
        }
        else
        {
            Debug.Log("Target not found!");
        }

        isRunning = false;
    }

    // --- HELPER: (RETRACE) ---
    List<Node> RetracePath(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node currentNode = end;

        while (currentNode != start)
        {
            path.Add(currentNode);
            currentNode = currentNode.parentNode; 
        }
        path.Reverse(); //reverse list since we saved from end to start
        return path;
    }


    IEnumerator RunDFS()
    {
        isRunning = true;
        gridManager.ClearPathfinding(); 

        Node startNode = gridManager.startNode;
        Node targetNode = gridManager.targetNode;

        //LIFO STACK
        Stack<Node> stack = new Stack<Node>();
        HashSet<Node> visited = new HashSet<Node>();

        stack.Push(startNode);

        bool pathFound = false;

        while (stack.Count > 0)
        {
            Node currentNode = stack.Pop(); 

        
            if (visited.Contains(currentNode))
                continue;

            visited.Add(currentNode);

            
            if (currentNode == targetNode)
            {
                pathFound = true;
                break;
            }

            
            if (currentNode != startNode)
            {
                currentNode.tileRef.GetComponent<SpriteRenderer>().color = visitedColor;
            }

          
            foreach (Node neighbor in gridManager.GetNeighbors(currentNode))
            {
                if (!neighbor.isWall && !visited.Contains(neighbor))
                {
                    neighbor.parentNode = currentNode;
                    stack.Push(neighbor);
                }
            }

            yield return new WaitForSeconds(searchDelay);
        }

        if (pathFound)
        {
            Debug.Log("Target found! Drawing the path now...");
            List<Node> path = RetracePath(startNode, targetNode);

            
            foreach (Node n in path)
            {
                if (n != startNode && n != targetNode)
                    n.tileRef.GetComponent<SpriteRenderer>().color = pathColor;
            }

            yield return StartCoroutine(MoveCrow(path));
        }
        else
        {
            Debug.Log("Target not found!");
        }

        isRunning = false;
    }


    IEnumerator RunAStar()
    {
        isRunning = true;
        gridManager.ClearPathfinding();

        Node startNode = gridManager.startNode;
        Node targetNode = gridManager.targetNode;

        // For A* "Open List" (to visit) "Closed List" (visited)
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        bool pathFound = false;

        while (openSet.Count > 0)
        {
            // 1. Find lowest F cost node inside Openset
            // Usually with priority queue but using list now
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                //Choose the lowest F cost one, if F costs are equel choose lowest H cost one.
                if (openSet[i].FCost < currentNode.FCost ||
                   (openSet[i].FCost == currentNode.FCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            //Target found?
            if (currentNode == targetNode)
            {
                pathFound = true;
                break;
            }

            //Visualisation
            if (currentNode != startNode)
            {
                currentNode.tileRef.GetComponent<SpriteRenderer>().color = visitedColor;
            }

            //look at neighbors
            foreach (Node neighbor in gridManager.GetNeighbors(currentNode))
            {
                if (neighbor.isWall || closedSet.Contains(neighbor)) continue;

                //Calculate new cost G: current dis. + 1 
                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

                //If we reached this neighbor with a shorter path or the neighbor isn't already in the list
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    //update values
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode); //calculate heuristic
                    neighbor.parentNode = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }

            //Animation wait
            yield return new WaitForSeconds(searchDelay);
        }

        if (pathFound)
        {
            Debug.Log("A* found the target!!");
            List<Node> path = RetracePath(startNode, targetNode);

            foreach (Node n in path)
            {
                if (n != startNode && n != targetNode)
                    n.tileRef.GetComponent<SpriteRenderer>().color = pathColor;
            }

            yield return StartCoroutine(MoveCrow(path));
        }
        else
        {
            Debug.Log("Tagret not found!");
        }
        isRunning = false;
    }


    IEnumerator MoveCrow(List<Node> path)
    {

        GameObject crow = GameObject.Find("The Crow");

        if (crow == null) yield break;

        foreach (Node step in path)
        {
            // target position: mid of grid + crow Offset
            Vector3 targetPos = step.worldPosition + gridManager.crowOffset;

            
            while (Vector3.Distance(crow.transform.position, targetPos) > 0.05f)
            {
                crow.transform.position = Vector3.MoveTowards(crow.transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null; // Bir sonraki frame'i bekle
            }
            
            crow.transform.position = targetPos;
        }
    }

    //Heuristic: Manhattan distance between 2 tiles: (Only horizontal + vertical) like walls doesn't exist.
    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.x - nodeB.x);
        int dstY = Mathf.Abs(nodeA.y - nodeB.y);
        return dstX + dstY;
    }
}