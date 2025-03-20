using UnityEngine;

public class Grunt : Enemy
{
    public float stabRange = 2f;
    public int stabDamage = 10;
    public float stabCooldown = 1f;
    private float lastStabTime = 0f;
    public GameObject stabEffectPrefab; // Optional visual effect

    private Animator animator; 

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (player != null)
        {
            MoveTowardsTarget(player.position);

            if (Vector3.Distance(transform.position, player.position) <= stabRange)
            {
                TryStab();
            }
        }
    }

    private void TryStab()
    {
        if (Time.time - lastStabTime >= stabCooldown)
        {
            Attack();
            lastStabTime = Time.time;
        }
    }

    public override void Attack()
    {
        
        if (animator != null)
        {
            animator.SetTrigger("Stab");
        }

        // Face the player
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Check if player is still in range (they might have moved during cooldown)
            if (Vector3.Distance(transform.position, player.position) <= stabRange)
            {
                HealthTracker playerHealth = player.GetComponent<HealthTracker>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(stabDamage);
                    Debug.Log($"Grunt stabs the player for {stabDamage} damage!");
                }
                else
                {
                    Debug.LogWarning("Player doesn't have a HealthTracker component!");
                }

                // Spawn stab effect
                if (stabEffectPrefab != null)
                {
                    Vector3 spawnPosition = transform.position + direction * (stabRange * 0.5f);
                    GameObject stabEffect = Instantiate(stabEffectPrefab, spawnPosition, transform.rotation);
                    Destroy(stabEffect, 1f); // Clean up after 1 second
                }

                // Apply knockback to player [if we want this]
                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.AddForce(direction * 5f, ForceMode2D.Impulse);
                }
            }
            else
            {
                Debug.Log("Grunt's stab missed!");
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stabRange);
    }
}