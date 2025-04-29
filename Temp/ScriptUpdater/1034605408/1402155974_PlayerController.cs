using UnityEngine;

public class JumpKingController : MonoBehaviour
{
    [Header("Jump Settings")]
    public float maxJumpForce = 15f;
    public float jumpChargeSpeed = 20f;
    public float gravityMultiplier = 3f; // Heavier gravity for Jump King feel

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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        CheckGround();
        HandleJumpCharge();
    }

    void FixedUpdate()
    {
        ApplyGravity();
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, groundLayer);
    }

    void HandleJumpCharge()
    {
        if (isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                StartJumpCharge();
            }

            if (Input.GetButton("Jump") && isChargingJump)
            {
                ContinueJumpCharge();
            }

            if (Input.GetButtonUp("Jump") && isChargingJump)
            {
                ReleaseJumpCharge();
            }
        }
    }

    void StartJumpCharge()
    {
        isChargingJump = true;
        currentJumpForce = 0f;
        rb.linearVelocity = Vector3.zero;

        if (jumpChargeParticles) jumpChargeParticles.Play();
        if (jumpChargeSound) jumpChargeSound.Play();
    }

    void ContinueJumpCharge()
    {
        currentJumpForce += jumpChargeSpeed * Time.deltaTime;
        currentJumpForce = Mathf.Clamp(currentJumpForce, 0f, maxJumpForce);
    }

    void ReleaseJumpCharge()
    {
        // Pure vertical jump - no horizontal movement
        rb.AddForce(Vector3.up * currentJumpForce, ForceMode.Impulse);

        isChargingJump = false;

        if (jumpChargeParticles) jumpChargeParticles.Stop();
        if (jumpReleaseSound) jumpReleaseSound.Play();
    }

    void ApplyGravity()
    {
        if (!isGrounded)
        {
            rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
        }
    }
}