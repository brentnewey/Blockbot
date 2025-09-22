using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    private GameObject pauseMenuControllerObject;

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            CreatePauseMenuController();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only add pause menu to gameplay scenes (not title screen)
        if (scene.name != "TitleScreen")
        {
            EnsurePauseMenuController();
        }
    }

    void CreatePauseMenuController()
    {
        if (pauseMenuControllerObject == null)
        {
            pauseMenuControllerObject = new GameObject("PauseMenuController");
            pauseMenuControllerObject.AddComponent<PauseMenuController>();
            DontDestroyOnLoad(pauseMenuControllerObject);
        }
    }

    void EnsurePauseMenuController()
    {
        if (pauseMenuControllerObject == null)
        {
            CreatePauseMenuController();
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}