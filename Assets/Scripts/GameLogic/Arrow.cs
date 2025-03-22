using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float maxLifetime = 5f;
    public float maxDistance = 30f;
    public float speed = 10f;
    public int damage = 10;
    private Vector2 direction;
    private Vector3 startPosition;
    private float ignoreCollisionsTime = 0.1f; // Ignore collisions for this many seconds after spawning
    private float spawnTime;

    private void Start()
    {
        startPosition = transform.position;
        spawnTime = Time.time;

        // Set a lifetime to prevent arrows from flying forever
        Destroy(gameObject, maxLifetime);

        Debug.Log($"Arrow spawned at {transform.position} with direction {direction}");
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // Calculate angle for proper rotation
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Debug.Log($"Arrow direction set to {direction}, angle: {angle}");
    }

    private void Update()
    {
        // Move arrow based on direction and speed
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        // Check if arrow has traveled too far
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled > maxDistance)
        {
            Debug.Log($"Arrow traveled max distance ({distanceTraveled}/{maxDistance}) and was destroyed");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Avoid immediate collisions with the shooter
        if (Time.time - spawnTime < ignoreCollisionsTime)
        {
            return;
        }

        // Check if we hit player
        if (collision.CompareTag("Player"))
        {
            // Try to get a HealthTracker component first
            HealthTracker playerHealth = collision.GetComponent<HealthTracker>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Arrow hit player for {damage} damage!");
            }
            else
            {
                // Fall back to PlayerController
                PlayerController playerController = collision.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.TakeDamage(damage);
                    Debug.Log($"Arrow hit player via PlayerController for {damage} damage!");
                }
                else
                {
                    // Last resort, try IDamageable
                    IDamageable damageable = collision.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(damage);
                        Debug.Log($"Arrow hit player via IDamageable for {damage} damage!");
                    }
                    else
                    {
                        Debug.LogError($"Arrow hit player but found no way to apply damage!");
                    }
                }
            }

            Destroy(gameObject); // Destroy the arrow on impact
        }
        // Check if we hit anything besides an enemy
        else if (!collision.CompareTag("Enemy"))
        {
            Debug.Log($"Arrow hit {collision.gameObject.name} and was destroyed");
            Destroy(gameObject); // Destroy the arrow on impact
        }
    }
}