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
        if (target == null) return false;

        // Always get the most current player position for range check
        GameObject currentPlayer = GameObject.FindGameObjectWithTag("Player");
        Vector3 targetPosition = currentPlayer != null ? currentPlayer.transform.position : target.position;

        // Calculate distance to current player position
        float currentDistance = Vector3.Distance(transform.position, targetPosition);

        return Time.time - lastAttackTime >= attackCooldown &&
               currentDistance <= attackRange;
    }

    public void ShootProjectile(Transform target, string attackName)
    {
        if (projectilePrefab != null && target != null)
        {
            // IMPORTANT: Force a refresh of the player position
            GameObject currentPlayer = GameObject.FindGameObjectWithTag("Player");
            Vector3 currentTargetPosition;

            if (currentPlayer != null)
            {
                // Use the current active player instead of the cached reference
                currentTargetPosition = currentPlayer.transform.position;
                Debug.Log($"Using REFRESHED player position: {currentTargetPosition}");
            }
            else
            {
                // Fallback to the provided transform
                currentTargetPosition = target.position;
                Debug.Log($"Using provided target position: {currentTargetPosition}");
            }

            // Calculate direction toward current target position
            Vector3 direction = (currentTargetPosition - transform.position).normalized;

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
                Debug.Log($"Arrow direction set to: {direction} targeting position: {currentTargetPosition}");
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