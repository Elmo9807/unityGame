using UnityEngine;
using System.Collections;

public class DaBigBoss : Enemy
{
    [Header("Dragon Stats")]
    public int regularFireballDamage = 25;
    public int megaFireballDamage = 40;
    public int biteDamage = 60;
    public int contactDamage = 15; // Damage when player touches the boss

    [Header("Movement Settings")]
    public float flyingHeight = 20f;
    public float flightTransitionSpeed = 4.5f; // Controls how fast the boss flies up/down
    public float groundedHeightOffset = 0.5f; // Offset from ground when grounded

    [Header("Phase Settings")]
    public int phaseChangeDamageThreshold = 100; // Damage for phase change
    public float phaseChangeCooldown = 2f;

    [Header("Attack Settings")]
    public float regularFireballCooldown = 2.0f;
    public float megaFireballCooldown = 3.5f;
    public float biteCooldown = 1.5f;
    public float biteRange = 2.5f;
    public float playerKnockbackForce = 15f;

    [Header("Projectile Settings")]
    public GameObject regularFireballPrefab;
    public GameObject megaFireballPrefab;
    public Vector2 mouthOffset = new Vector2(1.5f, 0.2f);
    public float megaFireballScale = 1.3f;
    public float fireballVisualScale = 3.0f; // Base scale factor to match collider size with visual

    [Header("Ground Detection")]
    public float bossGroundCheckDistance = 5f;
    public bool debugGroundDetection = true;


    private enum DragonState
    {
        Grounded,
        Flying,
        ChangingHeight
    }


    private float lastRegularFireballTime = -10f;
    private float lastMegaFireballTime = -10f;
    private float lastBiteTime = -10f;
    private float lastStateChangeTime = -10f;
    private DragonState currentState = DragonState.Grounded;
    private bool isChangingHeight = false;
    private int damageSinceLastPhaseChange = 0;
    private bool isFlying => currentState == DragonState.Flying;
    private bool isMega;


    private float lastGroundLevel;
    private float groundCheckTimer = 0f;
    private float groundCheckInterval = 0.5f;
    private float targetFlightHeight; // Target Y position when flying


    private ProjectileAttacker projectileAttacker;
    private BoxCollider2D bossCollider;

    protected override void Start()
    {

        Name = "Dragon Boss";
        MaxHealth = 1500;
        Health = MaxHealth;
        Speed = 3f;

        base.Start();


        bossCollider = GetComponent<BoxCollider2D>();
        if (bossCollider == null)
        {
            bossCollider = gameObject.AddComponent<BoxCollider2D>();
            bossCollider.size = new Vector2(3f, 3f);
        }


        AdjustColliderToSprite();


        SetupProjectileAttacker();


        SetupPlayerCollisions();


        currentState = DragonState.Grounded;
        damageSinceLastPhaseChange = 0;


        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }


        UpdateSpriteDirection();


        StartCoroutine(InitialGroundPositioning());
    }

    private void AdjustColliderToSprite()
    {
        if (bossCollider == null) return;


        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {

            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            bossCollider.size = new Vector2(spriteSize.x * 0.8f, spriteSize.y * 0.8f);

            if (debugGroundDetection)
                Debug.Log($"{Name} collider size set to {bossCollider.size} based on sprite");
        }
        else
        {

            bossCollider.size = new Vector2(4f, 4f);
        }
    }

    private IEnumerator InitialGroundPositioning()
    {

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();


        currentState = DragonState.Grounded;
        isChangingHeight = false;


        bool foundGround = false;
        float searchDistance = bossGroundCheckDistance;

        for (int attempt = 0; attempt < 5; attempt++)
        {

            if (debugGroundDetection)
                Debug.Log($"{Name} searching for ground (attempt {attempt + 1}) with distance {searchDistance}");


            lastGroundLevel = GetGroundLevel(searchDistance);

            if (lastGroundLevel > -9000) // Ground found
            {
                foundGround = true;


                float groundedY = lastGroundLevel + (bossCollider.size.y / 2) + groundedHeightOffset;


                transform.position = new Vector3(transform.position.x, groundedY, transform.position.z);

                if (debugGroundDetection)
                    Debug.Log($"{Name} positioned on ground at Y={groundedY}, ground at Y={lastGroundLevel}");

                break;
            }


            searchDistance += 3f;


            yield return new WaitForSeconds(0.1f);
        }

        if (!foundGround)
        {
            Debug.LogWarning($"{Name} couldn't find ground after multiple attempts!");


            lastGroundLevel = transform.position.y - (bossCollider.size.y / 2) - 1f;


            currentState = DragonState.Grounded;
        }


        Invoke("ForceGroundPosition", 1.0f);
    }

    private void ForceGroundPosition()
    {
        if (currentState == DragonState.Grounded && !isChangingHeight)
        {
            float groundedY = lastGroundLevel + (bossCollider.size.y / 2) + groundedHeightOffset;
            transform.position = new Vector3(transform.position.x, groundedY, transform.position.z);
            Debug.Log($"{Name} position forced to ground at Y={groundedY}");
        }
    }

    private void SetupProjectileAttacker()
    {
        projectileAttacker = GetComponent<ProjectileAttacker>();
        if (projectileAttacker == null)
        {
            projectileAttacker = gameObject.AddComponent<ProjectileAttacker>();


            if (regularFireballPrefab == null)
            {
                regularFireballPrefab = Resources.Load<GameObject>("Fireball");
                if (regularFireballPrefab == null)
                    Debug.LogWarning("Regular fireball prefab not found in Resources!");
            }

            if (megaFireballPrefab == null)
            {
                megaFireballPrefab = Resources.Load<GameObject>("MegaFireball");

            }

            projectileAttacker.projectilePrefab = regularFireballPrefab;
            projectileAttacker.attackCooldown = regularFireballCooldown;
            projectileAttacker.attackRange = 20f;
            projectileAttacker.projectileSpeed = 8f;
            projectileAttacker.spawnOffset = mouthOffset;
        }
    }

    private void SetupPlayerCollisions()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {

            Collider2D bossCollider = GetComponent<Collider2D>();
            if (bossCollider != null)
            {

                bossCollider.isTrigger = false;

                if (debugGroundDetection)
                    Debug.Log($"{Name} collision with player enabled");
            }
        }
    }

    protected override void Update()
    {

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


        groundCheckTimer += Time.deltaTime;
        if (groundCheckTimer >= groundCheckInterval)
        {
            if (currentState == DragonState.Grounded)
                CheckGround();
            groundCheckTimer = 0f;
        }


        if (IsPlayerValid)
        {
            UpdateFacing();
            UpdateMouthOffset();
        }


        if (currentState == DragonState.Flying && !isChangingHeight)
        {
            SmoothUpdateFlightHeight();
        }


        if (isChangingHeight) return;


        if (isFlying)
            UpdateFlyingAttacks();
        else
            UpdateGroundedAttacks();
    }

    protected override void FixedUpdate()
    {
        animator.SetInteger("DragonState", (int) currentState); // 0 = Grounded, 1 = Flying, 2 = Changing Height
        base.FixedUpdate();
    }


    private void SmoothUpdateFlightHeight()
    {
        if (lastGroundLevel > -9000)
        {
            targetFlightHeight = lastGroundLevel + flyingHeight;


            float currentY = transform.position.y;
            float targetY = targetFlightHeight;


            if (Mathf.Abs(currentY - targetY) > 0.1f)
            {
                float newY = Mathf.Lerp(currentY, targetY, flightTransitionSpeed * Time.deltaTime);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
        }
    }

    private void UpdateMouthOffset()
    {
        if (projectileAttacker != null)
        {
            projectileAttacker.spawnOffset = new Vector2(
                isFacingRight ? Mathf.Abs(mouthOffset.x) : -Mathf.Abs(mouthOffset.x),
                mouthOffset.y
            );
        }
    }

    private void CheckGround()
    {

        if (currentState != DragonState.Grounded || isChangingHeight)
            return;

        float groundLevel = GetGroundLevel();
        if (groundLevel > -9000)
        {
            lastGroundLevel = groundLevel;


            float desiredY = groundLevel + (bossCollider.size.y / 2) + groundedHeightOffset;


            if (Mathf.Abs(transform.position.y - desiredY) > 0.5f)
            {
                transform.position = Vector3.Lerp(
                    transform.position,
                    new Vector3(transform.position.x, desiredY, transform.position.z),
                    0.2f
                );
            }
        }
    }

    private void UpdateFlyingAttacks()
    {
        if (!IsPlayerValid) return;


        if (Time.time > lastRegularFireballTime + regularFireballCooldown)
        {
            ShootRegularFireball();
        }
    }

    private void UpdateGroundedAttacks()
    {
        if (!IsPlayerValid) return;


        if (currentPlayerDistance <= biteRange)
        {
            if (Time.time > lastBiteTime + biteCooldown)
            {
                PerformBiteAttack();
            }
        }

        else if (Time.time > lastMegaFireballTime + megaFireballCooldown)
        {
            ShootMegaFireball();
        }
    }


    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);


        damageSinceLastPhaseChange += amount;


        if (debugGroundDetection)
            Debug.Log($"{Name} damage since last phase: {damageSinceLastPhaseChange}/{phaseChangeDamageThreshold}");


        if (damageSinceLastPhaseChange >= phaseChangeDamageThreshold &&
            Time.time >= lastStateChangeTime + phaseChangeCooldown &&
            !isChangingHeight)
        {
            ChangePhase();
        }
    }

    private void ChangePhase()
    {

        damageSinceLastPhaseChange = 0;
        lastStateChangeTime = Time.time;


        bool shouldFly = (currentState == DragonState.Grounded);

        if (debugGroundDetection)
            Debug.Log($"{Name} changing to {(shouldFly ? "flying" : "grounded")} phase");


        StartCoroutine(ChangeHeight(shouldFly));
    }

    private IEnumerator ChangeHeight(bool shouldFly)
    {
        isChangingHeight = true;


        if (lastGroundLevel < -9000)
        {
            float newGroundLevel = GetGroundLevel();
            if (newGroundLevel > -9000)
                lastGroundLevel = newGroundLevel;
            else
                lastGroundLevel = transform.position.y - (bossCollider.size.y / 2) - 1f;
        }


        float targetY = shouldFly
            ? lastGroundLevel + flyingHeight // Flying height
            : lastGroundLevel + (bossCollider.size.y / 2) + groundedHeightOffset; // Grounded


        float startY = transform.position.y;
        float totalDistance = Mathf.Abs(targetY - startY);
        float totalDuration = totalDistance / (flyingHeight / 2.0f); // Adjust total time based on distance


        totalDuration = Mathf.Clamp(totalDuration, 2.0f, 4.0f);

        float timeElapsed = 0f;


        while (timeElapsed < totalDuration)
        {

            float t = timeElapsed / totalDuration;
            float easedT = t < 0.5f ? 2.0f * t * t : 1.0f - Mathf.Pow(-2.0f * t + 2.0f, 2.0f) / 2.0f;


            float newY = Mathf.Lerp(startY, targetY, easedT);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            timeElapsed += Time.deltaTime;
            yield return null;
        }


        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);


        targetFlightHeight = targetY;


        currentState = shouldFly ? DragonState.Flying : DragonState.Grounded;
        isChangingHeight = false;
    }

    private float GetGroundLevel(float customCheckDistance = -1f)
    {
        float checkDistance = (customCheckDistance > 0) ? customCheckDistance : bossGroundCheckDistance;


        if (debugGroundDetection)
        {
            Debug.DrawRay(transform.position, Vector2.down * checkDistance, Color.red, 0.5f);
        }


        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            checkDistance,
            groundLayer
        );

        if (hit.collider != null)
        {
            if (debugGroundDetection)
                Debug.Log($"{Name} found ground at Y={hit.point.y}");
            return hit.point.y;
        }


        for (float offset = 0.5f; offset <= 3f; offset += 0.5f)
        {

            for (float direction = -1f; direction <= 1f; direction += 2f)
            {
                Vector2 startPos = (Vector2)transform.position + new Vector2(offset * direction, 0);
                hit = Physics2D.Raycast(startPos, Vector2.down, checkDistance * 2, groundLayer);

                if (hit.collider != null)
                    return hit.point.y;
            }
        }

        return -9999f; // No ground found
    }

    private void ShootRegularFireball()
    {
        if (!IsPlayerValid) return;

        lastRegularFireballTime = Time.time;
        isMega = false;
        animator.SetTrigger("Fireball"); // CreateRegularFireball handled in animation event
    }

    private void CreateRegularFireball(GameObject prefab)
    {
        Vector3 targetPosition = _playerTransform.position;
        Vector2 startPos = transform.position + new Vector3(
            isFacingRight ? Mathf.Abs(mouthOffset.x) : -Mathf.Abs(mouthOffset.x),
            mouthOffset.y,
            0
        );
        Vector2 direction = ((Vector2)targetPosition - startPos).normalized;


        GameObject projectile = Instantiate(regularFireballPrefab, startPos, Quaternion.identity);
        FireballSound();

        float regularFireballScale = 2.5f; // Reduced from 4.0f
        projectile.transform.localScale = new Vector3(regularFireballScale, regularFireballScale, 1f);


        CircleCollider2D circleCollider = projectile.GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {

            SpriteRenderer spriteRenderer = projectile.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {

                Vector2 spriteSize = spriteRenderer.sprite.bounds.size;


                float spriteBoundsRadius = (spriteSize.x + spriteSize.y) / 4f;


                circleCollider.radius = spriteBoundsRadius;

                if (debugGroundDetection)
                    Debug.Log($"Regular fireball sprite size: {spriteSize}, setting collider radius to {spriteBoundsRadius}");
            }
        }


        FireballBehavior fireballBehavior = projectile.GetComponent<FireballBehavior>();
        if (fireballBehavior != null)
        {
            fireballBehavior.damage = regularFireballDamage;


            float speedMultiplier = 1.5f; // 50% faster than normal
            fireballBehavior.speed = projectileAttacker.projectileSpeed * speedMultiplier;

            fireballBehavior.SetDirection(direction);
        }
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    private void ShootMegaFireball()
    {
        if (!IsPlayerValid) return;

        lastMegaFireballTime = Time.time;
        isMega = true;
        animator.SetTrigger("Fireball"); // CreateMegaFireball handled in animation event
    }

    private void CreateMegaFireball(GameObject prefab)
    {

        Vector3 targetPosition = _playerTransform.position;
        Vector2 startPos = transform.position + new Vector3(
            isFacingRight ? Mathf.Abs(mouthOffset.x) : -Mathf.Abs(mouthOffset.x),
            mouthOffset.y,
            0
        );
        Vector2 direction = ((Vector2)targetPosition - startPos).normalized;


        GameObject projectile = Instantiate(prefab, startPos, Quaternion.identity);
        FireballSound();


        CircleCollider2D circleCollider = projectile.GetComponent<CircleCollider2D>();


        float visualScale = fireballVisualScale * megaFireballScale;


        projectile.transform.localScale = new Vector3(visualScale, visualScale, 1f);


        if (circleCollider != null)
        {

            SpriteRenderer spriteRenderer = projectile.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {

                Vector2 spriteSize = spriteRenderer.sprite.bounds.size;


                float spriteBoundsRadius = (spriteSize.x + spriteSize.y) / 4f;



                circleCollider.radius = spriteBoundsRadius;

                if (debugGroundDetection)
                    Debug.Log($"Mega fireball sprite size: {spriteSize}, setting collider radius to {spriteBoundsRadius}");
            }
            else
            {

                float originalRadius = circleCollider.radius;
                circleCollider.radius = originalRadius / visualScale;

                if (debugGroundDetection)
                    Debug.Log($"No sprite found, setting collider radius using fallback method: {circleCollider.radius}");
            }
        }


        FireballBehavior fireballBehavior = projectile.GetComponent<FireballBehavior>();
        if (fireballBehavior != null)
        {
            fireballBehavior.damage = megaFireballDamage;
            fireballBehavior.speed = projectileAttacker.projectileSpeed * 0.8f;
            fireballBehavior.SetDirection(direction);
        }


        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    private void FireballSound()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.DragonFireballThrow, this.transform.position);
    }

    private void FireballAnimationEvent()
    {
        if (isMega)
        {
            if (megaFireballPrefab != null)
                CreateMegaFireball(megaFireballPrefab);
            else if (regularFireballPrefab != null)
                CreateMegaFireball(regularFireballPrefab);
        }
        else
        {
            if (megaFireballPrefab != null)
            {
                CreateRegularFireball(regularFireballPrefab);
            }
        }

    }
    private void PerformBiteAttack()
    {
        if (!IsPlayerValid) return;

        lastBiteTime = Time.time;


        if (animator != null)
            animator.SetTrigger("Bite");


        if (currentPlayerDistance <= biteRange)
        {

            DamagePlayer(biteDamage);


            ApplyKnockbackToPlayer(playerKnockbackForce);

            if (debugGroundDetection)
                Debug.Log($"{Name} bit player for {biteDamage} damage with knockback");
        }
    }

    private void ApplyKnockbackToPlayer(float force)
    {
        if (!IsPlayerValid) return;

        Rigidbody2D playerRb = _playerObject.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {

            Vector2 knockbackDir = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;


            knockbackDir += Vector2.up * 0.3f;
            knockbackDir.Normalize();


            playerRb.linearVelocity = Vector2.zero;
            playerRb.AddForce(knockbackDir * force, ForceMode2D.Impulse);
        }
    }

    private void DamagePlayer(int damage)
    {
        if (!IsPlayerValid) return;


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

    protected override void UpdateFacing()
    {
        if (!IsPlayerValid) return;

        float xDistance = _playerTransform.position.x - transform.position.x;
        float facingThreshold = 0.5f;

        if (Mathf.Abs(xDistance) > facingThreshold)
        {
            bool shouldFaceRight = xDistance > 0;

            if (isFacingRight != shouldFaceRight)
            {
                isFacingRight = shouldFaceRight;
                UpdateSpriteDirection();
            }
        }
    }

    private void UpdateSpriteDirection()
    {

        bool spriteInitiallyFacingLeft = true;
        bool shouldFlipX = spriteInitiallyFacingLeft ? !isFacingRight : isFacingRight;

        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * (shouldFlipX ? -1 : 1);
        transform.localScale = localScale;
    }


    private new void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            DamagePlayer(contactDamage);
            ApplyKnockbackToPlayer(playerKnockbackForce * 0.5f);

            if (debugGroundDetection)
                Debug.Log($"Player collided with {Name}, applied {contactDamage} contact damage");
        }
    }


    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ApplyKnockbackToPlayer(playerKnockbackForce * 0.2f);
        }
    }


    protected override void EnforceGravity()
    {

    }

    protected override void Die()
    {
        base.Die();
        AudioManager.instance.SetMusicArea( (MusicArea) 3); // Transitions to outro of bgm when dragon dies
    }

    private void CallFlapSound()
    {
        if (currentState == 0) // if grounded, play idle flaps
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.DragonIdle, this.transform.position);
        }
        else if (currentState > 0)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.DragonFlying, this.transform.position);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();


        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, biteRange);


        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * bossGroundCheckDistance);


        if (Application.isPlaying && lastGroundLevel > -9000)
        {
            Gizmos.color = Color.cyan;
            Vector3 flightPos = new Vector3(transform.position.x, lastGroundLevel + flyingHeight, transform.position.z);
            Gizmos.DrawWireSphere(flightPos, 0.5f);
        }
    }
}