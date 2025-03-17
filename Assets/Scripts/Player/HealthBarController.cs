using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient healthGradient;
    [SerializeField] private float yOffset = 1.5f;
    [SerializeField] private Camera mainCamera;

    private Transform target;
    private Canvas parentCanvas;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
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
            return;

        Vector2 worldPosition = (Vector2)target.position + new Vector2(0, yOffset);

        if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            transform.position = mainCamera.WorldToScreenPoint(worldPosition);
        }
        else if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Vector2 viewportPosition = mainCamera.WorldToViewportPoint(worldPosition);
            transform.position = new Vector2(viewportPosition.x * Screen.width, viewportPosition.y * Screen.height);
        }
        else
        {
            transform.position = new Vector3(worldPosition.x, worldPosition.y, 0);
        }
    }
}
