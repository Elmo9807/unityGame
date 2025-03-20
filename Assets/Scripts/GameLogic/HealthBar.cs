using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Image fill;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);

    private Transform target;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void SetTarget(Transform playerTransform)
    {
        target = playerTransform;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // Position the health bar above the player
        transform.position = target.position + offset;

        // Optional: Make the health bar face the camera if using 3D sprites
        // transform.LookAt(transform.position + mainCamera.transform.forward);
    }

    public void SetMaxHealth(float maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = maxHealth;

        fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(float health)
    {
        slider.value = health;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}