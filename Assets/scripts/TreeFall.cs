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

    [Range(0.1f, 2f)]
    public float minPitch = 0.85f;

    [Range(0.1f, 2f)]
    public float maxPitch = 1.15f;

    private bool hasFallen = false;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
       
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
        yield return new WaitForSeconds(1f);

        GridManager gridManager = Object.FindFirstObjectByType<GridManager>();
        if (gridManager != null && rb != null)
        {
            Collider treeCollider = rb.GetComponent<Collider>();
            if (treeCollider != null)
            {
                gridManager.AddDynamicObstacle(rb.gameObject, treeCollider);
            }
        }
    }
}