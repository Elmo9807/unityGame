using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FMOD.Studio;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool isGamePaused;
    public bool isGameOver;

    //Define game states for fsm
    public enum GameState
    {
        Gameplay,
        Paused,
        GameOver
    }

    public GameState currentState;
    public GameState previousState;

    [Header("UI References")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private Canvas uiCanvas; // Reference to main UI canvas
    [SerializeField] public GameObject shopPanel;

    [Header("Player References")]
    public GameObject playerPrefab;
    public PowerupManager powerupManager;
    private GameObject currentPlayer;
    private PlayerController playerController;
    private Player playerData;
    private HealthTracker healthTracker;

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
            SceneManager.sceneLoaded += OnSceneLoaded;
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

                CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvasObject.AddComponent<GraphicRaycaster>();

                Debug.Log("Created new UI canvas");
            }
        }

        // Call this after ensuring references are set up
        DisableScreens();
    }

    void Start()
    {
        // Code moved to OnSceneLoaded(): This is to handle loading the game on a scene-by-scene basis, rather than whenever the script first starts.
        // This functionality is for DontDestroyOnLoad support
    }

    private void Update()
    {
        switch (currentState)
        {
            case GameState.Gameplay:
                CheckForPauseAndResume();
                break;

            case GameState.Paused:
                CheckForPauseAndResume();
                break;

            case GameState.GameOver:
                if (!isGameOver)
                {
                    isGameOver = true;
                    Time.timeScale = 0f;
                    DisplayGameOverUi();
                }
                break;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Try to find any necessary UI objects if they're not assigned
        if (gameOverUI == null)
        {
            GameObject foundUI = GameObject.FindGameObjectWithTag("GameOverUI");
            if (foundUI != null)
            {
                gameOverUI = foundUI;
                Debug.Log("Found GameOverUI by tag");
            }
        }

        if (pauseScreen == null)
        {
            GameObject foundUI = GameObject.FindGameObjectWithTag("PauseUI");
            if (foundUI != null)
            {
                pauseScreen = foundUI;
                Debug.Log("Found PauseUI by tag");
            }
        }

        if (shopPanel == null)
        {
            GameObject foundUI = GameObject.FindGameObjectWithTag("ShopUI");
            if (foundUI != null)
            {
                shopPanel = foundUI;
                Debug.Log("Found ShopUI by tag");
            }
        }

        // Find PowerupManager if not assigned
        if (powerupManager == null)
        {
            powerupManager = FindFirstObjectByType<PowerupManager>();
            if (powerupManager == null)
            {
                Debug.LogWarning("PowerupManager not found in scene. Some features may not work.");
            }
            else
            {
                Debug.Log("Found PowerupManager component");
            }
        }

        Debug.Log("Game Manager: Loading scene");

        // Unsubscribe to prevent multiple calls
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Screen.fullScreen = true;

        // Set game state
        isGameOver = false;
        isGamePaused = false;
        Time.timeScale = 1f;

        // Make sure GameOver UI is hidden initially
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        // Spawn player first since other methods depend on it
        SpawnPlayer();

        // Only proceed with these steps if we have a valid player
        if (currentPlayer != null)
        {
            SetupHealthUI();
            LoadPlayerData();

            // Log player data if available
            if (playerData != null)
            {
                Debug.Log("Player melee damage is: " + playerData.meleeAttackDamage);
            }
            else
            {
                Debug.LogWarning("playerData is null. Cannot access player stats.");
            }
        }

        // Try to unmute audio if AudioManager exists
        try
        {
            var audioManager = AudioManager.instance;
            if (audioManager != null)
            {
                audioManager.pauseUnmute();
                Debug.Log("Audio unpaused successfully");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Error accessing AudioManager: " + ex.Message);
        }
    }

    private void SpawnPlayer()
    {
        try
        {
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");

            if (existingPlayer != null)
            {
                currentPlayer = existingPlayer;
                Debug.Log("Using existing player in scene");
            }
            else if (playerPrefab != null)
            {
                // First check if the player prefab is valid
                if (!playerPrefab.GetComponent<PlayerController>())
                {
                    Debug.LogError("Player prefab does not have a PlayerController component!");
                }
                else
                {
                    currentPlayer = Instantiate(playerPrefab, new Vector3(-10, -2, 0), Quaternion.identity);
                    currentPlayer.tag = "Player";
                    Debug.Log("Spawned new player from prefab at position: " + currentPlayer.transform.position);
                }
            }
            else
            {
                Debug.LogError("No player prefab assigned and no player in scene!");
                return;
            }

            if (currentPlayer != null)
            {
                playerController = currentPlayer.GetComponent<PlayerController>();
                if (playerController == null)
                {
                    Debug.LogWarning("Player doesn't have a PlayerController component");
                    return;
                }

                try
                {
                    playerData = playerController.GetPlayerData(); //added for saving stats
                    if (playerData == null)
                    {
                        Debug.LogError("Player data is null after GetPlayerData() call");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Error getting player data: " + ex.Message);
                    return;
                }

                // Get HealthTracker from the current player instance, not the prefab
                healthTracker = currentPlayer.GetComponent<HealthTracker>();
                if (healthTracker == null)
                {
                    Debug.LogWarning("Health tracker component not found on player. Adding one...");
                    healthTracker = currentPlayer.AddComponent<HealthTracker>();
                }

                // Find camera follow component and assign target
                CameraFollow cameraFollow = FindFirstObjectByType<CameraFollow>();
                if (cameraFollow != null)
                {
                    cameraFollow.SetTarget(currentPlayer.transform);
                    Debug.Log("Camera target set to player: " + currentPlayer.name);
                }
                else
                {
                    Debug.LogWarning("No CameraFollow component found in scene! Make sure it's attached to your camera.");

                    // Try to add CameraFollow to main camera if not found
                    Camera mainCamera = Camera.main;
                    if (mainCamera != null)
                    {
                        cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
                        cameraFollow.SetTarget(currentPlayer.transform);
                        Debug.Log("Added CameraFollow to main camera and set target to player");
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error in SpawnPlayer: " + ex.Message);
        }
    }

    private void SetupHealthUI()
    {
        if (!createHealthUI) return;

        // First check if a PlayerHealthUI already exists
        PlayerHealthUI existingUI = FindFirstObjectByType<PlayerHealthUI>();
        if (existingUI != null)
        {
            healthUI = existingUI;
            Debug.Log("Using existing PlayerHealthUI in scene");

            if (currentPlayer != null && healthTracker != null)
            {
                healthUI.playerHealthTracker = healthTracker;
                Debug.Log("Connected existing health UI to player");
            }
            return;
        }

        if (currentPlayer != null && healthTracker != null)
        {
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
                Debug.Log("Health UI created from prefab and connected to player");
            }
            else
            {
                // Option 2: Create the PlayerHealthUI script
                GameObject healthUIObject = new GameObject("PlayerHealthUI");
                healthUIObject.transform.SetParent(uiCanvas.transform, false);

                healthUI = healthUIObject.AddComponent<PlayerHealthUI>();
                healthUI.playerHealthTracker = healthTracker;
                healthUI.useWorldSpace = false; // Make sure it's in screen space
                Debug.Log("Health UI created using PlayerHealthUI script and connected to player");
            }
        }
    }

    public void HandleGameOver()
    {
        GameOver();
        isGameOver = true;

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Game Over UI not assigned!");
        }

        Debug.Log("Game Over");
    }

    public void TogglePause()
    {
        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1f;

        Debug.Log(isGamePaused ? "Game Paused" : "Game Resumed");
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
    }

    public void PauseGame()
    {
        if (currentState != GameState.Paused)
        {
            previousState = currentState;
            ChangeState(GameState.Paused);
            Time.timeScale = 0f;

            if (pauseScreen != null)
            {
                pauseScreen.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Pause screen UI reference is missing!");
            }

            if (AudioManager.instance != null)
            {
                AudioManager.instance.pauseMute();
            }

            Debug.Log("Game paused");
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            ChangeState(previousState);
            Time.timeScale = 1f;

            if (pauseScreen != null)
            {
                pauseScreen.SetActive(false);
            }

            if (AudioManager.instance != null)
            {
                AudioManager.instance.pauseUnmute();
            }

            Debug.Log("Game Resumed");
        }
    }

    void CheckForPauseAndResume()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
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

        //// Respawn player if needed
        //if (currentPlayer == null)
        //{
        //    SpawnPlayer();
        //    SetupHealthUI();
        //}

        //Debug.Log("Game Restarted"); 
    }

    void DisableScreens()
    {
        if (pauseScreen != null)
        {
            pauseScreen.SetActive(false);
        }
        else
        {
            Debug.LogWarning("pauseScreen reference not set in the inspector!");
        }

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("gameOverUI reference not set in the inspector!");
        }

        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    public void SceneChange(string name)
    {
        SceneManager.LoadScene(name);
        Time.timeScale = 1;
    }

    public void GameOver()
    {
        ChangeState(GameState.GameOver);

        if (playerData != null && powerupManager != null && healthTracker != null)
        {
            SaveManager.SavePlayer(playerData, powerupManager, healthTracker);
        }
        else
        {
            Debug.LogWarning("Cannot save player data. One or more components are null.");
        }
    }

    public void DisplayGameOverUi()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("gameOverUI reference not set in the inspector!");
        }
    }

    public void LoadPlayerData()
    {
        PlayerSaveData data = SaveManager.LoadPlayer();
        if (data != null && playerData != null && powerupManager != null && healthTracker != null)
        {
            playerData.hasBow = data.hasBow;
            playerData.hasDash = data.hasDash;
            playerData.hasHealingPotion = data.hasHealingPot;
            playerData.meleeAttackDamage = data.meleeDmg;
            playerData.bowAttackDamage = data.rangedDmg;
            powerupManager.playerCurrency = data.currency;
            powerupManager.startWithBow = data.startWithBow;
            powerupManager.startWithDash = data.startWithDash;
            powerupManager.startWithHealingPotion = data.startWithPot;
            healthTracker.SetMaxHealth(data.maxHealth);
            playerData.MaxHealth = data.maxHealth;
            healthTracker.SetHealth(playerData.MaxHealth);
            playerData.Health = playerData.MaxHealth;
            Debug.Log("Max health stored is " + data.maxHealth);
        }
        else if (data == null)
        {
            Debug.Log("No saved player data found.");
        }
        else
        {
            Debug.LogWarning("Cannot load player data. One or more components are null.");
        }
    }

    public void ShowShopUi()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("shopPanel reference not set in the inspector!");
        }
    }

    public void HideShopUi()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("shopPanel reference not set in the inspector!");
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // remove scene to prevent memory leak
    }
}