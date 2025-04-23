using UnityEngine;

public class FireballBehaviour : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 20;
    public float lifetime = 3f;

    private Vector2 direction;
    private Vector3 startPosition;

    void Start()
    {

        startPosition = transform.position;

        Destroy(gameObject, lifetime);

        Debug.Log("Fireball spawned and moving in direction: " + direction);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (Random.value > 0.8f)
        {
            // UNITY PARTICLE EFFECTS PACKAGE PLACEHOLDER, I'LL GET TO IT EVENTUALLY
        }
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // Set the rotation to match the direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if hitting player
        if (collision.CompareTag("Player"))
        {
            // Try to damage player
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log("Fireball hit player for " + damage + " damage!");
            }
            else
            {
                // Fallback to standard health tracking
                HealthTracker playerHealth = collision.GetComponent<HealthTracker>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log("Fireball hit player through HealthTracker for " + damage + " damage!");
                }
            }

            // Destroy fireball on hit
            Destroy(gameObject);
        }
        // Check if hitting anything else like walls [EXCL ENEMIES]
        else if (!collision.CompareTag("Enemy"))
        {
            // Destroy fireball on hitting walls or other obstacles
            Destroy(gameObject);
        }
    }
}
