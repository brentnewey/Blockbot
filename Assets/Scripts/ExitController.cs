using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitController : MonoBehaviour, IPulsable
{
    public string nextScene;

    void Start()
    {
    }

    void Update()
    {
    }

    public void goToExit()
    {
        if (!string.IsNullOrEmpty(nextScene))
        {
            SceneManager.LoadScene(nextScene);
            return;
        }

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;
        if (nextIndex >= 0 && nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
            return;
        }

        // No more levels - handle end of game
#if UNITY_WEBGL
        // For WebGL, return to title screen instead of quitting
        SceneManager.LoadScene("TitleScreen");
#else
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
#endif
    }

    public void pulse()
    {
        Collider2D hit = Physics2D.OverlapCircle((Vector2)transform.position, .2f);
        if (hit)
        {
            PlayerController playerController = hit.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                goToExit();
            }
        }
    }

    public int pulsePriority()
    {
        return 2;
    }

    public bool canPulseDeadPlayer()
    {
        return false;
    }
}
