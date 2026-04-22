using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public float speed = 10f;
    public float mouseSensitivity = 2f;

    public Transform playerCamera;
    public Animator animator;

    // =======================
    // AUDIO & JUMP VARIABLES
    // =======================
    public AudioSource footstepSound;
    public float stepInterval = 0.4f;
    private float stepTimer = 0f;

    public AudioSource jumpSound;
    public float jumpHeight = 1.5f;

    private CharacterController controller;
    private float xRotation = 0f;

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
        // MOUSE LOOK (Smoothed with Time.deltaTime)
        // =======================
        // Multiplied by Time.deltaTime and 100f to keep sensitivity consistent across frame rates
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime * 100f;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime * 100f;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Rotates the camera up and down
        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        // Rotates the whole player body left and right
        transform.Rotate(Vector3.up * mouseX);

        // =======================
        // MOVEMENT
        // =======================
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Multiply horizontal movement by speed FIRST!
        move *= speed;

        // =======================
        // GRAVITY & JUMPING
        // =======================
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }

            // You can jump anytime you are on the ground (moving or stopped!)
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (animator != null)
                {
                    animator.SetTrigger("JumpTrigger");
                }

                if (jumpSound != null)
                {
                    jumpSound.Play();
                }
            }
        }

        // Apply gravity over time
        verticalVelocity += gravity * Time.deltaTime;

        // Apply the raw vertical velocity to our movement
        move.y = verticalVelocity;

        // Only multiply by Time.deltaTime here, so gravity stays accurate!
        controller.Move(move * Time.deltaTime);

        // =======================
        // ANIMATION & FOOTSTEPS
        // =======================
        float currentSpeed = new Vector2(moveX, moveZ).magnitude;

        if (animator != null)
        {
            animator.SetFloat("Speed", currentSpeed);
        }

        if (currentSpeed > 0.1f && controller.isGrounded)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                if (footstepSound != null)
                {
                    footstepSound.pitch = Random.Range(0.85f, 1.15f);
                    footstepSound.Play();
                }
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
            if (footstepSound != null && footstepSound.isPlaying)
            {
                footstepSound.Stop();
            }
        }
    }
}