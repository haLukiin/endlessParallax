using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainMenuCanvas; // Reference to the main menu
    public GameObject gameOverCanvas; // Full Canvas
    public CanvasGroup gameOverCanvasGroup; // Add this for smooth fading
    public TMP_Text quitText;         // "Press Escape to Quit"
    public TMP_Text restartText;      // "Press Space to Restart"
    public TMP_Text scoreText;        // Text to display current score
    public TMP_Text highscoreText;    // Text to display highscore on game over
    public TMP_Text topScoresText;    // Text to display top 7 scores
    public TMP_Text countdownText;    // Large text for 3, 2, 1, GO!
    public TMP_Text mainMenuHighscoreText; // Text to display highscore on Main Menu
    public TMP_Text mainMenuTopScoresText; // Text to display top 7 scores on Main Menu

    [Header("Highscore Entry UI")]
    public GameObject nameEntryPanel;   // Panel with input field
    public TMP_InputField nameInputField; // Input field for 3 letters

    [Header("Score Settings")]
    public float scoreMultiplier = 10f; // How fast the score increases
    public float gameOverDelay = 1.5f;   // How long to wait before showing Game Over screen

    [Header("Difficulty Scaling (Logarithmic)")]
    public float startSpeedMultiplier = 0.5f;
    public float speedLogScale = 0.5f;        // Controls how much the log affects speed
    public float speedTimeFactor = 0.1f;      // Controls how fast time scales inside the log
    
    [Header("Spawn Density (Logarithmic)")]
    public float startGapMultiplier = 1f;
    public float gapLogScale = 0.2f;          // Controls how much the log affects density
    public float gapTimeFactor = 0.05f;       // Controls how fast density increases

    [Header("Vertical Gap (Logarithmic)")]
    public float startVerticalGap = 0f;       // Small value = Large hole with -12 offset
    public float maxVerticalGap = 8f;         // Large value = Small hole with -12 offset
    public float mountainBaseOffset = -12f;   // Your specific mountain setup
    public float verticalGapLogScale = 2.5f;  
    public float verticalGapTimeFactor = 0.4f;
    
    public float speedMultiplier { get; private set; } = 0.5f;
    public float spawnDensityMultiplier { get; private set; } = 1f;
    public float currentVerticalGap { get; private set; } = 0f;

    public static GameManager Instance { get; private set; }
    public static bool skipMenuNextTime = false; // Flag to skip menu on reload

    private float currentScore = 0f;
    private float timeElapsed = 0f;
    private bool isGameOver = false;
    private bool isWaitingForStart = true;
    private bool isEnteringName = false;
    public bool IsCountingDown => isCountingDown;
    private bool isCountingDown = false;
    private const string TopScoresKey = "TopScores_Names_v3"; // New unique key

    [System.Serializable]
    public class HighscoreEntry
    {
        public string name;
        public int score;
    }

    [System.Serializable]
    public class HighscoreList
    {
        public List<HighscoreEntry> entries = new List<HighscoreEntry>();
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        speedMultiplier = startSpeedMultiplier;
        spawnDensityMultiplier = startGapMultiplier;
        currentVerticalGap = startVerticalGap;
        timeElapsed = 0f;

        Time.timeScale = 0f;
        isWaitingForStart = true;
        isCountingDown = false;
        isEnteringName = false;

        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);
        if (nameEntryPanel != null) nameEntryPanel.SetActive(false);
        if (quitText != null) quitText.gameObject.SetActive(false);
        if (restartText != null) restartText.gameObject.SetActive(false);
        if (highscoreText != null) highscoreText.gameObject.SetActive(false);
        if (topScoresText != null) topScoresText.gameObject.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        UpdateMainMenuHighscore();
        UpdateScoreText();

        // Check if we should skip the menu (from a quick restart)
        if (skipMenuNextTime)
        {
            skipMenuNextTime = false;
            StartGame();
        }
    }

    void UpdateMainMenuHighscore()
    {
        HighscoreList list = LoadHighscores();
        if (mainMenuHighscoreText != null)
        {
            if (list.entries.Count > 0)
                mainMenuHighscoreText.text = "Highscore: " + list.entries[0].score + " (" + list.entries[0].name + ")";
            else
                mainMenuHighscoreText.text = "Highscore: 0";
        }
        
        // Update top 7 on main menu if the text field exists
        if (mainMenuTopScoresText != null)
        {
            string display = "TOP 7 SCORES\n";
            for (int i = 0; i < list.entries.Count; i++)
            {
                display += (i + 1) + ". " + list.entries[i].name + " - " + list.entries[i].score + "\n";
            }
            mainMenuTopScoresText.text = display;
            mainMenuTopScoresText.gameObject.SetActive(true);
        }

        UpdateTopScoresUI(list);
    }

    public void StartGame()
    {
        if (!isWaitingForStart) return;
        isWaitingForStart = false;
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (countdownText != null) StartCoroutine(StartCountdown());
        else { isCountingDown = false; Time.timeScale = 1f; }
    }

    IEnumerator StartCountdown()
    {
        isCountingDown = true;
        Time.timeScale = 0f;
        countdownText.gameObject.SetActive(true);
        countdownText.text = "3"; yield return new WaitForSecondsRealtime(1f);
        countdownText.text = "2"; yield return new WaitForSecondsRealtime(1f);
        countdownText.text = "1"; yield return new WaitForSecondsRealtime(1f);
        countdownText.text = "Fly You Fool"; yield return new WaitForSecondsRealtime(0.5f);
        countdownText.gameObject.SetActive(false);
        isCountingDown = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (!isGameOver && !isCountingDown)
        {
            timeElapsed += Time.deltaTime;
            speedMultiplier = startSpeedMultiplier + speedLogScale * Mathf.Log(timeElapsed * speedTimeFactor + 1f);
            spawnDensityMultiplier = startGapMultiplier + gapLogScale * Mathf.Log(timeElapsed * gapTimeFactor + 1f);
            float verticalIncrease = verticalGapLogScale * Mathf.Log(timeElapsed * verticalGapTimeFactor + 1f);
            currentVerticalGap = Mathf.Min(maxVerticalGap, startVerticalGap + verticalIncrease);
            currentScore += Time.deltaTime * scoreMultiplier * speedMultiplier;
            UpdateScoreText();
        }
        else if (isGameOver)
        {
            if (isEnteringName)
            {
                // Let user press Enter to submit
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    SubmitName();
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Space) && (gameOverCanvas == null || gameOverCanvas.activeSelf))
                {
                    skipMenuNextTime = true; // Skip menu on next reload
                    RestartGame();
                }
                
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    skipMenuNextTime = false; // Return to menu on next reload
                    RestartGame();
                }
            }
        }
    }

    void UpdateScoreText()
    {
        if (scoreText != null) scoreText.text = "Score: " + Mathf.FloorToInt(currentScore).ToString();
    }

    public void StopAllMovement(Transform ignoreObject)
    {
        var spawners = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Where(m => m.GetType().Name.Contains("Spawner"));
        foreach (var s in spawners) s.enabled = false;

        var movers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Where(m => 
            m.GetType().Name.Contains("Move") || m.GetType().Name.Contains("Scroll") || 
            m.GetType().Name.Contains("Background") || m.GetType().Name.Contains("SeamCover") || m.GetType().Name.Contains("Parallax")
        );

        foreach (var m in movers)
        {
            m.enabled = false;
        }

        Rigidbody2D[] allBodies = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
        foreach (var rb in allBodies) { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; rb.bodyType = RigidbodyType2D.Kinematic; }
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        int finalScore = Mathf.FloorToInt(currentScore);
        Debug.Log("GameOver triggered! Score: " + finalScore);

        if (IsEligibleForHighscore(finalScore) && nameEntryPanel != null)
        {
            Debug.Log("Score is eligible for highscore. Showing name entry.");
            StartCoroutine(ShowNameEntryUI());
        }
        else
        {
            Debug.Log("Score not eligible or nameEntryPanel is missing. Showing normal Game Over.");
            StartCoroutine(ShowGameOverUI());
        }
    }

    bool IsEligibleForHighscore(int score)
    {
        HighscoreList list = LoadHighscores();
        if (list.entries.Count < 7) return true;
        return score > list.entries[list.entries.Count - 1].score;
    }

    IEnumerator ShowNameEntryUI()
    {
        Debug.Log("Waiting " + gameOverDelay + " seconds before showing name entry...");
        yield return new WaitForSecondsRealtime(gameOverDelay);
        Debug.Log("Delay finished. Activating UI.");
        isEnteringName = true;
        
        if (gameOverCanvas != null) 
        {
            gameOverCanvas.SetActive(true);
            if (gameOverCanvasGroup != null) gameOverCanvasGroup.alpha = 1f;
        }

        if (highscoreText != null)
        {
            highscoreText.text = "Your Score: " + Mathf.FloorToInt(currentScore);
            highscoreText.gameObject.SetActive(true);
        }
        UpdateTopScoresUI(LoadHighscores());
        
        if (nameEntryPanel != null)
        {
            nameEntryPanel.SetActive(true);
            nameEntryPanel.transform.SetAsLastSibling();
            if (nameInputField != null)
            {
                nameInputField.characterLimit = 3; 
                nameInputField.text = "";
                nameInputField.Select();
                nameInputField.ActivateInputField();
            }
        }
    }

    public void SubmitName()
    {
        if (!isEnteringName) return;
        
        string name = "AAA";
        
        if (nameInputField != null)
        {
            name = nameInputField.text.Trim().ToUpper();
            Debug.Log("Submitted text from field: '" + nameInputField.text + "' -> Final name: '" + name + "'");
        }
        else
        {
            Debug.LogError("nameInputField is NULL! Make sure it's assigned in the Inspector.");
        }
        
        if (string.IsNullOrEmpty(name)) name = "AAA";
        
        SaveNewHighscore(name, Mathf.FloorToInt(currentScore));
        
        if (nameEntryPanel != null) nameEntryPanel.SetActive(false);
        isEnteringName = false;
        StartCoroutine(ShowGameOverUI());
    }

    IEnumerator ShowGameOverUI()
    {
        yield return new WaitForSecondsRealtime(isEnteringName ? 0 : gameOverDelay);

        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);
            if (gameOverCanvasGroup != null)
            {
                gameOverCanvasGroup.alpha = 0f;
                while (gameOverCanvasGroup.alpha < 1f)
                {
                    gameOverCanvasGroup.alpha += Time.unscaledDeltaTime * 2f;
                    yield return null;
                }
            }
        }

        if (highscoreText != null) highscoreText.text = "Your Score: " + Mathf.FloorToInt(currentScore);
        UpdateTopScoresUI(LoadHighscores());
        if (quitText != null) { quitText.text = "Press Escape for Menu"; quitText.gameObject.SetActive(true); }
        if (restartText != null) restartText.gameObject.SetActive(true);
    }

    HighscoreList LoadHighscores()
    {
        string json = PlayerPrefs.GetString(TopScoresKey, "");
        if (string.IsNullOrEmpty(json)) return new HighscoreList();
        try { return JsonUtility.FromJson<HighscoreList>(json); }
        catch { return new HighscoreList(); }
    }

    void SaveNewHighscore(string name, int score)
    {
        HighscoreList list = LoadHighscores();
        list.entries.Add(new HighscoreEntry { name = name, score = score });
        list.entries = list.entries.OrderByDescending(e => e.score).Take(7).ToList();
        PlayerPrefs.SetString(TopScoresKey, JsonUtility.ToJson(list));
        PlayerPrefs.Save();
    }

    void UpdateTopScoresUI(HighscoreList list)
    {
        if (topScoresText != null)
        {
            string display = "TOP 7 SCORES\n";
            for (int i = 0; i < list.entries.Count; i++)
                display += (i + 1) + ". " + list.entries[i].name + " - " + list.entries[i].score + "\n";
            topScoresText.text = display;
            topScoresText.gameObject.SetActive(true);
        }
    }

    void RestartGame() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void QuitGame() { 
#if UNITY_EDITOR
        EditorApplication.isPlaying = false; 
#else
        Application.Quit(); 
#endif
    }
}
