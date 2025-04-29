using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float maxJumpForce = 15f;
    public float jumpChargeSpeed = 20f;
    public float gravityMultiplier = 2f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public Transform cameraTransform;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isChargingJump;
    private float currentJumpForce;
    private Vector3 lockedMoveDirection; // Direction locked when jumping
    private bool inJump; // Track if we're in a jump

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        CheckGround();
        HandleJumpCharge();
    }

    void FixedUpdate()
    {
        HandleMovement();
        ApplyGravity();
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, groundLayer);
        if (isGrounded && inJump)
        {
            inJump = false;
            lockedMoveDirection = Vector3.zero;
        }
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * moveZ + camRight * moveX).normalized;

        // Only allow movement control when grounded and not charging jump
        if (isGrounded && !isChargingJump)
        {
            Vector3 velocity = moveDirection * walkSpeed;
            velocity.y = rb.linearVelocity.y;
            rb.linearVelocity = velocity;

            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.2f);
            }
        }
        // When in air, maintain the locked direction (no air control)
        else if (inJump)
        {
            rb.linearVelocity = new Vector3(
                lockedMoveDirection.x * walkSpeed,
                rb.linearVelocity.y,
                lockedMoveDirection.z * walkSpeed
            );
        }
    }

    void HandleJumpCharge()
    {
        if (isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                isChargingJump = true;
                currentJumpForce = 0f;
                rb.linearVelocity = Vector3.zero; // Stop movement while charging
            }

            if (Input.GetButton("Jump") && isChargingJump)
            {
                currentJumpForce += jumpChargeSpeed * Time.deltaTime;
                currentJumpForce = Mathf.Clamp(currentJumpForce, 0f, maxJumpForce);
            }

            if (Input.GetButtonUp("Jump") && isChargingJump)
            {
                // Get movement direction at moment of jump
                float moveX = Input.GetAxis("Horizontal");
                float moveZ = Input.GetAxis("Vertical");
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();
                lockedMoveDirection = (camForward * moveZ + camRight * moveX).normalized;

                // Apply jump
                rb.AddForce(Vector3.up * currentJumpForce, ForceMode.Impulse);
                isChargingJump = false;
                inJump = true;
            }
        }
    }

    void ApplyGravity()
    {
        if (!isGrounded)
        {
            rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
        }
    }
}