using UnityEngine;
using System.Collections;

public class TreeFall : MonoBehaviour
{
    private Rigidbody rb;

    public enum FallDirection
    {
        Forward,
        Backward,
        Left,
        Right
    }

    [Header("Fall Settings")]
    public FallDirection chosenDirection = FallDirection.Forward;
    public float fallForce = 50f;

    [Header("Audio Settings")]
    public AudioSource treeFallSound;
    public AudioSource treeDragSound; // බිම දිගේ ඇදෙන සද්දෙට

    [Range(0.1f, 2f)]
    public float minPitch = 0.85f;

    [Range(0.1f, 2f)]
    public float maxPitch = 1.15f;

    private bool hasFallen = false;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();

        // Auto-disable if the atmosphere is Normal (Stormy turns fog ON, Normal turns fog OFF)
        if (!RenderSettings.fog)
        {
            this.enabled = false;
        }
    }

    void Update()
    {
        // ගහ වැටුනට පස්සේ බිම දිගේ ඇදෙනවා නම් සද්දෙ ප්ලේ කරන්න
        if (hasFallen && rb != null && treeDragSound != null)
        {
            // ගහේ වේගය (velocity) 0.1 කට වඩා වැඩි නම් විතරක් සද්දෙ ඇහෙන්න ඕන
            if (rb.linearVelocity.magnitude > 0.1f || rb.angularVelocity.magnitude > 0.1f)
            {
                if (!treeDragSound.isPlaying)
                {
                    treeDragSound.Play();
                }
            }
            else
            {
                if (treeDragSound.isPlaying)
                {
                    treeDragSound.Stop();
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Unity calls OnTriggerEnter even if the script is disabled! We must check it manually.
        if (!this.enabled) return;


        if ((other.name == "character" || other.name == "drone-model") && !hasFallen)
        {
            if (rb != null)
            {
                hasFallen = true;
                rb.constraints = RigidbodyConstraints.None;

                Vector3 pushDirection = Vector3.zero;

                switch (chosenDirection)
                {
                    case FallDirection.Forward:
                        pushDirection = transform.forward;
                        break;

                    case FallDirection.Backward:
                        pushDirection = -transform.forward;
                        break;

                    case FallDirection.Left:
                        pushDirection = -transform.right;
                        break;

                    case FallDirection.Right:
                        pushDirection = transform.right;
                        break;
                }

                Vector3 forcePosition = rb.worldCenterOfMass + Vector3.up * 10f;

                rb.AddForceAtPosition(
                    pushDirection * fallForce,
                    forcePosition,
                    ForceMode.Impulse
                );

                if (treeFallSound != null)
                {
                    treeFallSound.pitch = Random.Range(minPitch, maxPitch);
                    treeFallSound.Play();
                }

                Debug.Log("Signal Received: Tree is falling by " + other.name);
                
                StartCoroutine(UpdateGridAfterFall());
            }
        }
    }

    private IEnumerator UpdateGridAfterFall()
    {
        // Wait 1 second before starting to paint it red, as requested
        yield return new WaitForSeconds(1f);

        GridManager gridManager = Object.FindFirstObjectByType<GridManager>();
        if (gridManager == null || rb == null) yield break;

        Collider treeCollider = rb.GetComponent<Collider>();
        if (treeCollider == null) yield break;

        // Loop for a few more seconds to keep updating the grid as the tree bounces or settles
        float timer = 0f;
        while (timer < 4f)
        {
            gridManager.AddDynamicObstacle(rb.gameObject, treeCollider);
            yield return new WaitForSeconds(0.2f);
            timer += 0.2f;
        }

        // One final lock-in
        gridManager.AddDynamicObstacle(rb.gameObject, treeCollider);
    }
}