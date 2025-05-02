using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float minDistanceToPlayer = 2f;
    
    [Header("Vision Settings")]
    [SerializeField] private float visionRange = 5f;
    [SerializeField] private float visionAngle = 90f;
    [SerializeField] private float behindAwarenessRange = 3f; // Range to detect player behind
    [SerializeField] private float turnAroundDelay = 0.5f; // Delay before turning around
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private bool isPlayerDetected;
    private bool isPlayerBehind;
    private Rigidbody2D rb;
    private float turnAroundTimer;
    private bool isTurningAround;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        turnAroundTimer = 0f;
    }

    private void Update()
    {
        if (isTurningAround)
        {
            HandleTurningAround();
            return;
        }

        CheckPlayerPosition();

        if (isPlayerDetected)
        {
            MoveTowardsPlayer();
        }
        else
        {
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

        // If player is behind and within awareness range
        if (isCurrentlyBehind && distanceToPlayer <= behindAwarenessRange)
        {
            // Check for obstacles between enemy and player
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

        // Normal vision check
        isPlayerDetected = CanSeePlayer();
    }

    private bool IsPlayerBehind(Vector2 directionToPlayer)
    {
        // For a side-scroller, check if the player is behind based on the enemy's facing direction
        float dotProduct = Vector2.Dot(
            spriteRenderer.flipX ? Vector2.left : Vector2.right, 
            directionToPlayer.normalized
        );
        return dotProduct < -0.5f; // Behind if dot product is negative (more than 90 degrees)
    }

    private void InitiateTurnAround()
    {
        if (!isTurningAround)
        {
            isTurningAround = true;
            turnAroundTimer = turnAroundDelay;
            rb.linearVelocity = Vector2.zero;
            // Optional: Play turn around animation here
        }
    }

    private void HandleTurningAround()
    {
        turnAroundTimer -= Time.deltaTime;
        
        if (turnAroundTimer <= 0)
        {
            // Complete the turn
            spriteRenderer.flipX = !spriteRenderer.flipX;
            isTurningAround = false;
            // Check for player after turning
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

        float angle = Vector2.Angle(spriteRenderer.flipX ? Vector2.left : Vector2.right, directionToPlayer);
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

        if (distanceToPlayer > minDistanceToPlayer)
        {
            Vector2 movement = directionToPlayer.normalized * moveSpeed;
            rb.linearVelocity = new Vector2(movement.x, rb.linearVelocityY);

            // Flip sprite based on movement direction
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

    private void OnDrawGizmosSelected()
    {
        // Vision cone
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Vector3 rightDirection = Quaternion.Euler(0, 0, visionAngle / 2) * Vector3.right;
        Vector3 leftDirection = Quaternion.Euler(0, 0, -visionAngle / 2) * Vector3.right;
        
        Gizmos.DrawLine(transform.position, transform.position + rightDirection * visionRange);
        Gizmos.DrawLine(transform.position, transform.position + leftDirection * visionRange);

        // Behind awareness range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, behindAwarenessRange);
    }
}