using UnityEngine;
using System.Collections.Generic;

// ==============================================================================
// Description: Implements the BFS Pathfinding Algorithm as a secondary/backup search.
// Includes a Toggleable Debug Mode (Task 2, 3, 4) to visualize the AI's "brain".
// ==============================================================================
public class BFSSearchAlgorithm : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showDebug = true;

    // Debug State Tracking (To draw the Gizmos)
    private List<Node> debugFrontier = new List<Node>();
    private List<Node> debugExplored = new List<Node>();
    private List<Node> debugFinalPath = new List<Node>();

    void Update()
    {
        // Toggle Debug Mode with Tab key
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

        // Clear previous debug data
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

            // Track for debug (the nodes we have fully explored)
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
                    
                    // Track for debug (nodes we have seen but not fully explored yet)
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
            currentPath.Reverse(); // Reverse so it goes from start -> target
            
            // Save final path for debug drawing
            debugFinalPath = new List<Node>(currentPath);
            
            return currentPath;
        }

        return null;
    }

    // ==============================================================================
    // DEBUG VISUALIZER
    // Draws the search frontiers, explored nodes, and final path in the Scene View.
    // ==============================================================================
    void OnDrawGizmos()
    {
        if (!showDebug) return;

        // 1. Draw Explored Nodes (Red)
        if (debugExplored != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.4f); // Transparent Red
            foreach (Node node in debugExplored)
            {
                Gizmos.DrawCube(node.worldPosition, Vector3.one * 2.5f);
            }
        }

        // 2. Draw Frontier Nodes (Yellow)
        if (debugFrontier != null)
        {
            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.6f); // Transparent Yellow
            foreach (Node node in debugFrontier)
            {
                Gizmos.DrawCube(node.worldPosition, Vector3.one * 2.6f);
            }
        }

        // 3. Draw Final Path (Green Line & Spheres)
        if (debugFinalPath != null && debugFinalPath.Count > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < debugFinalPath.Count; i++)
            {
                Gizmos.DrawSphere(debugFinalPath[i].worldPosition, 0.5f);
                
                // Draw connecting lines
                if (i > 0)
                {
                    Gizmos.DrawLine(debugFinalPath[i - 1].worldPosition, debugFinalPath[i].worldPosition);
                }
            }
        }
    }
}
