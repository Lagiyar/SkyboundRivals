using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpiderGravityController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Jumping")]
    public float jumpForce = 8f;
    public float maxJumpCharge = 2f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float gravityAlignSpeed = 2f;
    public float maxGravityDistance = 50f;

    private Rigidbody rb;
    private GravitySource gravitySource;
    private float jumpCharge;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 1f;                // Linear damping
        rb.angularDamping = 5f;         // Angular damping
        rb.freezeRotation = true;    // Prevent physics rotation
    }

    void Update()
    {
        HandleJumpInput();
    }

    void FixedUpdate()
    {
        UpdateGravity();
        Move();
    }

    void UpdateGravity()
    {
        gravitySource = FindClosestGravitySource();
        if (gravitySource == null) return;

        // Apply gravity force
        Vector3 gravity = gravitySource.GetGravity(transform.position);
        rb.AddForce(gravity, ForceMode.Acceleration);

        // Smoothly align to surface
        Vector3 gravityUp = -gravity.normalized;
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, gravityUp) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, gravityAlignSpeed * Time.deltaTime);
    }

    void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Camera-relative movement
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * v + camRight * h).normalized;

        // Only move if grounded
        if (IsGrounded())
        {
            rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);

            // Optional: Rotate to face movement direction
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, transform.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -transform.up, groundCheckDistance, groundLayer);
    }

    void HandleJumpInput()
    {
        if (Input.GetKey(KeyCode.Space) && IsGrounded())
        {
            jumpCharge += Time.deltaTime;
            jumpCharge = Mathf.Clamp(jumpCharge, 0, maxJumpCharge);
        }

        if (Input.GetKeyUp(KeyCode.Space) && IsGrounded())
        {
            rb.AddForce(transform.up * jumpForce * (1 + jumpCharge), ForceMode.Impulse);
            jumpCharge = 0;
        }
    }

    GravitySource FindClosestGravitySource()
    {
        GravitySource[] sources = FindObjectsOfType<GravitySource>();
        GravitySource closest = null;
        float minDistance = maxGravityDistance;

        foreach (var source in sources)
        {
            float distance = Vector3.Distance(transform.position, source.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = source;
            }
        }
        return closest;
    }
}