using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    private Player playerData;

    [Header("Health UI")]
    [SerializeField] private HealthBarController healthBar;

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

    [Header("Physics Properties")]
    [SerializeField] private PhysicsMaterial2D nonStick;
    [SerializeField] private PhysicsMaterial2D bouncyMaterial; //these are never actually used, can be safely deleted if necessary

    private float speed = 8f;
    private float jumpingPower = 12f;
    private bool isFacingRight = true;
    private bool doubleJump;
    private float nextAttackTime = 0f;

    private void Start()
    {
        playerData = new Player(transform);

        healthBar = FindAnyObjectByType<HealthBarController>() ?? healthBar;

        if (healthBar != null)
        {
            healthBar.SetTarget(transform);
            healthBar.UpdateHealth(playerData.Health, playerData.MaxHealth);
        }

        playerData.OnHealthChanged += OnHealthChanged;
    }

    public int GetCurrentHealth()
    {
        return playerData.Health;
    }

    public int GetMaxHealth()
    {
        return playerData.MaxHealth;
    }

    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleJumping();
        HandleAttack();
        HandleInventoryInput();
        playerData.UpdateEffects(Time.deltaTime);
        }

    private void OnDestroy()
    {
        if (playerData != null)
        {
            playerData.OnHealthChanged -= OnHealthChanged;
        }
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
            doubleJump = false;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (IsGrounded())
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
                Debug.Log("First Jump");
            }
            else if (!doubleJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
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

    public void TakeDamage(float damage)
    {

        Debug.Log("TakeDamage called. Damage: " + damage); //Debugger, ensure TakeDamage is being called correctly.
        playerData.TakeDamage((int)damage);

        if(playerData.Health <= 0)
        {
            Debug.Log("The player is dead. Oh no...");
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