using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject blockPrefab;

    [Header("Grid Spacing")]
    public float spacingX = 1.6f;
    public float spacingZ = 0.8f;
    public float startZ   = 2.5f;
    public float centerX;

    [Header("Random Mode")]
    [Tooltip("If true, a random grid is generated instead of using the fixed pattern below")]
    public bool randomMode = false;

    [Tooltip("Number of rows in the random grid")]
    [Range(1, 15)]
    public int randomRows = 10;

    [Tooltip("Number of columns in the random grid – must be even for perfect symmetry")]
    [Range(2, 12)]
    public int randomCols = 6;

    [Tooltip("Chance (0-1) that any given cell in the left half contains a block")]
    [Range(0f, 1f)]
    public float fillChance = 0.75f;

    [Tooltip("Maximum health value assigned to a random block (1 = all green, 4 = mixed colours)")]
    [Range(1, 4)]
    public int maxRandomHealth = 4;

    [Tooltip("If true, the left half is mirrored to the right so the layout is always symmetric")]
    public bool symmetric = true;

    // Fixed pattern used when randomMode is false.
    // '0' = empty, '1'-'4' = block health. All rows must have the same length.
    private readonly string[] _pattern =
    {
        "011110",   // row 0 (bottom)
        "011110",
        "122221",
        "123321",
        "124421",
        "124421",
        "123321",
        "122221",
        "011110",
        "011110",   // row 9 (top)
    };

    void Start()
    {
        if (randomMode)
            SpawnRandom();
        else
            SpawnFromPattern();
    }

    private void SpawnFromPattern()
    {
        if (!ValidatePrefab()) return;

        int   rowCount   = _pattern.Length;
        int   colCount   = _pattern[0].Length;
        float startX     = centerX - (colCount - 1) * spacingX / 2f;

        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < colCount; col++)
            {
                char cell = _pattern[row][col];
                if (cell == '0') continue;

                SpawnBlock(GridToWorld(startX, row, col), cell - '0');
            }
        }
    }

    // Random spawner – with optional left-right symmetry
    private void SpawnRandom()
    {
        if (!ValidatePrefab()) return;

        float startX = centerX - (randomCols - 1) * spacingX / 2f;

        // Generate a 2D health grid [row, col].
        // 0 = empty cell, 1-4 = block health.
        int[,] grid = new int[randomRows, randomCols];

        if (symmetric)
        {
            // How many columns make up the left half.
            // With 6 cols: halfCols = 3  (cols 0,1,2 → mirror to cols 5,4,3)
            int halfCols = randomCols / 2;

            for (int row = 0; row < randomRows; row++)
            {
                // Left half (and mirrored right half)
                for (int col = 0; col < halfCols; col++)
                {
                    int health = Random.value <= fillChance
                        ? Random.Range(1, maxRandomHealth + 1)
                        : 0;

                    // Left side
                    grid[row, col] = health;

                    // Mirror to the right side
                    // col 0 → rightmost, col 1 → second from right, etc.
                    grid[row, randomCols - 1 - col] = health;
                }

                // Centre column (only exists when randomCols is odd)
                if (randomCols % 2 != 0)
                {
                    int centreCol = randomCols / 2;
                    grid[row, centreCol] = Random.value <= fillChance
                        ? Random.Range(1, maxRandomHealth + 1)
                        : 0;
                }
            }
        }
        else
        {
            // Fully random – every cell is independent
            for (int row = 0; row < randomRows; row++)
                for (int col = 0; col < randomCols; col++)
                    grid[row, col] = Random.value <= fillChance
                        ? Random.Range(1, maxRandomHealth + 1)
                        : 0;
        }

        // Spawn blocks from the generated grid
        for (int row = 0; row < randomRows; row++)
            for (int col = 0; col < randomCols; col++)
                if (grid[row, col] > 0)
                    SpawnBlock(GridToWorld(startX, row, col), grid[row, col]);
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------

    // Converts a grid row/col to a world position
    private Vector3 GridToWorld(float startX, int row, int col)
    {
        return new Vector3(
            startX + col * spacingX,
            0.25f,
            startZ + row * spacingZ
        );
    }

    // Spawns one block at a world position and sets its health
    private void SpawnBlock(Vector3 position, int health)
    {
        GameObject obj   = Instantiate(blockPrefab, position, Quaternion.identity);
        Block      block = obj.GetComponent<Block>();

        if (block != null)
            block.SetHealth(health);
        else
            Debug.LogWarning("[BlockSpawner] Spawned object has no Block component!");
    }

    // Returns false and logs an error if no prefab is assigned
    private bool ValidatePrefab()
    {
        if (blockPrefab != null) return true;

        Debug.LogError("[BlockSpawner] No blockPrefab assigned! " +
                       "Drag your Block prefab into the Inspector.");
        return false;
    }
}