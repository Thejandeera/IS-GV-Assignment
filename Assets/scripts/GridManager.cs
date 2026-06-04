using UnityEngine;
using System.Collections.Generic;


public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public LayerMask obstacleLayer; 
    public Vector2 gridWorldSize;   
    public float nodeRadius = 1.5f; 
    
    [Header("Visual Path Settings")]
    [Tooltip("The terrain texture index for your mud/dirt path. (0 is usually grass, 1 or 2 is mud)")]
    public int mudTextureIndex = 1; 

    Node[,] grid;
    float nodeDiameter;
    int gridSizeX, gridSizeY;

    void Start()
    {
        //calculating node size
        nodeDiameter = nodeRadius * 2;
        
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        
        CreateGrid();
        BuildAdjacencyList();
    }

    
    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        
        
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                
                
                Vector3 rayStart = new Vector3(worldPoint.x, 100f, worldPoint.z); 
                bool walkable = true;
                bool isMud = false;

                
                if (Terrain.activeTerrain != null)
                {
                    worldPoint.y = Terrain.activeTerrain.SampleHeight(worldPoint);
                    
                    
                    int dominantTexture = GetDominantTextureIndex(worldPoint);
                    if (dominantTexture == mudTextureIndex)
                    {
                        isMud = true;
                    }
                }

                //point check for hit the ray
                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 200f))
                {
                    if (Terrain.activeTerrain == null) worldPoint.y = hit.point.y;

                   
                    if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
                    {
                        walkable = false;
                    }
                }

                // doing sphere check for detect obstacle   
                if (walkable && Physics.CheckSphere(worldPoint, nodeRadius, obstacleLayer))
                {
                    walkable = false;
                }
              
                
                
                grid[x, y] = new Node(walkable, worldPoint, x, y, isMud);
            }
        }
    }

    
    void BuildAdjacencyList()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Node currentNode = grid[x, y];
                UpdateNodeNeighbors(currentNode);
            }
        }
    }

    
    void UpdateNodeNeighbors(Node currentNode)
    {
        currentNode.neighbors.Clear(); 

        if (currentNode.isWalkable)
        {
            
            AddNeighbor(currentNode, currentNode.gridX + 1, currentNode.gridY);
            AddNeighbor(currentNode, currentNode.gridX - 1, currentNode.gridY);
            AddNeighbor(currentNode, currentNode.gridX, currentNode.gridY + 1);
            AddNeighbor(currentNode, currentNode.gridX, currentNode.gridY - 1);
        }
    }

   
    void AddNeighbor(Node node, int checkX, int checkY)
    {
        if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
        {
            Node neighborNode = grid[checkX, checkY];
            
            
            if (neighborNode.isWalkable)
            {
                node.neighbors.Add(neighborNode);
            }
        }
    }


   


    /// <summary>
    /// Returns all nodes in the grid as a flat enumerable.
    /// Used by BFSSearchAlgorithm's debug visualizer to draw blocked nodes.
    /// </summary>
    public IEnumerable<Node> GetAllNodes()
    {
        if (grid == null) yield break;
        foreach (Node n in grid) yield return n;
    }

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
      
        Node nodeToUpdate = GetNodeFromWorldPoint(worldPosition);
        

        if (nodeToUpdate.isWalkable == isNowWalkable) return;
        

        nodeToUpdate.isWalkable = isNowWalkable;
        
      
        
        UpdateNodeNeighbors(nodeToUpdate);

      
        int x = nodeToUpdate.gridX;
        int y = nodeToUpdate.gridY;
        
        if (x + 1 < gridSizeX) UpdateNodeNeighbors(grid[x + 1, y]);
        if (x - 1 >= 0) UpdateNodeNeighbors(grid[x - 1, y]);
        if (y + 1 < gridSizeY) UpdateNodeNeighbors(grid[x, y + 1]);
        if (y - 1 >= 0) UpdateNodeNeighbors(grid[x, y - 1]);
      
    }

  
    int GetDominantTextureIndex(Vector3 worldPos)
    {
        Terrain t = Terrain.activeTerrain;
        if (t == null) return 0;

        //Access the terrain data
        TerrainData td = t.terrainData;
        
        
        float mapX = ((worldPos.x - t.transform.position.x) / td.size.x) * td.alphamapWidth;
        float mapZ = ((worldPos.z - t.transform.position.z) / td.size.z) * td.alphamapHeight;

        int x = Mathf.FloorToInt(mapX);
        int z = Mathf.FloorToInt(mapZ);
        
        if (x < 0 || z < 0 || x >= td.alphamapWidth || z >= td.alphamapHeight)
            return 0;

       
        float[,,] splatmapData = td.GetAlphamaps(x, z, 1, 1);
        float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];

        for (int i = 0; i < cellMix.Length; i++)
        {
            cellMix[i] = splatmapData[0, 0, i];
        }

      
        float maxMix = 0;
        int maxIndex = 0;
        for (int i = 0; i < cellMix.Length; i++)
        {
            if (cellMix[i] > maxMix)
            {
                maxIndex = i;
                maxMix = cellMix[i];
            }
        }
        return maxIndex;
    }


 
    public void AddDynamicObstacle(GameObject obstacleObj, Collider obstacleCollider)
    {
      
        int targetLayer = -1;
        for (int i = 0; i < 32; i++)
        {
            if ((obstacleLayer.value & (1 << i)) != 0)
            {
                targetLayer = i;
                break;
            }
        }

       
        if (targetLayer != -1)
        {
            obstacleObj.layer = targetLayer;
        }

      
        Bounds bounds = obstacleCollider.bounds;
        bounds.Expand(nodeRadius * 4); 

        Node minNode = GetNodeFromWorldPoint(bounds.min);
        Node maxNode = GetNodeFromWorldPoint(bounds.max);

        int startX = Mathf.Clamp(Mathf.Min(minNode.gridX, maxNode.gridX) - 2, 0, gridSizeX - 1);
        int endX = Mathf.Clamp(Mathf.Max(minNode.gridX, maxNode.gridX) + 2, 0, gridSizeX - 1);
        int startY = Mathf.Clamp(Mathf.Min(minNode.gridY, maxNode.gridY) - 2, 0, gridSizeY - 1);
        int endY = Mathf.Clamp(Mathf.Max(minNode.gridY, maxNode.gridY) + 2, 0, gridSizeY - 1);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Node node = grid[x, y];
                Vector3 worldPoint = node.worldPosition;
                
               
                Vector3 rayStart = new Vector3(worldPoint.x, 100f, worldPoint.z);
                bool walkable = true;

                Vector3 halfExtents = new Vector3(nodeRadius, 100f, nodeRadius);
                if (Physics.CheckBox(worldPoint + Vector3.up * 50f, halfExtents, Quaternion.identity, obstacleLayer))
                {
                    walkable = false;
                }

         
                if (node.isWalkable != walkable)
                {
                    node.isWalkable = walkable;
                    UpdateNodeNeighbors(node);

                    if (x + 1 < gridSizeX) UpdateNodeNeighbors(grid[x + 1, y]);
                    if (x - 1 >= 0) UpdateNodeNeighbors(grid[x - 1, y]);
                    if (y + 1 < gridSizeY) UpdateNodeNeighbors(grid[x, y + 1]);
                    if (y - 1 >= 0) UpdateNodeNeighbors(grid[x, y - 1]);
                }
            }
        }
    }

    // draw grid border and grid nodes
    void OnDrawGizmos()
    {
        
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null)
        {
            foreach (Node n in grid)
            {
                if (!n.isWalkable)
                {
                    Gizmos.color = new Color(1, 0, 0, 0.5f); 
                }
                else
                {
                    Gizmos.color = n.isMudPath ? new Color(1f, 0.6f, 0f, 0.6f) : new Color(0, 1, 0, 0.3f); 
                }
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}