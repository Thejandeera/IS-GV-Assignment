using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public float speed = 10f;
    public float sprintMultiplier = 1.8f;
    public float mouseSensitivity = 2f;

    public Transform playerCamera;
    public Animator animator;

    // --- පළමු කේතයෙන් ගත් මොඩල් එක හැරවීමේ Settings ---
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

        if (pushUI != null)
            pushUI.SetActive(false);

        // පළමු කේතයේ තිබූ Start Logic එක (Model එක ඉබේම සොයා ගැනීම)
        if (characterModel == null && animator != null)
        {
            characterModel = animator.transform;
        }
    }

    void Update()
    {
        // 1. Mouse Look Logic
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime * 100f;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime * 100f;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        transform.Rotate(Vector3.up * mouseX);

        // 2. Push Logic
        CheckForPushUI();

        if (Input.GetKey(KeyCode.F))
        {
            isPushing = true;
            if (animator != null) animator.SetBool("isPushing", true);
            HandleTreePushing();
        }
        else
        {
            ResetTreeMass();
            isPushing = false;
            if (animator != null) animator.SetBool("isPushing", false);
        }

        // 3. Push Sound
        HandlePushSound();

        // 4. Movement Logic
        float moveX = isPushing ? 0 : Input.GetAxis("Horizontal");
        float moveZ = isPushing ? 0 : Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        float activeSpeed = Input.GetKey(KeyCode.LeftShift) ? speed * sprintMultiplier : speed;
        move *= activeSpeed;

        // --- UPDATED: 8-WAY CHARACTER VISUAL ROTATION (මෙන්න ඔයාට අවශ්‍ය වුණු කොටස) ---
        // තල්ලු කරමින් සිටින විට මෙය ක්‍රියාත්මක නොවේ (ප්ලේයර් ඉදිරියට පමණක් මුහුණ ලා සිටිය යුතු බැවිනි)
        if (characterModel != null && characterModel != this.transform && !isPushing)
        {
            float targetAngle = 0f;
            Vector3 inputDir = new Vector3(moveX, 0f, moveZ).normalized;

            if (inputDir.magnitude >= 0.1f)
            {
                targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
                characterModel.localRotation = Quaternion.Slerp(characterModel.localRotation, targetRotation, Time.deltaTime * turnSpeed);
            }
        }

        // 5. Physics & Jump
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0) verticalVelocity = -2f;

            if (Input.GetButtonDown("Jump") && !isPushing)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (animator != null) animator.SetTrigger("JumpTrigger");
                if (jumpSound != null) jumpSound.Play();
            }
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;
        controller.Move(move * Time.deltaTime);

        // 6. Animation & Footsteps
        float currentSpeed = new Vector2(moveX, moveZ).magnitude;
        if (animator != null) animator.SetFloat("Speed", currentSpeed);
        HandleFootsteps(currentSpeed);
    }

    void HandlePushSound()
    {
        if (isPushing && currentTreeRb != null)
        {
            if (pushSoundSource != null && !pushSoundSource.isPlaying) pushSoundSource.Play();
        }
        else
        {
            if (pushSoundSource != null && pushSoundSource.isPlaying) pushSoundSource.Stop();
        }
    }

    void CheckForPushUI()
    {
        if (pushUI == null) return;
        RaycastHit hit;
        // Third Person කැමරාවට අනුව Raycast එක ප්ලේයර්ගේ ඇඟෙන් ඉදිරියට විදියි
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, pushDistance))
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
            if (footstepSound != null && footstepSound.isPlaying) footstepSound.Stop();
        }
    }
}

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

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

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

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        float activeSpeed = Input.GetKey(KeyCode.LeftShift) ? speed * sprintMultiplier : speed;
        move *= activeSpeed;

        // --- UPDATED: 8-WAY CHARACTER VISUAL ROTATION ---
        if (characterModel != null && characterModel != this.transform)
        {
            float targetAngle = 0f; // Default is facing forward (0 degrees)

            // Create an input vector based on your keyboard presses
            Vector3 inputDir = new Vector3(moveX, 0f, moveZ).normalized;

            // If the player is pressing ANY movement key...
            if (inputDir.magnitude >= 0.1f)
            {
                // Calculate the exact angle (Left, Right, Backwards, or Diagonals!)
                targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;
            }

            // Smoothly rotate the visual model to the correct angle
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            characterModel.localRotation = Quaternion.Slerp(characterModel.localRotation, targetRotation, Time.deltaTime * turnSpeed);
        }

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }

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

        verticalVelocity += gravity * Time.deltaTime;

        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);

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