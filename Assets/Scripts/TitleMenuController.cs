using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenuController : MonoBehaviour
{
    private int currentSelection = 0;
    private bool canInput = true;
    private float inputCooldown = 0.15f;

    private GUIStyle titleStyle;
    private GUIStyle menuStyle;
    private GUIStyle selectedStyle;

    void Start()
    {
        // Initialize GUI styles
        titleStyle = new GUIStyle();
        titleStyle.fontSize = 72;
        titleStyle.normal.textColor = Color.white;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;

        menuStyle = new GUIStyle();
        menuStyle.fontSize = 36;
        menuStyle.normal.textColor = Color.gray;
        menuStyle.alignment = TextAnchor.MiddleCenter;

        selectedStyle = new GUIStyle();
        selectedStyle.fontSize = 36;
        selectedStyle.normal.textColor = Color.white;
        selectedStyle.alignment = TextAnchor.MiddleCenter;
    }

    void Update()
    {
        if (!canInput) return;

        // Navigation
        #if !UNITY_WEBGL
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            currentSelection = 0;
            StartCoroutine(InputCooldown());
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            currentSelection = 1;
            StartCoroutine(InputCooldown());
        }
        #endif

        // Selection
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            SelectOption();
            StartCoroutine(InputCooldown());
        }
    }

    void OnGUI()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Title
        GUI.Label(new Rect(0, screenHeight * 0.2f, screenWidth, 100), "BLOCKBOT", titleStyle);

        // Menu options
        string startPrefix = currentSelection == 0 ? "> " : "  ";
        GUIStyle startStyle = currentSelection == 0 ? selectedStyle : menuStyle;

        GUI.Label(new Rect(0, screenHeight * 0.5f, screenWidth, 50), startPrefix + "Start Game", startStyle);

        #if !UNITY_WEBGL
        string quitPrefix = currentSelection == 1 ? "> " : "  ";
        GUIStyle quitStyle = currentSelection == 1 ? selectedStyle : menuStyle;
        GUI.Label(new Rect(0, screenHeight * 0.6f, screenWidth, 50), quitPrefix + "Quit", quitStyle);
        #endif

        // Instructions
        GUIStyle instructionStyle = new GUIStyle(menuStyle);
        instructionStyle.fontSize = 18;
        instructionStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        GUI.Label(new Rect(0, screenHeight * 0.85f, screenWidth, 30), "Use Arrow Keys/WASD to navigate, Enter/Space to select", instructionStyle);
    }

    void SelectOption()
    {
        if (currentSelection == 0)
        {
            // Start Game - Load Level1
            SceneManager.LoadScene("Level1");
        }
        #if !UNITY_WEBGL
        else if (currentSelection == 1)
        {
            // Quit Game
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        #endif
    }

    IEnumerator InputCooldown()
    {
        canInput = false;
        yield return new WaitForSeconds(inputCooldown);
        canInput = true;
    }
}