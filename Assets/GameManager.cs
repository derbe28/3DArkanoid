using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public Paddle paddle;
    public Ball ball;

    [Header("Settings")]
    public int startingLives = 3;

    public int lives { get; private set; }
    public int score { get; private set; }
    

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        lives = startingLives;
        score = 0;
    }
    
    // Called by Ball.cs when the ball enters the Out Zone.
    public void OnBallLost(Ball lostBall)
    {
        lives--;
        Debug.Log($"Life lost! Lives left: {lives}");

        if (lives <= 0)
        {
            Debug.Log("GAME OVER");
            // TODO: Show Game Over screen later
        }
        else
        {
            lostBall.ResetBall();
            paddle.AttachBall(lostBall);
        }
    }

    // Called by Block.cs when a block is destroyed.
    public void AddScore(int points)
    {
        score += points;
        Debug.Log($"Score: {score}");
        // TODO: Update UI text later
    }
}
