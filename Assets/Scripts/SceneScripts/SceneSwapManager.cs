using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwapManager : MonoBehaviour
{
    public static SceneSwapManager instance { get; private set; }

    
    private void Awake()
    {
        // singleton pattern
        if(instance == null)
        {
            instance = this;
        }

    }

    public enum Scene // make enums correspond to exact scene names
    {
        MainMenu,
        Dungeon01
    }

    public void LoadScene(Scene scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }

    public void LoadGame()
    {
        SceneManager.LoadScene(Scene.Dungeon01.ToString());
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // Grabs next scene according to index of build list. i.e. if dungeon01 is at 1, LoadNextScene() loads dungeon02 at 2
    }

    public void LoadMainMenu()
    {
        Debug.Log("SceneSwapManager: Loading main menu scene");
        SceneManager.LoadScene(Scene.MainMenu.ToString());
    }
}
