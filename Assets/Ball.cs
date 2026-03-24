using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Speed")]
    public float speed = 8f;            // Overall ball speed

    [Header("Wall Boundaries")]
    // These should match your actual wall positions in the scene
    public float leftBound  = -4.75f;   // X position of the left wall
    public float rightBound =  4.75f;   // X position of the right wall
    public float topBound   =  7.25f;   // Z position of the top wall

    [Header("Paddle Snap")]
    public float snapOffsetZ = 0.5f;    // How far in front of the paddle the ball sits

    [Header ("Bounce Protection")]
    public float bounceCooldown = 0.1f;
    private float lastBounceTime = -1f;

    private Vector3 velocity;
    private bool isLaunched = false;

    void Update()
    {
        if (!isLaunched) return;
        transform.position += velocity * Time.deltaTime;
        BounceOffWalls();
    }
    
    private void BounceOffWalls()
    {
        Vector3 pos = transform.position;

        // Left wall
        if (pos.x <= leftBound)
        {
            pos.x = leftBound;
            velocity.x = Mathf.Abs(velocity.x);    // Force direction to the right
            transform.position = pos;
        }
        // Right wall
        else if (pos.x >= rightBound)
        {
            pos.x = rightBound;
            velocity.x = -Mathf.Abs(velocity.x);   // Force direction to the left
            transform.position = pos;
        }

        // Top wall
        if (pos.z >= topBound)
        {
            pos.z = topBound;
            velocity.z = -Mathf.Abs(velocity.z);   // Force direction downward
            transform.position = pos;
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
    // Out-check before isLaunched, so the ball is always caught
    if (other.CompareTag("Out"))
    {
        BallLost();
        return;
    }

    if (!isLaunched) return;

    if (other.CompareTag("Paddle"))
    {
        BounceOffPaddle(other);
    }
    else if (other.CompareTag("Block"))
    {
        
        // Prevent multiple bounces in a short time
        if (Time.time - lastBounceTime < bounceCooldown) return;
        lastBounceTime = Time.time;
        // Find ALL blocks in a small radius around the ball at once
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.6f);
        BounceOffBlock(other);

        Vector3 totalOffset = Vector3.zero;
        int blockCount = 0;

        foreach (Collider hitCol in hitColliders)
        {
            if (!hitCol.CompareTag("Block")) continue;

            // Deal damage to every block in range
            Block block = hitCol.GetComponent<Block>();
            if (block != null)
                block.TakeHit();

            // Accumulate offset direction from each block's center
            totalOffset += transform.position - hitCol.bounds.center;
            blockCount++;
        }

        // Bounce based on the combined offset of all hit blocks
        if (blockCount > 0)
        {
            Vector3 avgOffset = totalOffset / blockCount;

            float overlapX = Mathf.Abs(avgOffset.x);
            float overlapZ = Mathf.Abs(avgOffset.z);

            if (overlapX > overlapZ)
                velocity.x = -velocity.x;
            else
                velocity.z = -velocity.z;
        }
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

    private void BounceOffBlock(Collider block)
    {
        Bounds b = block.bounds;
 
        Vector3 offset = transform.position - b.center;
 
        float overlapX = Mathf.Abs(offset.x) / (b.size.x / 2f);
        float overlapZ = Mathf.Abs(offset.z) / (b.size.z / 2f);
 
        if (overlapX > overlapZ)
            velocity.x = -velocity.x;
        else
            velocity.z = -velocity.z;
    }

    private void BallLost()
    {
        isLaunched = false;
        velocity = Vector3.zero;

        if (GameManager.Instance != null)
            GameManager.Instance.OnBallLost(this);
    }
    
    public void ResetBall()
    {
        isLaunched = false;
        velocity = Vector3.zero;
    }
}
