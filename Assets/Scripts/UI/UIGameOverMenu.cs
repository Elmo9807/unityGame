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
        AudioManager.instance.PlayOneShot(FMODEvents.instance.Click, this.transform.position);
        SceneSwapManager.instance.LoadMainMenu();
    }

    private void LoadGame()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.PlayGame, this.transform.position);
        SceneSwapManager.instance.LoadGame();
    }
}
