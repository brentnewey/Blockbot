using System;
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
        if (nextScene == null || nextScene == "")
        {
            string currentScene = SceneManager.GetActiveScene().name;
            int levelNumber = Int32.Parse(currentScene.Substring(5));
            string nextScene = "Level" + (levelNumber + 1).ToString();
            if (SceneManager.GetSceneByName(nextScene).IsValid())
            {
                SceneManager.LoadScene(nextScene);
            }
            else
            {
                Application.Quit();
                UnityEditor.EditorApplication.isPlaying = false;
            }

        }
        else
        {
            SceneManager.LoadScene(nextScene);
        }
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
