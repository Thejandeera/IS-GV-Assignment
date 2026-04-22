using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public float speed = 10f;
    public float mouseSensitivity = 2f;

    public Transform playerCamera;
    public Animator animator;

    // =======================
    // AUDIO VARIABLES
    // =======================
    public AudioSource footstepSound;
    public float stepInterval = 0.4f;
    private float stepTimer = 0f;

    private CharacterController controller;
    private float xRotation = 0f;

    // Variables for gravity
    private float verticalVelocity = 0f;
    public float gravity = -9.81f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // =======================
        // MOUSE LOOK
        // =======================
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // =======================
        // MOVEMENT & GRAVITY
        // =======================
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        verticalVelocity += gravity * Time.deltaTime;

        move.y = verticalVelocity;
        controller.Move(move * speed * Time.deltaTime);

        // =======================
        // ANIMATION
        // =======================
        float currentSpeed = new Vector2(moveX, moveZ).magnitude;

        if (animator != null)
        {
            animator.SetFloat("Speed", currentSpeed);
        }

        // =======================
        // FOOTSTEP LOGIC
        // =======================
        if (currentSpeed > 0.1f && controller.isGrounded)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                footstepSound.pitch = Random.Range(0.85f, 1.15f);
                footstepSound.Play();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;

            // NEW: Instantly cut off the audio the moment the player stops!
            if (footstepSound.isPlaying)
            {
                footstepSound.Stop();
            }
        }
    }
}