using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [Header("References")]
    public Paddle paddle;
    public BallManager ballManager;

    [Header("Settings")]
    public int startingLives = 3;

    private int lives { get; set; }
    private int score { get; set; }

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }

    void Start()
    {
        lives = startingLives;
        score = 0;

        // Register the starting ball so BallManager knows about it
        if (paddle != null && paddle.ball != null)
            ballManager.RegisterBall(paddle.ball);
    }

    // Called by BallManager when every active ball has been lost
    public void OnAllBallsLost()
    {
        lives--;
        Debug.Log($"Life lost! Lives remaining: {lives}");

        if (lives <= 0)
        {
            Debug.Log("GAME OVER");
            // TODO: Show Game Over screen
        }
        else
        {
            // Spawn a fresh ball attached to the paddle
            GameObject obj = Instantiate(ballManager.ballPrefab,
                                         paddle.transform.position, Quaternion.identity);
            Ball newBall = obj.GetComponent<Ball>();
            ballManager.RegisterBall(newBall);
            paddle.AttachBall(newBall);
        }
    }

    // Called by Block.cs when a block is destroyed
    public void AddScore(int points)
    {
        score += points;
        Debug.Log($"Score: {score}");
        // TODO: Update UI text
    }

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
                // No timer – extra balls just play until lost
                ballManager.SpawnExtraBalls();
                break;
            case PowerUpType.BulldozerBall:
                ballManager.SetBulldozerMode(true);
                break;
        }

        Debug.Log($"[GameManager] PowerUp activated: {type} for {duration}s");
    }

    // Doubles the paddle width, then restores it after the duration
    private IEnumerator WidePaddleRoutine(float duration)
    {
        paddle.SetWidth(2f);
        yield return new WaitForSeconds(duration);
        paddle.SetWidth(1f);
    }

    // Halves all ball speeds, then restores them after the duration
    private IEnumerator SlowBallRoutine(float duration)
    {
        ballManager.SetAllBallSpeedMultiplier(0.5f);
        yield return new WaitForSeconds(duration);
        ballManager.SetAllBallSpeedMultiplier(1f);
    }
}