using UnityEngine;

public class LedgeCheck : MonoBehaviour
{
    public bool IsOnLedge { get; private set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            IsOnLedge = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            IsOnLedge = false;
        }
    }
}
