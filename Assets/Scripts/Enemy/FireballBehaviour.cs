using UnityEngine;

public class FireballBehavior : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 20;
    public float lifetime = 3f;

    private Vector2 direction;
    private Vector3 startPosition;
    private float spawnTime;
    private float ignoreCollisionsTime = 0.1f; // Brief immunity period to prevent colliding with caster

    void Start()
    {
        startPosition = transform.position;
        spawnTime = Time.time;

        // Add a light component for glow effect
        AddFireballGlowEffect();

        // Add particle effect for fire trail
        AddFireTrailEffect();

        Destroy(gameObject, lifetime);

        Debug.Log("Fireball spawned and moving in direction: " + direction);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        // Rotate slightly to make it look more dynamic
        transform.Rotate(0, 0, 120 * Time.deltaTime);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore collisions briefly after spawning
        if (Time.time - spawnTime < ignoreCollisionsTime)
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            // Try to find the most appropriate damage component
            PlayerController playerController = collision.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
                Debug.Log("Fireball hit player via PlayerController for " + damage + " damage!");
            }
            else
            {
                IDamageable damageable = collision.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                    Debug.Log("Fireball hit player via IDamageable for " + damage + " damage!");
                }
                else
                {
                    HealthTracker playerHealth = collision.GetComponent<HealthTracker>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(damage);
                        Debug.Log("Fireball hit player via HealthTracker for " + damage + " damage!");
                    }
                }
            }

            // Create impact effect
            CreateImpactEffect();
            /*AudioManager.instance.PlayOneShot(FMODEvents.instance.MageFireballExplosion, this.transform.position); */
            // Destroy fireball
            Destroy(gameObject);
        }
        // Fixed here - check for ground layer instead of tag
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                (!collision.CompareTag("Enemy") && !collision.CompareTag("Player")))
        {
            // Create impact effect on walls/ground/other objects
            CreateImpactEffect();
            /*AudioManager.instance.PlayOneShot(FMODEvents.instance.MageFireballExplosion, this.transform.position);*/
            Destroy(gameObject);
        }
    }

    private void AddFireballGlowEffect()
    {
        // Add a point light if not in 2D mode
        GameObject lightObject = new GameObject("FireballLight");
        lightObject.transform.parent = transform;
        lightObject.transform.localPosition = Vector3.zero;

        // If using Universal RP, you'd use a 2D light instead
        // For simplicity we'll just add a sprite for the glow
        GameObject glowObject = new GameObject("GlowSprite");
        glowObject.transform.parent = transform;
        glowObject.transform.localPosition = Vector3.zero;

        SpriteRenderer glowRenderer = glowObject.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = GetComponent<SpriteRenderer>().sprite;
        glowRenderer.color = new Color(1f, 0.7f, 0.3f, 0.5f);
        glowRenderer.sortingOrder = -1; // Behind the main sprite
        glowObject.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
    }

    private void AddFireTrailEffect()
    {
        // Add a simple particle system for the trail
        ParticleSystem trailParticles = gameObject.AddComponent<ParticleSystem>();
        var main = trailParticles.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 0f;
        main.startSize = 0.2f;
        main.startColor = new Color(1f, 0.5f, 0.2f, 0.7f);

        var emission = trailParticles.emission;
        emission.rateOverTime = 20f;

        var shape = trailParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;
    }

    private void CreateImpactEffect()
    {
        // Create a simple flash effect
        GameObject impactObject = new GameObject("FireballImpact");
        impactObject.transform.position = transform.position;

        SpriteRenderer impactRenderer = impactObject.AddComponent<SpriteRenderer>();
        impactRenderer.sprite = GetComponent<SpriteRenderer>().sprite;
        impactRenderer.color = new Color(1f, 0.7f, 0.1f, 0.8f);

        // Scale up and fade out
        impactObject.AddComponent<FireballImpactEffect>();

        // Clean up after a short time
        Destroy(impactObject, 0.5f);
    }
}

// Small utility class for the impact effect
public class FireballImpactEffect : MonoBehaviour
{
    private float startTime;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        startTime = Time.time;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float elapsed = Time.time - startTime;
        float normalizedTime = elapsed / 0.5f; // 0.5 second duration

        // Scale up
        transform.localScale = Vector3.one * (1f + normalizedTime * 2f);

        // Fade out
        Color color = spriteRenderer.color;
        color.a = Mathf.Lerp(0.8f, 0f, normalizedTime);
        spriteRenderer.color = color;
    }
}