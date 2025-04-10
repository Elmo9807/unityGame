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

    void Awake()
    {
        currentHealth = maxHealth;

        if (showDebugLogs)
            Debug.Log($"[HealthTracker] Initialized with {currentHealth}/{maxHealth} health");
    }

    void Start()
    {
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
                        Debug.Log($"[HealthTracker] Health changed via event: {currentHealth}/{maxHealth}");
                };

                if (showDebugLogs)
                    Debug.Log("[HealthTracker] Successfully subscribed to player health events");

                // Set initial health from Player to HealthTracker
                this.currentHealth = playerComponent.Health;
                this.maxHealth = playerComponent.MaxHealth;
            }
        }

        if (healthUI == null)
        {
            healthUI = FindFirstObjectByType<PlayerHealthUI>();
            if (healthUI != null)
            {
                if (showDebugLogs)
                    Debug.Log("[HealthTracker] Found PlayerHealthUI in scene");

                healthUI.playerHealthTracker = this;
            }
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

        if (gameObject.tag != "Player")
        {
            gameObject.tag = "Player";
            if (showDebugLogs)
                Debug.Log("[HealthTracker] Set object tag to 'Player'");
        }

        UpdateUI();
    }

    public void SetMaxHealth(int value)
    {
        if (value <= 0) return;

        maxHealth = value;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
        }

        if (showDebugLogs)
            Debug.Log($"[HealthTracker] Max health set to {maxHealth}");

        UpdateUI();
    }

    public void SetHealth(int value)
    {
        if (value == currentHealth) return;

        int oldHealth = currentHealth;
        currentHealth = Mathf.Clamp(value, 0, maxHealth);

        UpdateUI();

        if (showDebugLogs)
            Debug.Log($"[HealthTracker] Health set from {oldHealth} to {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void SetHealthSlider(Slider slider)
    {
        if (slider == null) return;

        healthSlider = slider;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        if (showDebugLogs)
            Debug.Log("[HealthTracker] Health slider set with value: " + currentHealth);

        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        if (damage == 0)
        {
            UpdateUI();
            return;
        }

        if (damage <= 0) return;

        int oldHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        UpdateUI();

        if (showDebugLogs)
            Debug.Log($"[HealthTracker] Player took {damage} damage. Health: {oldHealth} -> {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        int oldHealth = currentHealth;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        UpdateUI();

        if (showDebugLogs)
            Debug.Log($"[HealthTracker] Player healed {amount} health. Health: {oldHealth} -> {currentHealth}");
    }

    // RECURSION PROTECTOR
    private bool _isUpdatingUI = false;
    private bool _triedFindingHealthUI = false;

    private void UpdateUI()
    {
        if (_isUpdatingUI) return;

        // Set recursion protection flag
        _isUpdatingUI = true;

        try
        {
            if (healthSlider != null)
            {
                healthSlider.value = currentHealth;
            }

            if (healthUI != null)
            {
                healthUI.UpdateHealthUI(currentHealth);
            }
            else if (showDebugLogs)
            {
                if (!_triedFindingHealthUI)
                {
                    _triedFindingHealthUI = true;

                    healthUI = FindFirstObjectByType<PlayerHealthUI>();
                    if (healthUI != null)
                    {
                        healthUI.playerHealthTracker = this;
                        healthUI.UpdateHealthUI(currentHealth);
                        Debug.Log("[HealthTracker] Found and updated PlayerHealthUI");
                    }
                }
            }
        }
        finally
        {
            _isUpdatingUI = false;
        }
    }

    void Die()
    {
        Debug.Log("[HealthTracker] Player has been slain.");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            gameObject.SetActive(false); // Better than destroying, in case we want to respawn
        }
    }
}