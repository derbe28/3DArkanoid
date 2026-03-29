using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton: every other script accesses GameManager via GameManager.instance
    public static GameManager instance { get; private set; }

    [Header("References")]
    public Paddle      paddle;
    public BallManager ballManager;
    public UIManager   uiManager;   // drag the UIManager GameObject here

    [Header("Settings")]
    public int startingLives = 3;

    // ---------------------------------------------------------------
    // Public read-only game state  –  UIManager reads these every frame
    // ---------------------------------------------------------------
    public int   lives       { get; private set; }
    public int   score       { get; private set; }
    public int   highscore   { get; private set; }
    public float sessionTime { get; private set; }  // seconds elapsed since Start
    public int   combo       { get; private set; }  // consecutive block hits without losing the ball

    // Key used to save the highscore via PlayerPrefs (persists between sessions)
    private const string HighscoreKey = "Arkanoid_Highscore";

    // Combo thresholds and their matching score multipliers.
    // Example: 10 blocks in a row -> every block gives x3 points.
    private static readonly int[] ComboThresholds  = { 0, 5, 10, 20 };
    private static readonly int[] ComboMultipliers = { 1, 2,  3,  4 };

    // ---------------------------------------------------------------
    // Unity lifecycle
    // ---------------------------------------------------------------
    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        highscore = PlayerPrefs.GetInt(HighscoreKey, 0);
        lives       = startingLives;
        score       = 0;
        combo       = 0;
        sessionTime = 0f;
    }

    void Start()
    {
        // Activate Fullscreen
        Screen.fullScreen = true;
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;

        // Mouse visibility
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        // Register the starting ball so BallManager knows about it
        if (paddle != null && paddle.ball != null)
            ballManager.RegisterBall(paddle.ball);
    }

    void Update()
    {
        // Increment the session timer every frame
        sessionTime += Time.deltaTime;
    }

    // ---------------------------------------------------------------
    // Called by BallManager when every active ball has been lost
    // ---------------------------------------------------------------
    public void OnAllBallsLost()
    {
        lives--;
        combo = 0; // losing all balls resets the combo streak

        Debug.Log($"Life lost! Lives remaining: {lives}");

        if (lives <= 0)
        {
            Debug.Log("GAME OVER");
            // TODO: Show Game Over screen
        }
        else
        {
            // Spawn a fresh ball and attach it to the paddle
            GameObject obj = Instantiate(ballManager.ballPrefab,
                                         paddle.transform.position, Quaternion.identity);
            Ball newBall   = obj.GetComponent<Ball>();
            ballManager.RegisterBall(newBall);
            paddle.AttachBall(newBall);
        }
    }

    // ---------------------------------------------------------------
    // Called by Block.cs when a block is destroyed
    // ---------------------------------------------------------------
    public void AddScore(int basePoints)
    {
        int multiplier = GetComboMultiplier();
        int earned     = basePoints * multiplier;

        score += earned;
        combo++;

        // Save a new highscore immediately so it survives an unexpected quit
        if (score > highscore)
        {
            highscore = score;
            PlayerPrefs.SetInt(HighscoreKey, highscore);
            PlayerPrefs.Save();
        }

        Debug.Log($"Score: {score}  |  Combo: {combo}  |  x{multiplier}");
    }

    // Returns the score multiplier that matches the current combo count
    public int GetComboMultiplier()
    {
        for (int i = ComboThresholds.Length - 1; i >= 0; i--)
        {
            if (combo >= ComboThresholds[i])
                return ComboMultipliers[i];
        }
        return 1;
    }

    // ---------------------------------------------------------------
    // Power-up activation  –  called by PowerUp.cs via OnTriggerEnter
    // ---------------------------------------------------------------
    public void ActivatePowerUp(PowerUpType type, float duration)
    {
        switch (type)
        {
            case PowerUpType.WidePaddle:
                StartCoroutine(WidePaddleRoutine(duration));
                // Show a timed entry in the powerup panel
                uiManager?.AddPowerupEntry("Wide Paddle", duration);
                break;

            case PowerUpType.SlowBall:
                StartCoroutine(SlowBallRoutine(duration));
                uiManager?.AddPowerupEntry("Slow Ball", duration);
                break;

            case PowerUpType.MultiBall:
                // No duration – extra balls stay until they fall out
                ballManager.SpawnExtraBalls();
                // Pass -1 so the UI entry has no countdown timer
                uiManager?.AddPowerupEntry("Multi Ball", -1f);
                break;

            case PowerUpType.BulldozerBall:
                // Ball.cs disables bulldozer mode automatically on top-wall hit
                ballManager.SetBulldozerMode(true);
                uiManager?.AddPowerupEntry("Bulldozer", -1f);
                break;
        }

        Debug.Log($"[GameManager] PowerUp activated: {type} for {duration}s");
    }

    // Widens the paddle for 'duration' seconds, then resets it to normal
    private IEnumerator WidePaddleRoutine(float duration)
    {
        paddle.SetWidth(2f);
        yield return new WaitForSeconds(duration);
        paddle.SetWidth(1f);
    }

    // Slows all balls for 'duration' seconds, then restores normal speed
    private IEnumerator SlowBallRoutine(float duration)
    {
        ballManager.SetAllBallSpeedMultiplier(0.5f);
        yield return new WaitForSeconds(duration);
        ballManager.SetAllBallSpeedMultiplier(1f);
    }
}