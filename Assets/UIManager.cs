using UnityEngine;
using UnityEngine.UI;
using TMPro;

// UIManager reads state from GameManager every frame and updates all HUD elements.
// Attach this script to an empty GameObject called "UIManager".
// Then drag the matching UI objects from the Hierarchy into each Inspector field.
public class UIManager : MonoBehaviour
{
    // ---------------------------------------------------------------
    // Inspector fields – drag the matching UI objects here in Unity
    // ---------------------------------------------------------------

    [Header("Left Panel – Timer (top)")]
    public TMP_Text timerText;

    [Header("Left Panel – Lives (bottom)")]
    public TMP_Text livesText;

    [Header("Right Panel – Score")]
    public TMP_Text scoreText;
    public TMP_Text highscoreText;

    [Header("Right Panel – Combo")]
    public TMP_Text comboText;
    // Optional: an Image (Image Type = Filled) that shows progress to the next combo tier
    public Image comboFillBar;

    // ---------------------------------------------------------------
    // Private state
    // ---------------------------------------------------------------

    // Only rebuild the lives display when the count actually changes
    private int _lastLivesDisplayed = -1;

    // ---------------------------------------------------------------
    // Unity lifecycle
    // ---------------------------------------------------------------
    void Update()
    {
        if (GameManager.instance == null) return;

        UpdateTimer();
        UpdateLives();
        UpdateScore();
        UpdateCombo();
    }

    // ---------------------------------------------------------------
    // Timer – MM:SS format, counts up
    // ---------------------------------------------------------------
    private void UpdateTimer()
    {
        if (timerText == null) return;

        float t   = GameManager.instance.sessionTime;
        int   min = Mathf.FloorToInt(t / 60f);
        int   sec = Mathf.FloorToInt(t % 60f);

        timerText.text = $"{min:00}:{sec:00}";
    }

    // ---------------------------------------------------------------
    // Lives – filled hearts for remaining lives, dark hearts for lost ones
    // Rebuilds only when the count changes, not every frame
    // ---------------------------------------------------------------
    private void UpdateLives()
    {
        if (livesText == null) return;

        int current = GameManager.instance.lives;
        if (current == _lastLivesDisplayed) return;

        _lastLivesDisplayed = current;

        int maxLives = GameManager.instance.startingLives;
        var sb = new System.Text.StringBuilder();

        for (int i = 0; i < maxLives; i++)
        {
            if (i > 0) sb.Append("  ");
            // Dark grey heart for lost lives – avoids missing-glyph warning from ♡
            sb.Append(i < current ? "♥" : "<color=#444444>♥</color>");
        }

        livesText.text = sb.ToString();
    }

    // ---------------------------------------------------------------
    // Score & highscore – always 6 digits with leading zeros
    // ---------------------------------------------------------------
    private void UpdateScore()
    {
        if (scoreText     != null) scoreText.text     = GameManager.instance.score.ToString("D6");
        if (highscoreText != null) highscoreText.text = GameManager.instance.highscore.ToString("D6");
    }

    // ---------------------------------------------------------------
    // Combo multiplier – text color changes with the multiplier level
    // ---------------------------------------------------------------
    private void UpdateCombo()
    {
        if (comboText == null) return;

        int multiplier = GameManager.instance.GetComboMultiplier();
        int combo      = GameManager.instance.combo;

        comboText.text = $"x{multiplier}";

        // Color shifts from white to yellow to orange to red as the multiplier rises
        comboText.color = multiplier switch
        {
            4 => new Color(0.90f, 0.15f, 0.15f),  // red    – x4
            3 => new Color(1.00f, 0.50f, 0.00f),  // orange – x3
            2 => new Color(1.00f, 0.90f, 0.10f),  // yellow – x2
            _ => Color.white,                      // white  – x1
        };

        // Optional fill bar: shows progress toward the next combo tier
        if (comboFillBar != null)
        {
            // Thresholds must match the ones defined in GameManager
            int[] thresholds = { 0, 5, 10, 20 };
            float fill = 1f; // default: already at the highest tier

            for (int i = 0; i < thresholds.Length - 1; i++)
            {
                if (combo < thresholds[i + 1])
                {
                    fill = (float)(combo - thresholds[i])
                         / (thresholds[i + 1] - thresholds[i]);
                    break;
                }
            }

            comboFillBar.fillAmount = fill;
        }
    }
}