using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton: every other script accesses GameManager via GameManager.instance
    public static GameManager instance { get; private set; }

    [Header("References")]
    public Paddle      paddle;
    public BallManager ballManager;

    [Header("Settings")]
    public int startingLives = 3;

    // ---------------------------------------------------------------
    // Public read-only game state – UIManager reads these every frame
    // ---------------------------------------------------------------
    public int   lives       { get; private set; }
    public int   score       { get; private set; }
    public int   highscore   { get; private set; }
    public float sessionTime { get; private set; }
    public int   combo       { get; private set; }

    // Key used to save the highscore between play sessions
    private const string HighscoreKey = "Arkanoid_Highscore";

    // Combo thresholds and their matching score multipliers
    // Example: 10 blocks in a row → every block gives x3 points
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
    }

    void Start()
    {
        lives       = startingLives;
        score       = 0;
        combo       = 0;
        sessionTime = 0f;

        if (paddle != null && paddle.ball != null)
            ballManager.RegisterBall(paddle.ball);
    }

    void Update()
    {
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
            GameObject obj = Instantiate(ballManager.ballPrefab, paddle.transform.position, Quaternion.identity);
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

    // Returns the score multiplier for the current combo count
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
    // Power-up activation – called by PowerUp.cs via OnTriggerEnter
    // ---------------------------------------------------------------
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
                // Ball.cs disables bulldozer mode automatically on top-wall hit
                ballManager.SetBulldozerMode(true);
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