using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target & Follow")]
    public Transform target;
    public Vector3 baseOffset = new Vector3(0f, 2f, -5f);

    [Header("Mouse Rotation")]
    public float mouseSensitivity = 100f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 70f;

    [Header("Rotation Smoothness")]
    public float rotationSmoothTime = 0.1f;
    private Vector3 currentRotation;
    private Vector3 rotationSmoothVelocity;

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minZoom = -2f;
    public float maxZoom = -10f;

    [Header("Collision Settings")]
    public float collisionRadius = 0.3f;
    public LayerMask collisionLayer; // Specify which layers the camera can collide with (not the ground layer)
    public LayerMask groundLayer;    // Specify which layer is considered the ground (to avoid clipping through it)

    private float mouseX, mouseY;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        mouseX = target.eulerAngles.y;
        mouseY = 15f;
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleRotation();
        HandleZoom();
        HandleCollision();
        FollowTarget();
    }

    void HandleRotation()
    {
        mouseX += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        mouseY = Mathf.Clamp(mouseY, minVerticalAngle, maxVerticalAngle);
    }

    void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            baseOffset.z += scrollInput * zoomSpeed;
            baseOffset.z = Mathf.Clamp(baseOffset.z, maxZoom, minZoom);
        }
    }

    void HandleCollision()
    {
        // Raycast to prevent clipping through the ground
        RaycastHit groundHit;
        if (Physics.Raycast(target.position, Vector3.down, out groundHit, Mathf.Infinity, groundLayer))
        {
            // If the camera is too close to the ground, adjust its position
            float groundDistance = groundHit.distance + collisionRadius;
            baseOffset.y = Mathf.Max(groundDistance, baseOffset.y); // Keeps the camera above the ground
        }

        // Raycast to prevent clipping through walls and other obstacles
        Vector3 direction = baseOffset.normalized; // Direction from target to camera
        RaycastHit hit;

        if (Physics.Raycast(target.position, direction, out hit, baseOffset.magnitude, collisionLayer))
        {
            // If there's a collision with a wall, move the camera to the hit point minus the collision radius
            float distance = Mathf.Clamp(hit.distance - collisionRadius, 0f, baseOffset.magnitude);
            baseOffset = direction * distance;
        }
    }

    void FollowTarget()
    {
        Vector3 targetRotation = new Vector3(mouseY, mouseX, 0f);

        // Smooth the rotation
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationSmoothVelocity, rotationSmoothTime);

        Quaternion rotation = Quaternion.Euler(currentRotation);
        Vector3 offsetPosition = rotation * baseOffset;

        // Apply the position and rotation
        transform.position = target.position + offsetPosition;
        transform.rotation = rotation;
    }
}
