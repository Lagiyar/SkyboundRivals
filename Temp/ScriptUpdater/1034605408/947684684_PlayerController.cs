using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Jump Settings")]
    public float maxJumpForce = 15f;
    public float jumpChargeSpeed = 20f;
    public float gravityMultiplier = 3f;

    [Header("References")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public Transform cameraTransform;

    [Header("Effects")]
    public ParticleSystem jumpChargeParticles;
    public AudioSource jumpChargeSound;
    public AudioSource jumpReleaseSound;

    // Properties
    public bool IsChargingJump { get { return isChargingJump; } }
    public float CurrentJumpForce { get { return currentJumpForce; } }
    public float MaxJumpForce { get { return maxJumpForce; } }

    private Rigidbody rb;
    private bool isGrounded;
    private bool isChargingJump;
    private float currentJumpForce;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        if (jumpChargeParticles != null) jumpChargeParticles.Play();
        if (jumpChargeSound != null) jumpChargeSound.Play();
    }

    void ContinueJumpCharge()
    {
        currentJumpForce += jumpChargeSpeed * Time.deltaTime;
        currentJumpForce = Mathf.Clamp(currentJumpForce, 0f, maxJumpForce);
    }

    void ReleaseJumpCharge()
    {
        rb.AddForce(Vector3.up * currentJumpForce, ForceMode.Impulse);
        isChargingJump = false;

        if (jumpChargeParticles != null) jumpChargeParticles.Stop();
        if (jumpReleaseSound != null) jumpReleaseSound.Play();
    }

    void ApplyGravity()
    {
        if (!isGrounded)
        {
            rb.AddForce(Physics.gravity * gravityMultiplier * rb.mass, ForceMode.Force);
        }
    }
}