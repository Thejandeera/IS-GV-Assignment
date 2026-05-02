using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public float speed = 10f;
    public float sprintMultiplier = 1.8f;
    public float mouseSensitivity = 2f;

    public Transform playerCamera;
    public Animator animator;

    public AudioSource footstepSound;
    public float stepInterval = 0.4f;
    private float stepTimer = 0f;

    public AudioSource jumpSound;
    public float jumpHeight = 1.5f;

    private CharacterController controller;
    private float xRotation = 0f;

    private float verticalVelocity = 0f;
    public float gravity = -9.81f;

    // --- Push Logic Settings ---
    private bool isPushing = false;
    [Header("Push Settings")]
    public float pushForce = 50f;      // තල්ලු කරන බලය
    public float pushDistance = 1.5f;  // ගහට කොපමණ ළං විය යුතුද
    public float pushingMass = 50f;    // තල්ලු කරන වෙලාවට ගහේ බර (Mass)
    public float originalMass = 10000f; // ගහේ සාමාන්‍ය බර

    private Rigidbody currentTreeRb;   // දැනට තල්ලු කරන ගහ මතක තබා ගැනීමට

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 1. Mouse Look (පරණ විදිහටමයි)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime * 100f;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime * 100f;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        transform.Rotate(Vector3.up * mouseX);

        // --- 2. Push Logic (F Key එක පාලනය කිරීම) ---
        if (Input.GetKey(KeyCode.F))
        {
            isPushing = true;
            if (animator != null) animator.SetBool("isPushing", true);
            
            // ගහ හඳුනාගෙන එහි බර අඩු කර තල්ලු කරන Function එක
            HandleTreePushing();
        }
        else
        {
            // F අතෑරපු සැනින් ගහේ බර ආපහු 10,000 කරනවා
            ResetTreeMass();

            isPushing = false;
            if (animator != null) animator.SetBool("isPushing", false);
        }

        // 3. Movement Logic
        float moveX = isPushing ? 0 : Input.GetAxis("Horizontal");
        float moveZ = isPushing ? 0 : Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        float activeSpeed = Input.GetKey(KeyCode.LeftShift) ? speed * sprintMultiplier : speed;
        move *= activeSpeed;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }

            if (Input.GetButtonDown("Jump") && !isPushing)
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

        // Footsteps (පරණ විදිහටමයි)
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

    // --- ගහ තල්ලු කිරීම සහ Mass එක අඩු කිරීමේ Function එක ---
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
                    
                    // තල්ලු කරන වෙලාවට Mass එක 50 (pushingMass) කරනවා
                    currentTreeRb.mass = pushingMass;

                    Vector3 pushDirection = transform.forward;
                    pushDirection.y = 0; // අහසට විසිවීම වැළැක්වීමට
                    currentTreeRb.AddForce(pushDirection * pushForce, ForceMode.Acceleration);
                }
            }
        }
    }

    // --- බර ආපහු 10,000 කිරීමේ Function එක ---
    void ResetTreeMass()
    {
        if (currentTreeRb != null)
        {
            currentTreeRb.mass = originalMass;
            currentTreeRb = null;
        }
    }
}