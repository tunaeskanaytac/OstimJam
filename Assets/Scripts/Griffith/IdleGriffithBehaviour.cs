using UnityEngine;

public class IdleGriffithBehaviour : MonoBehaviour
{
    private Collider2D _collider;
    [SerializeField] private GameObject canvas;

    private void Start()
    {
        _collider = GetComponent<CircleCollider2D>();
        Debug.Log($"Trigger object setup - Collider isTrigger: {_collider.isTrigger}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Log ALL collisions to see what's actually triggering
        Debug.Log($"Collision with: {other.gameObject.name}" +
                  $"\nTag: {other.tag}" +
                  $"\nLayer: {LayerMask.LayerToName(other.gameObject.layer)}" +
                  $"\nHas Rigidbody2D: {other.GetComponent<Rigidbody2D>() != null}");

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player tag detected - activating canvas");
            canvas.SetActive(true);
        }
    }

    // Optional: Add this to see continuous triggers
    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log($"Staying in trigger with: {other.gameObject.name}");
    }
}