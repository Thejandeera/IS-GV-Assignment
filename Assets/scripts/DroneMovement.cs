using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DroneMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5.0f;
    public float rotationSpeed = 5.0f;

    [Header("Animation Settings (Student 4)")]
    public float hoverAmplitude = 0.5f;
    public float hoverFrequency = 2.0f;
    public float bankAmount = 30.0f;
    public float propellerSpeed = 1500f;
    public Transform[] propellers;

    [Header("Pathfinding Setup")]
    public Transform target;
    private GridManager gridManager;
    private List<Node> currentPath;
    private int currentPathIndex = 0;
    private float baseY;

    [Header("Storm Helper Settings")]
    public Transform player; // Drag your "character" here in the Inspector!

    private bool startMoving = false;
    private bool isPrompting = false;
    private bool hasAnswered = false;
    private bool isVisible = false;

    public enum ActiveAlgorithm
    {
        BFS,
        AStar
    }

    [Header("Search Algorithms Setup")]
    public ActiveAlgorithm currentAlgorithm = ActiveAlgorithm.AStar;
    public BFSSearchAlgorithm bfsAlgorithm;
    public AStarSearchAlgorithm aStarAlgorithm;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();

        if (bfsAlgorithm == null) bfsAlgorithm = FindObjectOfType<BFSSearchAlgorithm>();
        if (aStarAlgorithm == null) aStarAlgorithm = FindObjectOfType<AStarSearchAlgorithm>();

        // Try to automatically find the player if it isn't assigned
        if (player == null)
        {
            GameObject pObj = GameObject.Find("character");
            if (pObj != null) player = pObj.transform;
        }

        // Hide the drone at the start of the game
        SetDroneVisible(false);
    }

    void Update()
    {
        // 1. STORM DETECTION & PROMPTING
        if (!hasAnswered)
        {
            // If the fog is turned on (which happens in your MakeStormy tool), trigger the prompt!
            if (RenderSettings.fog)
            {
                isPrompting = true;

                // Listen for player input
                if (Input.GetKeyDown(KeyCode.Y))
                {
                    hasAnswered = true;
                    isPrompting = false;
                    StartCoroutine(AppearAndStartSequence());
                }
                else if (Input.GetKeyDown(KeyCode.N))
                {
                    hasAnswered = true;
                    isPrompting = false;
                    Debug.Log("Navigation help declined.");
                }
            }
            else
            {
                isPrompting = false; // Turn off prompt if they switch back to Normal sunny weather
            }

            return; // Don't process any movement while waiting for the prompt
        }

        // 2. MOVEMENT LOGIC (Only runs after 'Y' is pressed and wait is over)
        if (isVisible)
        {
            SpinPropellers();
        }

        if (!startMoving)
        {
            if (isVisible) HoverInPlace();
            return;
        }

        // Try to find the target continuously if we don't have one
        if (target == null)
        {
            GameObject targetObj = GameObject.Find("Target");
            if (targetObj == null) targetObj = GameObject.Find("End");

            if (targetObj != null) target = targetObj.transform;
            else { HoverInPlace(); return; }
        }

        // Calculate path
        if (target != null && (currentPath == null || currentPath.Count == 0))
        {
            if (currentAlgorithm == ActiveAlgorithm.BFS && bfsAlgorithm != null)
            {
                currentPath = bfsAlgorithm.FindPath(gridManager, transform.position, target.position);
                if (currentPath != null && currentPath.Count > 0) currentPathIndex = 0;
            }
            else if (currentAlgorithm == ActiveAlgorithm.AStar && aStarAlgorithm != null)
            {
                currentPath = aStarAlgorithm.FindPath(gridManager, transform.position, target.position);
                if (currentPath != null && currentPath.Count > 0) currentPathIndex = 0;
            }
        }

        // Move the drone
        MoveAlongPath();
    }

    // --- GUI DISPLAY ---
    void OnGUI()
    {
        if (isPrompting)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 35;
            style.normal.textColor = Color.yellow;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;

            // Draw a simple shadow for readability
            GUIStyle shadowStyle = new GUIStyle(style);
            shadowStyle.normal.textColor = Color.black;

            string message = "STORM DETECTED!\nDo you need navigation help?\nPress [Y] for Yes or [N] for No";

            Rect rect = new Rect(0, Screen.height / 2 - 100, Screen.width, 200);
            Rect shadowRect = new Rect(2, Screen.height / 2 - 98, Screen.width, 200);

            GUI.Label(shadowRect, message, shadowStyle);
            GUI.Label(rect, message, style);
        }
    }

    // --- SEQUENCE LOGIC ---
    IEnumerator AppearAndStartSequence()
    {
        // 1. Teleport directly above the player
        if (player != null)
        {
            transform.position = player.position + new Vector3(0, 4.0f, 0);

            // Level out the rotation to face the same way as the player
            transform.rotation = Quaternion.Euler(0, player.eulerAngles.y, 0);
        }
        else
        {
            Debug.LogWarning("Player not found! Drone is spawning at its default location.");
        }

        // Record the new height for smooth hover bobbing
        baseY = transform.position.y;

        // 2. Make visible and turn on audio
        SetDroneVisible(true);

        // 3. Wait exactly 2 seconds
        yield return new WaitForSeconds(2.0f);

        // 4. Take off!
        startMoving = true;
        Debug.Log("Drone starting navigation!");
    }

    // --- HELPER FUNCTIONS ---
    void SetDroneVisible(bool state)
    {
        isVisible = state;

        // Turn meshes on or off
        MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer m in meshes)
        {
            m.enabled = state;
        }

        // Turn buzzing audio on or off
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null)
        {
            if (state) audio.Play();
            else audio.Stop();
        }
    }

    void HoverInPlace()
    {
        float bobOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        transform.position = new Vector3(transform.position.x, baseY + bobOffset, transform.position.z);

        Quaternion flatRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, flatRotation, Time.deltaTime * rotationSpeed);
    }

    void SpinPropellers()
    {
        if (propellers != null && propellers.Length > 0)
        {
            foreach (Transform prop in propellers)
            {
                if (prop != null) prop.Rotate(Vector3.up * propellerSpeed * Time.deltaTime, Space.Self);
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

            Vector3 currentPosXZ = new Vector3(transform.position.x, baseY, transform.position.z);
            Vector3 targetPosXZ = new Vector3(targetPosition.x, baseY, targetPosition.z);

            Vector3 newPosXZ = Vector3.MoveTowards(currentPosXZ, targetPosXZ, speed * Time.deltaTime);

            float bobOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
            transform.position = new Vector3(newPosXZ.x, baseY + bobOffset, newPosXZ.z);

            Vector3 direction = (targetPosXZ - currentPosXZ).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion flatLookRotation = Quaternion.LookRotation(direction);
                float turnAngle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
                float targetBankAngle = Mathf.Clamp(-turnAngle, -bankAmount, bankAmount);
                Quaternion bankRotation = Quaternion.Euler(0, 0, targetBankAngle);
                Quaternion finalTargetRotation = flatLookRotation * bankRotation;

                transform.rotation = Quaternion.Slerp(transform.rotation, finalTargetRotation, Time.deltaTime * rotationSpeed);
            }

            if (Vector3.Distance(currentPosXZ, targetPosXZ) < 0.1f)
            {
                currentPathIndex++;
            }
        }
        else
        {
            currentPath = null;
            currentPathIndex = 0;
            Debug.Log("Drone reached the target!");
        }
    }
}