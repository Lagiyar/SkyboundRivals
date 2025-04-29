using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpiderGravityController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public Camera playerCamera; // FIX: Assign in Inspector

    [Header("Jumping")]
    public float jumpForce = 8f;
    public float maxJumpCharge = 2f;
    public float groundCheckDistance = 0.5f; // FIX: Increased from 0.2f
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float gravityAlignSpeed = 2f;
    public float maxGravityDistance = 50f;

    private Rigidbody rb;
    private GravitySource gravitySource;
    private float jumpCharge;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 1f; // FIX: Unity uses 'drag', not 'linearDamping'
        rb.angularDamping = 5f;
        rb.freezeRotation = true;

        if (playerCamera == null) // FIX: Fallback to MainCamera
            playerCamera = Camera.main;
    }

    void Update()
    {
        Debug.Log($"Grounded: {IsGrounded()} | Input: ({Input.GetAxis("Horizontal")}, {Input.GetAxis("Vertical")})"); // Debug
        HandleJumpInput();
    }

    void FixedUpdate()
    {
        UpdateGravity();
        Move();
    }

    void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 camForward = playerCamera.transform.forward;
        Vector3 camRight = playerCamera.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * v + camRight * h).normalized;

        if (IsGrounded())
        {
            // FIX: Try both methods (uncomment one)
            rb.AddForce(moveDirection * moveSpeed * 50f, ForceMode.Force); // Stronger force
            // rb.velocity = moveDirection * moveSpeed; // Alternative
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