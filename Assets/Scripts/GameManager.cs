using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton: every other script accesses GameManager via GameManager.instance
    public static GameManager instance { get; private set; }

    [Header("References")]
    public Paddle      paddle;
    public BallManager ballManager;

    [Header("UI Screens")]
    public GameOverScreen gameOverScreen;
    public WinScreen winScreen;

    [Header("Settings")]
    public int startingLives = 3;

    public int   lives       { get; private set; }
    public int   score       { get; private set; }
    public int   highscore   { get; private set; }
    public float sessionTime { get; private set; }
    public int   combo       { get; private set; }

    // Tracks how many blocks are still alive on the field
    private int _remainingBlocks = 0;

    // True while a game-ending screen is shown – stops the session timer
    private bool _gameEnded = false;

    private const string HighscoreKey = "Arkanoid_Highscore";

    private static readonly int[] ComboThresholds  = { 0, 5, 10, 20 };
    private static readonly int[] ComboMultipliers = { 1, 2,  3,  4 };
    
    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;

        highscore = PlayerPrefs.GetInt(HighscoreKey, 0);
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        
        lives       = startingLives;
        score       = 0;
        combo       = 0;
        sessionTime = 0f;
        _gameEnded  = false;

        if (paddle != null && paddle.ball != null)
            ballManager.RegisterBall(paddle.ball);
    }

    void Update()
    {
        if (!_gameEnded)
            sessionTime += Time.deltaTime;
    }

    // Block.cs calls this in Awake() so we always know the total count
    public void RegisterBlock()
    {
        _remainingBlocks++;
    }

    // Block.cs calls this just before Destroy(gameObject)
    public void UnregisterBlock()
    {
        _remainingBlocks--;

        // If no blocks remain the player has cleared the level
        if (_remainingBlocks <= 0)
            TriggerWin();
    }

    // Called by BallManager when every active ball has been lost
    public void OnAllBallsLost()
    {
        lives--;
        combo = 0;

        Debug.Log($"Life lost! Lives remaining: {lives}");

        if (lives <= 0)
            TriggerGameOver();
        else
        {
            GameObject obj = Instantiate(ballManager.ballPrefab,
                                         paddle.transform.position, Quaternion.identity);
            Ball newBall   = obj.GetComponent<Ball>();
            ballManager.RegisterBall(newBall);
            paddle.AttachBall(newBall);
        }
    }

    // Score
    public void AddScore(int basePoints)
    {
        int multiplier = GetComboMultiplier();
        int earned     = basePoints * multiplier;

        score += earned;
        combo++;

        if (score > highscore)
        {
            highscore = score;
            PlayerPrefs.SetInt(HighscoreKey, highscore);
            PlayerPrefs.Save();
        }

        Debug.Log($"Score: {score}  |  Combo: {combo}  |  x{multiplier}");
    }

    public int GetComboMultiplier()
    {
        for (int i = ComboThresholds.Length - 1; i >= 0; i--)
        {
            if (combo >= ComboThresholds[i])
                return ComboMultipliers[i];
        }
        return 1;
    }

    // Game-ending states
    private void TriggerGameOver()
    {
        if (_gameEnded) return;
        _gameEnded = true;

        Debug.Log("GAME OVER");

        // Show cursor so the player can click the buttons
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;

        if (gameOverScreen != null)
            gameOverScreen.Show(score, highscore);
        else
            Debug.LogWarning("[GameManager] gameOverScreen is not assigned!");
    }

    private void TriggerWin()
    {
        if (_gameEnded) return;
        _gameEnded = true;

        Debug.Log("YOU WIN!");

        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;

        if (winScreen != null)
            winScreen.Show(score, highscore);
        else
            Debug.LogWarning("[GameManager] winScreen is not assigned!");
    }

    // Reloads the current scene (Retry)
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Loads the main menu scene
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // Power-up activation – called by PowerUp.cs via OnTriggerEnter
    public void ActivatePowerUp(PowerUpType type, float duration)
    {
        switch (type)
        {
            case PowerUpType.WidePaddle:
                StartCoroutine(WidePaddleRoutine(duration));
                break;
            case PowerUpType.SlowBall:
                StartCoroutine(SlowBallRoutine(duration));
                break;
            case PowerUpType.MultiBall:
                ballManager.SpawnExtraBalls();
                break;
            case PowerUpType.BulldozerBall:
                ballManager.SetBulldozerMode(true);
                break;
        }

        Debug.Log($"[GameManager] PowerUp activated: {type} for {duration}s");
    }

    private IEnumerator WidePaddleRoutine(float duration)
    {
        paddle.SetWidth(2f);
        yield return new WaitForSeconds(duration);
        paddle.SetWidth(1f);
    }

    private IEnumerator SlowBallRoutine(float duration)
    {
        ballManager.SetAllBallSpeedMultiplier(0.5f);
        yield return new WaitForSeconds(duration);
        ballManager.SetAllBallSpeedMultiplier(1f);
    }
    
    // Called by the ExitGame button on the GameOver and Win screens
    public void ExitGame()
    {
        Time.timeScale = 1f;
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;

    #if UNITY_EDITOR
        ResetHighscore();
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        ResetHighscore();
        Application.Quit();
    #endif
    }
    
    // Resets the saved highscore – call this from a button in your settings or pause menu
    public void ResetHighscore()
    {
        highscore = 0;
        PlayerPrefs.SetInt(HighscoreKey, 0);
        PlayerPrefs.Save();
    }
}