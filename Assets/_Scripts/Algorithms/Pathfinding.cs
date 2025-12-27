using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics; 

public class Pathfinding : MonoBehaviour
{
    [Header("Referanslar")]
    public GridManager gridManager;

    [Header("Ayarlar")]
    public float searchDelay = 0.02f;  
    public float moveSpeed = 5f;

    [Header("Renkler")]
    public Color visitedColor = new Color(1, 0.9f, 0.5f, 0.5f); 
    public Color pathColor = Color.green; 

    // --- BFS ---
    public IEnumerator RunBFS()
    {
        
        Stopwatch sw = new Stopwatch();
        sw.Start();
        int visitedCount = 0;

        Node startNode = gridManager.startNode;
        Node targetNode = gridManager.targetNode;

        Queue<Node> queue = new Queue<Node>();
        HashSet<Node> visited = new HashSet<Node>();

        queue.Enqueue(startNode);
        visited.Add(startNode);

        bool pathFound = false;

        while (queue.Count > 0)
        {
            Node currentNode = queue.Dequeue();

            if (currentNode == targetNode)
            {
                pathFound = true;
                break;
            }

            
            if (currentNode != startNode)
            {
                currentNode.tileRef.GetComponent<SpriteRenderer>().color = visitedColor;
                visitedCount++; 

                
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateStats(visitedCount, 0, sw.ElapsedMilliseconds);
                }
            }

            foreach (Node neighbor in gridManager.GetNeighbors(currentNode))
            {
                if (!neighbor.isWall && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    neighbor.parentNode = currentNode;
                    queue.Enqueue(neighbor);
                }
            }

            yield return new WaitForSeconds(searchDelay);
        }

        
        sw.Stop();
        if (pathFound) FinishPath(startNode, targetNode, visitedCount, sw.ElapsedMilliseconds);
    }

    
    public IEnumerator RunDFS()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        int visitedCount = 0;

        Node startNode = gridManager.startNode;
        Node targetNode = gridManager.targetNode;

        Stack<Node> stack = new Stack<Node>();
        HashSet<Node> visited = new HashSet<Node>();

        stack.Push(startNode);

        bool pathFound = false;

        while (stack.Count > 0)
        {
            Node currentNode = stack.Pop();

            if (visited.Contains(currentNode)) continue;
            visited.Add(currentNode);

            if (currentNode == targetNode)
            {
                pathFound = true;
                break;
            }

            if (currentNode != startNode)
            {
                currentNode.tileRef.GetComponent<SpriteRenderer>().color = visitedColor;
                visitedCount++;

                if (UIManager.Instance != null)
                    UIManager.Instance.UpdateStats(visitedCount, 0, sw.ElapsedMilliseconds);
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

        sw.Stop();
        if (pathFound) FinishPath(startNode, targetNode, visitedCount, sw.ElapsedMilliseconds);
    }

    
    public IEnumerator RunAStar()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        int visitedCount = 0;

        Node startNode = gridManager.startNode;
        Node targetNode = gridManager.targetNode;

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        bool pathFound = false;

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost ||
                   (openSet[i].FCost == currentNode.FCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                pathFound = true;
                break;
            }

            if (currentNode != startNode)
            {
                currentNode.tileRef.GetComponent<SpriteRenderer>().color = visitedColor;
                visitedCount++;

                if (UIManager.Instance != null)
                    UIManager.Instance.UpdateStats(visitedCount, 0, sw.ElapsedMilliseconds);
            }

            foreach (Node neighbor in gridManager.GetNeighbors(currentNode))
            {
                if (neighbor.isWall || closedSet.Contains(neighbor)) continue;

                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parentNode = currentNode;

                    if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                }
            }

            yield return new WaitForSeconds(searchDelay);
        }

        sw.Stop();
        if (pathFound) FinishPath(startNode, targetNode, visitedCount, sw.ElapsedMilliseconds);
    }

    void HandleResult(bool success, Node start, Node end, int visitedNodes, long timeMs)
    {
        if (success)
        {
            List<Node> path = RetracePath(start, end);
            foreach (Node n in path)
            {
                if (n != start && n != end)
                    n.tileRef.GetComponent<SpriteRenderer>().color = pathColor;
            }

            if (UIManager.Instance != null)
                UIManager.Instance.UpdateStats(visitedNodes, path.Count, timeMs);

            StartCoroutine(MoveCrow(path));
        }
        else
        {
            if (UIManager.Instance != null)
                UIManager.Instance.OnGameFinished(false);
        }
    }



    void FinishPath(Node start, Node end, int visitedNodes, long timeMs)
    {
        List<Node> path = RetracePath(start, end);

      
        foreach (Node n in path)
        {
            if (n != start && n != end)
                n.tileRef.GetComponent<SpriteRenderer>().color = pathColor;
        }

       
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateStats(visitedNodes, path.Count, timeMs);
        }

        StartCoroutine(MoveCrow(path));
    }

    List<Node> RetracePath(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node currentNode = end;
        while (currentNode != start)
        {
            path.Add(currentNode);
            currentNode = currentNode.parentNode;
        }
        path.Reverse();
        return path;
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.x - nodeB.x);
        int dstY = Mathf.Abs(nodeA.y - nodeB.y);
        return dstX + dstY;
    }

    IEnumerator MoveCrow(List<Node> path)
    {
        GameObject crow = GameObject.Find("The Crow");
        if (crow == null) yield break;

        foreach (Node step in path)
        {
            Vector3 targetPos = step.worldPosition + gridManager.crowOffset;
            while (Vector3.Distance(crow.transform.position, targetPos) > 0.05f)
            {
                crow.transform.position = Vector3.MoveTowards(crow.transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }
            crow.transform.position = targetPos;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnGameFinished(true);
        }
    }
}