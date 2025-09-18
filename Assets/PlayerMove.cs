using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
  
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float acceleration = 10f;
    public float deceleration = 10f;
    public float velocity = 0.8f;

    public float airControl = 0.1f;

    [Header("Jump settings")]
    public float jumpForce = 14f;
    [Tooltip("Multiplier to reduce jump height if jump ia cut short")]
    public float jumpCutMultiplier = 0.5f;
    public int maxJumpCount = 1;
    public float coyoteTime = 0.1f;
    [Tooltip("Buffer time in seconds")]
    public float jumpBufferTime = 0.1f;

    [Header("Gravity")]
    [Tooltip("Base Gravity scaling when rising/jumping")]
    public float gravityScale = 4f;
    [Tooltip("Multiplier applied when falling")]
    public float fallMulltiplier = 2f;
    [Tooltip("Max downward velocity")]
    public float maxFallSpeed = -20f;

    [Header("Ground Detection")]
    [Tooltip("Empty object transfer for ground check")]
    public Transform groundCheck;
    [Tooltip("Ground check circle's radius")]
    public float groundCheckRadius = 0.1f;
    [Tooltip("Layer the ground check will look for")]
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float horizontalInput;
    private int jumpCount;
    private bool isGrounded;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float velocityPower;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            jumpCount = maxJumpCount;
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
           if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime ;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (jumpBufferTimer > 0 && (coyoteTimer > 0 || jumpCount > 0))
        {
            Jump();
            jumpBufferTimer = 0;
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
        }

    }

    void FixedUpdate()
    {
        float targetSpeed = horizontalInput * moveSpeed;
        float speedDifference = targetSpeed - rb.velocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ?
            (isGrounded ? acceleration : acceleration * airControl):
            (isGrounded ? deceleration : deceleration * airControl);

        float movement = Mathf.Pow(Mathf.Abs(speedDifference) * accelRate, velocityPower) * Mathf.Sign(speedDifference);

        rb.AddForce(Vector2.right * movement);


        // Adjust gravity when falling
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = gravityScale * fallMulltiplier;
        }
        else//add
        {
            rb.gravityScale = gravityScale;
        }
        // Cap max fall speed
        if (rb.velocity.y < maxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);
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
        if(groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

    }
}
