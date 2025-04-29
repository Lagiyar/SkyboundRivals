using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // Player movement speed
    public float jumpForce = 10f; // Jump force
    public float bounceForce = 5f; // Bounce force when hitting the side of a platform
    private Rigidbody rb; // Rigidbody for the player
    private bool isGrounded; // Whether the player is grounded or not

    // Set up physics materials
    public PhysicsMaterial platformMaterial; // The material to apply to platforms with friction

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get the player's Rigidbody component
    }

    void Update()
    {
        MovePlayer(); // Handle player movement
        if (isGrounded && Input.GetButtonDown("Jump")) // Jump only if grounded
        {
            Jump(); // Handle jump
        }
    }

    void MovePlayer()
    {
        // Get player input for horizontal movement
        float horizontal = Input.GetAxis("Horizontal");

        // Calculate movement direction
        Vector3 movement = new Vector3(horizontal, 0, 0) * moveSpeed * Time.deltaTime;

        // Apply movement to the Rigidbody
        rb.MovePosition(transform.position + movement);
    }

    void Jump()
    {
        // Apply upward force for the jump
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // Detect collision with platform
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            // When the player lands on a platform, apply the friction material
            if (!isGrounded) // Only apply if the player is not already grounded
            {
                collision.collider.material = platformMaterial;
            }

            // Set player as grounded
            isGrounded = true;
        }
    }

    // Detect collision and bounce off the side of the platform
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            // Loop through all contacts in the collision
            foreach (ContactPoint contact in collision.contacts)
            {
                Vector3 normal = contact.normal;

                // Check if the player is hitting the side of the platform (angle is not too steep)
                if (Vector3.Angle(normal, Vector3.up) > 45) // Sides of the platform
                {
                    // Apply a bounce force in the opposite direction of the normal
                    Vector3 bounceDirection = normal * -1; // Reverse normal direction
                    rb.AddForce(bounceDirection * bounceForce, ForceMode.Impulse); // Apply the bounce force
                }
            }
        }
    }

    // Check when the player leaves the platform
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = false; // Player is no longer grounded
        }
    }
}
