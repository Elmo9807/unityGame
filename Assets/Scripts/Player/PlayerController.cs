using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private Player playerData;
    private HealthTracker healthTracker;
    private Player.HealthChangeHandler healthChangeHandler;

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

    [Header("Debug")]
    [SerializeField] private bool debugHealth = true;

    private float speed = 8f;
    private float jumpingPower = 12f;

    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    private bool isFacingRight = true;
    private bool doubleJump;
    private float nextAttackTime = 0f;

    // RECURSION PREVENTION FLAG
    private bool _isConnectingEvents = false;

    private void Awake()
    {
        playerData = new Player(transform);

        healthTracker = GetComponent<HealthTracker>();
        if (healthTracker == null)
        {
            Debug.LogWarning("[PlayerController] No HealthTracker found, adding one...");
            healthTracker = gameObject.AddComponent<HealthTracker>();


            if (playerData != null)
            {
                healthTracker.SetMaxHealth(playerData.MaxHealth);
                healthTracker.SetHealth(playerData.Health);
                Debug.Log($"[PlayerController] Initialized HealthTracker with Player's health: {playerData.Health}/{playerData.MaxHealth}");
            }
        }

        healthChangeHandler = (currentHealth, maxHealth) => {
            if (healthTracker != null)
            {
                if (debugHealth)
                    Debug.Log($"[PlayerController] Health changed event: {currentHealth}/{maxHealth}");

                healthTracker.SetHealth(currentHealth);
            }
        };
    }

    private void Start()
    {
        ConnectHealthEvents();

        if (healthTracker != null && playerData != null)
        {
            healthTracker.SetHealth(playerData.Health);
            if (debugHealth)
                Debug.Log($"[PlayerController] Initial health sync: {playerData.Health}/{playerData.MaxHealth}");
        }
    }

    private void OnEnable()
    {
        ConnectHealthEvents();
    }

    private void ConnectHealthEvents()
    {
        // IMPORTANT: RECURSION PROTECTOR
        if (_isConnectingEvents)
        {
            Debug.LogError("[PlayerController] Prevented recursive call to ConnectHealthEvents!");
            return;
        }

        _isConnectingEvents = true;

        try
        {
            if (playerData == null)
            {
                playerData = new Player(transform);
                Debug.Log("[PlayerController] Created new playerData");
            }

            if (healthTracker == null)
            {
                healthTracker = GetComponent<HealthTracker>();
                if (healthTracker == null)
                {
                    healthTracker = gameObject.AddComponent<HealthTracker>();
                    Debug.Log("[PlayerController] Created new HealthTracker component");
                }
            }

            if (playerData != null && healthTracker != null)
            {

                playerData.OnHealthChanged -= healthChangeHandler;

                playerData.OnHealthChanged += healthChangeHandler;

                if (debugHealth)
                    Debug.Log("[PlayerController] Connected Player health events to HealthTracker");
            }
            else
            {
                Debug.LogError($"[PlayerController] Failed to connect health events: playerData={playerData != null}, healthTracker={healthTracker != null}");
            }
        }
        finally
        {
            _isConnectingEvents = false;
        }
    }

    public int GetCurrentHealth()
    {
        return playerData != null ? playerData.Health : 0;
    }

    public int GetMaxHealth()
    {
        return playerData != null ? playerData.MaxHealth : 100;
    }

    public Player GetPlayerData()
    {
        if (playerData == null)
        {
            Debug.LogWarning("[PlayerController] GetPlayerData: playerData was null, re-creating it");
            playerData = new Player(transform);
            ConnectHealthEvents();
        }
        return playerData;
    }

    private void Update()
    {
        HandleMovement();
        HandleJumping();
        HandleAttack();
        HandleInventoryInput();

        // Safely update effects
        if (playerData != null)
        {
            playerData.UpdateEffects(Time.deltaTime);
        }
        else
        {
            // Emergency recovery
            Debug.LogError("[PlayerController] playerData is null in Update!");
            GetPlayerData(); // This will recreate it [V IMPORTANT DO NO DELETE OR RISK RE-INITIALISATION WIPING OUT PLAYER HEALTH DATA]
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
            coyoteTimeCounter = coyoteTime;
            doubleJump = false;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButton("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
                coyoteTimeCounter = 0f;
                jumpBufferCounter = 0f;
            }
            else if (!doubleJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
                doubleJump = true;
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
            // Try IDamageable interface first
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
                continue;
            }

            // Try Enemy component as fallback
            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.TakeDamage(Mathf.RoundToInt(attackDamage));
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (debugHealth)
            Debug.Log($"[PlayerController] TakeDamage called with damage: {damage}");

        int damageAmount = Mathf.RoundToInt(damage);

        bool recoveryNeeded = false;

        if (playerData == null)
        {
            playerData = new Player(transform);
            ConnectHealthEvents();
            recoveryNeeded = true;
            Debug.LogWarning("[PlayerController] Recreated missing playerData in TakeDamage");
        }

        if (healthTracker == null)
        {
            healthTracker = GetComponent<HealthTracker>();
            if (healthTracker == null)
            {
                healthTracker = gameObject.AddComponent<HealthTracker>();
            }
            recoveryNeeded = true;
            Debug.LogWarning("[PlayerController] Recreated missing healthTracker in TakeDamage");
        }

        if (recoveryNeeded)
        {

            healthTracker.SetMaxHealth(playerData.MaxHealth);
            healthTracker.SetHealth(playerData.Health);
            Debug.Log($"[PlayerController] Synced health values after recovery: {playerData.Health}/{playerData.MaxHealth}");
        }

        playerData.TakeDamage(damageAmount);
        healthTracker.TakeDamage(damageAmount);

        Debug.Log($"[PlayerController] After damage: Player:{playerData.Health}, HealthTracker:{healthTracker.CurrentHealth}");
    }

    private void HandleInventoryInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (playerData != null) playerData.inventory.UseWeapon(0, playerData);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (playerData != null) playerData.inventory.UseWeapon(1, playerData);
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            if (playerData != null) playerData.inventory.UseHealingPotion(playerData);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            if (playerData != null) playerData.inventory.UseConsumable(playerData);
        }
        else if (Input.GetKeyDown(KeyCode.I) && inventoryUI != null)
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