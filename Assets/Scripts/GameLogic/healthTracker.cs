using UnityEngine;
using UnityEngine.UI;

public class HealthTracker : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI Settings")]
    [SerializeField] private Slider healthSlider;
    private PlayerHealthUI healthUI;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    public int CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthUI == null)
        {
            healthUI = FindObjectOfType<PlayerHealthUI>();
        }

        // Initialize UI if available
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;

            if (showDebugLogs)
                Debug.Log("Health slider initialized with value: " + currentHealth);
        }
        else if (healthUI == null && showDebugLogs)
        {
            Debug.LogWarning("No health UI found. Health will be tracked but not displayed.");
        }
        if (gameObject.tag != "Player")
        {
            gameObject.tag = "Player";
        }
    }

    public void SetHealthSlider(Slider slider)
    {
        if (slider == null) return;

        healthSlider = slider;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        if (showDebugLogs)
            Debug.Log("Health slider set with value: " + currentHealth);
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        UpdateUI();

        if (showDebugLogs)
            Debug.Log("Player took " + damage + " damage. Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); 

        UpdateUI();

        if (showDebugLogs)
            Debug.Log("Player healed " + amount + " health. Current health: " + currentHealth);
    }

    private void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        // Update the UI if we have it
        if (healthUI != null)
        {
            healthUI.UpdateHealthUI(currentHealth);
        }
    }

    void Die()
    {
        Debug.Log("Player has been slain.");

        // Try to notify the GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HandleGameOver();
        }
        else
        {
            // If we don't have a game manager, just destroy the object
            gameObject.SetActive(false); // Better than destroying, in case we want to respawn
        }
    }
}