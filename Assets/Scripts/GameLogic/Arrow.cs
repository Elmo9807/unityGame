using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float maxLifetime = 5f;
    public float maxDistance = 30f;
    public float speed = 10f;
    public int damage = 10;
    private Vector2 direction;
    private Vector3 startPosition;
    private float ignoreCollisionsTime = 0.1f; // Ignore collisions for this many seconds after spawning, helps avoid arrow getting deleted too soon
    private float spawnTime;

    private void Start()
    {
        startPosition = transform.position;
        spawnTime = Time.time;

        Debug.Log($"Arrow spawned at {startPosition} with direction {direction}");

        Destroy(gameObject, maxLifetime);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // Set the rotation immediately to match the direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Debug.Log($"Arrow direction set to {direction}, angle: {angle}");
    }

    private void Update()
    {
        // Move the arrow in its set direction
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

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
        // Ignore collisions for a brief period after spawning to prevent hitting the archer
        if (Time.time - spawnTime < ignoreCollisionsTime)
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            HealthTracker playerHealth = collision.GetComponent<HealthTracker>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Arrow hit player for {damage} damage!");
            }

            Destroy(gameObject); // Destroy the arrow on impact
        }
        // Check if we hit anything besides an enemy
        else if (!collision.CompareTag("Enemy"))
        {
            Debug.Log($"Arrow hit non-player object: {collision.gameObject.name}");
            Destroy(gameObject); // Destroy the arrow on impact
        }
    }
}