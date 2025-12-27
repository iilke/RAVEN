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
            Debug.Log("Hedef Bulundu! Yol çiziliyor...");
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
            Debug.Log("Hedefe ulaþýlamýyor!");
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
            Debug.Log("DFS Hedefi Buldu!");
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
            Debug.Log("DFS Hedefe Ulaþamadý!");
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
}