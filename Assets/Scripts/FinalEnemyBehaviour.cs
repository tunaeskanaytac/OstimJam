using System;
using UnityEngine;

public class FinalEnemyBehaviour : MonoBehaviour
{
    [Header("Enemy Attributes")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float agroRange;
    // Cooldown related fields. Being used in Update() method.
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
    [SerializeField] private GameObject attackObject;
    [SerializeField] private Animator anim;

    [Header("Enemy Search")]
    // Timer and duration for continuing movement after player leaves agro range
    protected private float moveTimer = 0f;
    protected private float moveDuration = 1.6f;

    /// <summary>
    /// Initialization method for enemies.
    /// </summary>
    protected virtual void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        canAttack = true;
    }

    /// <summary>
    /// Update method handling common behavior for enemies.
    /// </summary>
    protected virtual void Update()
    {
        UpdateAnimations();
        Move();
        TurnDirection();
        canSee = CanSeePlayer();

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

        // if is agro and attack ready and target is within attack range, attack the player
        if (isAgro && canAttack && Vector2.Distance(transform.position, target.position) < attackRange)
        {
            Debug.Log("I must hit");
            anim.SetTrigger("EnemyAttack");
            rigidBody.linearVelocity = Vector2.zero;
            // attackObject.SetActive(true);
            Attack();
            canAttack = false;
        }
    }

    /// <summary>
    /// Handles the movement behavior of the enemy.
    /// </summary>
    protected virtual void Move()
    {
        Vector2 direction = (target.position - transform.position).normalized;

        // Check if the player is within the agro range and if enemy can see the player.
        if (Vector2.Distance(transform.position, target.position) < agroRange)
        {
            isSearching = false;
            isAgro = true;

            if (isAgro && canSee)
            {
                transform.Translate(direction * moveSpeed * Time.deltaTime);

                spriteRenderer.flipX = direction.x < 0;

                moveTimer = moveDuration;
            }
        }
        else
        {
            isAgro = false;

            // Continue moving if the timer hasn't expired yet
            if (moveTimer > 0f)
            {
                isSearching = true;
                var moveDirection = target.position.x > transform.position.x ? 1 : -1;
                rigidBody.linearVelocity = new Vector2(moveDirection * moveSpeed, rigidBody.linearVelocity.y);
                moveTimer -= Time.deltaTime; // Decrease the timer
            }
            else
            {
                isSearching = false;
                rigidBody.linearVelocity = Vector2.zero; // Stop moving if the timer has expired
            }
        }
    }

    private void UpdateAnimations()
    {
        anim.SetFloat("XVelocity", rigidBody.linearVelocity.x);
    }

    /// <summary>
    /// Turn the enemy sprite towards the player's direction.
    /// </summary>
    protected virtual void TurnDirection()
    {
        if (transform.position.x > target.position.x)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }

    /// <summary>
    /// Logic for enemy attacks.
    /// </summary>
    protected virtual void Attack()
    {
        // Calculate direction towards the player
        Vector2 directionToPlayer = target.position - transform.position;

        // Perform raycast towards the player to check if it's within attack range
        RaycastHit2D hit = Physics2D.Raycast(transform.position, target.position - transform.position, attackRange, LayerMask.GetMask("Player"));

        // Draw debug line to visualize the raycast
        Debug.DrawRay(transform.position, directionToPlayer.normalized * attackRange, Color.red);
        PlayerController player = hit.collider.GetComponent<PlayerController>();
        if (player != null && player._canGetHurt && !player.isParrying)
        {
            player.Die(); // Or trigger damage logic
        }
        else if (player != null && player._canGetHurt && player.isParrying)
        {
            OnParried(player.transform.position, 10f);
        }
        canAttack = false; // Reset the attack availability
    }

    /// <summary>
    /// Destroy enemy object on Death.
    /// </summary>
    public void Death()
    {
        anim.SetTrigger("Death");
        Debug.Log("Enemy has died!");
        Destroy(gameObject); // Destroy the enemy.
    }

    /// <summary>
    /// Checks if the enemy can visually perceive the player within the agro range and line of sight.
    /// </summary>
    /// <returns>
    /// True if the player is within agro range and has a clear line of sight; otherwise, false.
    /// </returns>
    protected virtual bool CanSeePlayer()
    {
        // Get the positions and direction between the enemy and the player
        Vector2 enemyPos = transform.position;
        Vector2 playerPos = target.position;
        //Vector2 directionToPlayer = playerPos - enemyPos;

        // Check if the enemy is facing left or right based on the direction to the player
        //bool facingLeft = directionToPlayer.x < 0;

        // Determine linecast direction based on the enemy's facing direction
        //Vector2 linecastDirection = facingLeft ? -directionToPlayer.normalized : directionToPlayer.normalized;
        Vector2 linecastOrigin = enemyPos;

        // Perform a linecast from the enemy towards the player
        RaycastHit2D hit = Physics2D.Linecast(linecastOrigin, playerPos, LayerMask.GetMask("Obstacles", "Ground"));

        // Set the end position of the line to the player's position initially
        Vector2 endLinePos = playerPos;

        // If an obstacle is hit, update the end position to the obstacle hit point
        if (hit.collider != null)
        {
            endLinePos = hit.point;
            Debug.DrawLine(linecastOrigin, endLinePos, Color.yellow); // Visualize the hit point in red
            return false; // Obstacle detected between enemy and player
        }

        // Draw a line in yellow from the enemy to the player or obstacle hit point
        Debug.DrawLine(linecastOrigin, endLinePos, Color.blue);

        // Return true if the player is within aggro range and has a clear line of sight
        return true;
    }

    public void OnParried(Vector2 parrySourcePosition, float knockbackForce)
    {
        Vector2 knockbackDirection = (transform.position - (Vector3)parrySourcePosition).normalized;
        rigidBody.linearVelocity = Vector2.zero; // Stop current motion
        rigidBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
    }
}
