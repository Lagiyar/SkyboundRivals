using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpiderGravityController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 12f;
    public float groundSnapDistance = 0.5f;
    public LayerMask groundLayer;

    [Header("Jump Settings")]
    public float jumpForce = 12f;
    public float maxJumpCharge = 2f;
    public float jumpCooldown = 0.2f;

    [Header("Gravity Settings")]
    public float gravityStrength = 9.8f;
    public float alignmentSpeed = 3f;
    public float maxGravityDistance = 100f;

    // Component References
    private Rigidbody rb;
    private Transform currentPlanet;
    private float lastJumpTime;
    private float currentJumpCharge;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 1.5f;
        rb.angularDamping = 5f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        HandleJump();
        DebugGroundStatus();
    }

    void FixedUpdate()
    {
        UpdateGravity();
        UpdateMovement();
        SnapToGround();
    }

    void UpdateGravity()
    {
        currentPlanet = FindNearestPlanet();
        if (currentPlanet == null) return;

        Vector3 gravityDirection = (transform.position - currentPlanet.position).normalized;
        rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);

        // Align to planet surface
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -gravityDirection) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, alignmentSpeed * Time.fixedDeltaTime);
    }

    void UpdateMovement()
    {
        if (!isGrounded) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(h, 0, v).normalized;

        if (moveDirection.magnitude > 0.1f)
        {
            // Camera-relative movement
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 worldDirection = camForward * v + camRight * h;
            rb.AddForce(worldDirection * moveSpeed * 10f, ForceMode.Force);

            // Smooth rotation
            Quaternion targetRotation = Quaternion.LookRotation(worldDirection, transform.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void HandleJump()
    {
        if (Time.time < lastJumpTime + jumpCooldown) return;

        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            currentJumpCharge += Time.deltaTime;
            currentJumpCharge = Mathf.Clamp(currentJumpCharge, 0, maxJumpCharge);
        }

        if (Input.GetKeyUp(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(transform.up * jumpForce * (1 + currentJumpCharge), ForceMode.Impulse);
            lastJumpTime = Time.time;
            currentJumpCharge = 0;
            isGrounded = false;
        }
    }

    void SnapToGround()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, groundSnapDistance, groundLayer))
        {
            isGrounded = true;
            // Small correction to prevent bouncing
            if (hit.distance > 0.1f)
            {
                rb.MovePosition(hit.point + hit.normal * 0.1f);
            }
        }
        else
        {
            isGrounded = false;
        }
    }

    Transform FindNearestPlanet()
    {
        Collider[] planets = Physics.OverlapSphere(transform.position, maxGravityDistance, groundLayer);
        Transform closestPlanet = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider planet in planets)
        {
            float distance = Vector3.Distance(transform.position, planet.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlanet = planet.transform;
            }
        }
        return closestPlanet;
    }

    void DebugGroundStatus()
    {
        Debug.DrawRay(transform.position, -transform.up * groundSnapDistance, isGrounded ? Color.green : Color.red);
    }
}