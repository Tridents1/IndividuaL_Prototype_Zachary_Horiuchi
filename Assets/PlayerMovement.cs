using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base horizontal movement speed.")]
    public float moveSpeed = 8f;

    [Tooltip("Rate of acceleration when starting to move.")]
    public float acceleration = 10f;

    [Tooltip("Rate of deceleration when stopping.")]
    public float deceleration = 10f;

    [Tooltip("Controls curve of acceleration (lower = snappier, higher = smoother).")]
    public float velocityPower = 0.9f;

    [Tooltip("Air control percentage (0 = none, 1 = full control).")]
    [Range(0f, 1f)] public float airControl = 0.8f;


    [Header("Jump Settings")]
    [Tooltip("Initial upward force when jumping.")]
    public float jumpForce = 14f;

    [Tooltip("Multiplier to reduce jump height if jump is cut short.")]
    public float jumpCutMultiplier = 0.5f;

    [Tooltip("Number of allowed jumps (1 = normal, 2 = double jump).")]
    public int maxJumpCount = 1;

    [Tooltip("Coyote time in seconds (allows jumping shortly after leaving ground).")]
    public float coyoteTime = 0.1f;

    [Tooltip("Buffer time in seconds (queues jump input before landing).")]
    public float jumpBufferTime = 0.1f;


    [Header("Gravity / Fall Settings")]
    [Tooltip("Base gravity scale (while rising).")]
    public float gravityScale = 4f;

    [Tooltip("Multiplier applied when falling.")]
    public float fallMultiplier = 2f;

    [Tooltip("Maximum downward velocity (terminal fall speed).")]
    public float maxFallSpeed = -20f;


    [Header("Ground Detection")]
    [Tooltip("Position where ground is checked.")]
    public Transform groundCheck;

    [Tooltip("Radius of the ground check circle.")]
    public float groundCheckRadius = 0.1f;

    [Tooltip("What layers count as ground.")]
    public LayerMask groundLayer;


    // Private
    private Rigidbody2D rb;
    private float horizontalInput;
    private int jumpCount;
    private bool isGrounded;
    private float coyoteTimer;
    private float jumpBufferTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            jumpCount = maxJumpCount;
            coyoteTimer = coyoteTime; // Reset coyote time
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        // Jump buffer (when you press jump right before hitting ground)
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        // Jump logic
        if (jumpBufferTimer > 0 && (coyoteTimer > 0 || jumpCount > 0))
        {
            Jump();
            jumpBufferTimer = 0;
        }

        // Jump cut: if player releases jump button early
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)//change to up
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
        }
    }

    void FixedUpdate()
    {
        // Target speed
        float targetSpeed = horizontalInput * moveSpeed;
        float speedDifference = targetSpeed - rb.velocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ?
            (isGrounded ? acceleration : acceleration * airControl) :
            (isGrounded ? deceleration : deceleration * airControl);

        float movement = Mathf.Pow(Mathf.Abs(speedDifference) * accelRate, velocityPower) * Mathf.Sign(speedDifference);

        rb.AddForce(Vector2.right * movement);

        // Adjust gravity when falling
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = gravityScale * fallMultiplier;
        }
        else//add
        {
            rb.gravityScale = gravityScale;
        }
        // Cap max fall speed
        if (rb.velocity.y < maxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);//Change to max fall speed
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpCount--;
        coyoteTimer = 0;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}

