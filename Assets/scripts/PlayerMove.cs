using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 10f;

    void Update()
    {
        // Get WASD or Arrow Key input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Move the capsule based on input
        Vector3 movement = new Vector3(moveX, 0, moveZ) * speed * Time.deltaTime;
        transform.Translate(movement);
    }
}