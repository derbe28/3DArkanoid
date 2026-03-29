using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Speed")]
    public float speed = 8f;

    [Header("Wall Boundaries")]
    public float leftBound = -4.75f;
    public float rightBound = 4.75f;
    public float topBound = 7.25f;

    [Header("Paddle Snap")]
    public float snapOffsetZ = 0.5f;

    [Header("Ball Size")]
    public float ballRadius = 0.25f;

    private Vector3 _velocity;
    private bool _isLaunched = false;
    private Vector3 _previousPosition;

    // Speed multiplier – changed by the SlowBall power-up
    private float _speedMultiplier = 1f;

    // If true, the ball passes through blocks without bouncing (BulldozerBall power-up)
    private bool _isBulldozer = false;
    
    private bool _isLost = false;

    // Cached reference to the MeshRenderer for color changes
    private MeshRenderer _meshRenderer;

    void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if (!_isLaunched)
            return;

        _previousPosition = transform.position;

        transform.position += _velocity * Time.deltaTime;

        CheckWallCollision();
        CheckBlockCollision();
    }

    private void CheckWallCollision()
    {
        Vector3 pos = transform.position;

        // Left wall
        if (pos.x - ballRadius <= leftBound)
        {
            pos.x = leftBound + ballRadius;
            _velocity.x = Mathf.Abs(_velocity.x);
        }

        // Right wall
        if (pos.x + ballRadius >= rightBound)
        {
            pos.x = rightBound - ballRadius;
            _velocity.x = -Mathf.Abs(_velocity.x);
        }

        // Top wall
        if (pos.z + ballRadius >= topBound)
        {
            pos.z = topBound - ballRadius;
            _velocity.z = -Mathf.Abs(_velocity.z);
            
            if (_isBulldozer)
                SetBulldozerMode(false);
        }

        transform.position = pos;
    }

    private void CheckBlockCollision()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            ballRadius,
            Physics.AllLayers,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Block"))
                continue;

            Bounds blockBounds = hit.bounds;

            // Put the ball back to the last safe position
            transform.position = _previousPosition;

            // Calculate overlap in X and Z
            float overlapLeft  = Mathf.Abs((_previousPosition.x + ballRadius) - blockBounds.min.x);
            float overlapRight = Mathf.Abs((blockBounds.max.x) - (_previousPosition.x - ballRadius));
            float overlapX     = Mathf.Min(overlapLeft, overlapRight);

            float overlapBottom = Mathf.Abs((_previousPosition.z + ballRadius) - blockBounds.min.z);
            float overlapTop    = Mathf.Abs((blockBounds.max.z) - (_previousPosition.z - ballRadius));
            float overlapZ      = Mathf.Min(overlapBottom, overlapTop);

            // Only bounce if NOT in bulldozer mode
            if (!_isBulldozer)
            {
                if (overlapX < overlapZ)
                    _velocity.x = -_velocity.x;
                else
                    _velocity.z = -_velocity.z;
            }

            // Always damage the block, regardless of bulldozer mode
            Block block = hit.GetComponent<Block>();
            if (block != null)
                block.TakeHit();

            // In bulldozer mode we keep going, so no break – hit every block we overlap
            if (!_isBulldozer)
                break;
        }
    }

    public void SnapToPaddle(Vector3 paddlePosition)
    {
        transform.position = new Vector3(
            paddlePosition.x,
            paddlePosition.y,
            paddlePosition.z + snapOffsetZ
        );
    }

    // Standard launch with a random direction
    public void Launch()
    {
        _isLaunched = true;

        float randomX = Random.Range(-0.5f, 0.5f);
        Vector3 direction = new Vector3(randomX, 0f, 1f).normalized;

        _velocity = direction * (speed * _speedMultiplier);
    }

    // Launches the ball in a specific direction – used by BallManager for extra balls
    public void LaunchInDirection(Vector3 direction)
    {
        _isLaunched = true;
        _velocity = direction.normalized * speed * _speedMultiplier;
    }

    // Called by the SlowBall power-up to halve (or restore) the ball speed
    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = multiplier;

        // Adjust the current velocity immediately so the change feels instant
        if (_isLaunched)
            _velocity = _velocity.normalized * (speed * _speedMultiplier);
    }

    // Called by the BulldozerBall power-up to toggle pass-through mode
    public void SetBulldozerMode(bool active)
    {
        _isBulldozer = active;

        // Change the ball color so the player can see the effect is active
        if (_meshRenderer != null)
            _meshRenderer.material.color = active ? Color.red : Color.white;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Out"))
        {
            BallLost();
            return;
        }

        if (!_isLaunched)
            return;

        if (other.CompareTag("Paddle"))
        {
            BounceOffPaddle(other);
        }
    }

    private void BounceOffPaddle(Collider paddle)
    {
        float hitOffset = (transform.position.x - paddle.bounds.center.x)
                          / (paddle.bounds.size.x / 2f);

        hitOffset = Mathf.Clamp(hitOffset, -0.8f, 0.8f);

        float newX = hitOffset;
        float newZ = Mathf.Sqrt(1f - newX * newX);

        _velocity = new Vector3(newX, 0f, newZ) * speed * _speedMultiplier;
    }

    private void BallLost()
    {
        if (_isLost) return;
        _isLost = true;
        _isLaunched = false;
        _velocity    = Vector3.zero;

        // Tell BallManager this ball is gone – it decides whether to lose a life
        if (BallManager.instance != null)
            BallManager.instance.UnregisterBall(this);
        
        Debug.Log("Ball Lost");

        Destroy(gameObject);
    }

    public void ResetBall()
    {
        _isLaunched    = false;
        _velocity      = Vector3.zero;
        _speedMultiplier = 1f;
        _isBulldozer   = false;

        if (_meshRenderer != null)
            _meshRenderer.material.color = Color.white;
    }
}