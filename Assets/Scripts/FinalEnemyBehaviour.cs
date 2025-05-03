using System;
using UnityEngine;

public class FinalEnemyBehaviour : MonoBehaviour
{
    [Header("Enemy Attributes")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float agroRange;
    private float timeSinceLastAttack = 0.0f;
    [SerializeField] private float attackCooldown = 2.0f;

    [Header("Enemy State")]
    [SerializeField] private bool isAgro;
    [SerializeField] private bool isSearching;
    [SerializeField] private bool isPatrolling;
    [SerializeField] private bool canSee;
    [SerializeField] private bool canAttack;

    [Header("Components")]
    private Transform target;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody;
    //[SerializeField] private GameObject attackObject;
    [SerializeField] private Animator anim;

    [Header("Enemy Search")]
    protected private float moveTimer = 0f;
    protected private float moveDuration = 1.6f;

    protected virtual void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        canAttack = true;
    }

    protected virtual void Update()
    {
        UpdateAnimations();
        canSee = CanSeePlayer();

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (distanceToPlayer < agroRange)
        {
            isAgro = true;
            isSearching = false;
        }
        else
        {
            isAgro = false;
        }

        // Cooldown logic
        if (!canAttack)
        {
            timeSinceLastAttack += Time.deltaTime;
            if (timeSinceLastAttack >= attackCooldown)
            {
                canAttack = true;
                timeSinceLastAttack = 0.0f;
            }
        }

        // Attack condition
        if (isAgro && canSee && canAttack && distanceToPlayer < attackRange)
        {
            Debug.Log("I must hit");
            anim.SetTrigger("EnemyAttack");
            rigidBody.linearVelocity = Vector2.zero;
            Attack();
            canAttack = false;
        }
        else if (isAgro && canSee && distanceToPlayer >= attackRange)
        {
            Move();
        }
        else
        {
            PatrolOrIdle();
        }

        TurnDirection();
    }

    protected virtual void Move()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        rigidBody.linearVelocity = new Vector2(direction.x * moveSpeed, rigidBody.linearVelocity.y);
        spriteRenderer.flipX = direction.x < 0;
    }

    private void PatrolOrIdle()
    {
        if (moveTimer > 0f)
        {
            isSearching = true;
            var moveDirection = target.position.x > transform.position.x ? 1 : -1;
            rigidBody.linearVelocity = new Vector2(moveDirection * moveSpeed, rigidBody.linearVelocity.y);
            moveTimer -= Time.deltaTime;
        }
        else
        {
            isSearching = false;
            rigidBody.linearVelocity = Vector2.zero;
        }
    }

    private void UpdateAnimations()
    {
        anim.SetFloat("XVelocity", Mathf.Abs(rigidBody.linearVelocity.x));
    }

    protected virtual void TurnDirection()
    {
        spriteRenderer.flipX = transform.position.x > target.position.x;
    }

    /// <summary>
    /// Logic for enemy attacks.
    /// </summary>
    protected virtual void Attack()
    {
        if (target == null) return;

        Vector2 directionToPlayer = (target.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, attackRange, LayerMask.GetMask("Player"));

        Debug.DrawRay(transform.position, directionToPlayer * attackRange, Color.red);

        if (hit.collider != null)
        {
            PlayerController player = hit.collider.GetComponent<PlayerController>();
            if (player != null)
            {
                if (player._canGetHurt && !player.isParrying)
                {
                    player.Die();
                }
                else if (player._canGetHurt && player.isParrying)
                {
                    OnParried(player.transform.position, 10f);
                }
            }
        }
    }

    public void Death()
    {
        anim.SetTrigger("Death");
        Debug.Log("Enemy has died!");
        Destroy(gameObject);
    }

    protected virtual bool CanSeePlayer()
    {
        Vector2 enemyPos = transform.position;
        Vector2 playerPos = target.position;
        RaycastHit2D hit = Physics2D.Linecast(enemyPos, playerPos, LayerMask.GetMask("Obstacles", "Ground"));

        if (hit.collider != null)
        {
            Debug.DrawLine(enemyPos, hit.point, Color.yellow);
            return false;
        }

        Debug.DrawLine(enemyPos, playerPos, Color.blue);
        return true;
    }

    public void OnParried(Vector2 parrySourcePosition, float knockbackForce)
    {
        Vector2 knockbackDirection = (transform.position - (Vector3)parrySourcePosition).normalized;
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, agroRange);

        if (Application.isPlaying && target != null && canSee)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}
