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

        Destroy(gameObject, maxLifetime);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    private void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
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
            Destroy(gameObject); // Destroy the arrow on impact
        }
    }
}