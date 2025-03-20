using UnityEngine;

public class Enemy : MonoBehaviour
{
    public string Name;
    public int Health;
    public int MaxHealth = 100;
    public float Speed = 5f;
    public float detectionRadius = 10f;

    public event System.Action OnDeath;
    public delegate void HealthChangeHandler(int currentHealth, int maxHealth);
    public event HealthChangeHandler OnHealthChanged;

    public Transform player;

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found! Please ensure the Player object is tagged with 'Player'.");
        }

        // Initialize health
        Health = MaxHealth;
        OnHealthChanged?.Invoke(Health, MaxHealth);
    }

    protected virtual void Update()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= detectionRadius)
            {
                MoveTowardsTarget(player.position);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        Health -= amount;
        OnHealthChanged?.Invoke(Health, MaxHealth);
        if (Health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    // Base movement method that can be overridden
    public virtual void MoveTowardsTarget(Vector3 targetPosition)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime);
    }

    // Base attack method that can be overridden
    public virtual void Attack()
    {
        // Base implementation does nothing
    }
}

