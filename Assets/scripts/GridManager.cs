using UnityEngine;
using System.Collections.Generic;

// ==============================================================================
// GRAPHICS & WORLD BUILDER ROLE (YOUR ROLE)
// Description: This script converts the 3D map into a 2D mathematical grid.
// It uses Physics to detect where the "Obstacle" logs are and marks those 
// specific grid cells as unwalkable so the AI knows to avoid them.
// ==============================================================================
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public LayerMask obstacleLayer; // The layer assigned to the fallen logs/rocks
    public Vector2 gridWorldSize;   // Total size of the map area to cover
    public float nodeRadius = 1.5f; // How big each square grid cell is

    Node[,] grid;
    float nodeDiameter;
    int gridSizeX, gridSizeY;

    void Start()
    {
        nodeDiameter = nodeRadius * 2;
        // Calculate exactly how many grid squares fit into our world size
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        
        CreateGrid();
        BuildAdjacencyList();
    }

    // VIVA PROOF: "I divided the terrain into a grid and used raycasts/spheres to check for obstacles."
    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        
        // Find the bottom-left corner of the map to start drawing the grid
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // Find the exact 3D world position of this specific grid cell
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                
                // FIRE PHYSICS CHECK: If a sphere hits our 'Obstacle' layer, this cell becomes FALSE (Blocked)
                bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius, obstacleLayer);
                
                // Create the Node data container and save it into our 2D array
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    // VIVA PROOF: "Walkable nodes are connected to neighbors and stored in an adjacency list."
    void BuildAdjacencyList()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Node currentNode = grid[x, y];
                
                // We only need to find neighbors for paths the drone can actually fly on
                if (currentNode.isWalkable)
                {
                    currentNode.neighbors.Clear(); // Clear old neighbors before rebuilding

                    // Add the 4 adjacent cells (Right, Left, Up, Down)
                    AddNeighbor(currentNode, x + 1, y);
                    AddNeighbor(currentNode, x - 1, y);
                    AddNeighbor(currentNode, x, y + 1);
                    AddNeighbor(currentNode, x, y - 1);
                }
            }
        }
    }

    // Helper function to make sure a neighbor is safely inside the map boundaries before adding it
    void AddNeighbor(Node node, int checkX, int checkY)
    {
        if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
        {
            Node neighborNode = grid[checkX, checkY];
            
            // Only add the neighbor to the list if it is NOT blocked by a log
            if (neighborNode.isWalkable)
            {
                node.neighbors.Add(neighborNode);
            }
        }
    }


    // ==============================================================================
    // TEAM INTEGRATION API (DAY 3 RUBRIC REQUIREMENT)
    // These functions are left ready for Student 2 and Student 3 to call from 
    // their own scripts. They do not contain their algorithm logic, just the hooks!
    // ==============================================================================

    /// <summary>
    /// FOR STUDENT 3 (INTELLIGENT SEARCH / A* PATHFINDING):
    /// Student 3 will call this function to convert their drone's 3D position 
    /// (and the goal's 3D position) into my mathematical Grid Nodes so they can run A*.
    /// </summary>
    public Node GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        
        return grid[x, y];
    }

    /// <summary>
    /// FOR STUDENT 2 (INTERACTION / DYNAMIC OBSTACLES):
    /// Student 2 will call this function the exact moment the player moves a log.
    /// They will pass in the location of the log, and I will update my graph 
    /// to make that path Walkable again for the drone.
    /// </summary>
    public void UpdateNodeWalkability(Vector3 worldPosition, bool isNowWalkable)
    {
        // 1. Find the specific node on the grid where the log was just moved from
        Node nodeToUpdate = GetNodeFromWorldPoint(worldPosition);
        
        // 2. Change its status (e.g., from blocked to walkable)
        nodeToUpdate.isWalkable = isNowWalkable;
        
        // 3. Rebuild the adjacency list so Student 3's A* algorithm knows a new path just opened up!
        BuildAdjacencyList(); 
    }


    // ==============================================================================
    // VIVA DEBUGGER (Visualizing the Graph for the Teachers)
    // ==============================================================================
    void OnDrawGizmos()
    {
        // Draw the white outline box showing the total map size
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null)
        {
            foreach (Node n in grid)
            {
                // Draw Green cubes for Walkable ground, Red cubes for Blocked Obstacles
                Gizmos.color = n.isWalkable ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.5f);
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}