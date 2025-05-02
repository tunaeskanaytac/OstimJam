using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool IsGrounded { get; private set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            IsGrounded = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            IsGrounded = false;
        }
    }
}
