using UnityEngine;
using TMPro;
public class GameOverScreen : MonoBehaviour
{
    [Header("Text fields")]
    public TMP_Text scoreText;
    public TMP_Text highscoreText;

    // Called by GameManager.TriggerGameOver()
    public void Show(int finalScore, int highscore)
    {
        gameObject.SetActive(true);

        if (scoreText     != null) scoreText.text     = $"Score\n{finalScore.ToString("D6")}";
        if (highscoreText != null) highscoreText.text = $"Best\n{highscore.ToString("D6")}";
    }
}