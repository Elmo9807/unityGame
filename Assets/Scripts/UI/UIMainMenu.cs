using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    [SerializeField] Button _Play;
    [SerializeField] Button _Quit;
    [SerializeField] Button _NewGame;
    void Start()
    {
        _Play.onClick.AddListener(StartGame);
        _Quit.onClick.AddListener(CloseGame);
        _NewGame.onClick.AddListener(NewGame);
    }


    private void StartGame()
    {
        AudioManager.instance.FadeoutAll();

        AudioManager.instance.PlayOneShot(FMODEvents.instance.PlayGame, this.transform.position);

        SceneSwapManager.instance.LoadGame();
    }

    private void NewGame()
    {
        SaveManager.DeletePlayerSave();
        AudioManager.instance.FadeoutAll();

        AudioManager.instance.PlayOneShot(FMODEvents.instance.PlayGame, this.transform.position);

        SceneSwapManager.instance.LoadGame();
    }



    public void CloseGame()
    {
        Application.Quit();
        Debug.Log("Player quit the game");
    }
}
