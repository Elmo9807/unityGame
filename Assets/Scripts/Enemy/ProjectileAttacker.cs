using UnityEngine;

public class ProjectileAttacker : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float attackCooldown = 1.5f;
    public float attackRange = 10f;
    private float lastAttackTime;

    public bool CanAttack(Transform target)
    {
        return Time.time - lastAttackTime >= attackCooldown &&
               Vector3.Distance(transform.position, target.position) <= attackRange;
    }

    public void ShootProjectile(Transform target, string attackName)
    {
        if (projectilePrefab != null && target != null)
        {
            // Calculate direction toward target
            Vector3 direction = (target.position - transform.position).normalized;

            // Spawn the arrow at a small offset in the direction it's firing
            Vector3 spawnPosition = transform.position + direction * 0.5f;

            // Instantiate at the offset position
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            // Check if it's an arrow and set direction
            Arrow arrowComponent = projectile.GetComponent<Arrow>();
            if (arrowComponent != null)
            {
                arrowComponent.speed = projectileSpeed;
                arrowComponent.SetDirection(direction);
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            lastAttackTime = Time.time;
            Debug.Log($"{gameObject.name} shoots {attackName}!");
        }
    }

    // Visualize the attack range in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}