using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 2, -4);
    public float distance = 5.0f;
    public float mouseSensitivity = 3.0f;
    public float rotationSmoothTime = 0.1f;
    public Vector2 pitchClamp = new Vector2(-30, 60);

    private float yaw = 0f;
    private float pitch = 15f;
    private Vector3 rotationSmoothVelocity;
    private Vector3 currentRotation;

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

        // Apply rotation and position
        Quaternion rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);
        Vector3 cameraPosition = player.position + rotation * offset.normalized * distance;

        transform.position = cameraPosition;
        transform.LookAt(player.position + Vector3.up * 1.5f); // Adjust if needed
    }
}
