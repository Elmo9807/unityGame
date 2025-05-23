using System.Collections;
using UnityEngine;
using FMOD.Studio;
using System;

//Anything we want to do with influencing the player object, we put in here guys, thanks.
public class PlayerController : MonoBehaviour
{
    private Player playerData;
    private HealthTracker healthTracker;
    private Player.HealthChangeHandler healthChangeHandler;

    // Components
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform bowFirePoint;
    // [SerializeField] private TrailRenderer dashTrail;
    [SerializeField] private ParticleSystem dashParticles;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private bool invincible = false;

    // Attacking
    [Header("Attacking")]
    [SerializeField] private float attackRate = 2f;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private float heavyAttackRate = 10f;
    [SerializeField] private float heavyAttackRange = 2f;
    [SerializeField] private float heavyAttackDamageMultiplier = 2f;
    private float nextAttackTime = 0f;
    private float nextHeavyAttackTime = 0f;
    private float nextBowAttackTime = 0f;
    private float bowCooldown = 0.5f;

    // Movement
    [Header("Movement")]
    [SerializeField] private float dashForce = 24f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private bool dashInvulnerability = true;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpingPower = 11f;
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;
    private bool isFacingRight = true;
    private bool doubleJump;

    // Dashing
    private bool isDashing = false;
    private bool canDash = true;
    private float dashTimeLeft;
    private float lastDashTime = -10f;
    private Vector2 dashDirection;

    private bool isFallingThrough = false;

    private float horizontalInput;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool attackPressed;
    private bool heavyAttackPressed;
    private bool bowAttackPressed;
    private bool dashPressed;
    private bool useHealPressed;

    private bool isProcessingDamage = false;

    // Initialization
    private void Awake()
    {
        playerData = new Player();

        healthTracker = GetComponent<HealthTracker>();
        if (healthTracker == null)
        {
            healthTracker = gameObject.AddComponent<HealthTracker>();
            healthTracker.SetMaxHealth(playerData.MaxHealth);
            healthTracker.SetHealth(playerData.Health);
        }

        healthChangeHandler = (currentHealth, maxHealth) => {
            if (!isProcessingDamage)
                healthTracker.SetHealth(currentHealth);
        };

        playerData.OnDeath += HandlePlayerDeath;

        // dashTrail.emitting = false;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();

        playerData.OnHealthChanged += healthChangeHandler;
        healthTracker.SetHealth(playerData.Health);

        if (bowFirePoint == null)
            bowFirePoint = transform;

        if (arrowPrefab == null)
            arrowPrefab = Resources.Load<GameObject>("PlayerProjectile");

        /* playerData.hasBow = true;
        playerData.hasDash = true;
        playerData.hasDoubleJump = true;
        playerData.hasHealingPotion = true; */
    }

    // Main Update
    private void Update()
    {
        GatherInput();

        if (!isDashing)
        {
            if (playerData.hasBow && bowAttackPressed) HandleBowAttack();
            if (attackPressed) HandleAttack();
            if (heavyAttackPressed) HandleHeavyAttack();
            if (playerData.hasDash && dashPressed) HandleDashInput();
            if (useHealPressed && playerData.hasHealingPotion) HandleHealPotion();
        }
        else
        {
            UpdateDashTimer();
        }

        HandleJumpBuffer();
        HandleOneWayPlatforms();

        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
            doubleJump = false;
            animator.SetBool("isJumping", false);




        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            animator.SetBool("isJumping", true);
        }



        playerData.UpdateEffects(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            ApplyMovement();
            ProcessJump();
            animator.SetFloat("xVelocity", Math.Abs(rb.linearVelocity.x)); // speed of running animation is based off of player's x velocity
            animator.SetFloat("yVelocity", rb.linearVelocity.y); // triggers falling animation if y velocity is negative (going doing), else triggers jumping animation if y velocity is positive (going up)
        }
        else
        {
            ProcessDashPhysics();

        }
    }

    // Input Handling
    private void GatherInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            jumpPressed = true;

        jumpHeld = Input.GetButton("Jump");
        attackPressed = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Z);
        bowAttackPressed = Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.X);
        heavyAttackPressed = Input.GetKeyDown(KeyCode.R);
        dashPressed = Input.GetKeyDown(KeyCode.LeftShift);
        useHealPressed = Input.GetKeyDown(KeyCode.H);
    }

    private void HandleJumpBuffer()
    {
        if (jumpHeld)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    // Movement
    private void ApplyMovement()
    {
        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);

        if ((isFacingRight && horizontalInput < 0f) || (!isFacingRight && horizontalInput > 0f))
            Flip();
    }

    private void ProcessJump()
    {
        if (jumpPressed)
        {

            if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f) // jump 1
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
                coyoteTimeCounter = 0f;
                jumpBufferCounter = 0f;
                AudioManager.instance.PlayOneShot(FMODEvents.instance.PlayerJump, this.transform.position);
            }
            else if (playerData.hasDoubleJump && !doubleJump) // jump 2
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
                doubleJump = true;
                AudioManager.instance.PlayOneShot(FMODEvents.instance.PlayerDoubleJump, this.transform.position);
            }

        }

        if (jumpPressed)
            jumpPressed = false;
    }

    //void OnGUI() // Jump debugging
    //{
    //    GUILayout.Label($"Grounded: {IsGrounded()}");
    //    GUILayout.Label($"Double Jump: {doubleJump}");
    //    GUILayout.Label($"Coyote Time: {coyoteTimeCounter}");
    //    GUILayout.Label($"isJumping: {animator.GetBool("isJumping")}");
    //    GUILayout.Label($"Linear Y Velocity: {rb.linearVelocity.y}");
    //    GUILayout.Label($"jumpPressed: {jumpPressed}");
    //}

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

    // Combat
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
        if (Time.time >= nextBowAttackTime)
        {
            ShootArrow();
            nextBowAttackTime = Time.time + bowCooldown;
        }
    }

    private void HandleHeavyAttack()
    {
        if (Time.time >= nextHeavyAttackTime)
        {
            PerformHeavyAttack();
            nextHeavyAttackTime = Time.time + 1f / heavyAttackRate;
        }
    }

    private void HandleHealPotion()
    {
        playerData.UseHealingPotion();
        AudioManager.instance.PlayOneShot(FMODEvents.instance.Heal, this.transform.position);
    }

    private void ShootArrow()
    {
        if (animator != null)
            animator.SetTrigger("BowAttack");
        AudioManager.instance.PlayOneShot(FMODEvents.instance.BowAttack, this.transform.position);

        Vector2 direction;
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f)
            direction = new Vector2(horizontalInput, verticalInput).normalized;
        else
            direction = isFacingRight ? Vector2.right : Vector2.left;

        GameObject arrowObject = Instantiate(arrowPrefab, bowFirePoint.position, Quaternion.identity);
        PlayerArrow arrow = arrowObject.GetComponent<PlayerArrow>();

        arrow.baseDamage = (int)playerData.bowAttackDamage;
        arrow.SetDirection(direction);

        arrowObject.layer = LayerMask.NameToLayer("Projectile");
        arrowObject.tag = "PlayerProjectile";

        Collider2D playerCollider = GetComponent<Collider2D>();
        Collider2D arrowCollider = arrowObject.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(arrowCollider, playerCollider);
    }

    private void PerformAttack()
    {
        if (animator != null)
            animator.SetTrigger("Attack");
        AudioManager.instance.PlayOneShot(FMODEvents.instance.SwordAttack, this.transform.position);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(playerData.meleeAttackDamage);
                AudioManager.instance.PlayOneShot(FMODEvents.instance.SwordHit, this.transform.position);
                continue;
            }

            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
                enemyComponent.TakeDamage(Mathf.RoundToInt(playerData.meleeAttackDamage));
            AudioManager.instance.PlayOneShot(FMODEvents.instance.SwordHit, this.transform.position);
        }
    }

    private void PerformHeavyAttack()
    {
        if (animator != null)
            animator.SetTrigger("HeavyAttack");
        AudioManager.instance.PlayOneShot(FMODEvents.instance.SwordHeavyAttack, this.transform.position);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, heavyAttackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(playerData.meleeAttackDamage * heavyAttackDamageMultiplier);
                AudioManager.instance.PlayOneShot(FMODEvents.instance.SwordHit, this.transform.position);
                continue;
            }

            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
                enemyComponent.TakeDamage(Mathf.RoundToInt(playerData.meleeAttackDamage * heavyAttackDamageMultiplier));
            AudioManager.instance.PlayOneShot(FMODEvents.instance.SwordHit, this.transform.position);
        }
    }

    // Dash
    private void HandleDashInput()
    {
        if (canDash && Time.time >= lastDashTime + dashCooldown)
            StartDash();
    }

    private void StartDash()
    {
        float vertical = Input.GetAxisRaw("Vertical");

        if (horizontalInput == 0 && vertical == 0)
            dashDirection = isFacingRight ? Vector2.right : Vector2.left;
        else
            dashDirection = new Vector2(horizontalInput, vertical).normalized;

        isDashing = true;
        canDash = false;
        dashTimeLeft = dashDuration;
        lastDashTime = Time.time;
        AudioManager.instance.PlayOneShot(FMODEvents.instance.PlayerDash, this.transform.position);

        // dashTrail.emitting = true;

        if (dashParticles != null)
            dashParticles.Play();

        if (animator != null)
            animator.SetTrigger("Dash");
    }

    private void UpdateDashTimer()
    {
        dashTimeLeft -= Time.deltaTime;

        if (dashTimeLeft <= 0)
            EndDash();
    }

    private void ProcessDashPhysics()
    {
        rb.linearVelocity = dashDirection * dashForce;
    }

    private void EndDash()
    {
        isDashing = false;
        rb.linearVelocity = rb.linearVelocity * 0.6f;

        // dashTrail.emitting = false;

        Invoke("ResetDash", 0.1f);
    }

    private void ResetDash()
    {
        canDash = true;
    }

    private void HandleOneWayPlatforms()
    {
        if (Input.GetKey(KeyCode.S) && !isFallingThrough)
        {
            StartCoroutine(DisableCollisionTemporarily());
        }
    }

    IEnumerator DisableCollisionTemporarily()
    {
        Collider2D platformCollider = GetPlatformUnderneath();
        if (platformCollider != null)
        {
            isFallingThrough = true;

            Physics2D.IgnoreCollision(platformCollider, playerCollider, true);
            yield return new WaitUntil(() => playerCollider.bounds.max.y < platformCollider.bounds.min.y ||
            playerCollider.bounds.max.x < platformCollider.bounds.min.x ||
            playerCollider.bounds.min.x > playerCollider.bounds.max.x);
            yield return new WaitForSeconds(0.1f);
            Physics2D.IgnoreCollision(platformCollider, playerCollider, false);

            isFallingThrough = false;
        }
    }

    Collider2D GetPlatformUnderneath()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, 1f);
        if (hit.collider != null && hit.collider.CompareTag("OneWayPlatform"))
        {
            return hit.collider;
        }
        return null;
    }

    // Public Methods
    public int GetCurrentHealth()
    {
        return playerData.Health;
    }

    public int GetMaxHealth()
    {
        return playerData.MaxHealth;
    }

    public Player GetPlayerData()
    {
        return playerData;
    }

    public void TakeDamage(float damage)
    {
        if (isProcessingDamage) return;
        isProcessingDamage = true;

        try
        {
            if (isDashing && dashInvulnerability || invincible)
                return;

            int damageAmount = Mathf.RoundToInt(damage);
            playerData.TakeDamage(damageAmount);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.PlayerHurt, this.transform.position);


            // Add death check here
            if (playerData.Health <= 0)
            {
                // Call game over since we can't directly access HealthTracker.Die()
                if (GameManager.Instance != null)
                {

                    GameManager.Instance.GameOver();
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }
        finally
        {
            isProcessingDamage = false;
        }
    }

    private void HandlePlayerDeath()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.PlayerDie, this.transform.position);
        AudioManager.instance.FadeoutAll();
        GameManager.Instance.GameOver();
    }

    // Gizmos
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    private void CallFootsteps() // This is called on certain frames of the walk animation. 
    {
        // Check if grounded
        if (Math.Abs(rb.linearVelocity.y) < 0.1f)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.PlayerFootstepAction, transform.position);
        }
    }

}