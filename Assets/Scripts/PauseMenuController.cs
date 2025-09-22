using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public static bool IsGamePaused = false;
    private static PauseMenuController instance;

    private int currentSelection = 0;
    private bool canInput = true;
    private float inputCooldown = 0.15f;

    private GUIStyle titleStyle;
    private GUIStyle menuStyle;
    private GUIStyle selectedStyle;
    private GUIStyle backgroundStyle;

    // Auto-create instance when accessed
    public static PauseMenuController Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject pauseMenuObj = new GameObject("PauseMenuController");
                instance = pauseMenuObj.AddComponent<PauseMenuController>();
                DontDestroyOnLoad(pauseMenuObj);
            }
            return instance;
        }
    }

    void Awake()
    {
        // Singleton pattern - ensure only one instance exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        InitializeStyles();
    }

    void Start()
    {
        // Move initialization to Awake to handle singleton creation
    }

    void InitializeStyles()
    {
        titleStyle = new GUIStyle();
        titleStyle.fontSize = 48;
        titleStyle.normal.textColor = Color.white;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;

        menuStyle = new GUIStyle();
        menuStyle.fontSize = 32;
        menuStyle.normal.textColor = Color.gray;
        menuStyle.alignment = TextAnchor.MiddleCenter;

        selectedStyle = new GUIStyle();
        selectedStyle.fontSize = 32;
        selectedStyle.normal.textColor = Color.white;
        selectedStyle.alignment = TextAnchor.MiddleCenter;

        backgroundStyle = new GUIStyle();
        backgroundStyle.normal.background = CreateColorTexture(new Color(0, 0, 0, 0.8f));
    }

    Texture2D CreateColorTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        if (IsGamePaused && canInput)
        {
            HandleMenuInput();
        }
    }

    void HandleMenuInput()
    {
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

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            SelectOption();
            StartCoroutine(InputCooldown());
        }
    }

    void SelectOption()
    {
        if (currentSelection == 0)
        {
            ResumeGame();
        }
        else if (currentSelection == 1)
        {
            QuitToTitleScreen();
        }
    }

    void PauseGame()
    {
        IsGamePaused = true;
        Time.timeScale = 0f;
        currentSelection = 0;
    }

    void ResumeGame()
    {
        IsGamePaused = false;
        Time.timeScale = 1f;
    }

    void QuitToTitleScreen()
    {
        Time.timeScale = 1f;
        IsGamePaused = false;
        SceneManager.LoadScene("TitleScreen");
    }

    void OnGUI()
    {
        if (!IsGamePaused) return;

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Semi-transparent background
        GUI.Box(new Rect(0, 0, screenWidth, screenHeight), "", backgroundStyle);

        // Title
        GUI.Label(new Rect(0, screenHeight * 0.3f, screenWidth, 60), "PAUSED", titleStyle);

        // Menu options
        string resumePrefix = currentSelection == 0 ? "> " : "  ";
        string quitPrefix = currentSelection == 1 ? "> " : "  ";

        GUIStyle resumeStyle = currentSelection == 0 ? selectedStyle : menuStyle;
        GUIStyle quitStyle = currentSelection == 1 ? selectedStyle : menuStyle;

        GUI.Label(new Rect(0, screenHeight * 0.5f, screenWidth, 40), resumePrefix + "Resume", resumeStyle);
        GUI.Label(new Rect(0, screenHeight * 0.6f, screenWidth, 40), quitPrefix + "Quit to Title", quitStyle);

        // Instructions
        GUIStyle instructionStyle = new GUIStyle(menuStyle);
        instructionStyle.fontSize = 18;
        instructionStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        GUI.Label(new Rect(0, screenHeight * 0.8f, screenWidth, 30), "Use Arrow Keys/WASD to navigate, Enter/Space to select, ESC to resume", instructionStyle);
    }

    IEnumerator InputCooldown()
    {
        canInput = false;
        yield return new WaitForSecondsRealtime(inputCooldown);
        canInput = true;
    }
}