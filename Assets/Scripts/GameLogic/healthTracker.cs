using UnityEngine;

public class HealthTracker : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    [SerializeField] private GameObject healthBarPrefab;
    private GameObject healthBarInstance;
    private HealthBarController healthBar;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
            healthBar = healthBarInstance.GetComponent<HealthBarController>();

            if (healthBar != null)
            {
                healthBar.UpdateHealth(currentHealth, maxHealth);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Player suffered " + damage + " damage. Current hitpoints: " + currentHealth);

        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player has been slain.");
        Destroy(gameObject);
    }
}