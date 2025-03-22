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
    private Player playerComponent;

    void Start()
    {
        currentHealth = maxHealth;

        // Try to find player component and subscribe to events
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerComponent = playerController.GetPlayerData();
            if (playerComponent != null)
            {
                // Subscribe to health changed events
                playerComponent.OnHealthChanged += (currentHealth, maxHealth) =>
                {
                    this.currentHealth = currentHealth;
                    UpdateUI();

                    if (showDebugLogs)
                        Debug.Log($"[HealthTracker] Health changed: {currentHealth}/{maxHealth}");
                };

                if (showDebugLogs)
                    Debug.Log("[HealthTracker] Successfully subscribed to player health events");
            }
        }

        // Look for a PlayerHealthUI if we don't have one
        if (healthUI == null)
        {
            healthUI = FindFirstObjectByType<PlayerHealthUI>();
            if (healthUI != null && showDebugLogs)
                Debug.Log("[HealthTracker] Found PlayerHealthUI in scene");
        }

        // Initialize slider UI if available
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;

            if (showDebugLogs)
                Debug.Log("[HealthTracker] Health slider initialized with value: " + currentHealth);
        }
        else if (healthUI == null && showDebugLogs)
        {
            Debug.LogWarning("[HealthTracker] No health UI found. Health will be tracked but not displayed.");
        }

        // Ensure the object has the Player tag
        if (gameObject.tag != "Player")
        {
            gameObject.tag = "Player";
            if (showDebugLogs)
                Debug.Log("[HealthTracker] Set object tag to 'Player'");
        }

        // Force update the UI once at start
        UpdateUI();
    }

    public void SetHealthSlider(Slider slider)
    {
        if (slider == null) return;

        healthSlider = slider;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        if (showDebugLogs)
            Debug.Log("[HealthTracker] Health slider set with value: " + currentHealth);

        // Force update the UI after setting slider
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        UpdateUI();

        if (showDebugLogs)
            Debug.Log("[HealthTracker] Player took " + damage + " damage. Current health: " + currentHealth);

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
            Debug.Log("[HealthTracker] Player healed " + amount + " health. Current health: " + currentHealth);
    }

    private void UpdateUI()
    {
        // Update slider if we have one
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;

            if (showDebugLogs)
                Debug.Log("[HealthTracker] Updated health slider to: " + currentHealth);
        }

        // Update the PlayerHealthUI if we have it
        if (healthUI != null)
        {
            healthUI.UpdateHealthUI(currentHealth);

            if (showDebugLogs)
                Debug.Log("[HealthTracker] Updated PlayerHealthUI to: " + currentHealth);
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning("[HealthTracker] No PlayerHealthUI found to update");

            // Try to find it again in case it was created after this component
            healthUI = FindFirstObjectByType<PlayerHealthUI>();
            if (healthUI != null)
            {
                healthUI.playerHealthTracker = this;
                healthUI.UpdateHealthUI(currentHealth);
                Debug.Log("[HealthTracker] Found and updated PlayerHealthUI");
            }
        }
    }

    void Die()
    {
        Debug.Log("[HealthTracker] Player has been slain.");

        // Try to notify the GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HandleGameOver();
        }
        else
        {
            // If we don't have a game manager, just disable the object
            gameObject.SetActive(false); // Better than destroying, in case we want to respawn
        }
    }
}