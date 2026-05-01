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

    public enum ActiveAlgorithm
    {
        BFS,
        AStar
    }

    [Header("Search Algorithms Setup")]
    public ActiveAlgorithm currentAlgorithm = ActiveAlgorithm.AStar; // Default to A*
    public BFSSearchAlgorithm bfsAlgorithm;
    public AStarSearchAlgorithm aStarAlgorithm;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found in the scene!");
        }

        // Try to find the scripts automatically if not assigned
        if (bfsAlgorithm == null) bfsAlgorithm = FindObjectOfType<BFSSearchAlgorithm>();
        if (aStarAlgorithm == null) aStarAlgorithm = FindObjectOfType<AStarSearchAlgorithm>();
        
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
            if (currentAlgorithm == ActiveAlgorithm.BFS && bfsAlgorithm != null)
            {
                currentPath = bfsAlgorithm.FindPath(gridManager, transform.position, target.position);
                
                if (currentPath != null && currentPath.Count > 0)
                {
                    currentPathIndex = 0;
                    Debug.Log("Path found using secondary BFS Algorithm!");
                }
            }
            else if (currentAlgorithm == ActiveAlgorithm.AStar && aStarAlgorithm != null)
            {
                currentPath = aStarAlgorithm.FindPath(gridManager, transform.position, target.position);
                
                if (currentPath != null && currentPath.Count > 0)
                {
                    currentPathIndex = 0;
                    Debug.Log("Path found using primary A* Algorithm!");
                }
            }
            else
            {
                Debug.LogWarning("Selected Search Algorithm script not found! Please attach it to an object in the scene.");
            }
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
}