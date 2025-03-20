using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool isGamePaused;
    public bool isGameOver;

    [Header("UI References")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private Canvas uiCanvas; // Reference to main UI canvas

    [Header("Player References")]
    public GameObject playerPrefab;
    private GameObject currentPlayer;
    private PlayerController playerController;

    [Header("Health UI")]
    [SerializeField] private Slider healthSliderPrefab;
    [SerializeField] private bool createHealthUI = true;
    private PlayerHealthUI healthUI;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Make sure we have a UI canvas
        if (uiCanvas == null)
        {
            // Try to find an existing canvas
            uiCanvas = FindFirstObjectByType<Canvas>();

            // If no canvas exists, create one
            if (uiCanvas == null)
            {
                GameObject canvasObject = new GameObject("UI_Canvas");
                uiCanvas = canvasObject.AddComponent<Canvas>();
                uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }
        }
    }

    void Start()
    {
        SpawnPlayer();

        SetupHealthUI();

        isGameOver = false;
        isGamePaused = false;
        Time.timeScale = 1f;

        // Make sure GameOver UI is hidden initially
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    private void SpawnPlayer()
    {
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");

        if (existingPlayer != null)
        {
            currentPlayer = existingPlayer;
            Debug.Log("Using existing player in scene");
        }
        else if (playerPrefab != null)
        {
            currentPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            currentPlayer.tag = "Player";
            Debug.Log("Spawned new player from prefab");
        }
        else
        {
            Debug.LogError("No player prefab assigned and no player in scene!");
        }

        if (currentPlayer != null)
        {
            playerController = currentPlayer.GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogWarning("Player doesn't have a PlayerController component");
            }
        }
    }

    private void SetupHealthUI()
    {
        if (!createHealthUI) return;
        PlayerHealthUI existingUI = FindFirstObjectByType<PlayerHealthUI>();
        if (existingUI != null)
        {
            healthUI = existingUI;
            Debug.Log("Using existing PlayerHealthUI in scene");

            if (currentPlayer != null)
            {
                healthUI.playerHealthTracker = currentPlayer.GetComponent<HealthTracker>();
            }
            return;
        }

        if (currentPlayer != null)
        {
            HealthTracker healthTracker = currentPlayer.GetComponent<HealthTracker>();

            if (healthTracker == null)
            {
                Debug.LogWarning("Player doesn't have a HealthTracker component. Adding one...");
                healthTracker = currentPlayer.AddComponent<HealthTracker>();
            }

            // Create the health UI
            if (healthSliderPrefab != null)
            {
                // Option 1: Use a prefab
                GameObject healthBarObject = Instantiate(healthSliderPrefab.gameObject, uiCanvas.transform);
                Slider healthSlider = healthBarObject.GetComponent<Slider>();

                // Position it in the top-left
                RectTransform sliderRect = healthSlider.GetComponent<RectTransform>();
                sliderRect.anchorMin = new Vector2(0, 1);
                sliderRect.anchorMax = new Vector2(0, 1);
                sliderRect.pivot = new Vector2(0, 1);
                sliderRect.anchoredPosition = new Vector2(20, -20);
                sliderRect.sizeDelta = new Vector2(200, 30);

                // Connect it to the health tracker
                healthTracker.SetHealthSlider(healthSlider);
                Debug.Log("Health UI created from prefab");
            }
            else
            {
                // Option 2: Use the PlayerHealthUI script
                GameObject healthUIObject = new GameObject("PlayerHealthUI");
                healthUIObject.transform.SetParent(uiCanvas.transform, false);

                healthUI = healthUIObject.AddComponent<PlayerHealthUI>();
                healthUI.playerHealthTracker = healthTracker;
                Debug.Log("Health UI created using PlayerHealthUI script");
            }
        }
    }

    public void HandleGameOver()
    {
        isGameOver = true;

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Game Over UI not assigned!");
        }

        Time.timeScale = 0f;
        Debug.Log("Game Over");
    }

    public void TogglePause()
    {
        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1f;
        Debug.Log(isGamePaused ? "Game Paused" : "Game Resumed");
    }

    public void RestartGame()
    {
        isGameOver = false;
        isGamePaused = false;
        Time.timeScale = 1f;

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        // Respawn player if needed
        if (currentPlayer == null)
        {
            SpawnPlayer();
            SetupHealthUI();
        }

        Debug.Log("Game Restarted");
    }
}