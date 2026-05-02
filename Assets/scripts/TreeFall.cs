using UnityEngine;

public class TreeFall : MonoBehaviour
{
    private Rigidbody rb;
    
    public enum FallDirection { Forward, Backward, Left, Right }
    
    [Header("Fall Settings")]
    public FallDirection chosenDirection = FallDirection.Forward; 
    public float fallForce = 50f; 

    [Header("Audio Settings")]
    public AudioSource treeFallSound; // Inspector එකෙන් Audio Source එක මෙතනට Drag කරන්න
    [Range(0.1f, 2f)]
    public float minPitch = 0.85f;    // සද්දේ වෙනස් වෙන්න ඕනේ අවම මට්ටම
    [Range(0.1f, 2f)]
    public float maxPitch = 1.15f;    // සද්දේ වෙනස් වෙන්න ඕනේ උපරිම මට්ටම

    private bool hasFallen = false;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // "character" කියන එක ඔයාගේ character එකේ නම බව සහතික කරගන්න
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

                // ගහේ ඉහළට බලය දීම
                Vector3 forcePosition = rb.worldCenterOfMass + Vector3.up * 10f; 
                rb.AddForceAtPosition(pushDirection * fallForce, forcePosition, ForceMode.Impulse);

                // --- Sound Logic එක මෙතනින් ආරම්භ වේ ---
                if (treeFallSound != null)
                {
                    // සද්දේ හැමතිස්සෙම එකම වගේ ඇහෙන්නේ නැති වෙන්න Pitch එක පොඩ්ඩක් වෙනස් කරනවා (Realistic effect)
                    treeFallSound.pitch = Random.Range(minPitch, maxPitch);
                    treeFallSound.Play();
                }
                
                Debug.Log("Signal Received: ගහ පාරට පතබෑවෙනවා සහ ශබ්දය ඇහෙනවා!");
            }
        }
    }
}