# GridManager.cs - Explanation

## Role Alignment
**Intelligent Systems (IS): Custom Graph Formulation**
This script is the core of your IS role. It programmatically extracts the 3D world geometry and converts it into a mathematical graph (adjacency list) that the AI algorithms can navigate. 

## Code Explanation & Mechanism

### 1. Grid Creation & Data Extraction
```csharp
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

            // ... Terrain height sampling ...

            // Physics Raycast to detect obstacles
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 200f))
            {
                if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
                {
                    walkable = false;
                }
            }

            // Physics SphereCheck for volumetric obstacles
            if (walkable && Physics.CheckSphere(worldPoint, nodeRadius, obstacleLayer))
            {
                walkable = false;
            }
            
            grid[x, y] = new Node(walkable, worldPoint, x, y, isMud);
        }
    }
}
```
**Theory:** 
This function performs **Spatial Discretization**. It takes the continuous 3D world and samples it at regular intervals (`nodeDiameter`). It drops a raycast from the sky down to the ground. If the raycast hits an object on the `obstacleLayer` (which you set up in `Student1Tools`), it marks that grid point as `walkable = false`. This creates the initial Vertex set ($V$) of our graph.

### 2. Building the Adjacency List
```csharp
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
        AddNeighbor(currentNode, currentNode.gridX + 1, currentNode.gridY); // Right
        AddNeighbor(currentNode, currentNode.gridX - 1, currentNode.gridY); // Left
        AddNeighbor(currentNode, currentNode.gridX, currentNode.gridY + 1); // Up
        AddNeighbor(currentNode, currentNode.gridX, currentNode.gridY - 1); // Down
    }
}
```
**Theory:**
This establishes the Edges ($E$) of the graph. For every walkable node, it checks its 4-way neighbors (up, down, left, right). If the neighbor is within the grid bounds and is also walkable, it adds it to the node's `neighbors` list. This results in a fully populated **Adjacency List** data structure, which is optimal for A* and BFS searches.

### 3. Coordinate Translation
```csharp
public Node GetNodeFromWorldPoint(Vector3 worldPosition)
{
    float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
    float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
    // ... clamps and math to return grid[x, y]
}
```
**Theory:**
This acts as a bridging function. Search algorithms work in discrete indices (`[x,y]`), but the drone and targets exist in continuous 3D coordinates. This function normalizes the 3D position into a percentage of the grid's size, and maps it directly to the corresponding `Node` in the 2D array.

### 4. Dynamic Obstacle Integration
```csharp
public void AddDynamicObstacle(GameObject obstacleObj, Collider obstacleCollider)
{
    // ... calculates bounding box of moved object ...
    for (int x = startX; x <= endX; x++)
    {
        for (int y = startY; y <= endY; y++)
        {
            // Physics.CheckBox to re-evaluate walkability
            // Updates node.isWalkable and calls UpdateNodeNeighbors()
        }
    }
}
```
**Theory:**
When the player moves an object (Student 2's role), the graph becomes invalid. This function takes the bounding box of the moved object, finds the localized sub-grid of nodes affected, and recalculates their walkability and neighbor lists. This ensures the graph adapts dynamically to the physical world changes without recalculating the entire massive grid (saving massive CPU cycles).
