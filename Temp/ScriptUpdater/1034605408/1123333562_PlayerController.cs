using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float maxJumpForce = 15f;
    public float jumpChargeSpeed = 20f;
    public float gravityMultiplier = 2f;

    [Header("References")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public Transform cameraTransform;

    [Header("Effects")]
    public ParticleSystem jumpChargeParticles;
    public AudioSource jumpChargeSound;
    public AudioSource jumpReleaseSound;

    // Properties
    public bool IsChargingJump => isChargingJump;
    public float CurrentJumpForce => currentJumpForce;
    public float MaxJumpForce => maxJumpForce;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isChargingJump;
    private float currentJumpForce;
    private Vector3 lockedMoveDirection;
    private bool inJump;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
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

        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;

        Vector3 moveDirection = (camForward * moveZ + camRight * moveX).normalized;

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
                rb.linearVelocity = Vector3.zero;
                if (jumpChargeParticles) jumpChargeParticles.Play();
                if (jumpChargeSound) jumpChargeSound.Play();
            }

            if (Input.GetButton("Jump") && isChargingJump)
            {
                currentJumpForce += jumpChargeSpeed * Time.deltaTime;
                currentJumpForce = Mathf.Clamp(currentJumpForce, 0f, maxJumpForce);
            }

            if (Input.GetButtonUp("Jump") && isChargingJump)
            {
                float moveX = Input.GetAxis("Horizontal");
                float moveZ = Input.GetAxis("Vertical");
                Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
                lockedMoveDirection = (camForward * moveZ + camRight * moveX).normalized;

                rb.AddForce(Vector3.up * currentJumpForce, ForceMode.Impulse);
                isChargingJump = false;
                inJump = true;
                if (jumpChargeParticles) jumpChargeParticles.Stop();
                if (jumpReleaseSound) jumpReleaseSound.Play();
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