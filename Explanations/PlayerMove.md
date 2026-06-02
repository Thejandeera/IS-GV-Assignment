# PlayerMove.cs - Explanation

## Role Alignment
This file primarily aligns with **Student 2 (The Systems Engineer: Player Interaction Physics)**, but it tightly integrates with your role as **Student 1 (Custom Graph Formulation)** by updating the graph dynamically.

## Code Explanation & Mechanism

### 1. Physics and Movement
```csharp
void Update()
{
    // ... camera rotation logic ...

    float moveX = isPushing ? 0 : Input.GetAxis("Horizontal");
    float moveZ = isPushing ? 0 : Input.GetAxis("Vertical");
    Vector3 move = transform.right * moveX + transform.forward * moveZ;
    
    // Gravity simulation
    if (controller.isGrounded)
    {
        if (verticalVelocity < 0) verticalVelocity = -2f;
        // Jump logic using kinematic equation
        if (Input.GetButtonDown("Jump") && !isPushing)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    verticalVelocity += gravity * Time.deltaTime;
    move.y = verticalVelocity;
    controller.Move(move * Time.deltaTime);
}
```
**Mechanism & Theory:**
- Uses Unity's `CharacterController`, which allows for kinematic movement (ignores complex rigidbody physics, making player movement feel snappy and responsive).
- **Gravity Equation:** Implements physics mathematically using $v = \sqrt{2gh}$ (velocity = square root of 2 * gravity * height) to calculate the exact impulse required to reach the desired `jumpHeight`.

### 2. Interacting with the Environment & Graph (Integration with Student 1)
```csharp
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
                // Physics force application
                treeRb.mass = pushingMass;
                Vector3 pushDirection = transform.forward;
                pushDirection.y = 0;
                treeRb.AddForce(pushDirection * pushForce, ForceMode.Acceleration);

                // Graph Recalculation (Student 1 integration)
                if (gridManager != null)
                {
                    gridUpdateTimer += Time.deltaTime;
                    if (gridUpdateTimer >= 0.15f)
                    {
                        gridManager.AddDynamicObstacle(hit.collider.gameObject, hit.collider);
                        gridUpdateTimer = 0f;
                    }
                }
            }
        }
    }
}
```
**Mechanism & Theory:**
- **Physics Interaction:** Uses a Raycast to detect objects tagged "Pushable". It temporarily reduces the object's mass so the player can push it using `AddForce`.
- **Dynamic Graph Severing:** As the object moves, the script continuously passes the object's collider data to your `gridManager.AddDynamicObstacle()` method. This allows your graph formulation algorithm to dynamically sever edges in real-time, effectively blocking off paths on the NavMesh as the player manipulates the environment. The `gridUpdateTimer` acts as a throttle to prevent recalculating the graph every single frame (optimizing CPU usage).
