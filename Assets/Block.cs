using UnityEngine;

public class Block : MonoBehaviour
{
    [Header("Block Type")]
    [Tooltip("How many hits this block can take before it breaks")]
    [Range(1, 4)]
    public int health = 1;

    private MeshRenderer meshRenderer;

    // Color for each health level
    private static readonly Color ColorRed    = new Color(0.9f, 0.15f, 0.15f);
    private static readonly Color ColorOrange = new Color(1.0f, 0.50f, 0.00f);
    private static readonly Color ColorYellow = new Color(1.0f, 0.90f, 0.10f);
    private static readonly Color ColorGreen  = new Color(0.2f, 0.80f, 0.20f);

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        UpdateColor();
    }

    public void TakeHit()
    {
        health--;

        if (health <= 0)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.AddScore(10);

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

    private void UpdateColor()
    {
        if (meshRenderer == null) return;

        switch (health)
        {
            case 4: meshRenderer.material.color = ColorRed;    break;
            case 3: meshRenderer.material.color = ColorOrange; break;
            case 2: meshRenderer.material.color = ColorYellow; break;
            default:meshRenderer.material.color = ColorGreen;  break;
        }
    }
}
