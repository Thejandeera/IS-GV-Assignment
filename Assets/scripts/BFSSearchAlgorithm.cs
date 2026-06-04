using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class BFSSearchAlgorithm : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showDebug = true;

  
    private List<Node> debugFrontier = new List<Node>();
    private List<Node> debugExplored = new List<Node>();
    private List<Node> debugFinalPath = new List<Node>();

    private GridManager gridManager;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            showDebug = !showDebug;
            Debug.Log($"<color=cyan><b>BFS Debug Mode:</b></color> {(showDebug ? "ON" : "OFF")}");
        }
    }

    /// <summary>
    /// Executes the BFS Search Algorithm to find a path.
    /// Acts as a backup if A* fails.
    /// </summary>
    public List<Node> FindPath(GridManager gridManager, Vector3 startPos, Vector3 targetPos)
    {
        if (gridManager == null) return null;

        Node startNode = gridManager.GetNodeFromWorldPoint(startPos);
        Node targetNode = gridManager.GetNodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null || !startNode.isWalkable || !targetNode.isWalkable)
        {
            return null;
        }

       
        debugFrontier.Clear();
        debugExplored.Clear();
        debugFinalPath.Clear();

        Queue<Node> frontier = new Queue<Node>();
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();

        frontier.Enqueue(startNode);
        cameFrom[startNode] = null;

        bool foundPath = false;

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();

            
            debugExplored.Add(current);

            if (current == targetNode)
            {
                foundPath = true;
                break;
            }

            foreach (Node neighbor in current.neighbors)
            {
                if (neighbor.isWalkable && !cameFrom.ContainsKey(neighbor))
                {
                    frontier.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                    
                    
                    debugFrontier.Add(neighbor);
                }
            }
        }

        if (foundPath)
        {
            List<Node> currentPath = new List<Node>();
            Node current = targetNode;
            
            while (current != startNode)
            {
                currentPath.Add(current);
                current = cameFrom[current];
            }
            currentPath.Reverse();
            
          
            debugFinalPath = new List<Node>(currentPath);
            
            return currentPath;
        }

        return null;
    }

    
    void OnDrawGizmos()
    {
        if (!showDebug) return;

       
        if (gridManager != null)
        {
            Gizmos.color = new Color(0.4f, 0.4f, 0.4f, 0.35f);
            foreach (Node node in gridManager.GetAllNodes())
            {
                if (!node.isWalkable)
                    Gizmos.DrawCube(node.worldPosition, Vector3.one * 2.5f);
            }
        }

       
        if (debugExplored != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.4f); 
            foreach (Node node in debugExplored)
            {
                Gizmos.DrawCube(node.worldPosition, Vector3.one * 2.5f);
            }
        }

        
        if (debugFrontier != null)
        {
            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.6f); 
            foreach (Node node in debugFrontier)
            {
                Gizmos.DrawCube(node.worldPosition, Vector3.one * 2.6f);
            }
        }

      
        if (debugFinalPath != null && debugFinalPath.Count > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < debugFinalPath.Count; i++)
            {
                Gizmos.DrawSphere(debugFinalPath[i].worldPosition, 0.5f);
                
                
                if (i > 0)
                {
                    Gizmos.DrawLine(debugFinalPath[i - 1].worldPosition, debugFinalPath[i].worldPosition);
                }
            }
        }
    }
}
