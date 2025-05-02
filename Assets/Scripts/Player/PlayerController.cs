using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
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

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _collider = GetComponent<BoxCollider2D>();

        _startPos = transform.position;
    }

    private void Update()
    {
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

        Vector2 originalSize = _collider.size;
        Vector2 originalOffset = _collider.offset;

        _collider.size = new Vector2(originalSize.x, originalSize.y / 2);
        //_collider.offset = new Vector2(originalOffset.x, originalOffset.y / 2);

        _rb.linearVelocity = new Vector2(_rollSpeed * _facingDirection, _rb.linearVelocity.y);
        yield return new WaitForSeconds(_rollTime);
        _collider.size = originalSize;
        _collider.offset = originalOffset;

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
        transform.localScale = new Vector3(0.6f, 0.6f, 1);
        _rb.simulated = true;
    }

    private void UpdateAnimations()
    {
        _anim.SetBool("_isRunning", _isRunning);
        _anim.SetBool("isGrounded", IsGrounded());
    }

    private void OnDrawGizmos()
    {
        if (_collider == null) return;

        float extraHeight = 0.2f;
        Gizmos.color = Color.red;

        Vector2 boxCastPos = (Vector2)_collider.bounds.center + Vector2.down * extraHeight / 2;

        Gizmos.DrawWireCube(boxCastPos, new Vector2(_collider.bounds.size.x / 1.1f, _collider.bounds.size.y + extraHeight));
    }
}
