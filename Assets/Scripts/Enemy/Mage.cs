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

    [Header("Collision Settings")]
    public float collisionCheckRadius = 0.5f; // Radius for collision detection
    public float wallAvoidanceForce = 20f; // Force applied to avoid walls

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


    protected override void FixedUpdate()
    {
        base.FixedUpdate();


        if (!isTeleporting && !isCasting && rb != null)
        {
            AvoidWalls();
        }
    }


    private void AvoidWalls()
    {

        float[] rayAngles = { 0, 45, 90, 135, 180, 225, 270, 315 };
        float rayDistance = collisionCheckRadius;
        bool wallDetected = false;
        Vector2 avoidanceDirection = Vector2.zero;

        foreach (float angle in rayAngles)
        {

            float radian = angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));


            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, rayDistance, obstacleLayer);


            if (showDebugLogs)
            {
                Debug.DrawRay(transform.position, direction * rayDistance,
                    hit.collider != null ? Color.red : Color.green, 0.1f);
            }

            if (hit.collider != null)
            {

                wallDetected = true;
                float distanceFactor = 1f - (hit.distance / rayDistance);
                avoidanceDirection += -direction * distanceFactor;

                if (showDebugLogs)
                {
                    Debug.Log($"[Mage] Wall detected at {hit.distance} units in direction {angle} degrees");
                }
            }
        }


        if (wallDetected)
        {
            avoidanceDirection.Normalize();
            rb.AddForce(avoidanceDirection * wallAvoidanceForce, ForceMode2D.Force);

            if (showDebugLogs)
            {
                Debug.Log($"[Mage] Applying avoidance force: {avoidanceDirection * wallAvoidanceForce}");
            }
        }
    }


    private bool IsPointInsideCollider(Vector2 point, Collider2D collider)
    {
        return collider.OverlapPoint(point);
    }

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


        if (IsPlayerValid)
        {
            currentPlayerDistance = Vector3.Distance(transform.position, _playerTransform.position);

            if (currentPlayerDistance > detectionRadius)
            {
                Debug.Log($"[Mage] Found player but they're outside detection radius ({currentPlayerDistance} > {detectionRadius})");

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


        newPosition = FindValidTeleportPosition(newPosition);


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

    private Vector3 FindValidTeleportPosition(Vector3 proposedPosition)
    {

        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            if (showDebugLogs)
                Debug.LogWarning($"{Name} has no Collider2D component for teleport collision check");
            return proposedPosition;
        }


        if (Physics2D.OverlapBox(proposedPosition, collider.bounds.size, 0, obstacleLayer))
        {
            if (showDebugLogs)
                Debug.Log($"{Name} cannot teleport to initial position due to collision, trying alternatives");

            float[] angles = { 0, 30, 60, 90, 120, 150, 180, 210, 240, 270, 300, 330 };
            float[] distances = { teleportDistance, teleportDistance * 0.75f, teleportDistance * 0.5f, teleportDistance * 0.25f };


            foreach (float distance in distances)
            {
                foreach (float angle in angles)
                {
                    float radians = angle * Mathf.Deg2Rad;
                    Vector2 dir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

                    Vector3 testPosition = _playerTransform.position + new Vector3(dir.x, dir.y, 0) * distance;
                    testPosition.y = Mathf.Max(testPosition.y, _playerTransform.position.y + levitationHeight);


                    if (!Physics2D.OverlapBox(testPosition, collider.bounds.size, 0, obstacleLayer))
                    {

                        if (HasLineOfSightTo(testPosition))
                        {
                            if (showDebugLogs)
                                Debug.Log($"{Name} found valid teleport position at angle {angle} and distance {distance}");
                            return testPosition;
                        }
                    }
                }
            }


            if (showDebugLogs)
                Debug.LogWarning($"{Name} could not find a valid teleport position, staying in place");
            return transform.position; // Stay in current position
        }


        if (!HasLineOfSightTo(proposedPosition))
        {
            if (showDebugLogs)
                Debug.Log($"{Name} cannot teleport to initial position due to lack of line of sight");
            return transform.position; // Stay in current position
        }

        return proposedPosition; // Original position was valid
    }


    private bool HasLineOfSightTo(Vector3 targetPos)
    {
        Vector2 direction = (targetPos - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, targetPos);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleLayer);
        return hit.collider == null;
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


        if (IsPlayerValid && HasLineOfSightTo(_playerTransform.position))
        {
            Attack();
        }
        else if (showDebugLogs)
        {
            Debug.Log($"{Name} lost line of sight to player during casting");
        }

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


        if (Physics2D.OverlapPoint(desiredPos, obstacleLayer))
        {

            desiredPos = FindNearestSafePosition(desiredPos);
        }

        if (rb != null)
        {
            Vector2 direction = ((Vector2)desiredPos - (Vector2)transform.position).normalized;
            float distance = Vector2.Distance(transform.position, desiredPos);


            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleLayer);
            if (hit.collider != null)
            {

                direction = Vector2.Reflect(direction, hit.normal).normalized;

                if (showDebugLogs)
                    Debug.Log($"{Name} detected obstacle, adjusting flight path");
            }

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


    private Vector3 FindNearestSafePosition(Vector3 position)
    {

        float[] distances = { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f };
        float[] angles = { 0, 45, 90, 135, 180, 225, 270, 315 };

        foreach (float distance in distances)
        {
            foreach (float angle in angles)
            {
                float radians = angle * Mathf.Deg2Rad;
                Vector2 offset = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * distance;
                Vector3 testPosition = position + new Vector3(offset.x, offset.y, 0);


                if (!Physics2D.OverlapPoint(testPosition, obstacleLayer))
                {
                    return testPosition;
                }
            }
        }


        return transform.position;
    }

    protected override void CheckSurroundings()
    {
        base.CheckSurroundings();

        isGrounded = false;
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
            AudioManager.instance.PlayOneShot(FMODEvents.instance.MageFireballThrow, this.transform.position);

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
        else if ((obstacleLayer.value & (1 << collision.gameObject.layer)) != 0)
        {

            Vector2 pushDirection = ((Vector2)transform.position - (Vector2)collision.contacts[0].point).normalized;
            rb.AddForce(pushDirection * wallAvoidanceForce, ForceMode2D.Impulse);

            if (showDebugLogs)
                Debug.Log($"{Name} is pushing away from obstacle: {collision.gameObject.name}");
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, teleportThreshold);


        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, collisionCheckRadius);
    }
}