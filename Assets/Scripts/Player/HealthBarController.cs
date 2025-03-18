using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient healthGradient;
    [SerializeField] private float yOffset = 1.5f;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Canvas parentCanvas;

    private Transform target;

    private void Awake()
    {
        // Find canvas if not assigned
        if (parentCanvas == null)
        {
            parentCanvas = GetComponentInParent<Canvas>();

            if (parentCanvas == null)
            {
                Debug.LogError("Health bar must be a child of a Canvas!");
            }
        }

        // Find camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
        transform.position = target.position + new Vector3(0, 1.5f, 0);
        Debug.Log("Health bar target set to: " + (target != null ? target.name : "null"));
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;

            if (fillImage != null && healthGradient != null)
            {
                fillImage.color = healthGradient.Evaluate(currentHealth / maxHealth);
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (target != null)
        {
            transform.position = target.position + new Vector3(0, 1.5f, 0);
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0);
        }

        if (parentCanvas == null)
        {
            Debug.LogError("No Canvas found for health bar.");
            return;
        }

        if (mainCamera == null)
        {
            Debug.LogError("No camera found for health bar.");
            return;
        }

        Vector3 worldPosition = target.position + new Vector3(0, yOffset, 0);

        if (parentCanvas.renderMode == RenderMode.WorldSpace)
        {
            transform.position = worldPosition;
        }
        else if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            transform.position = mainCamera.WorldToScreenPoint(worldPosition);
        }
        else if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Vector2 viewportPosition = mainCamera.WorldToViewportPoint(worldPosition);
            transform.position = new Vector2(viewportPosition.x * Screen.width, viewportPosition.y * Screen.height);
        }
    }
}