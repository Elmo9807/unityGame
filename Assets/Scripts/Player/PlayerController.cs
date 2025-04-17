using UnityEngine;
using FMOD.Studio;

//Anything we want to do with influencing the player object, we put in here guys, thanks.
public class PlayerController : MonoBehaviour
{
    private Player playerData;
    private HealthTracker healthTracker;
    private Player.HealthChangeHandler healthChangeHandler;

    // Components
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

    // Attacking
    [SerializeField] private float attackRate = 2f;
    [SerializeField] private float attackRange = 0.5f;
    private float nextAttackTime = 0f;
    private float nextBowAttackTime = 0f;
    private float bowCooldown = 0.5f;

    // Movement
    [SerializeField] private float dashForce = 24f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private bool dashInvulnerability = true;
    private float speed = 8f;
    private float jumpingPower = 8f;
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

    // Input
    private float horizontalInput;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool attackPressed;
    private bool bowAttackPressed;
    private bool dashPressed;
    private bool useHealPressed;

    private bool isProcessingDamage = false;

    // Audio
    private EventInstance playerFootstepRough;

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
        /*playerFootstepRough = AudioManager.instance.CreateInstance(FMODEvents.instance.PlayerFootstepRough);*/ // initializes footstep audio

        playerData.OnHealthChanged += healthChangeHandler;
        healthTracker.SetHealth(playerData.Health);

        if (bowFirePoint == null)
            bowFirePoint = transform;

        if (arrowPrefab == null)
            arrowPrefab = Resources.Load<GameObject>("PlayerProjectile");

        playerData.hasBow = true;
        playerData.hasDash = true;
        playerData.hasDoubleJump = true;
        playerData.hasHealingPotion = true;
    }

    // Main Update
    private void Update()
    {
        GatherInput();

        if (!isDashing)
        {
            if (playerData.hasBow && bowAttackPressed) HandleBowAttack();
            if (attackPressed) HandleAttack();
            if (playerData.hasDash && dashPressed) HandleDashInput();
            if (useHealPressed && playerData.hasHealingPotion) playerData.UseHealingPotion();
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

        playerData.UpdateEffects(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            ApplyMovement();
            ProcessJump();
            UpdateSound(); // plays footstep audio
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
            if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
                coyoteTimeCounter = 0f;
                jumpBufferCounter = 0f;
                /*AudioManager.instance.PlayOneShot(FMODEvents.instance.PlayerJump, this.transform.position);*/
            }
            else if (playerData.hasDoubleJump && !doubleJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
                doubleJump = true;
                /*AudioManager.instance.PlayOneShot(FMODEvents.instance.PlayerJump, this.transform.position);*/
            }
        }

        if (jumpPressed)
            jumpPressed = false;
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

    private void ShootArrow()
    {
        if (animator != null)
            animator.SetTrigger("BowAttack");

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
        /*AudioManager.instance.PlayOneShot(FMODEvents.instance.SwordAttack, this.transform.position);*/

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(playerData.meleeAttackDamage);
                /*AudioManager.instance.PlayOneShot(FMODEvents.instance.SwordHit, this.transform.position);*/
                continue;
            }

            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
                enemyComponent.TakeDamage(Mathf.RoundToInt(playerData.meleeAttackDamage));
                /*AudioManager.instance.PlayOneShot(FMODEvents.instance.SwordHit, this.transform.position);*/
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
            if (isDashing && dashInvulnerability)
                return;

            int damageAmount = Mathf.RoundToInt(damage);
            playerData.TakeDamage(damageAmount);

            // Add death check here
            if (playerData.Health <= 0)
            {
                // Call game over since we can't directly access HealthTracker.Die()
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.HandleGameOver();
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
        GameManager.Instance.HandleGameOver();
    }

    // Gizmos
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    private void UpdateSound()
    {   // if player is moving on ground, starts footstep loop sound
        if (rb.linearVelocity.x != 0 && IsGrounded())
        {
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
}