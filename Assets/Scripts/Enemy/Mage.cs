using UnityEngine;
using System.Collections;
using FMODUnity;
using FMOD.Studio;

public class Mage : Enemy
{
    [Header("Mage Settings")]
    public float levitationHeight = 2f;
    public float hoverAmplitude = 0.2f;
    public float hoverFrequency = 2f;

    [Header("Attack Settings")]
    public float attackRange = 10f;
    public float teleportThreshold = 2f;
    public float teleportDistance = 6f;
    public float castingTime = 1f;
    public float teleportCooldown = 5f;

    private ProjectileAttacker projectileAttacker;
    private float hoverStartTime;
    private bool isTeleporting = false;
    private float lastTeleportTime = -10f;
    private bool isCasting = false;
    private float baseY;
    private Vector3 targetPosition;

    private StudioEventEmitter emitter; // for levitating sfx
    private EventInstance fireballThrow;

    protected override void Start()
    {
        Speed = 4f;
        MaxHealth = 80;
        Name = "Mage";

        base.Start();

        projectileAttacker = GetComponent<ProjectileAttacker>();
        if (projectileAttacker == null)
        {
            projectileAttacker = gameObject.AddComponent<ProjectileAttacker>();
            projectileAttacker.projectilePrefab = Resources.Load<GameObject>("Fireball");
            if (projectileAttacker.projectilePrefab == null)
            {
                Debug.LogWarning("Fireball prefab not found in Resources folder.");
            }

            projectileAttacker.attackCooldown = 2f;
            projectileAttacker.attackRange = attackRange;
            projectileAttacker.projectileSpeed = 8f;
        }

        hoverStartTime = Time.time;
        baseY = transform.position.y;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.mass = 10f;
            rb.linearDamping = 3f;
            rb.angularDamping = 5f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
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

                if (currentPlayerDistance > detectionRadius * 1.5f)
                {
                    Debug.Log($"[Mage] Player moved out of extended detection range ({detectionRadius * 1.5f}), stopping pursuit");
                    _playerTransform = null;
                    _playerObject = null;
                }
            }

            lastPlayerCheckTime = Time.time;
        }

        UpdateRigidbodyState();

        if (IsPlayerValid)
        {
            targetPosition = GetPlayerPosition();

            CheckForTeleport();

            if (isCasting)
                return;

            if (isTeleporting)
                return;

            UpdateFacing();

            if (currentPlayerDistance <= attackRange && currentPlayerDistance <= detectionRadius &&
                projectileAttacker.CanAttack(_playerTransform))
            {
                StartCoroutine(CastFireball());
            }
            else if (currentPlayerDistance <= detectionRadius)
            {
                Fly(targetPosition);
            }
            else
            {
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }
        }
    }

    [System.Obsolete]
    private void UpdateRigidbodyState()
    {
        if (rb == null) return;

        if (isCasting || isTeleporting)
        {
            rb.isKinematic = true;
        }
        else
        {
            rb.isKinematic = false;
        }
    }

    protected override void FindPlayer()
    {
        base.FindPlayer(); // Call base implementation first

        // Check if player is within detection range
        if (IsPlayerValid)
        {
            currentPlayerDistance = Vector3.Distance(transform.position, _playerTransform.position);

            if (currentPlayerDistance > detectionRadius)
            {
                Debug.Log($"[Mage] Found player but they're outside detection radius ({currentPlayerDistance} > {detectionRadius})");
                // Don't acknowledge player is beyond detection radius
                _playerTransform = null;
                _playerObject = null;
            }
            else
            {
                Debug.Log($"[Mage] Found player within detection radius ({currentPlayerDistance} <= {detectionRadius})");
            }
        }
    }

    private void CheckForTeleport()
    {
        if (currentPlayerDistance < teleportThreshold && Time.time > lastTeleportTime + teleportCooldown)
        {
            StartCoroutine(Teleport());
        }
    }

    private IEnumerator Teleport()
    {
        if (isTeleporting)
            yield break;

        isTeleporting = true;
        lastTeleportTime = Time.time;

        if (showDebugLogs)
            Debug.Log($"{Name} is teleporting away from player");

        float startTime = Time.time;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color originalColor = spriteRenderer.color;

        while (Time.time < startTime + 0.5f)
        {
            float t = (Time.time - startTime) / 0.5f;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1 - t);
            yield return null;
        }

        Vector3 directionFromPlayer = (transform.position - _playerTransform.position).normalized;
        Vector3 newPosition = _playerTransform.position + directionFromPlayer * teleportDistance;

        newPosition.y = _playerTransform.position.y + levitationHeight;

        transform.position = newPosition;
        baseY = transform.position.y;

        UpdateFacing();

        startTime = Time.time;

        while (Time.time < startTime + 0.5f)
        {
            float t = (Time.time - startTime) / 0.5f;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, t);
            yield return null;
        }

        spriteRenderer.color = originalColor;

        isTeleporting = false;
    }

    private IEnumerator CastFireball()
    {
        if (isCasting)
            yield break;

        isCasting = true;

        if (animator != null)
        {
            animator.SetTrigger("Cast");
        }

        yield return new WaitForSeconds(castingTime);

        Attack();

        isCasting = false;
    }

    private void Fly(Vector3 targetPos)
    {
        Vector3 desiredPos = new Vector3(
            targetPos.x,
            targetPos.y + levitationHeight,
            transform.position.z
        );

        float hoverOffset = hoverAmplitude * Mathf.Sin(hoverFrequency * (Time.time - hoverStartTime));
        desiredPos.y += hoverOffset;

        if (rb != null)
        {
            Vector2 direction = ((Vector2)desiredPos - (Vector2)transform.position).normalized;
            float distance = Vector2.Distance(transform.position, desiredPos);

            float speedMultiplier = Mathf.Min(distance, 1f);

            rb.AddForce(direction * Speed * 5f * speedMultiplier, ForceMode2D.Force);

            if (rb.linearVelocity.magnitude > Speed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * Speed;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                desiredPos,
                Speed * Time.deltaTime
            );
        }
    }

    protected override void CheckSurroundings()
    {
        base.CheckSurroundings();

        isGrounded = true;
    }

    public override void MoveTowardsTarget(Vector3 targetPosition)
    {
        Fly(targetPosition);
    }

    public override void Attack()
    {
        if (projectileAttacker != null && _playerTransform != null)
        {
            projectileAttacker.ShootProjectile(_playerTransform, "fireball");
            /*AudioManager.instance.PlayOneShot(FMODEvents.instance.MageFireballThrow, this.transform.position);*/

            if (showDebugLogs)
                Debug.Log($"{Name} cast fireball at player!");
        }
    }

    protected override void EnforceGravity()
    {
        return;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector2 pushDirection = ((Vector2)transform.position - (Vector2)collision.transform.position).normalized;

            rb.AddForce(pushDirection * 3f, ForceMode2D.Impulse);

            if (currentPlayerDistance < teleportThreshold && Time.time > lastTeleportTime + teleportCooldown * 0.5f)
            {
                StartCoroutine(Teleport());
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, teleportThreshold);
    }
}