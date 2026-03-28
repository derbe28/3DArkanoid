using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    public static BallManager instance { get; private set; }

    [Header("References")]
    [Tooltip("Drag your Ball prefab here")]
    public GameObject ballPrefab;

    // Keeps track of every ball currently in play
    private readonly List<Ball> _activeBalls = new List<Ball>();

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }

    // Called once at startup (by GameManager) to register the first ball
    public void RegisterBall(Ball ball)
    {
        if (ball != null && !_activeBalls.Contains(ball))
            _activeBalls.Add(ball);
    }

    // Called by Ball.cs when it enters the Out zone and destroys itself
    public void UnregisterBall(Ball ball)
    {
        _activeBalls.Remove(ball);

        // If every ball is gone, tell GameManager to deduct a life
        if (_activeBalls.Count == 0)
            GameManager.instance.OnAllBallsLost();
    }

    // Spawns 2 extra balls at the position of the first active ball (MultiBall power-up)
    public void SpawnExtraBalls()
    {
        if (_activeBalls.Count == 0 || ballPrefab == null) return;

        Vector3 spawnPos = _activeBalls[0].transform.position;

        for (int i = 0; i < 2; i++)
        {
            GameObject obj = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
            Ball newBall   = obj.GetComponent<Ball>();

            if (newBall != null)
            {
                newBall.LaunchInDirection(GetExtraDirection(i));
                RegisterBall(newBall);
            }
        }
    }

    // Gives each extra ball a slightly different angle so they spread out
    private Vector3 GetExtraDirection(int index)
    {
        float[] offsets = { -0.4f, 0.4f };
        float x = (index < offsets.Length) ? offsets[index] : Random.Range(-0.5f, 0.5f);
        return new Vector3(x, 0f, 1f).normalized;
    }

    // Sets the speed multiplier on every active ball (SlowBall power-up)
    public void SetAllBallSpeedMultiplier(float multiplier)
    {
        foreach (Ball b in _activeBalls)
            b.SetSpeedMultiplier(multiplier);
    }

    // Toggles bulldozer mode on every active ball (BulldozerBall power-up)
    public void SetBulldozerMode(bool active)
    {
        foreach (Ball b in _activeBalls)
            b.SetBulldozerMode(active);
    }

    // Read-only access to the active ball list (useful for future features)
    public List<Ball> GetActiveBalls() => _activeBalls;
}