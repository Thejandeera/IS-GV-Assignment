using UnityEngine;

public class TreePush : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // මේ function එක ප්ලේයර්ගේ script එකෙන් call කරනවා
    public void PushTree(Vector3 direction, float force)
    {
        if (rb != null)
        {
            // ගහට බලයක් දීලා තල්ලු කරනවා
            rb.AddForce(direction * force, ForceMode.Acceleration);
        }
    }
}