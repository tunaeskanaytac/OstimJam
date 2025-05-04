using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private int _facingDirection = 1;
    private bool _isFacingRight = true;
    private bool _dying = false;
    private bool _isKnockedBack = false;

    [Header("Components")]
    private Transform target;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody;
    //[SerializeField] private GameObject attackObject;
    [SerializeField] private Animator anim;

    [Header("Enemy Search")]
    private float moveTimer = 0f;
    private float moveDuration = 1.6f;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        canAttack = true;
    }

    private void Update()
    {
        if (_isKnockedBack || _dying)
            return;

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
        if (isAgro && canSee && canAttack && distanceToPlayer < attackRange && !_dying)
        {
            Debug.Log("I must hit");
            
            anim.SetTrigger("EnemyAttack");
            rigidBody.linearVelocity = Vector2.zero;
            Attack();
            canAttack = false;
        }
        else if (isAgro && canSee && distanceToPlayer >= attackRange)
        {
            if (!_dying && !_isKnockedBack)
            {
                Move();
            }
        }
        else
        {
            if (!_isKnockedBack)
            {
                PatrolOrIdle();
            }
            else
            {
                rigidBody.linearVelocity = Vector2.zero; // Stop any movement during knockback
            }
        }

        TurnDirection();
    }

    private void TurnDirection()
    {
        float directionToTarget = target.position.x - transform.position.x;

        if (_isFacingRight && directionToTarget < 0)
        {
            Flip();
        }
        else if (!_isFacingRight && directionToTarget > 0)
        {
            Flip();
        }
    }

    private void Flip()
    {
        _facingDirection *= -1;
        _isFacingRight = !_isFacingRight;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (_isFacingRight ? 1 : -1);
        transform.localScale = scale;
    }

    private void Move()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        rigidBody.linearVelocity = new Vector2(direction.x * moveSpeed, rigidBody.linearVelocity.y);
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

    /// <summary>
    /// Logic for enemy attacks.
    /// </summary>
    private void Attack()
    {
        if (target == null) return;
    
        rigidBody.linearVelocity = Vector2.zero;

        // Delay the hit logic to match animation timing
        StartCoroutine(DelayedAttack(0.4f)); // 0.4 seconds delay — adjust to match your animation
        canAttack = false;
    }

    private IEnumerator DelayedAttack(float delay)
    {
        anim.SetTrigger("EnemyAttack");
        yield return new WaitForSeconds(delay);
        
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (target != null && distanceToPlayer <= attackRange)
        {
            Vector2 directionToPlayer = (target.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, attackRange, LayerMask.GetMask("Player"));

            Debug.DrawRay(transform.position, directionToPlayer * attackRange, Color.red);

            if (hit.collider != null)
            {
                PlayerController player = hit.collider.GetComponent<PlayerController>();
                if (player != null && !player._isDying)
                {
                    if (player._canGetHurt && !player.isParrying)
                    {
                        player.Die();
                    }
                    else if (player.isParrying)
                    {
                        OnParried(player.transform.position, 4f);
                    }
                }
            }
        }
    }

    public IEnumerator Death()
    {
        _dying = true;
        anim.SetTrigger("Death");
        Timer.instance.AddTime(3f);
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    private bool CanSeePlayer()
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
        if (!_isKnockedBack)
        {
            StartCoroutine(HandleKnockback(parrySourcePosition, knockbackForce, 0.5f));
        }
    }

    private IEnumerator HandleKnockback(Vector2 source, float force, float duration)
    {
        _isKnockedBack = true;

        Vector2 knockbackDir = (transform.position - (Vector3)source).normalized;
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.AddForce(knockbackDir * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(duration);

        _isKnockedBack = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sword"))
        {
            StartCoroutine(Death());
        }
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
