using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    [SerializeField] Button _Play;
    [SerializeField] Button _Quit;
    void Start()
    {
        _Play.onClick.AddListener(StartGame);
        _Play.onClick.AddListener(CloseGame);
    }


    private void StartGame()
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.PlayGame, this.transform.position);
        SceneSwapManager.instance.LoadGame();
    }

    public void CloseGame()
    {
        Application.Quit();
        Debug.Log("PLayer quit the game");
    }
}
