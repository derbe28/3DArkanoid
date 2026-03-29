using UnityEngine;

public class Block : MonoBehaviour
{
    [Header("Block Type")]
    [Tooltip("How many hits this block can take before it breaks")]
    [Range(1, 4)]
    public int health = 1;

    [Header("Power-Up Drop")]
    public GameObject[] powerUpPrefabs;

    [Tooltip("Probability (0 = never, 1 = always) that this block drops a power-up")]
    [Range(0f, 1f)]
    public float dropChance = 0.25f;

    private MeshRenderer _meshRenderer;

    private static readonly Color ColorRed    = new Color(0.9f, 0.15f, 0.15f);
    private static readonly Color ColorOrange = new Color(1.0f, 0.50f, 0.00f);
    private static readonly Color ColorYellow = new Color(1.0f, 0.90f, 0.10f);
    private static readonly Color ColorGreen  = new Color(0.2f, 0.80f, 0.20f);

    void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        UpdateColor();

        // Tell GameManager this block exists so it can track the remaining count
        if (GameManager.instance != null)
            GameManager.instance.RegisterBlock();
    }

    public void TakeHit()
    {
        health--;

        if (health <= 0)
        {
            // Add score before unregistering so the win check fires after the score is updated
            if (GameManager.instance != null)
                GameManager.instance.AddScore(10);

            TryDropPowerUp();

            // Tell GameManager this block is gone – may trigger the win condition
            if (GameManager.instance != null)
                GameManager.instance.UnregisterBlock();

            Destroy(gameObject);
        }
        else
        {
            UpdateColor();
        }
    }

    public void SetHealth(int newHealth)
    {
        health = Mathf.Clamp(newHealth, 1, 4);
        UpdateColor();
    }

    private void TryDropPowerUp()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;

        if (Random.value <= dropChance)
        {
            int index = Random.Range(0, powerUpPrefabs.Length);
            Instantiate(powerUpPrefabs[index], transform.position, Quaternion.identity);
        }
    }

    private void UpdateColor()
    {
        if (_meshRenderer == null) return;

        switch (health)
        {
            case 4:  _meshRenderer.material.color = ColorRed;    break;
            case 3:  _meshRenderer.material.color = ColorOrange; break;
            case 2:  _meshRenderer.material.color = ColorYellow; break;
            default: _meshRenderer.material.color = ColorGreen;  break;
        }
    }
}