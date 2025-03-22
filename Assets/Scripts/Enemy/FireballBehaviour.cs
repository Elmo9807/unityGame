using UnityEngine;

public class FireballBehavior : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 20;
    public float lifetime = 3f;

    private Vector2 direction;
    private Vector3 startPosition;
    private float spawnTime;
    private float ignoreCollisionsTime = 0.1f; 

    void Start()
    {
        startPosition = transform.position;
        spawnTime = Time.time;

        
        AddFireballGlowEffect();

        
        AddFireTrailEffect();

        Destroy(gameObject, lifetime);

        Debug.Log("Fireball spawned and moving in direction: " + direction);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        
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
        
        if (Time.time - spawnTime < ignoreCollisionsTime)
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            
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

            
            CreateImpactEffect();

            
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground") || (!collision.CompareTag("Enemy")))
        {
            
            CreateImpactEffect();

            Destroy(gameObject);
        }
    }

    private void AddFireballGlowEffect()
    {
        
        GameObject lightObject = new GameObject("FireballLight");
        lightObject.transform.parent = transform;
        lightObject.transform.localPosition = Vector3.zero;

        
        
        GameObject glowObject = new GameObject("GlowSprite");
        glowObject.transform.parent = transform;
        glowObject.transform.localPosition = Vector3.zero;

        SpriteRenderer glowRenderer = glowObject.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = GetComponent<SpriteRenderer>().sprite;
        glowRenderer.color = new Color(1f, 0.7f, 0.3f, 0.5f);
        glowRenderer.sortingOrder = -1; 
        glowObject.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
    }

    private void AddFireTrailEffect()
    {
        
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
        
        GameObject impactObject = new GameObject("FireballImpact");
        impactObject.transform.position = transform.position;

        SpriteRenderer impactRenderer = impactObject.AddComponent<SpriteRenderer>();
        impactRenderer.sprite = GetComponent<SpriteRenderer>().sprite;
        impactRenderer.color = new Color(1f, 0.7f, 0.1f, 0.8f);

        
        impactObject.AddComponent<FireballImpactEffect>();

        
        Destroy(impactObject, 0.5f);
    }
}


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
        float normalizedTime = elapsed / 0.5f; 

        
        transform.localScale = Vector3.one * (1f + normalizedTime * 2f);

        
        Color color = spriteRenderer.color;
        color.a = Mathf.Lerp(0.8f, 0f, normalizedTime);
        spriteRenderer.color = color;
    }
}