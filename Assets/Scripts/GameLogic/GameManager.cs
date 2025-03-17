using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool isGamePaused;
    public bool isGameOver;

    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private HealthBarController healthBar;

    private PlayerController playerController;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        playerController = FindFirstObjectByType<PlayerController>();
    }

    private void Start()
    {
        isGameOver = false;
        isGamePaused = false;
        UpdateHealthBar();
   
    }

    public void HandleGameOver()
    {
        isGameOver = true;
        gameOverUI.SetActive(true); //UI Required
        Time.timeScale = 0f; 
    }

    public void UpdateHealthBar()
    {
        if (playerController != null)
        {
            healthBar.UpdateHealth(playerController.GetCurrentHealth(), playerController.GetMaxHealth());
        }
    }

    public void TogglePause()
    {
        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1f;
    }
}