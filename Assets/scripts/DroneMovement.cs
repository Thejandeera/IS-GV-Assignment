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
    public Transform player;

    private bool startMoving = false;
    private bool isPrompting = false;
    private bool hasAnswered = false;
    private bool isVisible = false;

  
    private bool isWaitingForEnter = false;
    private bool showEnterMessage = false;

    public enum ActiveAlgorithm { BFS, AStar }

    [Header("Search Algorithms Setup")]
    public ActiveAlgorithm currentAlgorithm = ActiveAlgorithm.AStar;
    public BFSSearchAlgorithm bfsAlgorithm;
    public AStarSearchAlgorithm aStarAlgorithm;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();

        if (bfsAlgorithm == null) bfsAlgorithm = FindObjectOfType<BFSSearchAlgorithm>();
        if (aStarAlgorithm == null) aStarAlgorithm = FindObjectOfType<AStarSearchAlgorithm>();

        if (player == null)
        {
            GameObject pObj = GameObject.Find("character");
            if (pObj != null) player = pObj.transform;
        }

       
        SetDroneVisible(false);
    }

    void Update()
    {
        
        if (!hasAnswered)
        {
            if (RenderSettings.fog)
            {
                isPrompting = true;

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
                isPrompting = false;
            }
            return;
        }

        
        if (isVisible) SpinPropellers();

     
        if (isWaitingForEnter)
        {
            HoverInPlace();

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                isWaitingForEnter = false;
                showEnterMessage = false; 
                startMoving = true;
                Debug.Log("Drone starting navigation!");
            }
            return; 
        }

      
        if (!startMoving) return;

       
        if (target == null)
        {
            GameObject targetObj = GameObject.Find("Target") ?? GameObject.Find("End");
            if (targetObj != null) target = targetObj.transform;
            else { HoverInPlace(); return; }
        }

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

        
        MoveAlongPath();
    }

    
    void OnGUI()
    {
        if (isPrompting || showEnterMessage)
        {
            float boxWidth = 450;
            float boxHeight = 130;
            Rect boxRect = new Rect((Screen.width - boxWidth) / 2, Screen.height / 2 - 150, boxWidth, boxHeight);

            
            GUI.color = new Color(0, 0, 0, 0.85f);
            GUI.Box(boxRect, "");
            GUI.color = Color.white;

            
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(1f, 0.8f, 0.2f); 

            GUIStyle subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            subStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);

          
            if (isPrompting)
            {
                GUI.Label(new Rect(boxRect.x, boxRect.y + 20, boxWidth, 40), "STORM DETECTED", titleStyle);
                GUI.Label(new Rect(boxRect.x, boxRect.y + 65, boxWidth, 40), "Navigation assistance available.\nPress [Y] to deploy Drone  |  Press [N] to dismiss", subStyle);
            }
            else if (showEnterMessage)
            {
                GUI.Label(new Rect(boxRect.x, boxRect.y + 20, boxWidth, 40), "DRONE DEPLOYED", titleStyle);
                GUI.Label(new Rect(boxRect.x, boxRect.y + 65, boxWidth, 40), "Ready for guidance.\nPress [ENTER] when you are ready to follow.", subStyle);
            }
        }
    }

 
    IEnumerator AppearAndStartSequence()
    {
        
        if (player != null)
        {
            transform.position = player.position + new Vector3(0, 4.0f, 0);
            transform.rotation = Quaternion.Euler(0, player.eulerAngles.y, 0);
        }

        baseY = transform.position.y;

        
        SetDroneVisible(true);

        
        isWaitingForEnter = true;
        showEnterMessage = true;

        
        yield return new WaitForSeconds(4.0f);
        showEnterMessage = false;

        
    }


    void SetDroneVisible(bool state)
    {
        isVisible = state;

        MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer m in meshes) m.enabled = state;

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
        if (propellers != null)
            foreach (Transform prop in propellers)
                if (prop != null) prop.Rotate(Vector3.up * propellerSpeed * Time.deltaTime, Space.Self);
    }

    void MoveAlongPath()
    {
        if (currentPath == null || currentPath.Count == 0) { HoverInPlace(); return; }

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
                Quaternion finalTargetRotation = flatLookRotation * Quaternion.Euler(0, 0, targetBankAngle);

                transform.rotation = Quaternion.Slerp(transform.rotation, finalTargetRotation, Time.deltaTime * rotationSpeed);
            }

            if (Vector3.Distance(currentPosXZ, targetPosXZ) < 0.1f) currentPathIndex++;
        }
        else
        {
            currentPath = null;
            currentPathIndex = 0;
            Debug.Log("Drone reached the target!");
        }
    }

    

    /// <summary>
    /// Safely stops the current flight path and recalculates a new one
    /// to the SAME target from the drone's current world position.
    /// The drone will hover in place for one frame, then resume moving
    /// on the very next Update() cycle.
    /// 
    /// Usage from any other script:
    ///     FindObjectOfType<DroneMovement>().RecalculatePath();
    /// </summary>
    public void RecalculatePath()
    {
        currentPath = null;
        currentPathIndex = 0;
        Debug.Log("<color=orange><b>Drone:</b></color> Path interrupted — recalculating from current position...");
    }

    /// <summary>
    /// Safely stops the current flight path, switches to a NEW target,
    /// and recalculates a route from the drone's current world position.
    /// Use this when the destination itself has changed during gameplay
    /// (e.g., a moving objective, a player-chosen waypoint, etc.).
    ///
    /// Usage from any other script:
    ///     Transform newDest = someGameObject.transform;
    ///     FindObjectOfType<DroneMovement>().RecalculatePath(newDest);
    /// </summary>
    public void RecalculatePath(Transform newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogWarning("<color=red>RecalculatePath:</color> New target is null. Ignoring request.");
            return;
        }

        
        target = newTarget;

        
        currentPath = null;
        currentPathIndex = 0;

        Debug.Log($"<color=orange><b>Drone:</b></color> Target changed to '{newTarget.name}' — recalculating path...");

       
    }
}