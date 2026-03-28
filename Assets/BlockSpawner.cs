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

    // PATTERN: edit this to change the block layout.
    private readonly string[] _pattern = new string[]
    {
        "011110",   // row 0 (Bottom)
        "011110",   // row 1
        "122221",   // row 2
        "123321",   // row 3
        "124421",   // row 4
        "124421",   // row 5
        "123321",   // row 6
        "122221",   // row 7
        "011110",   // row 8
        "011110",   // row 9 (Top)
    };

    void Start()
    {
        SpawnBlocks();
    }

    // Reads the pattern array and places one block per non-zero character.
    private void SpawnBlocks()
    {
        if (blockPrefab == null)
        {
            Debug.LogError("[BlockSpawner] No blockPrefab assigned! Drag your Block prefab into the Inspector.");
            return;
        }

        int rowCount = _pattern.Length;
        int colCount = _pattern[0].Length;

        // Calculate the X offset to center the grid around centerX
        float totalWidth = (colCount - 1) * spacingX;
        float startX = centerX - totalWidth / 2f;

        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < colCount; col++)
            {
                char cell = _pattern[row][col];

                // '0' means no block here
                if (cell == '0') continue;

                // Convert char '1'-'4' to int 1-4
                int healthValue = cell - '0';

                // Calculate world position of this block
                float x = startX + col * spacingX;
                float z = startZ + row * spacingZ;
                Vector3 spawnPos = new Vector3(x, 0.25f, z);

                // Spawn and configure the block
                GameObject obj = Instantiate(blockPrefab, spawnPos, Quaternion.identity);
                Block block = obj.GetComponent<Block>();

                if (block != null)
                    block.SetHealth(healthValue);
                else
                    Debug.LogWarning("[BlockSpawner] Spawned object has no Block component!");
            }
        }
    }
}