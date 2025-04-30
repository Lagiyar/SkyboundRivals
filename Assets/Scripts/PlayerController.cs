using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float maxJumpForce = 15f;
    public float jumpChargeSpeed = 20f;
    public float gravityMultiplier = 2f;
    public float moveSpeed = 5f;

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
    private Vector3 savedPosition;
    private bool inJump;

    void Start()
    {


        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.freezeRotation = true;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {

        // Save position when you press T
        if (Input.GetKeyDown(KeyCode.T))
        {
            savedPosition = transform.position;
            Debug.Log("Position saved: " + savedPosition);
        }

        // Teleport back to saved position when you press R
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = savedPosition;
            Debug.Log("Teleported to saved position");
        }
    

    CheckGround();
        HandleJumpCharge();
        HandleMovement();
    }

    void FixedUpdate()
    {
        HandleJumpMovement();
        ApplyGravity();
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.33f, groundLayer);
        if (isGrounded && inJump)
        {
            inJump = false;
            lockedMoveDirection = Vector3.zero;
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

                // Stop horizontal movement but keep vertical (if falling slightly)
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

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
                Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;

                float inputX = Input.GetAxisRaw("Horizontal");
                float inputZ = Input.GetAxisRaw("Vertical");

                lockedMoveDirection = (camForward * inputZ + camRight * inputX).normalized;

                Vector3 jumpVector = Vector3.up * currentJumpForce + lockedMoveDirection * currentJumpForce * 0.5f;
                rb.AddForce(jumpVector, ForceMode.Impulse);

                isChargingJump = false;
                inJump = true;

                if (jumpChargeParticles) jumpChargeParticles.Stop();
                if (jumpReleaseSound) jumpReleaseSound.Play();
            }
        }
    }

    void HandleMovement()
    {
        if (isGrounded && !isChargingJump && !inJump)
        {
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputZ = Input.GetAxisRaw("Vertical");

            Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;

            Vector3 moveDirection = (camForward * inputZ + camRight * inputX).normalized;

            Vector3 targetVelocity = moveDirection * moveSpeed;
            targetVelocity.y = rb.linearVelocity.y;

            rb.linearVelocity = targetVelocity;
        }
        else if (isGrounded && (isChargingJump || inJump))
        {
            // Prevent movement while charging or in jump
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void HandleJumpMovement()
    {
        if (inJump)
        {
            // No air control
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void ApplyGravity()
    {
        if (!isGrounded)
        {
            rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        }
    }



}
