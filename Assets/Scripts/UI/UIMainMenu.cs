using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    [SerializeField] Button _Play;
    void Start()
    {
        _Play.onClick.AddListener(StartGame);
    }


    private void StartGame()
    {
        SceneSwapManager.instance.LoadGame();
    }
}
