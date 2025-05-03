using UnityEngine;
using UnityEngine.Events;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private int damageAmount = 10;
    public bool isPlayerHitbox = false;
    
    public UnityEvent onParried;
    private bool isParried = false;

    private void Start()
    {
        if (onParried == null)
            onParried = new UnityEvent();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.Die();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isParried) return;

        if (isPlayerHitbox)
        {
            // Check for parry
            AttackHitbox otherHitbox = other.GetComponent<AttackHitbox>();
            if (otherHitbox != null && otherHitbox.isPlayerHitbox != isPlayerHitbox)
            {
                // Parry successful!
                HandleParry();
                otherHitbox.HandleParry();
                return;
            }
        }

        // Normal attack logic
        if (other.CompareTag(isPlayerHitbox ? "Enemy" : "Player"))
        {
            //PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            //if (playerHealth != null && !isParried)
            //{
            //    playerHealth.TakeDamage(damageAmount);
            //}
        }
    }

    public void HandleParry()
    {
        isParried = true;
        onParried?.Invoke();
        Debug.Log(gameObject.name + " was parried!");
    }

    private void OnDisable()
    {
        isParried = false;
    }

    public void SetDamage(int amount)
    {
        damageAmount = amount;
    }
}