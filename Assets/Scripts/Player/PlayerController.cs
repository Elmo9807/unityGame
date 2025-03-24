using UnityEngine;
using UnityEngine.AI;
using FMOD.Studio;

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

    [Header("Bow Properties")]
    [SerializeField] private GameObject arrowPrefab; 
    [SerializeField] private Transform bowFirePoint; 
    private bool hasBow = false; 
    private float nextBowAttackTime = 0f;
    private float bowCooldown = 0.5f;


    [Header("Dash Properties")]
    [SerializeField] private float dashForce = 24f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private TrailRenderer dashTrail;
    [SerializeField] private ParticleSystem dashParticles;
    [SerializeField] private bool dashInvulnerability = true;

    [Header("Debug")]
    [SerializeField] private bool debugHealth = true;
    [SerializeField] private bool debugDash = true;

    private float speed = 8f;
    private float jumpingPower = 8f;

    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    private bool isFacingRight = true;
    private bool doubleJump;
    private float nextAttackTime = 0f;

    
    private bool _isConnectingEvents = false;

    
    private bool _isProcessingDamage = false;

    
    private bool isDashing = false;
    private bool canDash = true;
    private float dashTimeLeft;
    private float lastDashTime = -10f;
    private Vector2 dashDirection;

    
    private float horizontalInput;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool attackPressed;
    private bool dashPressed;
    private bool useItem1Pressed;
    private bool useItem2Pressed;
    private bool useHealPressed;
    private bool useConsumablePressed;
    private bool toggleInventoryPressed;

    // audio
    private EventInstance playerFootstepRough;

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
            if (healthTracker != null && !_isProcessingDamage)
            {
                if (debugHealth)
                    Debug.Log($"[PlayerController] Health changed event: {currentHealth}/{maxHealth}");

                healthTracker.SetHealth(currentHealth);
            }
        };

        
        if (dashTrail != null)
        {
            dashTrail.emitting = false;
        }
    }

    private void Start()
    {
        playerFootstepRough = AudioManager.instance.CreateInstance(FMODEvents.instance.PlayerFootstepRough);

        ConnectHealthEvents();

        if (healthTracker != null && playerData != null)
        {
            healthTracker.SetHealth(playerData.Health);
            if (debugHealth)
                Debug.Log($"[PlayerController] Initial health sync: {playerData.Health}/{playerData.MaxHealth}");
        }

        if (bowFirePoint == null)
        {
            bowFirePoint = transform;
        }

        
        SetupPlayerWeapons();
    }

    private void OnEnable()
    {
        ConnectHealthEvents();
    }

    private void ConnectHealthEvents()
    {
        
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
        
        GatherInput();

        
        if (!isDashing)
        {
            HandleBowAttack();
            if (attackPressed) HandleAttack();
            if (dashPressed) HandleDashInput();
            HandleInventoryInputs();
        }
        else
        {
            
            UpdateDashTimer();
        }

        
        HandleJumpBuffer();

        
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
            doubleJump = false;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        
        if (playerData != null)
        {
            playerData.UpdateEffects(Time.deltaTime);
        }
        else
        {
            
            Debug.LogError("[PlayerController] playerData is null in Update!");
            GetPlayerData(); 
        }
    }

    private void GatherInput()
    {
        
        horizontalInput = Input.GetAxisRaw("Horizontal");

        
        if (Input.GetButtonDown("Jump"))
            jumpPressed = true;

        jumpHeld = Input.GetButton("Jump");

        
        attackPressed = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Z);

        
        dashPressed = Input.GetKeyDown(KeyCode.LeftShift);

        
        useItem1Pressed = Input.GetKeyDown(KeyCode.Alpha1);
        useItem2Pressed = Input.GetKeyDown(KeyCode.Alpha2);
        useHealPressed = Input.GetKeyDown(KeyCode.H);
        useConsumablePressed = Input.GetKeyDown(KeyCode.C);
        toggleInventoryPressed = Input.GetKeyDown(KeyCode.I);
    }

    private void HandleJumpBuffer()
    {
        if (jumpHeld)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            
            ApplyMovement();
            ProcessJump();
            UpdateSound();
        }
        else
        {
            
            ProcessDashPhysics();
            UpdateSound();
        }
    }

    private void ApplyMovement()
    {
        
        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);

        
        if ((isFacingRight && horizontalInput < 0f) || (!isFacingRight && horizontalInput > 0f))
        {
            Flip();
        }
    }

    private void ProcessJump()
    {
        
        if (jumpPressed)
        {
            if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f)
            {
                
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
                coyoteTimeCounter = 0f;
                jumpBufferCounter = 0f;
                AudioManager.instance.PlayOneShot(FMODEvents.instance.PlayerJump, this.transform.position);
                if (debugHealth) 
                    Debug.Log("[PlayerController] Performed first jump");
            }
            else if (!doubleJump)
            {
                
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
                doubleJump = true;
                AudioManager.instance.PlayOneShot(FMODEvents.instance.PlayerJump, this.transform.position);
                if (debugHealth) 
                    Debug.Log("[PlayerController] Performed double jump");
            }
        }

        
        
        if (jumpPressed)
            jumpPressed = false;
    }

    private void SetupPlayerWeapons()
    {
        
        WeaponLoader weaponLoader = FindFirstObjectByType<WeaponLoader>();
        if (weaponLoader == null)
        {
            
            GameObject weaponLoaderGO = new GameObject("WeaponLoader");
            weaponLoader = weaponLoaderGO.AddComponent<WeaponLoader>();

            
            weaponLoader.arrowPrefab = arrowPrefab;
            if (weaponLoader.arrowPrefab == null)
            {
                
                weaponLoader.arrowPrefab = Resources.Load<GameObject>("PlayerProjectile");

                if (weaponLoader.arrowPrefab == null)
                {
                    Debug.LogWarning("PlayerProjectile prefab not found. Bow attacks will not work properly.");
                }
            }
        }

        
        if (playerData != null && playerData.inventory != null)
        {
            Bow bow = weaponLoader.CreateBow();
            playerData.inventory.AddWeapon(bow);
            hasBow = true;
            Debug.Log("Added bow to player's inventory");

            
            HealingPotion extraPotion = weaponLoader.CreateHealingPotion("standard");
            playerData.inventory.AddHealingPotion(extraPotion);
            Debug.Log("Added healing potion to player's inventory");
        }
        else
        {
            Debug.LogWarning("PlayerData or inventory not initialized, cannot add bow.");
        }
    }

    private void HandleAttack()
    {
        if (Time.time >= nextAttackTime)
        {
            PerformAttack();
            nextAttackTime = Time.time + 1f / attackRate;
        }
    }

    private void HandleBowAttack()
    {
        
        if (hasBow && Time.time >= nextBowAttackTime && (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.X)))
        {
            BowAttack();
            nextBowAttackTime = Time.time + bowCooldown;
        }
    }

    private void PerformAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        AudioManager.instance.PlayOneShot(FMODEvents.instance.SwordAttack, this.transform.position);
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {

            
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.SwordHit, this.transform.position);
                damageable.TakeDamage(attackDamage);
                continue;
            }

            
            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.SwordHit, this.transform.position);
                enemyComponent.TakeDamage(Mathf.RoundToInt(attackDamage));
            }
        }
    }

    private void BowAttack()
    {
        
        
        if (playerData != null && playerData.inventory != null && playerData.inventory.weapons.Count > 1)
        {
            playerData.inventory.UseWeapon(1, playerData);

            if (animator != null)
            {
                animator.SetTrigger("BowAttack");
            }
            AudioManager.instance.PlayOneShot(FMODEvents.instance.BowAttack, this.transform.position);
            Debug.Log("Player fired bow");
        }
        else
        {
            Debug.LogWarning("No bow available in inventory");
        }
    }

    private void HandleDashInput()
    {
        if (canDash && Time.time >= lastDashTime + dashCooldown)
        {
            StartDash();
        }
    }

    private void StartDash()
    {
        
        float vertical = Input.GetAxisRaw("Vertical");

        
        if (horizontalInput == 0 && vertical == 0)
        {
            dashDirection = isFacingRight ? Vector2.right : Vector2.left;
        }
        else
        {
            
            dashDirection = new Vector2(horizontalInput, vertical).normalized;
        }

        isDashing = true;
        canDash = false;
        dashTimeLeft = dashDuration;
        lastDashTime = Time.time;

        
        if (dashTrail != null)
        {
            dashTrail.emitting = true;
        }

        if (dashParticles != null)
        {
            dashParticles.Play();
        }

        
        if (animator != null)
        {
            animator.SetTrigger("Dash");
        }

        if (debugDash)
        {
            Debug.Log($"[PlayerController] Dashing in direction: {dashDirection}, force: {dashForce}");
        }
    }

    private void UpdateDashTimer()
    {
        dashTimeLeft -= Time.deltaTime;

        if (dashTimeLeft <= 0)
        {
            EndDash();
        }
    }

    private void ProcessDashPhysics()
    {
        
        rb.linearVelocity = dashDirection * dashForce;
    }

    private void EndDash()
    {
        isDashing = false;

        
        rb.linearVelocity = rb.linearVelocity * 0.6f;

        if (dashTrail != null)
        {
            dashTrail.emitting = false;
        }

        Invoke("ResetDash", 0.1f);

        if (debugDash)
        {
            Debug.Log("[PlayerController] Dash ended");
        }
    }

    private void ResetDash()
    {
        canDash = true;
    }

    private void HandleInventoryInputs()
    {
        if (useItem1Pressed)
        {
            if (playerData != null) playerData.inventory.UseWeapon(0, playerData);
        }
        else if (useItem2Pressed)
        {
            if (playerData != null) playerData.inventory.UseWeapon(1, playerData);
        }
        else if (useHealPressed)
        {
            if (playerData != null) playerData.inventory.UseHealingPotion(playerData);
        }
        else if (useConsumablePressed)
        {
            if (playerData != null) playerData.inventory.UseConsumable(playerData);
        }
        else if (toggleInventoryPressed && inventoryUI != null)
        {
            inventoryUI.ToggleInventory();
        }
    }

    public void TakeDamage(float damage)
    {
        
        if (_isProcessingDamage) return;

        _isProcessingDamage = true;

        try
        {
            if (isDashing && dashInvulnerability)
            {
                if (debugDash || debugHealth)
                    Debug.Log("[PlayerController] Damage avoided while dashing!");
                return;
            }

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

            
            int currentPlayerHealth = playerData.Health;
            if (healthTracker.CurrentHealth != currentPlayerHealth)
            {
                Debug.LogWarning($"[PlayerController] Health desync detected! Player:{currentPlayerHealth}, HealthTracker:{healthTracker.CurrentHealth}. Fixing...");
                healthTracker.SetHealth(currentPlayerHealth);
            }

            Debug.Log($"[PlayerController] After damage: Player:{playerData.Health}, HealthTracker:{healthTracker.CurrentHealth}");
        }
        finally
        {
            _isProcessingDamage = false;
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

    private void UpdateSound()
    {   // if player is moving on ground, starts footstep loop sound
        if(rb.linearVelocity.x != 0 && IsGrounded()){
            // checks if footsteps already playing
            PLAYBACK_STATE playbackState;
            playerFootstepRough.getPlaybackState(out playbackState);
            if (playbackState != PLAYBACK_STATE.PLAYING)
            {
                playerFootstepRough.start();
            }
        }
        else // else, stops footstep loop sound
        {
            playerFootstepRough.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        
        Gizmos.color = Color.blue;
        Vector3 dashEndPoint = transform.position + (isFacingRight ? Vector3.right : Vector3.left) * dashForce * dashDuration * 0.5f;
        Gizmos.DrawLine(transform.position, dashEndPoint);
    }
}