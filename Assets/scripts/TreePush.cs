using UnityEngine;

public class TreePush : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void PushTree(Vector3 direction, float force)
    {
        if (rb != null)
        {
            rb.AddForce(direction * force, ForceMode.Acceleration);
        }
    }
}