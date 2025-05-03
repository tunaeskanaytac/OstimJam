using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Add this to your PlayerController class
public class PlayerController : MonoBehaviour
{
    // Add these new fields with the existing attack-related fields
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.5f; // Time between attacks
    private bool canAttack = true;
    private float cooldownTimer = 0f;
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private float attackDuration = 0.2f;
    private bool isAttacking = false;
    private float attackTimer = 5f;
    #region References
    private Rigidbody2D _rb;
    private BoxCollider2D _collider;
    private Animator _anim;
    private Vector2 _startPos;
    #endregion

    #region Serialized Fields
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private GroundCheck groundCheck;
    [SerializeField] private LedgeCheck isOnLedgeFace;
    [SerializeField] private LedgeCheck isOnLedgeLegs;
    #endregion

    #region State Flags
    private bool _canMove = true;
    private bool _isFacingRight = true;
    private bool _isRunning;
    public bool _canGetHurt = true;
    #endregion

    #region Movement Variables
    private float _movementInputDirection;
    private float coyoteTimeCounter;
    private int _facingDirection = 1;
    #endregion

    #region Roll
    private bool _canRoll = true;
    private bool _isRolling = false;
    private float _rollTime = 0.5f;
    private float _rollSpeed = 10f;
    private int _extraJump = 1;
    #endregion

    [Header("Ledge Info")]
    [SerializeField] private Vector2 offset1;
    [SerializeField] private Vector2 offset2;

    private Vector2 _climbBegunPosition;
    private Vector2 _climbOverPosition;
    private bool _canGrabLedge = true;
    private bool _canClimb = true;

    private void Start()
    {
        if (attackHitbox != null)
        {
            // Make sure the hitbox is marked as a player hitbox
            AttackHitbox hitboxComponent = attackHitbox.GetComponent<AttackHitbox>();
            if (hitboxComponent != null)
            {
                hitboxComponent.isPlayerHitbox = true;
            }
            attackHitbox.SetActive(false);
        }
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _collider = GetComponent<BoxCollider2D>();

        _startPos = transform.position;
    }

    private void Update()
    {
        // Add this to your existing Update method
        if (!canAttack)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                canAttack = true;
            }
        }

        // Update attack timer
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                EndAttack();
            }
        }

        CheckForLedge();
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        HandleCoyoteTime();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    private void CheckInput()
    {
        if (_isRolling)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            PerformAttack();
        }
        _movementInputDirection = Input.GetAxisRaw("Horizontal");
        // CHECK ROLL
        if (Input.GetKeyDown(KeyCode.LeftShift) && _canRoll && !_isRolling && groundCheck.IsGrounded)
        {
            StartCoroutine(Roll());
        }

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && groundCheck.IsGrounded)
        {
            Jump();
            _extraJump = 1;
        }
        else if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && _extraJump > 0 && !groundCheck.IsGrounded)
        {
            Jump();
            _extraJump = 0;
        }
    }

    private void CheckMovementDirection()
    {
        if (_isFacingRight && _movementInputDirection < 0 && _canMove)
        {
            Flip();
        }
        else if (!_isFacingRight && _movementInputDirection > 0 && _canMove)
        {
            Flip();
        }

        _isRunning = Mathf.Abs(_movementInputDirection) > 0;
    }

    private void ApplyMovement()
    {
        if (_isRolling)
        {
            _rb.linearVelocity = new Vector2(_rollSpeed * _facingDirection, _rb.linearVelocity.y);
            return;
        }
        else if (_canMove)
        {
            _rb.linearVelocity = new Vector2(movementSpeed * _movementInputDirection, _rb.linearVelocity.y);
        }
        else if (!_canMove)
        {
            return;
        }
    }

    private void Flip()
    {
        _facingDirection *= -1;
        _isFacingRight = !_isFacingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }

    private void Jump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
    }

    private IEnumerator Roll()
    {
        _anim.SetTrigger("Roll");
        _canRoll = false;
        _isRolling = true;
        _canGetHurt = true;

        Vector2 originalSize = _collider.size;
        Vector2 originalOffset = _collider.offset;

        _collider.size = new Vector2(originalSize.x, originalSize.y / 2);
        //_collider.offset = new Vector2(originalOffset.x, originalOffset.y / 2);

        _rb.linearVelocity = new Vector2(_rollSpeed * _facingDirection, _rb.linearVelocity.y);
        yield return new WaitForSeconds(_rollTime);
        _collider.size = originalSize;
        _collider.offset = originalOffset;

        _canGetHurt = false;
        _isRolling = false;
        _canRoll = true;
    }

    private bool IsGrounded()
    {
        float extraHeight = 0.2f;
        RaycastHit2D raycastHit = Physics2D.BoxCast(
            _collider.bounds.center, new Vector3(_collider.bounds.size.x / 1.1f, _collider.bounds.size.y, _collider.bounds.size.z), 0, Vector2.down, extraHeight, groundLayer);

        return raycastHit.collider != null;
    }

    private void HandleCoyoteTime()
    {
        if (groundCheck.IsGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    public void Die()
    {
        StartCoroutine(Respawn(1f));
    }

    private IEnumerator Respawn(float duration)
    {
        _rb.linearVelocity = new Vector2(0, 0);
        _rb.simulated = false;
        transform.localScale = new Vector3(0, 0, 0);
        yield return new WaitForSeconds(duration);
        transform.position = _startPos;
        transform.localScale = new Vector3(1f, 1f, 1f);
        _rb.simulated = true;
    }

    private void UpdateAnimations()
    {
        _anim.SetBool("_isRunning", _isRunning);
        _anim.SetBool("isGrounded", groundCheck.IsGrounded);
        _anim.SetFloat("YVelocity", _rb.linearVelocityY);
    }

    private void CheckForLedge()
    {
        if (!isOnLedgeFace.IsOnLedge && isOnLedgeLegs.IsOnLedge && _canGrabLedge)
        {
            _canGrabLedge = false;

            // Determine climb positions based on current facing direction
            Vector2 ledgePosition = isOnLedgeFace.transform.position;

            if (_isFacingRight)
            {
                _climbBegunPosition = ledgePosition + offset1;
                _climbOverPosition = ledgePosition + offset2;
            }
            else
            {
                Vector2 flippedOffset1 = new Vector2(-offset1.x, offset1.y);
                Vector2 flippedOffset2 = new Vector2(-offset2.x, offset2.y);
                _climbBegunPosition = ledgePosition + flippedOffset1;
                _climbOverPosition = ledgePosition + flippedOffset2;
            }
            _rb.linearVelocity = new Vector2(0, 0);
            _rb.simulated = false;
            transform.localScale = new Vector3(0, 0, 0);
            StartCoroutine(ClimbLedge());
        }
    }

    private IEnumerator ClimbLedge()
    {
        _canMove = false;
        _rb.linearVelocity = Vector2.zero;
        _anim.SetTrigger("ClimbLedge");

        // Snap player to the climb begin position
        transform.position = _climbBegunPosition;

        yield return new WaitForSeconds(0.5f); // Adjust to match your climb animation

        transform.position = _climbOverPosition;

        yield return new WaitForSeconds(0.2f); // Wait a bit after animation
        transform.localScale = new Vector3(1f, 1f, 1f);
        _rb.simulated = true;
        _canMove = true;
        _canGrabLedge = true;
    }

    private void OnDrawGizmos()
    {
        if (_collider == null) return;

        float extraHeight = 0.2f;
        Gizmos.color = Color.red;

        Vector2 boxCastPos = (Vector2)_collider.bounds.center + Vector2.down * extraHeight / 2;

        Gizmos.DrawWireCube(boxCastPos, new Vector2(_collider.bounds.size.x / 1.1f, _collider.bounds.size.y + extraHeight));
    }

    public void PerformAttack()
    {
        if (isAttacking || !canAttack) return;
        _anim.SetBool("isAttacking", true);
        isAttacking = true;
        canAttack = false;
        cooldownTimer = attackCooldown;
        attackTimer = attackDuration;
        attackHitbox.SetActive(true);
    }

    private void EndAttack()
    {
        _anim.SetBool("isAttacking", false);
        isAttacking = false;
        attackHitbox.SetActive(false);
    }

    private IEnumerator DelayAttack(float duration)
    {
        yield return new WaitForSeconds(duration);
    }
}