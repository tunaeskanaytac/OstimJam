using UnityEngine;
using System.Collections;

namespace Enemy
{
    public class EnemyController : MonoBehaviour
    {
        private static readonly int IsAttacking = Animator.StringToHash("isAttacking");

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float detectionRange = 5f;
        [SerializeField] private float stopDistance = 1f;
        
        [Header("Vision Settings")]
        [SerializeField] private float fieldOfViewAngle = 90f;
        [SerializeField] private float closeDetectionRadius = 2f;
        [SerializeField] private bool debugVision = true;
        
        [Header("Attack Settings")]
        [SerializeField] private GameObject attackObject;
        [SerializeField] private float attackDuration = 0.5f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float attackRange = 1.5f;
        
        [Header("References")]
        private Rigidbody2D rb;
        private Transform playerTransform;
        private Animator animator;
        
        [Header("State")]
        private bool isPlayerInRange;
        private bool isPlayerDetected;
        private Vector2 movementDirection;
        private bool isFacingRight = true;
        private bool canAttack = true;
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            FindPlayer();
            
            // Ensure attack object starts disabled
            if (attackObject != null)
            {
                attackObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (playerTransform == null) return;
            
            CheckPlayerDetection();
            CalculateMovementDirection();
            CheckAttack();
        }

        private void CheckAttack()
        {
            if (!isPlayerDetected || !canAttack) return;

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange)
            {
                StartCoroutine(PerformAttack());
            }
        }

        private IEnumerator PerformAttack()
        {
            canAttack = false;
            animator.SetBool(IsAttacking, true);

            // Activate attack object
            if (attackObject != null)
            {
                attackObject.SetActive(true);
                
                // Wait for attack duration
                yield return new WaitForSeconds(attackDuration);
                
                // Deactivate attack object
                attackObject.SetActive(false);
                
                // Wait for cooldown
                yield return new WaitForSeconds(attackCooldown - attackDuration);
            }
            else
            {
                Debug.LogWarning("Attack object is not assigned!");
                yield return new WaitForSeconds(attackCooldown);
            }
            
            animator.SetBool(IsAttacking, false);
            canAttack = true;
        }

        private void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("Player not found! Make sure the player has the 'Player' tag.");
            }
        }

        private void FixedUpdate()
        {
            if (isPlayerInRange && isPlayerDetected)
            {
                Move();
            }
            else
            {
                StopMoving();
            }
        }

        private void CheckPlayerDetection()
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            isPlayerInRange = distanceToPlayer <= detectionRange && distanceToPlayer > stopDistance;
            
            if (isPlayerInRange)
            {
                // Always detect if player is within close detection radius
                if (distanceToPlayer <= closeDetectionRadius)
                {
                    isPlayerDetected = true;
                }
                else
                {
                    // Use field of view detection for distances beyond closeDetectionRadius
                    Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
                    Vector2 facingDirection = isFacingRight ? Vector2.right : Vector2.left;
                    float angle = Vector2.Angle(facingDirection, directionToPlayer);
                    isPlayerDetected = angle <= fieldOfViewAngle * 0.5f;
                }
            }
            else
            {
                isPlayerDetected = false;
            }
        }

        private void CalculateMovementDirection()
        {
            if (isPlayerInRange && isPlayerDetected)
            {
                movementDirection = (playerTransform.position - transform.position).normalized;
                FlipSprite();
            }
            else
            {
                movementDirection = Vector2.zero;
            }
        }

        private void Move()
        {
            rb.linearVelocity = new Vector2(movementDirection.x * moveSpeed, rb.linearVelocity.y);
        }

        private void StopMoving()
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        private void FlipSprite()
        {
            if (movementDirection.x != 0)
            {
                bool shouldFaceRight = movementDirection.x > 0;
                if (isFacingRight != shouldFaceRight)
                {
                    isFacingRight = shouldFaceRight;
                    transform.localScale = new Vector3(
                        Mathf.Sign(movementDirection.x),
                        transform.localScale.y,
                        transform.localScale.z
                    );
                }
            }
        }

        public void Die()
        {
            Destroy(gameObject);
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!debugVision) return;
            
            // Draw main detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Draw close detection radius (360-degree detection)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, closeDetectionRadius);
            
            // Draw stop distance
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stopDistance);
            
            // Draw vision cone
            Gizmos.color = Color.blue;
            Vector3 facingDirection = isFacingRight ? Vector3.right : Vector3.left;
            Vector3 rightBoundary = Quaternion.Euler(0, 0, fieldOfViewAngle * 0.5f) * facingDirection;
            Vector3 leftBoundary = Quaternion.Euler(0, 0, -fieldOfViewAngle * 0.5f) * facingDirection;
            
            // Only draw vision cone from close detection radius to detection range
            Vector3 closeRightStart = transform.position + rightBoundary * closeDetectionRadius;
            Vector3 closeLeftStart = transform.position + leftBoundary * closeDetectionRadius;
            Vector3 farRightEnd = transform.position + rightBoundary * detectionRange;
            Vector3 farLeftEnd = transform.position + leftBoundary * detectionRange;
            
            Gizmos.DrawLine(closeRightStart, farRightEnd);
            Gizmos.DrawLine(closeLeftStart, farLeftEnd);
            
            // Draw arc for outer detection range
            int segments = 20;
            Vector3 previousPoint = farRightEnd;
            for (int i = 1; i <= segments; i++)
            {
                float angle = fieldOfViewAngle * ((float)i / segments - 0.5f);
                Vector3 currentPoint = transform.position + 
                    (Quaternion.Euler(0, 0, angle) * facingDirection) * detectionRange;
                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
            
            // Previous Gizmos drawing code remains unchanged
            
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}