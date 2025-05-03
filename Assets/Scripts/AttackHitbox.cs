using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private int damageAmount = 10;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Hitbox triggered");
        if (other.CompareTag("Player"))
        {
            Debug.Log("Sword on player");
            // PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            // if (playerHealth != null)
            // {
            //     playerHealth.TakeDamage(damageAmount);
            // }
        }
    }

    public void SetDamage(int amount)
    {
        damageAmount = amount;
    }
}