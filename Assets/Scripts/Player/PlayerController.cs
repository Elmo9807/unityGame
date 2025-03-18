using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private Player playerData;

    [Header("Inventory UI")]
    [SerializeField] private InventoryUI inventoryUI;

    [Header("Movement Properties")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Attack Properties")]
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackRate = 2f;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Animator animator;

    private float speed = 8f;
    private float jumpingPower = 12f;

    private float coyoteTime = 0.2f ; 
    private float coyoteTimeCounter ; 

    private float jumpBufferTime = 0.2f ; 
    private float jumpBufferCounter ; 


    private bool isFacingRight = true;
    private bool doubleJump;
    private float nextAttackTime = 0f;

    private void Start()
    {
        playerData = new Player();
    }

    private void Update()
    {
        HandleMovement();
        HandleJumping();
        HandleAttack();
        HandleInventoryInput();
        playerData.UpdateEffects(Time.deltaTime);
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);

        if ((isFacingRight && horizontal < 0f) || (!isFacingRight && horizontal > 0f))
        {
            Flip();
        }
    }

    private void HandleJumping()
    {
    if (IsGrounded())
    {
        coyoteTimeCounter = coyoteTime;
        doubleJump = false; 
    }
    else
    {
        coyoteTimeCounter -= Time.deltaTime;  // Count down coyote time when not grounded
    }

    if (Input.GetButton("Jump"))
    {
        jumpBufferCounter = jumpBufferTime ; 
    }
    else 
    {
        jumpBufferCounter -= Time.deltaTime ; 
    }

    
    if (Input.GetButtonDown("Jump"))
    {
        // First jump with coyote time
        if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower); // Use rb.velocity instead of linearVelocity
            Debug.Log("First Jump");
             // Reset coyote time after jumping
             coyoteTimeCounter = 0f;
             
             jumpBufferCounter = 0f ; 
        }
        
        else if (!doubleJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower); // Double jump logic
            doubleJump = true;  
            Debug.Log("Double Jump");

            
            
        }
    }
}


    private void HandleAttack()
    {
        if (Time.time >= nextAttackTime && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Z)))
        {
            Attack();
            nextAttackTime = Time.time + 1f / attackRate;
        }
    }

    private void Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            damageable?.TakeDamage(attackDamage);
        }
    }

    private void HandleInventoryInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            playerData.inventory.UseWeapon(0, playerData);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            playerData.inventory.UseWeapon(1, playerData);
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            playerData.inventory.UseHealingPotion(playerData);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            playerData.inventory.UseConsumable(playerData);
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryUI.ToggleInventory();
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.3f, groundLayer);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}