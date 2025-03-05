using UnityEngine;

public class EnemyRangedAttack : MonoBehaviour
{
    public GameObject projectilePrefab; // The projectile prefab
    public Transform firePoint; // Where the projectile spawns
    public float projectileSpeed = 5f; // Do I really need to explain this?
    public float attackCooldown = 2f; // Cooldown
    public float shootRange = 10f; // Max range

    private float lastAttackTime;
    private Transform playerTransform;

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (playerTransform != null)
        {
            if (Vector2.Distance(transform.position, playerTransform.position) <= shootRange) //range checker
            {
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    Shoot();
                    lastAttackTime = Time.time;
                }
            }
        }
        else
        {
            Debug.LogWarning("Player object not found.");
        }
    }

            private void Shoot()
            {
                GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity); // Spawn projectile at the fire point
        RangedAttack projectileScript = projectile.GetComponent<RangedAttack>();

                if (projectileScript != null)
                {                   
                    Vector2 direction = (playerTransform.position - firePoint.position).normalized; // Calculate direction from the enemy to the player
            projectileScript.SetDirection(direction);
                }
            }
}
