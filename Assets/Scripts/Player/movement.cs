using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private bool isGrounded;
    private float playerSpd = 4.0f;
    private float jumpHeight = 5.0f;
    private float gravVal = 1f;
    private float groundCheckRadius = 0.1f;

    public Transform groundCheck;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D is not attached to the player!");
        }

        rb.gravityScale = gravVal;
        if (groundCheck == null)
        {
            Debug.LogError("GroundCheck Transform is not assigned!");
        }


        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, LayerMask.GetMask("Ground"));
            Debug.Log("Is Grounded: " + isGrounded);
        }
        else
        {
            Debug.LogError("GroundCheck is null, cannot check if grounded!");
        }

        if (rb != null)
        {
            movementInput = new Vector2(Input.GetAxis("Horizontal"), 0);
            rb.linearVelocity = new Vector2(movementInput.x * playerSpd, rb.linearVelocity.y);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight); // Apply jump force
            }
        }
        else
        {
            Debug.LogError("Rigidbody2D is not assigned!");
        }
    }
}