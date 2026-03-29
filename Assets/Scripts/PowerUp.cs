using UnityEngine;

// All possible power-up types – add new entries here when you add new power-ups
public enum PowerUpType
{
    MultiBall,
    WidePaddle,
    SlowBall,
    BulldozerBall
}

public class PowerUp : MonoBehaviour
{
    [Tooltip("Which effect this power-up has when collected")]
    public PowerUpType type;

    [Tooltip("How fast the power-up falls toward the paddle")]
    public float fallSpeed = 3f;

    [Tooltip("How long the effect lasts in seconds (not used by MultiBall and BulldozerBall)")]
    public float duration = 8f;

    void Update()
    {
        // Move the power-up downward (toward the paddle) each frame
        transform.position += Vector3.back * (fallSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Paddle"))
        {
            GameManager.instance.ActivatePowerUp(type, duration);
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Out"))
        {
            Destroy(gameObject);
        }
    }
}