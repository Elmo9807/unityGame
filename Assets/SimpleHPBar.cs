using UnityEngine;
using UnityEngine.UI;

public class SimpleHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;

    private void Start()
    {
        PlayerController playerController = FindFirstObjectByType<PlayerController>();

        if (playerController != null)
        {
            healthSlider.maxValue = playerController.GetMaxHealth();
            healthSlider.value = playerController.GetCurrentHealth();
        }
    }

    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        Debug.Log($"Simple health bar updating: {currentHealth}/{maxHealth}");
        healthSlider.value = currentHealth;

        // Color logic if needed
        float normalizedHealth = (float)currentHealth / maxHealth;
        fillImage.color = Color.Lerp(Color.red, Color.green, normalizedHealth);
    }

    private void OnDestroy()
    {
        PlayerController controller = FindFirstObjectByType<PlayerController>();

        if (controller != null)
        {
            controller.UnsubscribeFromHealthChanges(UpdateHealthBar);
        }
    }
}