using UnityEngine;

public class DroneMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 10.0f;

    void Update()
    {
        Vector3 moveDirection = Vector3.zero;

        // Check for specific key presses
        if (Input.GetKey(KeyCode.T)) 
        {
            moveDirection += Vector3.forward; // Move Forward
        }
        if (Input.GetKey(KeyCode.B)) 
        {
            moveDirection += Vector3.back;    // Move Backward
        }
        if (Input.GetKey(KeyCode.F)) 
        {
            moveDirection += Vector3.left;    // Move Left
        }
        if (Input.GetKey(KeyCode.H)) 
        {
            moveDirection += Vector3.right;   // Move Right
        }

        // Normalize the vector so diagonal movement isn't faster
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        // Apply the movement to the drone's transform (Space.Self makes it move relative to where it is facing)
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.Self);
    }
}