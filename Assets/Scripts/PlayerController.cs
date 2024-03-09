using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]

public class PlayerController : MonoBehaviour
{
    public float runSpeed = 8f;
    public float airSpeed = 3f;
    public float jumpImpulse = 10f;

    [SerializeField]
    private float coyoteTime = 0.15f;
    private float coyoteTimeCounter;

    [SerializeField]
    private float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;
    private bool isJumping;
    private Vector3 startPosition;



    TouchingDirections touchingDirections;

    Vector2 moveInput;


    public bool _isFacingRight = true;
    public bool IsFacingRight
    {
        get
        {
            return _isFacingRight;
        }
        private set
        {
            if (_isFacingRight != value)
            {
                transform.localScale *= new Vector2(-1, 1);
            }
            _isFacingRight = value;
        }
    }

    public float CurrentMoveSpeed
    {
        get
        {
            if (IsMoving && !touchingDirections.IsOnWall)
            {
                if (touchingDirections.IsGrounded)
                {
                    return runSpeed;
                }
                else
                {
                    return airSpeed;
                }
            }
            else
            {
                return 0;
            }
        }

    }


    [SerializeField]
    private bool _isMoving = false;

    public bool IsMoving
    {
        get
        {
            return _isMoving;
        }
        private set
        {
            _isMoving = value;
            animator.SetBool(AnimationStrings.isMoving, value);
        }
    }

    Rigidbody2D rb;
    Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (touchingDirections.IsGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
        if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f && !isJumping)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpImpulse);

            jumpBufferCounter = 0f;

            StartCoroutine(JumpCooldown());
        }
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);

            coyoteTimeCounter = 0f;
        }
        if (touchingDirections.IsOnWall && Input.GetKey(KeyCode.LeftShift))
        {
            WallGrab();
        }
        else if (touchingDirections.IsOnWall && !touchingDirections.IsGrounded && rb.velocity.y <= 0
        && (Input.GetKey("d") || Input.GetKey("a")))
        {
            Wallslide();
            rb.gravityScale = 0.9f;
        }
        else
        {
            rb.gravityScale = 0.9f;
        }
    }

    [SerializeField]
    private float wallSlideSpeed = 10f;
    private void Wallslide()
    {
        rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
    }

    private void WallGrab()
    {
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(rb.velocity.x, 0);
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.velocity.y);

        animator.SetFloat(AnimationStrings.yVelocity, rb.velocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        IsMoving = moveInput != Vector2.zero;

        SetFacingDirection(moveInput);
    }

    private void SetFacingDirection(Vector2 moveInput)
    {
        if (moveInput.x > 0 && !IsFacingRight)
        {
            // Face right
            IsFacingRight = true;
        }
        else if (moveInput.x < 0 && IsFacingRight)
        {
            // Face left
            IsFacingRight = false;
        }
    }


    public void Die()
    {
        transform.position = startPosition;
    }


    private IEnumerator JumpCooldown()
    {
        isJumping = true;
        yield return new WaitForSeconds(0.4f);
        isJumping = false;
    }
}
