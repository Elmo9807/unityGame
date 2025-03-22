using UnityEngine;

public class ProjectileAttacker : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float attackCooldown = 1.5f;
    public float attackRange = 10f;
    private float lastAttackTime;
    private Enemy enemyComponent;

    void Awake()
    {
        
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

        
        if (cooldownReady && inRange)
        {
            
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
                return false; 
            }
        }

        return cooldownReady && inRange;
    }

    
    public bool CanAttack()
    {
        if (enemyComponent == null) return false;

        bool cooldownReady = Time.time - lastAttackTime >= attackCooldown;

        if (!cooldownReady) return false;

        
        Vector3 targetPosition = enemyComponent.GetPlayerPosition();
        float distance = Vector3.Distance(transform.position, targetPosition);
        bool inRange = distance <= attackRange;

        
        if (inRange)
        {
            
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
                return false; 
            }
        }

        return inRange;
    }

    public void ShootProjectile(Transform target, string attackName)
    {
        
        Vector3 targetPosition;

        if (target != null)
        {
            targetPosition = target.position;
        }
        else if (enemyComponent != null)
        {
            
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
                return; 
            }

            
            Vector3 dir3D = new Vector3(direction.x, direction.y, 0);
            Vector3 spawnPosition = transform.position + dir3D * 0.5f;

            
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            
            Arrow arrowComponent = projectile.GetComponent<Arrow>();
            if (arrowComponent != null)
            {
                arrowComponent.speed = projectileSpeed;
                arrowComponent.SetDirection(direction);
                Debug.Log($"Arrow direction set to: {direction} targeting position: {targetPosition}");
            }

            
            FireballBehavior fireballComponent = projectile.GetComponent<FireballBehavior>();
            if (fireballComponent != null)
            {
                fireballComponent.speed = projectileSpeed;
                fireballComponent.SetDirection(direction);
                Debug.Log($"Fireball direction set to: {direction} targeting position: {targetPosition}");
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            lastAttackTime = Time.time;
            Debug.Log($"{gameObject.name} shoots {attackName}!");
        }
    }

    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}