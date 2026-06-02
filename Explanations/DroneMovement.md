# DroneMovement.cs - Explanation

## Role Alignment
While this file primarily aligns with **Student 4 (Agent Controller)** for movement animations, it is critical for you to understand it because **it is the consumer of your Custom Graph Formulation (Student 1)**.

## Code Explanation & Mechanism

### Interfacing with the Graph (Student 1's Work)
```csharp
[Header("Pathfinding Setup")]
private GridManager gridManager;
private List<Node> currentPath;

void Start()
{
    gridManager = FindObjectOfType<GridManager>();
    // ...
}

void Update()
{
    // ...
    if (target != null && (currentPath == null || currentPath.Count == 0))
    {
        if (currentAlgorithm == ActiveAlgorithm.BFS && bfsAlgorithm != null)
        {
            currentPath = bfsAlgorithm.FindPath(gridManager, transform.position, target.position);
        }
        else if (currentAlgorithm == ActiveAlgorithm.AStar && aStarAlgorithm != null)
        {
            currentPath = aStarAlgorithm.FindPath(gridManager, transform.position, target.position);
        }
    }
    // ...
}
```
**Mechanism & Theory:**
- The drone requires `GridManager` (your script) to function. 
- It passes your `gridManager` instance into the search algorithms. The algorithms use your `GetNodeFromWorldPoint` to find the start and end nodes, and traverse your Adjacency List (`Node.neighbors`) to return a `List<Node> currentPath`.
- The drone then translates that abstract mathematical path back into 3D space by reading the `worldPosition` of each Node in the list.

### Drone Kinematics
```csharp
void MoveAlongPath()
{
    if (currentPathIndex < currentPath.Count)
    {
        Vector3 targetPosition = currentPath[currentPathIndex].worldPosition;
        Vector3 newPosXZ = Vector3.MoveTowards(currentPosXZ, targetPosXZ, speed * Time.deltaTime);
        
        // Sine wave for hovering effect
        float bobOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        transform.position = new Vector3(newPosXZ.x, baseY + bobOffset, newPosXZ.z);

        // Rotation and banking
        Vector3 direction = (targetPosXZ - currentPosXZ).normalized;
        Quaternion flatLookRotation = Quaternion.LookRotation(direction);
        float turnAngle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        float targetBankAngle = Mathf.Clamp(-turnAngle, -bankAmount, bankAmount);
        
        // ...
    }
}
```
**Theory:**
- **Translation:** Uses `Vector3.MoveTowards` for linear interpolation between your graph nodes.
- **Animation Math:** Uses a `Mathf.Sin` wave function combined with `Time.time` to create a smooth, continuous up-and-down hovering animation mathematically, without needing an Animator component.
- **Rotational Banking:** Calculates the angle between its current forward vector and the target direction to apply a "bank" angle, simulating the physics of a drone leaning into turns.
