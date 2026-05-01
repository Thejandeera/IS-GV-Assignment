using UnityEngine;
using System.Collections.Generic;

public class DroneMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5.0f;
    public float rotationSpeed = 5.0f;

    [Header("Animation Settings (Student 4)")]
    public float hoverAmplitude = 0.5f; // How high the drone bobs
    public float hoverFrequency = 2.0f; // How fast the drone bobs
    public float bankAmount = 30.0f; // Max angle the drone tilts when turning
    public float propellerSpeed = 1500f; // Speed of propeller rotation
    public Transform[] propellers; // Drag the propeller objects here in the Inspector
    
    [Header("Pathfinding Setup")]
    public Transform target;
    private GridManager gridManager;
    private List<Node> currentPath;
    private int currentPathIndex = 0;

    private bool startMoving = false;
    private float baseY; // To keep the drone at a consistent height while bobbing

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found in the scene!");
        }

        // Record starting height for hover bobbing
        baseY = transform.position.y; 
    }

    void Update()
    {
        // Always spin propellers, even when waiting
        SpinPropellers();

        // Wait for the user to press Enter to start
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            startMoving = true;
            Debug.Log("Enter key pressed! Initiating drone sequence...");
        }

        // Do nothing until Enter is pressed
        if (!startMoving) 
        {
            HoverInPlace();
            return;
        }

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
                HoverInPlace();
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

    void HoverInPlace()
    {
        // Just bob up and down when waiting
        float bobOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        transform.position = new Vector3(transform.position.x, baseY + bobOffset, transform.position.z);
        
        // Level out the rotation
        Quaternion flatRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, flatRotation, Time.deltaTime * rotationSpeed);
    }

    void SpinPropellers()
    {
        if (propellers != null && propellers.Length > 0)
        {
            foreach (Transform prop in propellers)
            {
                if (prop != null)
                {
                    // Rotate the propeller around its local Y axis
                    prop.Rotate(Vector3.up * propellerSpeed * Time.deltaTime, Space.Self);
                }
            }
        }
    }

    void MoveAlongPath()
    {
        if (currentPath == null || currentPath.Count == 0) 
        {
            HoverInPlace();
            return;
        }

        if (currentPathIndex < currentPath.Count)
        {
            Vector3 targetPosition = currentPath[currentPathIndex].worldPosition;
            
            // XZ movement logic
            Vector3 currentPosXZ = new Vector3(transform.position.x, baseY, transform.position.z);
            Vector3 targetPosXZ = new Vector3(targetPosition.x, baseY, targetPosition.z);

            // Move towards the target node
            Vector3 newPosXZ = Vector3.MoveTowards(currentPosXZ, targetPosXZ, speed * Time.deltaTime);

            // Apply Hover Bobbing to Y axis
            float bobOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
            transform.position = new Vector3(newPosXZ.x, baseY + bobOffset, newPosXZ.z);

            // Rotate and Bank towards the target node
            Vector3 direction = (targetPosXZ - currentPosXZ).normalized;
            if (direction != Vector3.zero)
            {
                // Calculate the flat look rotation
                Quaternion flatLookRotation = Quaternion.LookRotation(direction);
                
                // Calculate Banking: Get the angle between our current forward and the target direction
                float turnAngle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
                
                // We want to tilt into the turn (like an airplane banking).
                // Clamp it so we don't barrel roll.
                float targetBankAngle = Mathf.Clamp(-turnAngle, -bankAmount, bankAmount);
                
                // Apply the bank angle to the Z axis
                Quaternion bankRotation = Quaternion.Euler(0, 0, targetBankAngle);
                
                // Combine the look rotation with the bank rotation
                Quaternion finalTargetRotation = flatLookRotation * bankRotation;

                // Smoothly interpolate to the final rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, finalTargetRotation, Time.deltaTime * rotationSpeed);
            }

            // Check if we are close enough to the node to move to the next one
            if (Vector3.Distance(currentPosXZ, targetPosXZ) < 0.1f)
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