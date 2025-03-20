using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public HealthTracker playerHealthTracker;
    public Slider healthSlider;
    public Text healthText; // Optional: for displaying numerical health, let me know if you guys want to do this one

    [Header("UI Positioning")]
    public float topOffset = 20f;
    public float leftOffset = 20f;
    public float sliderWidth = 200f;
    public float sliderHeight = 30f;

    private Canvas uiCanvas;
    private RectTransform sliderRectTransform;

    void Awake()
    {
        FindOrCreateCanvas();

        if (healthSlider == null)
        {
            CreateHealthSlider();
        }
        else
        {
            sliderRectTransform = healthSlider.GetComponent<RectTransform>();
            PositionHealthBar();
        }

        if (healthText == null && sliderRectTransform != null)
        {
            CreateHealthText();
        }
    }

    void Start()
    {
        if (playerHealthTracker == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealthTracker = player.GetComponent<HealthTracker>();
                if (playerHealthTracker == null)
                {
                    Debug.LogError("Player doesn't have a HealthTracker component!");
                }
            }
            else
            {
                Debug.LogError("Player not found! Please ensure the Player is tagged with 'Player'");
            }
        }

        // Connect health tracker to slider
        if (playerHealthTracker != null && healthSlider != null)
        {
            healthSlider.maxValue = playerHealthTracker.maxHealth;
            healthSlider.value = playerHealthTracker.CurrentHealth; // Using the property from HealthTracker

            UpdateHealthText(playerHealthTracker.CurrentHealth);

            Debug.Log("Health UI successfully connected to player's HealthTracker");
        }
    }

    private void FindOrCreateCanvas()
    { 
        uiCanvas = GetComponentInParent<Canvas>();

        if (uiCanvas == null)
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    uiCanvas = canvas;
                    transform.SetParent(uiCanvas.transform, false);
                    Debug.Log("Found existing ScreenSpaceOverlay canvas to use");
                    break;
                }
            }
        }

        if (uiCanvas == null)
        {
            GameObject canvasObject = new GameObject("HealthUI_Canvas");
            uiCanvas = canvasObject.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            transform.SetParent(canvasObject.transform, false);
            Debug.Log("Created new canvas for Health UI");
        }
    }

    private void CreateHealthSlider()
    {
        GameObject sliderObject = new GameObject("HealthSlider");
        sliderObject.transform.SetParent(transform, false);

        sliderRectTransform = sliderObject.AddComponent<RectTransform>();

        healthSlider = sliderObject.AddComponent<Slider>();

        healthSlider.minValue = 0;
        healthSlider.maxValue = 100; // Default value, will be updated with healthTracker information
        healthSlider.value = 100;    
        healthSlider.wholeNumbers = true;

        PositionHealthBar();

        CreateSliderVisuals(sliderObject);

        Debug.Log("Created health slider with proper positioning");
    }

    private void CreateSliderVisuals(GameObject sliderObject)
    {
        GameObject bgObject = new GameObject("Background");
        bgObject.transform.SetParent(sliderObject.transform, false);
        Image bgImage = bgObject.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        RectTransform bgRect = bgImage.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.sizeDelta = Vector2.zero;

        GameObject fillAreaObject = new GameObject("Fill Area");
        fillAreaObject.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillAreaObject.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0);
        fillAreaRect.anchorMax = new Vector2(1, 1);
        fillAreaRect.offsetMin = new Vector2(5, 5);
        fillAreaRect.offsetMax = new Vector2(-5, -5);

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(fillAreaObject.transform, false);
        Image fillImage = fillObject.AddComponent<Image>();
        fillImage.color = Color.red;
        RectTransform fillRect = fillImage.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.sizeDelta = Vector2.zero;

        healthSlider.targetGraphic = bgImage;
        healthSlider.fillRect = fillRect;
        healthSlider.direction = Slider.Direction.LeftToRight;
    }

    private void PositionHealthBar()
    {
        if (sliderRectTransform != null)
        {
            // Reset any inherited properties
            sliderRectTransform.localScale = Vector3.one;
            sliderRectTransform.localRotation = Quaternion.identity;

            // Set anchors explicitly to top-left
            sliderRectTransform.anchorMin = new Vector2(0, 1);
            sliderRectTransform.anchorMax = new Vector2(0, 1);
            sliderRectTransform.pivot = new Vector2(0, 1);

            // Use absolute positioning relative to anchor
            sliderRectTransform.anchoredPosition = new Vector2(leftOffset, -topOffset);
            sliderRectTransform.sizeDelta = new Vector2(sliderWidth, sliderHeight);

            Debug.Log($"Health UI positioned at: {sliderRectTransform.anchoredPosition} with size: {sliderRectTransform.sizeDelta}");
        }
    }

    private void CreateHealthText() //Text logic if we want it for the player HP bar
    {
        GameObject textObject = new GameObject("HealthText");
        textObject.transform.SetParent(transform, false);

        healthText = textObject.AddComponent<Text>();

        healthText.fontSize = 18;
        healthText.alignment = TextAnchor.MiddleCenter;
        healthText.color = Color.white;
        healthText.text = "100/100";

        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (healthText.font == null)
        {
            Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
            if (fonts.Length > 0)
            {
                healthText.font = fonts[0];
            }
        }

        RectTransform textRect = healthText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 1);
        textRect.anchorMax = new Vector2(0, 1);
        textRect.pivot = new Vector2(0, 1);
        textRect.anchoredPosition = new Vector2(leftOffset + sliderWidth / 2, -topOffset - sliderHeight - 5);
        textRect.sizeDelta = new Vector2(sliderWidth, 30);
    }

    private void UpdateHealthText(int currentHealth)
    {
        if (healthText != null && playerHealthTracker != null)
        {
            healthText.text = $"{currentHealth}/{playerHealthTracker.maxHealth}";
        }
    }

    // Method that can be called from HealthTracker to update the UI
    public void UpdateHealthUI(int currentHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        UpdateHealthText(currentHealth);
    }
}