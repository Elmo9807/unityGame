<<<<<<< HEAD
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float horizontal;
    private float speed = 8f;
    private float jumpingPower = 12f;
    private bool isFacingRight = true;

    private bool doubleJump;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        // Reset double jump when grounded
        if (IsGrounded())
        {
            doubleJump = false;
        }

        // Handle jumping (calculate force at the moment of key press)
        if (Input.GetButtonDown("Jump"))
        {
            if (IsGrounded())
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower); // Jump instantly
                Debug.Log("First Jump");
            }
            else if (!doubleJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower); // Double jump instantly
                doubleJump = true;
                Debug.Log("Double Jump");
            }
        }

        Flip();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.3f, groundLayer);
    }

    private void Flip()
    {
        if ((isFacingRight && horizontal < 0f) || (!isFacingRight && horizontal > 0f))
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
>>>>>>> temp-backup
}