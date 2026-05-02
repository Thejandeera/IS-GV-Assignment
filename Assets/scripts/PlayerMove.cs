using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public float speed = 10f;
    public float sprintMultiplier = 1.8f;
    public float mouseSensitivity = 2f;

    public Transform playerCamera;
    public Animator animator;

    [Header("Model Rotation Settings")]
    public Transform characterModel;
    public float turnSpeed = 15f;

    public AudioSource footstepSound;
    public float stepInterval = 0.4f;
    private float stepTimer = 0f;

    public AudioSource jumpSound;
    public float jumpHeight = 1.5f;

    private CharacterController controller;
    private float xRotation = 0f;

    private float verticalVelocity = 0f;
    public float gravity = -9.81f;

    private bool isPushing = false;

    [Header("Push Settings")]
    public float pushForce = 50f;
    public float pushDistance = 1.5f;
    public float pushingMass = 50f;
    public float originalMass = 10000f;

    [Header("UI Settings")]
    public GameObject pushUI;

    [Header("Push Audio")]
    public AudioSource pushSoundSource;

    private Rigidbody currentTreeRb;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        // --- MERGED: Keep Friend's UI setup ---
        if (pushUI != null)
        {
            pushUI.SetActive(false);
        }

        // --- MERGED: Keep Your Auto-assign Model setup ---
        if (characterModel == null && animator != null)
        {
            characterModel = animator.transform;
        }
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime * 100f;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime * 100f;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        transform.Rotate(Vector3.up * mouseX);

        CheckForPushUI();

        if (Input.GetKey(KeyCode.F))
        {
            isPushing = true;

            if (animator != null)
                animator.SetBool("isPushing", true);

            HandleTreePushing();
        }
        else
        {
            ResetTreeMass();
            isPushing = false;

            if (animator != null)
                animator.SetBool("isPushing", false);
        }

        if (isPushing && currentTreeRb != null)
        {
            if (pushSoundSource != null && !pushSoundSource.isPlaying)
            {
                pushSoundSource.Play();
            }
        }
        else
        {
            if (pushSoundSource != null && pushSoundSource.isPlaying)
            {
                pushSoundSource.Stop();
            }
        }

        float moveX = isPushing ? 0 : Input.GetAxis("Horizontal");
        float moveZ = isPushing ? 0 : Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        float activeSpeed = Input.GetKey(KeyCode.LeftShift) ? speed * sprintMultiplier : speed;
        move *= activeSpeed;

        // --- MERGED: Your 8-Way Rotation Feature ---
        if (characterModel != null && characterModel != this.transform)
        {
            float targetAngle = 0f; // Default is facing forward (0 degrees)

            // Create an input vector based on your keyboard presses
            Vector3 inputDir = new Vector3(moveX, 0f, moveZ).normalized;

            // If the player is pressing ANY movement key...
            if (inputDir.magnitude >= 0.1f)
            {
                // Calculate the exact angle (Left, Right, Backwards, Diagonals)
                targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;
            }

            // Smoothly rotate the visual model to the correct angle
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            characterModel.localRotation = Quaternion.Slerp(characterModel.localRotation, targetRotation, Time.deltaTime * turnSpeed);
        }

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity = -2f;

            // Cannot jump while pushing
            if (Input.GetButtonDown("Jump") && !isPushing)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (animator != null)
                    animator.SetTrigger("JumpTrigger");

                if (jumpSound != null)
                    jumpSound.Play();
            }
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);

        float currentSpeed = new Vector2(moveX, moveZ).magnitude;

        if (animator != null)
            animator.SetFloat("Speed", currentSpeed);

        HandleFootsteps(currentSpeed);
    }

    // --- MERGED: Friend's new Push Functions ---

    void CheckForPushUI()
    {
        if (pushUI == null) return;

        RaycastHit hit;

        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, pushDistance))
        {
            if (hit.collider.CompareTag("Pushable"))
            {
                pushUI.SetActive(!isPushing);
            }
            else
            {
                pushUI.SetActive(false);
            }
        }
        else
        {
            pushUI.SetActive(false);
        }
    }

    void HandleTreePushing()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, pushDistance))
        {
            if (hit.collider.CompareTag("Pushable"))
            {
                Rigidbody treeRb = hit.collider.GetComponent<Rigidbody>();

                if (treeRb != null)
                {
                    currentTreeRb = treeRb;
                    currentTreeRb.mass = pushingMass;

                    Vector3 pushDirection = transform.forward;
                    pushDirection.y = 0;

                    currentTreeRb.AddForce(pushDirection * pushForce, ForceMode.Acceleration);
                }
            }
        }
    }

    void ResetTreeMass()
    {
        if (currentTreeRb != null)
        {
            currentTreeRb.mass = originalMass;
            currentTreeRb = null;
        }
    }

    void HandleFootsteps(float currentSpeed)
    {
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

                stepTimer = Input.GetKey(KeyCode.LeftShift) ? stepInterval / sprintMultiplier : stepInterval;
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