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

    private Vector3 velocity;
    private bool isLaunched = false;
    private Vector3 previousPosition;

    void Update()
    {
        if (!isLaunched)
            return;

        previousPosition = transform.position;

        transform.position += velocity * Time.deltaTime;

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
            velocity.x = Mathf.Abs(velocity.x);
        }

        // Right wall
        if (pos.x + ballRadius >= rightBound)
        {
            pos.x = rightBound - ballRadius;
            velocity.x = -Mathf.Abs(velocity.x);
        }

        // Top wall
        if (pos.z + ballRadius >= topBound)
        {
            pos.z = topBound - ballRadius;
            velocity.z = -Mathf.Abs(velocity.z);
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
            transform.position = previousPosition;

            // Calculate overlap in X and Z
            float overlapLeft = Mathf.Abs((previousPosition.x + ballRadius) - blockBounds.min.x);
            float overlapRight = Mathf.Abs((blockBounds.max.x) - (previousPosition.x - ballRadius));
            float overlapX = Mathf.Min(overlapLeft, overlapRight);

            float overlapBottom = Mathf.Abs((previousPosition.z + ballRadius) - blockBounds.min.z);
            float overlapTop = Mathf.Abs((blockBounds.max.z) - (previousPosition.z - ballRadius));
            float overlapZ = Mathf.Min(overlapBottom, overlapTop);

            // Smaller overlap tells us which side was hit
            if (overlapX < overlapZ)
            {
                velocity.x = -velocity.x;
            }
            else
            {
                velocity.z = -velocity.z;
            }

            Block block = hit.GetComponent<Block>();
            if (block != null)
            {
                block.TakeHit();
            }

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

    public void Launch()
    {
        isLaunched = true;

        float randomX = Random.Range(-0.5f, 0.5f);
        Vector3 direction = new Vector3(randomX, 0f, 1f).normalized;

        velocity = direction * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Out"))
        {
            BallLost();
            return;
        }

        if (!isLaunched)
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

        velocity = new Vector3(newX, 0f, newZ) * speed;
    }

    private void BallLost()
    {
        isLaunched = false;
        velocity = Vector3.zero;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBallLost(this);
        }
    }

    public void ResetBall()
    {
        isLaunched = false;
        velocity = Vector3.zero;
    }
}