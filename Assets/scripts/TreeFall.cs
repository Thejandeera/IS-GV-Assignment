using UnityEngine;

public class TreeFall : MonoBehaviour
{
    private Rigidbody rb;
    
    public enum FallDirection { Forward, Backward, Left, Right }
    
    [Header("Fall Settings")]
    public FallDirection chosenDirection = FallDirection.Forward; 
    public float fallForce = 50f; 

    private bool hasFallen = false;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "character" && !hasFallen) 
        {
            if (rb != null)
            {
                hasFallen = true;
                rb.constraints = RigidbodyConstraints.None;
                
                Vector3 pushDirection = Vector3.zero;

                switch (chosenDirection)
                {
                    case FallDirection.Forward: pushDirection = transform.forward; break;
                    case FallDirection.Backward: pushDirection = -transform.forward; break;
                    case FallDirection.Left: pushDirection = -transform.right; break;
                    case FallDirection.Right: pushDirection = transform.right; break;
                }

                // --- මෙන්න මෙතන තමයි වෙනස කරන්නේ ---
                // ගහේ ඉහළට (උදා: අඩි 10ක් උඩට) බලය දෙනවා
                // එතකොට ගහ විසි වෙන්නේ නැතුව වේගයෙන් පාරට පතබෑවෙනවා (Rotate වෙනවා)
                Vector3 forcePosition = rb.worldCenterOfMass + Vector3.up * 10f; 
                rb.AddForceAtPosition(pushDirection * fallForce, forcePosition, ForceMode.Impulse);
                
                Debug.Log("Signal Received: ගහ පාරට පතබෑවෙනවා!");
            }
        }
    }
}