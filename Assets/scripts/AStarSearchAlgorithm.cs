using UnityEngine;
using System.Collections.Generic;


public class AStarSearchAlgorithm : MonoBehaviour
{
   
    
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
            return null; 
        }

       
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
                       
                        openSet.UpdatePriority(neighbor, fCost[neighbor]);
                    }
                }
            }
        }

        Debug.LogWarning("AStarSearch: No path found to the target.");
        return null; 
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
        
        
        path.Reverse();
        return path;
    }
}


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

            
            if (leftChildIndex <= lastIndex && elements[leftChildIndex].Priority < elements[smallestIndex].Priority)
            {
                smallestIndex = leftChildIndex;
            }

            
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
