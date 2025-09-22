using UnityEngine;
using UnityEngine.SceneManagement;

public static class StartupLoader
{
    private const string FirstLevelName = "TitleScreen";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureFirstLevelLoaded()
    {
        if (Application.isEditor)
        {
            return;
        }

        var activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != FirstLevelName)
        {
            SceneManager.LoadScene(FirstLevelName);
        }
    }
}
