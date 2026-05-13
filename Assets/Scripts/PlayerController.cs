using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;

    [Header("Checagem de chão")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveInput;
    private bool facingRight = true;
    private bool movementLocked = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (movementLocked)
            return;

        moveInput = 0f;

        if (Input.GetKey(KeyCode.A))
        {
            moveInput = -1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            moveInput = 1f;
        }

        if (moveInput < 0 && facingRight)
        {
            Flip();
        }
        else if (moveInput > 0 && !facingRight)
        {
            Flip();
        }

        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            ForceJump(jumpForce);
        }
    }

    void FixedUpdate()
    {
        if (movementLocked)
            return;

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public void ForceJump(float customJumpForce)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, customJumpForce);
    }

    public bool IsFacingRight()
    {
        return facingRight;
    }

    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;
        moveInput = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}