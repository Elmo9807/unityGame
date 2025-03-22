using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public string Name;
    public int Health;
    public int MaxHealth = 100;
    public float Speed = 5f;
    public float detectionRadius = 10f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    public event System.Action OnDeath;
    public delegate void HealthChangeHandler(int currentHealth, int maxHealth);
    public event HealthChangeHandler OnHealthChanged;

    public Transform player;

    protected virtual void Start()
    {
        FindPlayer();

        // Initialize health
        Health = MaxHealth;
        OnHealthChanged?.Invoke(Health, MaxHealth);

        if (showDebugLogs)
            Debug.Log($"{Name} initialized with {Health} health");
    }

    protected virtual void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius)
        {
            MoveTowardsTarget(player.position);
        }
    }

    public virtual void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        Health -= amount;

        if (showDebugLogs)
            Debug.Log($"{Name} took {amount} damage. Health: {Health}/{MaxHealth}");

        OnHealthChanged?.Invoke(Health, MaxHealth);

        if (Health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (showDebugLogs)
            Debug.Log($"{Name} has been defeated!");

        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    protected void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
                if (showDebugLogs)
                    Debug.Log($"{Name} found player: {player.name}");
            }
            else
            {
                Debug.LogWarning($"{Name} couldn't find player! Retrying next frame...");
            }
        }
    }

    // Base movement method that can be overridden
    public virtual void MoveTowardsTarget(Vector3 targetPosition)
    {
        // Always refresh player reference to get current position
        if (targetPosition == player?.position)
        {
            GameObject currentPlayer = GameObject.FindGameObjectWithTag("Player");
            if (currentPlayer != null)
            {
                targetPosition = currentPlayer.transform.position;
                Debug.Log($"{Name} updated target position to current player: {targetPosition}");
            }
        }

        // Calculate direction to move
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Use MoveTowards for consistent movement speed
        transform.position = Vector3.MoveTowards(
            transform.position,
            new Vector3(targetPosition.x, targetPosition.y, transform.position.z),
            Speed * Time.deltaTime
        );

        if (showDebugLogs)
            Debug.Log($"{Name} moving towards {targetPosition} at speed {Speed}");
    }

    // Base attack method that can be overridden
    public virtual void Attack()
    {
        // Base implementation does nothing
        if (showDebugLogs)
            Debug.Log($"{Name} tried to attack but has no attack implementation");
    }

    // Visualize detection radius in editor
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}

