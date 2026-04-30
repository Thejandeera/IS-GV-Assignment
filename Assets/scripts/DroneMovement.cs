using UnityEngine;
using System.Collections.Generic;

public class DroneMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5.0f;
    public float rotationSpeed = 5.0f;
    
    [Header("Pathfinding Setup")]
    public Transform target;
    private GridManager gridManager;
    private List<Node> currentPath;
    private int currentPathIndex = 0;

    private bool startMoving = false;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found in the scene!");
        }
    }

    void Update()
    {
        // Wait for the user to press Enter to start
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            startMoving = true;
            Debug.Log("Enter key pressed! Initiating drone sequence...");
        }

        // Do nothing until Enter is pressed
        if (!startMoving) return;

        // 1. Try to find the target continuously if we don't have one
        if (target == null)
        {
            GameObject targetObj = GameObject.Find("Target");
            if (targetObj == null) targetObj = GameObject.Find("End");
            
            if (targetObj != null) 
            {
                target = targetObj.transform;
                Debug.Log("Successfully found Target: " + target.name);
            }
            else 
            {
                return; // Do nothing until we find a target
            }
        }

        // 2. Continuously try to find a path if we have a target and no path yet
        if (target != null && (currentPath == null || currentPath.Count == 0))
        {
            FindPathBFS(transform.position, target.position);
        }

        // 3. Move along the path
        MoveAlongPath();
    }

    void MoveAlongPath()
    {
        if (currentPath == null || currentPath.Count == 0) return;

        if (currentPathIndex < currentPath.Count)
        {
            Vector3 targetPosition = currentPath[currentPathIndex].worldPosition;
            // Keep the drone's height if you don't want it to snap to the ground
            targetPosition.y = transform.position.y; 

            // Move towards the target node
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // Rotate towards the target node
            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }

            // Check if we are close enough to the node to move to the next one
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                currentPathIndex++;
            }
        }
        else
        {
            // Reached the end of the path
            currentPath = null;
            currentPathIndex = 0;
            Debug.Log("Drone reached the target!");
        }
    }

    void FindPathBFS(Vector3 startPos, Vector3 targetPos)
    {
        if (gridManager == null) return;

        Node startNode = gridManager.GetNodeFromWorldPoint(startPos);
        Node targetNode = gridManager.GetNodeFromWorldPoint(targetPos);

        // DEBUGGING LOGS: Tell the user exactly why the drone isn't moving
        if (startNode == null) Debug.LogWarning("Start node is null!");
        if (targetNode == null) Debug.LogWarning("Target node is null!");
        if (startNode != null && !startNode.isWalkable) Debug.LogWarning("Start node (Drone position) is NOT walkable! The drone might be inside an obstacle.");
        if (targetNode != null && !targetNode.isWalkable) Debug.LogWarning("Target node is NOT walkable! Move the Target out of the obstacles.");

        if (startNode == null || targetNode == null || !startNode.isWalkable || !targetNode.isWalkable)
        {
            return;
        }

        Queue<Node> frontier = new Queue<Node>();
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();

        frontier.Enqueue(startNode);
        cameFrom[startNode] = null;

        bool foundPath = false;

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();

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
                }
            }
        }

        if (foundPath)
        {
            currentPath = new List<Node>();
            Node current = targetNode;
            
            while (current != startNode)
            {
                currentPath.Add(current);
                current = cameFrom[current];
            }
            // We don't add the start node to the path as we are already there
            currentPath.Reverse(); // Reverse so it goes from start -> target
            currentPathIndex = 0;
        }
    }
}