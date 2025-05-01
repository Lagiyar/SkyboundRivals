using UnityEngine;

public class BouncePad : MonoBehaviour
{
    public float bounceForce = 15f; // Customize as needed
    public Vector3 bounceDirection = Vector3.up;
    public AudioSource bounceSound;

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.rigidbody;
        if (rb != null)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // Optional: reset vertical velocity
            rb.AddForce(bounceDirection.normalized * bounceForce, ForceMode.VelocityChange);
        }
        if (bounceSound != null)
        {
            bounceSound.Play();
        }
    }
}
