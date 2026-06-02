# Node.cs - Explanation

## Role Alignment
**Intelligent Systems (IS): Custom Graph Formulation**
This file is the fundamental building block of the mathematical graph used for pathfinding algorithms (like A* and BFS). As the student responsible for "Custom Graph Formulation," this is your primary data structure.

## Code Explanation & Mechanism

```csharp
public class Node
{
    public bool isWalkable;
    public bool isMudPath;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    
    public List<Node> neighbors;

    public Node(bool _isWalkable, Vector3 _worldPos, int _gridX, int _gridY, bool _isMudPath = false)
    {
        isWalkable = _isWalkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        isMudPath = _isMudPath;
        neighbors = new List<Node>();
    }
}
```

### Mechanism & Theory

**1. Mathematical Graph Theory Formulation:**
In graph theory, a graph $G = (V, E)$ consists of Vertices ($V$) and Edges ($E$). 
- This `Node` class represents a **Vertex** ($V$) in the mathematical graph.
- It abstracts a continuous 3D space (represented by `worldPosition`) into a discrete mathematical point on a 2D grid (`gridX`, `gridY`).

**2. Adjacency List:**
- The `public List<Node> neighbors;` variable is the programmatic implementation of an **Adjacency List**. 
- Instead of using an adjacency matrix (which would be $O(V^2)$ in space complexity and mostly sparse), an adjacency list is highly efficient. It stores direct references to connected, walkable adjacent nodes (the Edges).

**3. State Variables for Search Algorithms:**
- `isWalkable`: A boolean flag determining if this node is an obstacle or open space. If it is false, search algorithms will ignore it.
- `worldPosition`: Crucial for translating the discrete graph coordinates back into the 3D Unity environment so the drone knows where to move.
- `isMudPath`: A terrain-specific variable that could be used by Student 3 (Heuristic Design) to add movement penalties (e.g., mud costs more to traverse than grass).
