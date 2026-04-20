using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public float speed = 10f;
    public float mouseSensitivity = 2f;

    // Drag your nested Camera into this slot in the Inspector
    public Transform playerCamera;

    private CharacterController controller;
    private float xRotation = 0f;

    // Variables for gravity
    private float verticalVelocity = 0f;
    public float gravity = -9.81f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Locks the mouse cursor to the center of the screen and hides it
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // =======================
        // MOUSE LOOK
        // =======================
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Up and Down looking (Rotates the Camera)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevents your head from flipping upside down
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Left and Right looking (Rotates the whole Player body)
        transform.Rotate(Vector3.up * mouseX);


        // =======================
        // MOVEMENT & GRAVITY
        // =======================
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Calculate movement relative to where the player is looking
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Apply gravity to push the player down onto the terrain
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // Keep a small downward force when walking
        }
        verticalVelocity += gravity * Time.deltaTime;

        // Add the gravity to our movement vector
        move.y = verticalVelocity;

        // Tell the Character Controller to move us (this respects collisions!)
        controller.Move(move * speed * Time.deltaTime);
    }
}