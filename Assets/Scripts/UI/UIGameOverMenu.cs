using UnityEngine;
using UnityEngine.UI;

public class UIGameOverMenu : MonoBehaviour
{
    [SerializeField] Button _MainMenu;
    [SerializeField] Button _Restart;
    void Start()
    {
        _MainMenu.onClick.AddListener(LoadMainMenu);
        _Restart.onClick.AddListener(LoadGame);
    }

    private void LoadMainMenu()
    {
        SceneSwapManager.instance.LoadMainMenu();
    }

    private void LoadGame()
    {
        SceneSwapManager.instance.LoadGame();
    }
}
