using UnityEngine;
using System.Collections.Generic;

// ==============================================================================
// INTELLIGENT SEARCH / A* PATHFINDING ROLE (STUDENT 3)
// Description: Implements the A* Pathfinding Algorithm and Priority Queue Data Structure.
// Leaves the existing codebase untouched, as requested.
// ==============================================================================
public class AStarSearchAlgorithm : MonoBehaviour
{
    // ==============================================================================
    // HEURISTIC FUNCTION
    // Mathematical Justification: Manhattan Distance
    // The GridManager uses a 4-way connected graph (Up, Down, Left, Right).
    // Because diagonal movement is not possible, the shortest geometric path 
    // is constrained to the X and Y axes. The Manhattan distance formula
    // (h(n) = |x1 - x2| + |y1 - y2|) perfectly represents the exact minimum 
    // number of steps required to reach the target on this grid. Thus, the 
    // heuristic is completely ADMISSIBLE, as it will never overestimate the true cost.
    // ==============================================================================
    private float GetHeuristicCost(Node a, Node b)
    {
        return Mathf.Abs(a.gridX - b.gridX) + Mathf.Abs(a.gridY - b.gridY);
    }

    /// <summary>
    /// Executes the A* Search Algorithm to find the optimal path.
    /// Can be called by other scripts without modifying existing systems.
    /// </summary>
    public List<Node> FindPath(GridManager gridManager, Vector3 startPos, Vector3 targetPos)
    {
        if (gridManager == null) return null;

        Node startNode = gridManager.GetNodeFromWorldPoint(startPos);
        Node targetNode = gridManager.GetNodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null || !startNode.isWalkable || !targetNode.isWalkable)
        {
            Debug.LogWarning("AStarSearch: Invalid start or target node.");
            return null; // Invalid path
        }

        // The Priority Queue for the Frontier (Open Set)
        PriorityQueue<Node> openSet = new PriorityQueue<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        Dictionary<Node, float> gCost = new Dictionary<Node, float>();
        Dictionary<Node, float> fCost = new Dictionary<Node, float>();

        openSet.Enqueue(startNode, 0);
        gCost[startNode] = 0;
        fCost[startNode] = GetHeuristicCost(startNode, targetNode);

        while (openSet.Count > 0)
        {
            Node current = openSet.Dequeue();

            if (current == targetNode)
            {
                return RetracePath(startNode, targetNode, cameFrom);
            }

            closedSet.Add(current);

            foreach (Node neighbor in current.neighbors)
            {
                if (!neighbor.isWalkable || closedSet.Contains(neighbor)) continue;

                // For a uniform grid, the movement cost to a neighbor is 1 unit.
                float tentativeGCost = gCost[current] + 1f;

                bool inOpenSet = openSet.Contains(neighbor);

                if (!inOpenSet || tentativeGCost < gCost[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gCost[neighbor] = tentativeGCost;
                    fCost[neighbor] = tentativeGCost + GetHeuristicCost(neighbor, targetNode);

                    if (!inOpenSet)
                    {
                        openSet.Enqueue(neighbor, fCost[neighbor]);
                    }
                    else
                    {
                        // Update existing node if we found a shorter path to it
                        openSet.UpdatePriority(neighbor, fCost[neighbor]);
                    }
                }
            }
        }

        Debug.LogWarning("AStarSearch: No path found to the target.");
        return null; // Path not found
    }

    private List<Node> RetracePath(Node startNode, Node endNode, Dictionary<Node, Node> cameFrom)
    {
        List<Node> path = new List<Node>();
        Node current = endNode;

        while (current != startNode)
        {
            path.Add(current);
            current = cameFrom[current];
        }
        
        // Path currently goes from End -> Start, we need to reverse it so it can be traversed
        path.Reverse();
        return path;
    }
}

// ==============================================================================
// PRIORITY QUEUE IMPLEMENTATION (STUDENT 3 RUBRIC REQUIREMENT)
// Description: A generic Min-Heap Priority Queue.
// Time Complexity: O(log N) for Insertion (Enqueue) and Extraction (Dequeue).
// ==============================================================================
public class PriorityQueue<T>
{
    private struct PriorityItem
    {
        public T Item;
        public float Priority;
    }

    private List<PriorityItem> elements = new List<PriorityItem>();

    public int Count => elements.Count;

    public void Enqueue(T item, float priority)
    {
        elements.Add(new PriorityItem { Item = item, Priority = priority });
        BubbleUp(elements.Count - 1);
    }

    public T Dequeue()
    {
        if (elements.Count == 0) throw new System.InvalidOperationException("Priority Queue is empty.");

        T firstItem = elements[0].Item;
        
        // Move the last item to the top and bubble it down
        elements[0] = elements[elements.Count - 1];
        elements.RemoveAt(elements.Count - 1);

        if (elements.Count > 0)
        {
            BubbleDown(0);
        }

        return firstItem;
    }

    public bool Contains(T item)
    {
        // For performance, a separate HashSet could be maintained, but for this scale
        // checking the underlying list is sufficient.
        return elements.Exists(e => EqualityComparer<T>.Default.Equals(e.Item, item));
    }

    public void UpdatePriority(T item, float newPriority)
    {
        int index = elements.FindIndex(e => EqualityComparer<T>.Default.Equals(e.Item, item));
        if (index >= 0)
        {
            float oldPriority = elements[index].Priority;
            elements[index] = new PriorityItem { Item = item, Priority = newPriority };

            if (newPriority < oldPriority)
            {
                BubbleUp(index);
            }
            else
            {
                BubbleDown(index);
            }
        }
    }

    private void BubbleUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;

            if (elements[index].Priority >= elements[parentIndex].Priority)
                break;

            Swap(index, parentIndex);
            index = parentIndex;
        }
    }

    private void BubbleDown(int index)
    {
        int lastIndex = elements.Count - 1;
        while (true)
        {
            int leftChildIndex = index * 2 + 1;
            int rightChildIndex = index * 2 + 2;
            int smallestIndex = index;

            // Check if left child is smaller
            if (leftChildIndex <= lastIndex && elements[leftChildIndex].Priority < elements[smallestIndex].Priority)
            {
                smallestIndex = leftChildIndex;
            }

            // Check if right child is smaller than current smallest
            if (rightChildIndex <= lastIndex && elements[rightChildIndex].Priority < elements[smallestIndex].Priority)
            {
                smallestIndex = rightChildIndex;
            }

            if (smallestIndex == index)
                break;

            Swap(index, smallestIndex);
            index = smallestIndex;
        }
    }

    private void Swap(int indexA, int indexB)
    {
        PriorityItem temp = elements[indexA];
        elements[indexA] = elements[indexB];
        elements[indexB] = temp;
    }
}
