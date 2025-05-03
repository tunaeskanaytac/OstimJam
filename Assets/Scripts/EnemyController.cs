using UnityEngine;
using UnityEngine.Events; // For the attack event

public class EnemyController : MonoBehaviour
{
    // Add these fields to the existing ones
    [Header("Parry Settings")]
    [SerializeField] private float parryStunDuration = 1.5f;
    private bool isStunned = false;
    private float stunTimer = 0f;
    
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private bool canAttack = true;
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private float attackDuration = 0.2f; // How long the hitbox stays active
    
    // Event that can be used to trigger attack animations or effects
    public UnityEvent onAttackPerformed;

    private float attackTimer;
    private bool isWithinAttackRange;
    private bool isAttacking;
    private float currentAttackDuration;


    [Header("Behavior Type")]
    [SerializeField] private EnemyBehavior defaultBehavior = EnemyBehavior.Idle;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float minDistanceToPlayer = 2f;
    [SerializeField] private float patrolSpeed = 2f;
    
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waitTimeAtPoint = 1f;
    [SerializeField] private float patrolPointThreshold = 0.1f;
    
    [Header("Vision Settings")]
    [SerializeField] private float visionRange = 5f;
    [SerializeField] private float visionAngle = 90f;
    [SerializeField] private float behindAwarenessRange = 3f;
    [SerializeField] private float turnAroundDelay = 0.5f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private bool isPlayerDetected;
    private Rigidbody2D rb;
    private float turnAroundTimer;
    private bool isTurningAround;
    
    // Patrol variables
    private int currentPatrolIndex;
    private float waitTimer;
    private bool isWaitingAtPoint;

    public enum EnemyBehavior
    {
        Idle,
        Patrol
    }

    private EnemyBehavior currentBehavior;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        turnAroundTimer = 0f;
        currentBehavior = defaultBehavior;
        
        // Validate patrol points
        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning("No patrol points assigned to " + gameObject.name);
            currentBehavior = EnemyBehavior.Idle;
        }
        attackTimer = 0f;
        
        // Initialize the event if it's null
        if (onAttackPerformed == null)
            onAttackPerformed = new UnityEvent();
        
        // Add this to existing Start method
        if (attackHitbox != null)
        {
            AttackHitbox hitboxComponent = attackHitbox.GetComponent<AttackHitbox>();
            if (hitboxComponent != null)
            {
                hitboxComponent.onParried.AddListener(OnParried);
            }
        }
        
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    private void Update()
    {
        // Add this at the start of the existing Update method
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
            }
            return; // Skip normal behavior while stunned
        }
        
        // Update attack timer
        if (!canAttack)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                canAttack = true;
            }
        }
        
        // Update attack duration
        if (isAttacking)
        {
            currentAttackDuration -= Time.deltaTime;
            if (currentAttackDuration <= 0)
            {
                EndAttack();
            }
        }

        if (isTurningAround)
        {
            HandleTurningAround();
            return;
        }

        CheckPlayerPosition();

        if (isPlayerDetected)
        {
            // Check if we can attack before moving
            if (isWithinAttackRange && canAttack)
            {
                Attack();
            }
            else
            {
                MoveTowardsPlayer();
            }
        }
        else
        {
            // Execute default behavior when player is not detected
            switch (currentBehavior)
            {
                case EnemyBehavior.Idle:
                    HandleIdle();
                    break;
                case EnemyBehavior.Patrol:
                    HandlePatrol();
                    break;
            }
        }
    }

    private void HandleIdle()
    {
        rb.linearVelocity = Vector2.zero;
    }

    private void HandlePatrol()
    {
        if (patrolPoints.Length == 0) return;

        if (isWaitingAtPoint)
        {
            rb.linearVelocity = Vector2.zero;
            waitTimer -= Time.deltaTime;
            
            if (waitTimer <= 0)
            {
                isWaitingAtPoint = false;
                // Move to next patrol point
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
            return;
        }

        Vector2 targetPosition = patrolPoints[currentPatrolIndex].position;
        Vector2 directionToTarget = (targetPosition - (Vector2)transform.position).normalized;
        
        // Move towards patrol point
        rb.linearVelocity = new Vector2((directionToTarget * patrolSpeed).x, rb.linearVelocity.y);

        // Update sprite direction
        if (rb.linearVelocity.x != 0)
        {
            spriteRenderer.flipX = rb.linearVelocity.x < 0;
        }

        // Check if we reached the patrol point
        if (Vector2.Distance(transform.position, targetPosition) < patrolPointThreshold)
        {
            isWaitingAtPoint = true;
            waitTimer = waitTimeAtPoint;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void CheckPlayerPosition()
    {
        if (player == null) return;

        Vector2 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Check if player is behind
        bool isCurrentlyBehind = IsPlayerBehind(directionToPlayer);

        if (isCurrentlyBehind && distanceToPlayer <= behindAwarenessRange)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                directionToPlayer.normalized,
                distanceToPlayer,
                obstacleLayer | playerLayer
            );

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                InitiateTurnAround();
                return;
            }
        }

        isPlayerDetected = CanSeePlayer();
    }

    private bool IsPlayerBehind(Vector2 directionToPlayer)
    {
        float dotProduct = Vector2.Dot(
            spriteRenderer.flipX ? Vector2.left : Vector2.right, 
            directionToPlayer.normalized
        );
        return dotProduct < -0.5f;
    }

    private void InitiateTurnAround()
    {
        if (!isTurningAround)
        {
            isTurningAround = true;
            turnAroundTimer = turnAroundDelay;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void HandleTurningAround()
    {
        turnAroundTimer -= Time.deltaTime;
        
        if (turnAroundTimer <= 0)
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
            isTurningAround = false;
            isPlayerDetected = CanSeePlayer();
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector2 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > visionRange)
            return false;

        // Get the forward direction based on sprite orientation
        Vector2 forwardDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        float angle = Vector2.Angle(forwardDirection, directionToPlayer);
        
        if (angle > visionAngle / 2)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            directionToPlayer.normalized,
            distanceToPlayer,
            obstacleLayer | playerLayer
        );

        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    private void MoveTowardsPlayer()
    {
        if (player == null) return;

        Vector2 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Update attack range status
        isWithinAttackRange = distanceToPlayer <= attackRange;

        // Only move if we're outside attack range
        if (distanceToPlayer > attackRange)
        {
            Vector2 movement = directionToPlayer.normalized * moveSpeed;
            rb.linearVelocity = new Vector2(movement.x, rb.linearVelocityY);

            if (movement.x != 0)
            {
                spriteRenderer.flipX = movement.x < 0;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Attack()
    {
        // Reset attack availability
        canAttack = false;
        attackTimer = attackCooldown;

        // Trigger the attack event
        onAttackPerformed?.Invoke();

        // Empty attack method to be implemented based on your game's needs
        PerformAttack();
    }

    protected virtual void PerformAttack()
    {
        Debug.Log("Attacking!");
        if (attackHitbox == null)
        {
            Debug.LogWarning("Attack hitbox not assigned to " + gameObject.name);
            return;
        }

        // Start the attack
        isAttacking = true;
        currentAttackDuration = attackDuration;
        
        // Enable the hitbox
        attackHitbox.SetActive(true);

        // Make sure hitbox is on the correct side based on enemy's facing direction
        Vector3 hitboxLocalPos = attackHitbox.transform.localPosition;
        hitboxLocalPos.x = Mathf.Abs(hitboxLocalPos.x) * (spriteRenderer.flipX ? -1 : 1);
        attackHitbox.transform.localPosition = hitboxLocalPos;
    }
    
    private void EndAttack()
    {
        isAttacking = false;
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }

    private void OnParried()
    {
        isStunned = true;
        stunTimer = parryStunDuration;
        EndAttack(); // End the current attack
        
        // Optional: Add visual feedback
        // You might want to play an animation, particle effect, or sound here
        Debug.Log("Enemy was parried!");
    }


    // Public method to change behavior
    public void SetBehavior(EnemyBehavior newBehavior)
    {
        currentBehavior = newBehavior;
        
        // Reset patrol state if switching to patrol
        if (newBehavior == EnemyBehavior.Patrol)
        {
            currentPatrolIndex = 0;
            isWaitingAtPoint = false;
            waitTimer = 0f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Vision cone
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Get the base direction based on sprite orientation
        Vector2 baseDirection;
        if (spriteRenderer == null)
        {
            baseDirection = Vector2.right;
        }
        else
        {
            baseDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }

        // Calculate the vision cone angles
        Vector3 rightDirection = Quaternion.Euler(0, 0, visionAngle / 2) * baseDirection;
        Vector3 leftDirection = Quaternion.Euler(0, 0, -visionAngle / 2) * baseDirection;
        
        Gizmos.DrawLine(transform.position, transform.position + rightDirection * visionRange);
        Gizmos.DrawLine(transform.position, transform.position + leftDirection * visionRange);

        // Behind awareness range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, behindAwarenessRange);

        // Draw patrol path
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(patrolPoints[i].position, 0.3f);
                    if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                    if (i == patrolPoints.Length - 1 && patrolPoints[0] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
                    }
                }
            }
        }

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Visualize attack hitbox position if assigned
        if (attackHitbox != null)
        {
            Gizmos.color = Color.magenta;
            // This will show where the hitbox is/will be
            Gizmos.DrawWireCube(attackHitbox.transform.position, 
                attackHitbox.GetComponent<Collider2D>()?.bounds.size ?? Vector3.one);
        }

    }

    public void Die()
    {
        // Ölüm animasyonu, yok etme vs.
        Destroy(gameObject);
    }
}