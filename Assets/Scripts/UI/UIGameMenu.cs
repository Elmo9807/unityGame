using UnityEngine;
using UnityEngine.UI;

public class UIGameMenu : MonoBehaviour
{
    [SerializeField] Button _MainMenu;
    [SerializeField] Button _Resume;
    void Start()
    {
        _MainMenu.onClick.AddListener(LoadMainMenu);
        _Resume.onClick.AddListener(Resume);
    }

    private void LoadMainMenu()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.Click, this.transform.position);
        SceneSwapManager.instance.LoadMainMenu();
    }

    private void Resume()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.Click, this.transform.position);
        GameManager.Instance.ResumeGame();
    }
}
