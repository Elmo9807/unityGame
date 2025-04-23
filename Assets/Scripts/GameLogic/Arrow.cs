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
    private bool hasHitSomething = false; // Flag to prevent multiple hits

    private void Start()
    {
        startPosition = transform.position;
        spawnTime = Time.time;

        // Set proper rotation based on direction
        UpdateRotation();

        Destroy(gameObject, maxLifetime);

        Debug.Log($"Arrow spawned with damage: {damage}, speed: {speed}, direction: {direction}");
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // Update rotation immediately when direction is set
        UpdateRotation();
    }

    private void UpdateRotation()
    {
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    // Handle both trigger and collision events
    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.collider);
    }

    private void HandleCollision(Collider2D collision)
    {
        // Prevent multiple hits and respect ignore time
        if (hasHitSomething || Time.time - spawnTime < ignoreCollisionsTime)
        {
            return;
        }

        // Debug log all collisions
        Debug.Log($"Arrow collided with: {collision.gameObject.name}, tag: {collision.tag}");

        // Handle player arrows hitting enemies
        if (collision.CompareTag("Enemy"))
        {
            hasHitSomething = true;

            // Try to find the appropriate damage component
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log($"Arrow hit enemy via IDamageable for {damage} damage!");
            }
            else
            {
                Enemy enemy = collision.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"Arrow hit enemy via Enemy component for {damage} damage!");
                }
                else
                {
                    Debug.LogWarning($"Arrow hit enemy '{collision.gameObject.name}' but found no way to apply damage!");
                }
            }

            // Destroy the arrow after hitting an enemy
            Destroy(gameObject);
        }
        // Handle enemy arrows hitting player
        else if (collision.CompareTag("Player"))
        {
            hasHitSomething = true;

            // Try PlayerController first
            PlayerController playerController = collision.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
                Debug.Log($"Arrow hit player via PlayerController for {damage} damage!");
            }
            // Then try HealthTracker
            else
            {
                HealthTracker playerHealth = collision.GetComponent<HealthTracker>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log($"Arrow hit player via HealthTracker for {damage} damage!");
                }
                else
                {
                    Debug.LogWarning($"Arrow hit player but found no way to apply damage!");
                }
            }

            // Destroy the arrow after hitting the player
            Destroy(gameObject);
        }
        // Check if we hit terrain/walls
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                 (!collision.CompareTag("Enemy") && !collision.CompareTag("Player")))
        {
            hasHitSomething = true;
            Debug.Log($"Arrow hit environment: {collision.gameObject.name}");
            Destroy(gameObject); // Destroy the arrow on impact with environment
        }
    }

    // Visualize the arrow's path in the editor
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction * 1f);
        }
    }
}