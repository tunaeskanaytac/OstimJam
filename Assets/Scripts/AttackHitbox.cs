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
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.Die();
            }
        }
        // Normal attack logic
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player._canGetHurt)
            {
                player.Die();
            }
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