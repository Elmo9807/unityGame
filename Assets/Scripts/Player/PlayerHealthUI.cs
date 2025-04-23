using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public HealthTracker playerHealthTracker;
    public Slider healthSlider;
    public Text healthText; // Optional: for displaying numerical health

    [Header("UI Positioning")]
    public float verticalOffset = 1.5f; // How far above the player
    public float sliderWidth = 1.0f;
    public float sliderHeight = 0.2f;

    [Header("UI Type")]
    public bool useWorldSpace = false; // Changed to false by default for screen space UI

    private Canvas uiCanvas;
    private RectTransform sliderRectTransform;
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;

        if (useWorldSpace)
        {
            SetupWorldSpaceUI();
        }
        else
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
            healthSlider.value = playerHealthTracker.CurrentHealth;

            UpdateHealthText(playerHealthTracker.CurrentHealth);

            Debug.Log("Health UI successfully connected to player's HealthTracker");
        }
    }

    void Update()
    {
        if (useWorldSpace && playerHealthTracker != null)
        {
            // Make sure UI stays with player if it's world space
            transform.position = playerHealthTracker.transform.position;
        }
    }

    private void SetupWorldSpaceUI()
    {
        // Create a world space canvas
        GameObject canvasObject = new GameObject("PlayerHealthWorldCanvas");
        canvasObject.transform.SetParent(transform, false);

        uiCanvas = canvasObject.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.WorldSpace;

        // Set the canvas size and scale
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(sliderWidth, sliderHeight * 2); // Give some extra height for text if needed
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f); // Scale down to appropriate world size

        // Position the canvas above the player
        canvasObject.transform.localPosition = new Vector3(0, verticalOffset, 0);

        // Add necessary components to canvas
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        // Create the health slider
        GameObject sliderObject = new GameObject("HealthSlider");
        sliderObject.transform.SetParent(uiCanvas.transform, false);

        sliderRectTransform = sliderObject.AddComponent<RectTransform>();
        sliderRectTransform.anchorMin = new Vector2(0, 0.5f);
        sliderRectTransform.anchorMax = new Vector2(1, 0.5f);
        sliderRectTransform.pivot = new Vector2(0.5f, 0.5f);
        sliderRectTransform.sizeDelta = new Vector2(0, sliderHeight * 100);
        sliderRectTransform.anchoredPosition = Vector2.zero;

        healthSlider = sliderObject.AddComponent<Slider>();
        healthSlider.minValue = 0;
        healthSlider.maxValue = 100;
        healthSlider.value = 100;
        healthSlider.wholeNumbers = true;

        // Create slider visuals
        CreateSliderVisuals(sliderObject);

        // Add a component to make the canvas face the camera
        canvasObject.AddComponent<FaceCamera>();
    }

    // Component to make UI face camera
    private class FaceCamera : MonoBehaviour
    {
        private Camera mainCamera;

        void Start()
        {
            mainCamera = Camera.main;
        }

        void LateUpdate()
        {
            if (mainCamera != null)
            {
                transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up);
            }
        }
    }

    private void FindOrCreateCanvas()
    {
        // First, check if we're attached to a canvas
        uiCanvas = GetComponentInParent<Canvas>();

        if (uiCanvas == null)
        {
            // Look for an existing Screen Space Overlay canvas
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
            // Create a new canvas if none was found
            GameObject canvasObject = new GameObject("HealthUI_Canvas");
            uiCanvas = canvasObject.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObject.AddComponent<GraphicRaycaster>();

            transform.SetParent(uiCanvas.transform, false);
            Debug.Log("Created new canvas for Health UI");
        }
    }

    private void CreateHealthSlider()
    {
        GameObject sliderObject = new GameObject("HealthSlider");
        // Make slider a direct child of the canvas, not of the PlayerHealthUI transform
        sliderObject.transform.SetParent(uiCanvas.transform, false);

        sliderRectTransform = sliderObject.AddComponent<RectTransform>();

        healthSlider = sliderObject.AddComponent<Slider>();

        healthSlider.minValue = 0;
        healthSlider.maxValue = 100;
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

            // Explicitly set anchors to top-left
            sliderRectTransform.anchorMin = new Vector2(0, 1);
            sliderRectTransform.anchorMax = new Vector2(0, 1);
            sliderRectTransform.pivot = new Vector2(0, 1);

            // Position in the top-left with margin
            sliderRectTransform.anchoredPosition = new Vector2(20, -20);
            sliderRectTransform.sizeDelta = new Vector2(200, 30);

            Debug.Log($"Health UI positioned at: {sliderRectTransform.anchoredPosition} with size: {sliderRectTransform.sizeDelta}");
        }
    }

    private void CreateHealthText()
    {
        GameObject textObject = new GameObject("HealthText");
        textObject.transform.SetParent(uiCanvas.transform, false);

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
        textRect.anchoredPosition = new Vector2(20, -50); // Adjust this value to match the health bar
        textRect.sizeDelta = new Vector2(200, 30);
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