using UnityEngine;
using System.Collections;

public class DaBigBoss : Enemy
{
    [Header("Dragon Stats")]
    public int fireballDamage = 25;
    public int biteDamage = 40;
    public int breathAttackDamage = 20;
    public float flyingHeight = 5f;

    [Header("Attack Settings")]
    public float fireballCooldown = 2.5f;
    public float biteCooldown = 1.5f;
    public float breathCooldown = 4f;
    public float biteRange = 2.5f;
    public float breathRange = 6f;
    public float phaseChangeCooldown = 15f;

    [Header("Projectile Settings")]
    public GameObject fireballPrefab;
    public GameObject breathEffectPrefab;
    public Vector2 mouthOffset = new Vector2(1.5f, 0.2f);

    // State machine
    private enum DragonState
    {
        Flying,
        Grounded,
        ChangingHeight
    }

    // Attack tracking
    private float lastFireballTime = -10f;
    private float lastBiteTime = -10f;
    private float lastBreathTime = -10f;
    private float lastStateChangeTime = 0f;
    private DragonState currentState = DragonState.Flying;
    private bool isChangingHeight = false;

    // Components
    private ProjectileAttacker projectileAttacker;

    protected override void Start()
    {
        // Set base stats
        Name = "DaBigBoss";
        MaxHealth = 500;
        Health = MaxHealth;
        Speed = 3f;

        base.Start();

        // Set up components
        SetupProjectileAttacker();

        // Start in flying state
        currentState = DragonState.Flying;
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }

        // Move to flying height initially
        StartCoroutine(ChangeHeight(true));
    }

    private void SetupProjectileAttacker()
    {
        projectileAttacker = GetComponent<ProjectileAttacker>();
        if (projectileAttacker == null)
        {
            projectileAttacker = gameObject.AddComponent<ProjectileAttacker>();

            // Try to load fireball prefab if not assigned
            if (fireballPrefab == null)
            {
                fireballPrefab = Resources.Load<GameObject>("Fireball");
                if (fireballPrefab == null)
                {
                    Debug.LogWarning("Fireball prefab not found in Resources. Please assign it in the inspector.");
                }
            }

            projectileAttacker.projectilePrefab = fireballPrefab;
            projectileAttacker.attackCooldown = fireballCooldown;
            projectileAttacker.attackRange = 20f; // Long range
            projectileAttacker.projectileSpeed = 8f;
            projectileAttacker.spawnOffset = mouthOffset;
        }
    }

    protected override void Update()
    {
        // Only run basic player detection from base class
        if (Time.time > lastPlayerCheckTime + playerCheckInterval)
        {
            if (!IsPlayerValid)
            {
                FindPlayer();
            }
            else
            {
                lastKnownPlayerPosition = _playerTransform.position;
                currentPlayerDistance = Vector3.Distance(transform.position, _playerTransform.position);
            }
            lastPlayerCheckTime = Time.time;
        }

        // Update facing
        if (IsPlayerValid)
        {
            UpdateFacing();

            // Update mouth offset based on facing direction
            if (projectileAttacker != null)
            {
                projectileAttacker.spawnOffset = new Vector2(
                    isFacingRight ? Mathf.Abs(mouthOffset.x) : -Mathf.Abs(mouthOffset.x),
                    mouthOffset.y
                );
            }
        }

        // If we're changing height, don't process other state logic
        if (isChangingHeight) return;

        // State timer for phase changes
        if (Time.time > lastStateChangeTime + phaseChangeCooldown &&
            currentState != DragonState.ChangingHeight)
        {
            ChangePhase();
            return;
        }

        // Process state-specific behavior
        switch (currentState)
        {
            case DragonState.Flying:
                UpdateFlyingState();
                break;

            case DragonState.Grounded:
                UpdateGroundedState();
                break;
        }
    }

    private void UpdateFlyingState()
    {
        if (!IsPlayerValid) return;

        // When flying, move horizontally to stay above player
        Vector3 targetPosition = new Vector3(
            _playerTransform.position.x,
            transform.position.y,
            transform.position.z
        );

        // Move towards target
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            Speed * 0.5f * Time.deltaTime
        );

        // Shoot fireballs at player
        if (Time.time > lastFireballTime + fireballCooldown)
        {
            ShootFireball();
        }
    }

    private void UpdateGroundedState()
    {
        if (!IsPlayerValid) return;

        // When grounded, move towards player
        Vector3 targetPosition = new Vector3(
            _playerTransform.position.x,
            transform.position.y,
            transform.position.z
        );

        // Check if we should bite or breathe
        if (currentPlayerDistance <= biteRange)
        {
            // Stop moving if in bite range
            if (Time.time > lastBiteTime + biteCooldown)
            {
                PerformBiteAttack();
                return;
            }
        }
        else if (currentPlayerDistance <= breathRange)
        {
            // Stop moving for breath attack
            if (Time.time > lastBreathTime + breathCooldown)
            {
                StartCoroutine(PerformBreathAttack());
                return;
            }
        }

        // Move towards player if not attacking
        if (Vector3.Distance(transform.position, targetPosition) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                Speed * Time.deltaTime
            );
        }
    }

    private void ChangePhase()
    {
        // Toggle between flying and grounded states
        if (currentState == DragonState.Flying)
        {
            StartCoroutine(ChangeHeight(false)); // Move down
        }
        else
        {
            StartCoroutine(ChangeHeight(true)); // Move up
        }

        lastStateChangeTime = Time.time;
    }

    private IEnumerator ChangeHeight(bool moveUp)
    {
        isChangingHeight = true;
        currentState = DragonState.ChangingHeight;

        // Calculate target Y position
        float targetY = moveUp ?
            GetGroundLevel() + flyingHeight : // Flying height
            GetGroundLevel() + 1f;            // Grounded height

        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(transform.position.x, targetY, transform.position.z);

        // Set gravity based on state
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = moveUp ? 0f : 0f; // No gravity in either state for this simplified version
        }

        // Set new state that we're transitioning to
        DragonState newState = moveUp ? DragonState.Flying : DragonState.Grounded;

        // Move to target height
        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure we reached the target
        transform.position = targetPos;
        currentState = newState;
        isChangingHeight = false;

        if (showDebugLogs)
            Debug.Log($"{Name} changed to {newState} state");
    }

    private float GetGroundLevel()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 20f, groundLayer);
        if (hit.collider != null)
        {
            return hit.point.y;
        }
        return 0f; // Default ground level
    }

    private void ShootFireball()
    {
        if (!IsPlayerValid) return;

        lastFireballTime = Time.time;

        if (projectileAttacker != null)
        {
            projectileAttacker.ShootProjectile(_playerTransform, "fireball");

            if (showDebugLogs)
                Debug.Log($"{Name} shot a fireball at player");
        }
    }

    private void PerformBiteAttack()
    {
        if (!IsPlayerValid) return;

        lastBiteTime = Time.time;

        // Trigger animation if animator exists
        if (animator != null)
        {
            animator.SetTrigger("Bite");
        }

        // Apply damage if in range
        if (currentPlayerDistance <= biteRange)
        {
            DamagePlayer(biteDamage);

            // Apply knockback to player
            Rigidbody2D playerRb = _playerObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 knockbackDir = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
                playerRb.AddForce(knockbackDir * 10f, ForceMode2D.Impulse);
            }

            if (showDebugLogs)
                Debug.Log($"{Name} bit player for {biteDamage} damage");
        }
    }

    private IEnumerator PerformBreathAttack()
    {
        if (!IsPlayerValid)
        {
            yield break; // Properly end the coroutine instead of returning
        }

        lastBreathTime = Time.time;

        // Trigger animation if animator exists
        if (animator != null)
        {
            animator.SetTrigger("Breath");
        }

        // Short windup
        yield return new WaitForSeconds(0.5f);

        // Create breath effect
        GameObject breathEffect = null;

        if (breathEffectPrefab != null)
        {
            // Calculate mouth position
            Vector3 mouthPos = transform.position + new Vector3(
                isFacingRight ? Mathf.Abs(mouthOffset.x) : -Mathf.Abs(mouthOffset.x),
                mouthOffset.y,
                0
            );

            breathEffect = Instantiate(breathEffectPrefab, mouthPos, Quaternion.identity);

            // Rotate based on facing
            float angle = isFacingRight ? 0f : 180f;
            breathEffect.transform.rotation = Quaternion.Euler(0, 0, angle);

            // Set parent to follow dragon's mouth
            breathEffect.transform.parent = transform;
        }

        // Apply damage to player if in breath line
        float breathDuration = 2f;
        float damageInterval = 0.25f;
        float elapsed = 0f;
        float lastDamageTime = -damageInterval;

        while (elapsed < breathDuration)
        {
            // Check if it's time to apply damage
            if (Time.time > lastDamageTime + damageInterval && IsPlayerValid)
            {
                // Check if player is in front of dragon
                bool playerInFront = (isFacingRight && _playerTransform.position.x > transform.position.x) ||
                                     (!isFacingRight && _playerTransform.position.x < transform.position.x);

                if (playerInFront && currentPlayerDistance <= breathRange)
                {
                    // Check angle (straight line attack)
                    Vector2 dirToPlayer = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
                    Vector2 forwardDir = isFacingRight ? Vector2.right : Vector2.left;

                    float angle = Vector2.Angle(forwardDir, dirToPlayer);

                    if (angle < 20f) // Narrow cone
                    {
                        DamagePlayer(breathAttackDamage);
                        lastDamageTime = Time.time;

                        if (showDebugLogs)
                            Debug.Log($"{Name} breath hit player for {breathAttackDamage} damage");
                    }
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Clean up breath effect
        if (breathEffect != null)
        {
            Destroy(breathEffect);
        }
    }

    private void DamagePlayer(int damage)
    {
        if (!IsPlayerValid) return;

        // Try different player damage methods
        PlayerController playerController = _playerObject.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(damage);
            return;
        }

        HealthTracker playerHealth = _playerObject.GetComponent<HealthTracker>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            return;
        }

        IDamageable playerDamageable = _playerObject.GetComponent<IDamageable>();
        if (playerDamageable != null)
        {
            playerDamageable.TakeDamage(damage);
        }
    }

    // Override base class EnforceGravity to prevent gravity in flying state
    protected override void EnforceGravity()
    {
        // No gravity enforcement for this boss
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Draw attack ranges
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, biteRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, breathRange);
    }
}