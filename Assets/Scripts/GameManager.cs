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
    public GameObject gameOverCanvas; // Full Canvas
    public CanvasGroup gameOverCanvasGroup; // Add this for smooth fading
    public TMP_Text quitText;         // "Press Escape to Quit"
    public TMP_Text restartText;      // "Press Space to Restart"
    public TMP_Text scoreText;        // Text to display current score
    public TMP_Text highscoreText;    // Text to display highscore on game over
    public TMP_Text topScoresText;    // Text to display top 5 scores
    public TMP_Text countdownText;    // Large text for 3, 2, 1, GO!

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

    private float currentScore = 0f;
    private float timeElapsed = 0f;
    private bool isGameOver = false;
    public bool IsCountingDown => isCountingDown;
    private bool isCountingDown = true;
    private const string TopScoresKey = "TopScores";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Reset multipliers
        speedMultiplier = startSpeedMultiplier;
        spawnDensityMultiplier = startGapMultiplier;
        currentVerticalGap = startVerticalGap;
        timeElapsed = 0f;

        // Reset time scale in case it was left at 0 or something else
        Time.timeScale = 1f;

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(false);

        if (quitText != null)
            quitText.gameObject.SetActive(false);
            
        if (restartText != null)
            restartText.gameObject.SetActive(false);
            
        if (highscoreText != null)
            highscoreText.gameObject.SetActive(false);

        if (topScoresText != null)
            topScoresText.gameObject.SetActive(false);

        UpdateScoreText();

        // Start the countdown
        if (countdownText != null)
        {
            StartCoroutine(StartCountdown());
        }
        else
        {
            isCountingDown = false;
        }
    }

    IEnumerator StartCountdown()
    {
        isCountingDown = true;
        Time.timeScale = 0f; // Pause movement and gameplay
        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";
        yield return new WaitForSecondsRealtime(1f);
        
        countdownText.text = "2";
        yield return new WaitForSecondsRealtime(1f);
        
        countdownText.text = "1";
        yield return new WaitForSecondsRealtime(1f);
        
        countdownText.text = "GO!";
        yield return new WaitForSecondsRealtime(0.5f);

        countdownText.gameObject.SetActive(false);
        isCountingDown = false;
        Time.timeScale = 1f; // Resume gameplay
    }

    void Update()
    {
        if (!isGameOver && !isCountingDown)
        {
            timeElapsed += Time.deltaTime;

            // Logarithmic speed increase: V = Vstart + A * log(B * t + 1)
            speedMultiplier = startSpeedMultiplier + speedLogScale * Mathf.Log(timeElapsed * speedTimeFactor + 1f);

            // Logarithmic gap decrease (density increase): D = Dstart + A * log(B * t + 1)
            spawnDensityMultiplier = startGapMultiplier + gapLogScale * Mathf.Log(timeElapsed * gapTimeFactor + 1f);

            // Logarithmic vertical gap INCREASE: G = Gstart + A * log(B * t + 1)
            float verticalIncrease = verticalGapLogScale * Mathf.Log(timeElapsed * verticalGapTimeFactor + 1f);
            currentVerticalGap = Mathf.Min(maxVerticalGap, startVerticalGap + verticalIncrease);

            // Increase score based on time and speed
            currentScore += Time.deltaTime * scoreMultiplier * speedMultiplier;
            UpdateScoreText();
        }
        else if (isGameOver)
        {
            // Check for Restart (Only if UI is active or after delay)
            if (Input.GetKeyDown(KeyCode.Space) && (gameOverCanvas == null || gameOverCanvas.activeSelf))
            {
                RestartGame();
            }
            
            // Check for Quit
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                QuitGame();
            }
        }
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            // Display score as a whole number
            scoreText.text = "Score: " + Mathf.FloorToInt(currentScore).ToString();
        }
    }

    public void StopAllMovement(Transform ignoreObject)
    {
        // 1. Stop all Spawners
        var spawners = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Where(m => m.GetType().Name.Contains("Spawner"));
        foreach (var s in spawners) s.enabled = false;

        // 2. Find all objects with movement scripts
        var movers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Where(m => 
            m.GetType().Name.Contains("Move") || 
            m.GetType().Name.Contains("Scroll") || 
            m.GetType().Name.Contains("Background") ||
            m.GetType().Name.Contains("SeamCover") ||
            m.GetType().Name.Contains("Parallax")
        );

        foreach (var m in movers)
        {
            // Stop the script
            m.enabled = false;

            string typeName = m.GetType().Name;
            bool isBackground = typeName.Contains("Background") || typeName.Contains("Scroll");
            
            // Check if this object is the one we hit, or a parent of the one we hit
            bool isHitObject = (ignoreObject != null) && (m.transform == ignoreObject || ignoreObject.IsChildOf(m.transform));

            // If it's an obstacle (Move/SeamCover/etc) and NOT the hit object and NOT background, hide it!
            if (!isBackground && !isHitObject)
            {
                // We only want to hide the actual obstacle game objects
                if (typeName.Contains("Move") || typeName.Contains("SeamCover"))
                {
                    m.gameObject.SetActive(false);
                }
            }
        }

        // 3. Stop all Physics (Rigidbody2D)
        Rigidbody2D[] allBodies = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
        foreach (var rb in allBodies)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    public void GameOver()
    {
        if (isGameOver) return; // Prevent multiple calls
        
        isGameOver = true;
        int finalScore = Mathf.FloorToInt(currentScore);

        HandleTopScores(finalScore);
        
        // Start the delayed UI showing
        StartCoroutine(ShowGameOverUI());
    }

    IEnumerator ShowGameOverUI()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(gameOverDelay);

        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);

            // Smooth fade-in if CanvasGroup is assigned
            if (gameOverCanvasGroup != null)
            {
                gameOverCanvasGroup.alpha = 0f;
                while (gameOverCanvasGroup.alpha < 1f)
                {
                    gameOverCanvasGroup.alpha += Time.unscaledDeltaTime * 2f; // Fade in over 0.5s
                    yield return null;
                }
            }
        }

        if (quitText != null)
            quitText.gameObject.SetActive(true);
            
        if (restartText != null)
            restartText.gameObject.SetActive(true);
    }

    void HandleTopScores(int newScore)
    {
        // 1. Load existing scores
        string scoresString = PlayerPrefs.GetString(TopScoresKey, "");
        List<int> highScores = new List<int>();

        if (!string.IsNullOrEmpty(scoresString))
        {
            highScores = scoresString.Split(',').Select(int.Parse).ToList();
        }

        // 2. Add new score, sort and keep top 5
        highScores.Add(newScore);
        highScores = highScores.OrderByDescending(s => s).Take(5).ToList();

        // 3. Save back to PlayerPrefs
        string newScoresString = string.Join(",", highScores);
        PlayerPrefs.SetString(TopScoresKey, newScoresString);
        PlayerPrefs.Save();

        // 4. Update UI
        if (highscoreText != null)
        {
            highscoreText.text = "Current Best: " + highScores[0].ToString();
            highscoreText.gameObject.SetActive(true);
        }

        if (topScoresText != null)
        {
            string display = "TOP 5 SCORES\n";
            for (int i = 0; i < highScores.Count; i++)
            {
                display += (i + 1) + ". " + highScores[i] + "\n";
            }
            topScoresText.text = display;
            topScoresText.gameObject.SetActive(true);
        }
    }

    void RestartGame()
    {
        // Ensure time is moving before loading new scene
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}