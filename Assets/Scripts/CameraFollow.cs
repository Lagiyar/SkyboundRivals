using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 2, -4);
    public float distance = 5.0f;
    public float mouseSensitivity = 3.0f;
    public float rotationSmoothTime = 0.1f;
    public Vector2 pitchClamp = new Vector2(-30, 60);
    public float cameraCollisionRadius = 0.3f;
    public LayerMask collisionLayers;

    private float yaw = 0f;
    private float pitch = 15f;
    private Vector3 rotationSmoothVelocity;
    private Vector3 currentRotation;

    private Vector3 currentPositionVelocity;

    void LateUpdate()
    {
        if (player == null) return;

        // Mouse input
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchClamp.x, pitchClamp.y);

        // Smooth rotation
        Vector3 targetRotation = new Vector3(pitch, yaw);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationSmoothVelocity, rotationSmoothTime);

        // Calculate desired camera position
        Quaternion rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);
        Vector3 desiredCameraPosition = player.position + rotation * offset.normalized * distance;

        // Raycast to avoid wall clipping
        Vector3 directionToCamera = (desiredCameraPosition - player.position).normalized;
        float targetDistance = distance;
        RaycastHit hit;

        if (Physics.SphereCast(player.position, cameraCollisionRadius, directionToCamera, out hit, distance, collisionLayers))
        {
            targetDistance = hit.distance - 0.1f; // slightly pull in to avoid clipping
            targetDistance = Mathf.Max(0.5f, targetDistance); // prevent too close
        }

        Vector3 targetPosition = player.position + rotation * offset.normalized * targetDistance;

        // Smooth "springrig-like" camera movement
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentPositionVelocity, 0.05f);

        // Always look at player (adjust height if needed)
        transform.LookAt(player.position + Vector3.up * 1.5f);
    }
}
