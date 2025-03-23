using UnityEngine;

public class ProjectileAttacker : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float attackCooldown = 1.5f;
    public float attackRange = 10f;
    private float lastAttackTime;
    private Enemy enemyComponent;

    [Header("Projectile Settings")]
    public Vector2 spawnOffset = Vector2.zero; // Default to no offset

    void Awake()
    {
        // Cache the enemy component for better reliability
        enemyComponent = GetComponent<Enemy>();
        if (enemyComponent == null)
        {
            Debug.LogError($"ProjectileAttacker on {gameObject.name} requires an Enemy component!");
        }
    }

    public bool CanAttack(Transform target)
    {
        if (target == null) return false;

        bool cooldownReady = Time.time - lastAttackTime >= attackCooldown;
        float distance = Vector3.Distance(transform.position, target.position);
        bool inRange = distance <= attackRange;

        // Additional line-of-sight check for ranged attackers
        if (cooldownReady && inRange)
        {
            // Check if there's a clear line of sight to the target (no ground in the way)
            Vector2 startPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 endPos = new Vector2(target.position.x, target.position.y);
            Vector2 direction = (endPos - startPos).normalized;

            RaycastHit2D hit = Physics2D.Raycast(
                startPos,
                direction,
                distance,
                LayerMask.GetMask("Ground"));

            if (hit.collider != null)
            {
                Debug.Log($"No line of sight to target - blocked by {hit.collider.name}");
                return false; // Line of sight is blocked
            }
        }

        return cooldownReady && inRange;
    }

    // Overload that doesn't require transform
    public bool CanAttack()
    {
        if (enemyComponent == null) return false;

        bool cooldownReady = Time.time - lastAttackTime >= attackCooldown;

        if (!cooldownReady) return false;

        // Get player position from enemy component
        Vector3 targetPosition = enemyComponent.GetPlayerPosition();
        float distance = Vector3.Distance(transform.position, targetPosition);
        bool inRange = distance <= attackRange;

        // Additional line-of-sight check
        if (inRange)
        {
            // Check if there's a clear line of sight to the target (no ground in the way)
            Vector2 startPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 endPos = new Vector2(targetPosition.x, targetPosition.y);
            Vector2 direction = (endPos - startPos).normalized;

            RaycastHit2D hit = Physics2D.Raycast(
                startPos,
                direction,
                distance,
                LayerMask.GetMask("Ground"));

            if (hit.collider != null)
            {
                Debug.Log($"No line of sight to target - blocked by {hit.collider.name}");
                return false; // Line of sight is blocked
            }
        }

        return inRange;
    }

    public void ShootProjectile(Transform target, string attackName)
    {
        // Get live target position directly
        Vector3 targetPosition;

        if (target != null)
        {
            targetPosition = target.position;
        }
        else if (enemyComponent != null)
        {
            // Fallback to Enemy component's player position
            targetPosition = enemyComponent.GetPlayerPosition();
            Debug.Log($"Using enemy's tracked player position: {targetPosition}");
        }
        else
        {
            Debug.LogError($"ShootProjectile on {gameObject.name} has no valid target!");
            return;
        }

        if (projectilePrefab != null)
        {
            // Check if there's a clear line of sight to the target (no ground in the way)
            Vector2 startPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 endPos = new Vector2(targetPosition.x, targetPosition.y);
            Vector2 direction = (endPos - startPos).normalized;
            float distanceToTarget = Vector2.Distance(startPos, endPos);
            RaycastHit2D hit = Physics2D.Raycast(
                startPos,
                direction,
                distanceToTarget,
                LayerMask.GetMask("Ground"));

            if (hit.collider != null)
            {
                Debug.Log($"Can't shoot {attackName} - target obscured by {hit.collider.name} at distance {hit.distance}");
                return; // Don't shoot if there's something blocking the shot
            }

            // Spawn the projectile directly at the center of the attacker (our fix)
            // Add the optional spawnOffset to fine-tune positioning if needed
            Vector3 spawnPosition = transform.position + new Vector3(spawnOffset.x, spawnOffset.y, 0);

            // Instantiate at the center position
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            // Check if it's an arrow and set direction
            Arrow arrowComponent = projectile.GetComponent<Arrow>();
            if (arrowComponent != null)
            {
                arrowComponent.speed = projectileSpeed;
                arrowComponent.SetDirection(direction);
                Debug.Log($"Arrow direction set to: {direction} targeting position: {targetPosition}");
            }

            // Check if it's a fireball and set direction
            FireballBehavior fireballComponent = projectile.GetComponent<FireballBehavior>();
            if (fireballComponent != null)
            {
                fireballComponent.speed = projectileSpeed;
                fireballComponent.SetDirection(direction);
                Debug.Log($"Fireball direction set to: {direction} targeting position: {targetPosition}");
            }

            // Properly set the rotation based on direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            lastAttackTime = Time.time;
            Debug.Log($"{gameObject.name} shoots {attackName} from position {spawnPosition}!");
        }
    }

    // Visualize the attack range in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}