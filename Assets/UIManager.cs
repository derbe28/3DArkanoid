using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// UIManager reads state from GameManager every frame and updates all HUD elements.
//
// SETUP: Attach this script to an empty GameObject called "UIManager".
// Then drag the matching UI objects from the Hierarchy into each Inspector field.
public class UIManager : MonoBehaviour
{
    // ---------------------------------------------------------------
    // Inspector fields – drag the matching UI objects here in Unity
    // ---------------------------------------------------------------

    [Header("Left Panel – Timer (top)")]
    public TMP_Text timerText;

    [Header("Left Panel – Lives (bottom)")]
    // A TMP_Text that will show hearts, e.g.  "♥ ♥ ♥"
    public TMP_Text livesText;

    [Header("Left Panel – Active Power-ups (middle)")]
    // Empty GameObject with a Vertical Layout Group component
    public Transform  powerupPanel;
    // Prefab: a simple UI object containing one TMP_Text child and one Slider child
    // (You can make this in 5 minutes – see setup notes below)
    public GameObject powerupEntryPrefab;

    [Header("Right Panel – Score")]
    public TMP_Text scoreText;
    public TMP_Text highscoreText;

    [Header("Right Panel – Combo")]
    public TMP_Text comboText;
    // Optional: an Image (Image Type = Filled) that visualises progress to the next combo tier
    public Image    comboFillBar;

    // ---------------------------------------------------------------
    // Private state
    // ---------------------------------------------------------------

    // Only rebuild the lives display when the count actually changes
    private int _lastLivesDisplayed = -1;

    // One entry per currently active power-up shown in the left panel
    private readonly List<PowerupUIEntry> _activePowerupEntries = new List<PowerupUIEntry>();

    // ---------------------------------------------------------------
    // Unity lifecycle
    // ---------------------------------------------------------------
    void Update()
    {
        // Safety check: if GameManager is not ready yet, do nothing
        if (GameManager.instance == null) return;

        UpdateTimer();
        UpdateLives();
        UpdateScore();
        UpdateCombo();
        TickPowerupEntries();
    }

    // ---------------------------------------------------------------
    // Timer  –  MM:SS format
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
    // Lives  –  simple heart symbols, e.g. "♥ ♥ ♥"
    // Rebuilds only when the count changes (not every frame)
    // ---------------------------------------------------------------
    private void UpdateLives()
    {
        if (livesText == null) return;

        int current = GameManager.instance.lives;
        if (current == _lastLivesDisplayed) return; // nothing changed, skip

        _lastLivesDisplayed = current;

        // Build a string with one heart per remaining life, separated by spaces
        // Lost lives are shown as empty hearts so the player sees the maximum
        int maxLives = GameManager.instance.startingLives;
        var sb = new System.Text.StringBuilder();

        for (int i = 0; i < maxLives; i++)
        {
            if (i > 0) sb.Append("  "); // spacing between hearts
            sb.Append(i < current ? "♥" : "♡"); // filled or empty heart
        }

        livesText.text = sb.ToString();
    }

    // ---------------------------------------------------------------
    // Score & highscore  –  always 6 digits with leading zeros
    // ---------------------------------------------------------------
    private void UpdateScore()
    {
        if (scoreText     != null) scoreText.text     = GameManager.instance.score.ToString("D6");
        if (highscoreText != null) highscoreText.text = GameManager.instance.highscore.ToString("D6");
    }

    // ---------------------------------------------------------------
    // Combo multiplier
    // ---------------------------------------------------------------
    private void UpdateCombo()
    {
        if (comboText == null) return;

        int multiplier = GameManager.instance.GetComboMultiplier();
        int combo      = GameManager.instance.combo;

        comboText.text = $"x{multiplier}";

        // Color changes with multiplier level so the player notices it at a glance
        comboText.color = multiplier switch
        {
            4 => new Color(0.90f, 0.15f, 0.15f),  // red    – x4
            3 => new Color(1.00f, 0.50f, 0.00f),  // orange – x3
            2 => new Color(1.00f, 0.90f, 0.10f),  // yellow – x2
            _ => Color.white,                      // white  – x1
        };

        // Optional fill bar: how far through the current tier is the combo?
        if (comboFillBar != null)
        {
            // These thresholds must match the ones in GameManager
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
    
    public PowerupUIEntry AddPowerupEntry(string label, float duration)
    {
        if (powerupPanel == null || powerupEntryPrefab == null)
        {
            Debug.LogWarning("[UIManager] powerupPanel or powerupEntryPrefab is not set!");
            return null;
        }

        GameObject     go    = Instantiate(powerupEntryPrefab, powerupPanel);
        PowerupUIEntry entry = new PowerupUIEntry(go, label, duration);
        _activePowerupEntries.Add(entry);
        return entry;
    }

    // Removes a power-up entry before its timer naturally expires
    public void RemovePowerupEntry(PowerupUIEntry entry)
    {
        if (entry == null) return;
        _activePowerupEntries.Remove(entry);
        if (entry.rootObject != null) Destroy(entry.rootObject);
    }

    // Called every frame – counts down timers and removes expired entries
    private void TickPowerupEntries()
    {
        for (int i = _activePowerupEntries.Count - 1; i >= 0; i--)
        {
            PowerupUIEntry entry = _activePowerupEntries[i];
            entry.Tick(Time.deltaTime);

            if (entry.IsExpired())
                RemovePowerupEntry(entry);
        }
    }

    public class PowerupUIEntry
    {
        // The instantiated prefab GameObject (used to destroy it later)
        public GameObject rootObject { get; }

        private readonly string  _label;
        private readonly float   _totalDuration; // -1 = infinite
        private          float   _remaining;

        private readonly TMP_Text _text;          // the text child in the prefab
        private readonly Slider   _slider;        // the slider child in the prefab

        public PowerupUIEntry(GameObject root, string label, float duration)
        {
            rootObject     = root;
            _label         = label;
            _totalDuration = duration;
            _remaining     = duration;

            // Automatically find the text and slider anywhere inside the prefab
            _text   = root.GetComponentInChildren<TMP_Text>();
            _slider = root.GetComponentInChildren<Slider>();

            // Initialise the slider to full
            if (_slider != null)
            {
                _slider.minValue = 0f;
                _slider.maxValue = 1f;
                _slider.value    = 1f;
            }

            RefreshDisplay();
        }

        // Called every frame by UIManager.TickPowerupEntries()
        public void Tick(float deltaTime)
        {
            if (_totalDuration < 0f) return; // infinite – nothing to count down

            _remaining = Mathf.Max(_remaining - deltaTime, 0f);

            if (_slider != null)
                _slider.value = _remaining / _totalDuration;

            RefreshDisplay();
        }

        // Returns true once the countdown reaches zero
        public bool IsExpired() => _totalDuration >= 0f && _remaining <= 0f;

        private void RefreshDisplay()
        {
            if (_text == null) return;

            // Show the remaining time rounded up, or just the label if infinite
            _text.text = _totalDuration < 0f
                ? _label
                : $"{_label}  {Mathf.CeilToInt(_remaining)}s";
        }
    }
}