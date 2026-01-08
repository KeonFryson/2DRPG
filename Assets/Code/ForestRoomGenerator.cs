using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ForestRoomTilemapGenerator : MonoBehaviour
{
    [Header("Room Size")]
    [SerializeField] private Data data;
    public int width = 12;
    public int height = 12;

    [Header("Tilemaps")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public Tilemap voidTilemap;

    [Header("Tiles")]
    public List<TileBase> WallTiles;
    public List<TileBase> FloorTiles;
    public TileBase VoidTile;

    [Header("Chances")]
    [Range(0f, 0.4f)] public float wallChance = 0.18f;
    [Range(0f, 0.5f)] public float treeChance = 0.15f;
    [Range(0f, 1f)] public float chestSpawnChance = 0.8f;

    [Header("Props")]
    public List<GameObject> treePrefabs;
    public List<GameObject> chestPrefabs;
    [Range(1, 5)] public int minChests = 1;
    [Range(1, 5)] public int maxChests = 3;

    private HashSet<Vector3Int> wallPositions = new HashSet<Vector3Int>();
    private HashSet<Vector3Int> treePositions = new HashSet<Vector3Int>();
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private int offsetX;
    private int offsetY;

    private void Awake()
    {
        if (data != null)
        {
            width = data.value;
            height = data.value;
        }
    }

    void Start()
    {
        GenerateRoom();
    }

    void GenerateRoom()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        if (voidTilemap != null)
            voidTilemap.ClearAllTiles();

        wallPositions.Clear();
        treePositions.Clear();
        ClearSpawnedObjects();

        // Calculate offset to center the room at (0,0)
        offsetX = -width / 2;
        offsetY = -height / 2;

        // First pass: place floors
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x + offsetX, y + offsetY, 0);
                floorTilemap.SetTile(pos, RandomFloor());
            }
        }

        // Place void tiles around the room
        for (int x = -1; x <= width; x++)
        {
            for (int y = -1; y <= height; y++)
            {
                if (x < 0 || y < 0 || x >= width || y >= height)
                {
                    Vector3Int pos = new Vector3Int(x + offsetX, y + offsetY, 0);
                    if (voidTilemap != null && VoidTile != null)
                        voidTilemap.SetTile(pos, VoidTile);
                }
            }
        }

        // Second pass: place walls on borders
        // Bottom border (excluding corners)
        for (int x = 2; x < width - 2; x += 2)
        {
            Place2x2Wall(x, 0);
        }

        // Top border (excluding corners)
        for (int x = 2; x < width - 2; x += 2)
        {
            Place2x2Wall(x, height - 2);
        }

        // Left border (full height)
        for (int y = 0; y < height; y += 2)
        {
            Place2x2Wall(0, y);
        }

        // Right border (full height)
        for (int y = 0; y < height; y += 2)
        {
            Place2x2Wall(width - 2, y);
        }

        // Third pass: place random interior walls
        for (int x = 2; x < width - 2; x += 2)
        {
            for (int y = 2; y < height - 2; y += 2)
            {
                if (Random.value < wallChance)
                {
                    Place2x2Wall(x, y);
                }
            }
        }

        // Fourth pass: place trees
        if (treePrefabs != null && treePrefabs.Count > 0)
        {
            PlaceTrees();
        }

        // Fifth pass: place chests
        if (chestPrefabs != null && chestPrefabs.Count > 0 && Random.value < chestSpawnChance)
        {
            PlaceChests();
        }
    }

    void Place2x2Wall(int x, int y)
    {
        // Select one wall tile for the entire 2x2 block
        TileBase wallTile = RandomWall();

        // Place a 2x2 wall starting at position (x, y)
        for (int offsetX = 0; offsetX < 2; offsetX++)
        {
            for (int offsetY = 0; offsetY < 2; offsetY++)
            {
                Vector3Int wallPos = new Vector3Int(x + offsetX + this.offsetX, y + offsetY + this.offsetY, 0);

                // Only place wall if it's within the room bounds and hasn't been placed yet
                if (x + offsetX >= 0 && x + offsetX < width &&
                    y + offsetY >= 0 && y + offsetY < height &&
                    !wallPositions.Contains(wallPos))
                {
                    wallTilemap.SetTile(wallPos, wallTile);
                    wallPositions.Add(wallPos);
                }
            }
        }
    }

    void PlaceTrees()
    {
        // Place random trees in the interior
        for (int x = 2; x < width - 2; x++)
        {
            for (int y = 2; y < height - 2; y++)
            {
                Vector3Int pos = new Vector3Int(x + offsetX, y + offsetY, 0);

                // Only place tree if no wall is present and random chance succeeds
                if (!wallPositions.Contains(pos) && Random.value < treeChance)
                {
                    // Check if adjacent tiles are also free (optional - prevents clustering)
                    if (!IsAdjacentToWallOrTree(pos))
                    {
                        SpawnTree(pos);
                        treePositions.Add(pos);
                    }
                }
            }
        }
    }

    void PlaceChests()
    {
        int chestsToSpawn = Random.Range(minChests, maxChests + 1);
        int spawnAttempts = 0;
        int maxAttempts = 100;
        int chestsSpawned = 0;

        while (chestsSpawned < chestsToSpawn && spawnAttempts < maxAttempts)
        {
            spawnAttempts++;

            // Random position in the interior
            int x = Random.Range(3, width - 3);
            int y = Random.Range(3, height - 3);
            Vector3Int pos = new Vector3Int(x + offsetX, y + offsetY, 0);

            // Check if position is valid (no walls, trees, or other chests nearby)
            if (!wallPositions.Contains(pos) &&
                !treePositions.Contains(pos) &&
                IsPositionClearForChest(pos))
            {
                SpawnChest(pos);
                chestsSpawned++;
            }
        }

        Debug.Log($"Spawned {chestsSpawned} chest(s) in the room");
    }

    void SpawnTree(Vector3Int gridPos)
    {
        if (treePrefabs.Count == 0) return;

        Vector3 worldPos = floorTilemap.CellToWorld(gridPos) + floorTilemap.cellSize / 2f;
        GameObject treePrefab = treePrefabs[Random.Range(0, treePrefabs.Count)];
        GameObject tree = Instantiate(treePrefab, worldPos, Quaternion.identity, transform);
        spawnedObjects.Add(tree);
    }

    void SpawnChest(Vector3Int gridPos)
    {
        if (chestPrefabs.Count == 0) return;

        Vector3 worldPos = floorTilemap.CellToWorld(gridPos) + floorTilemap.cellSize / 2f;
        GameObject chestPrefab = chestPrefabs[Random.Range(0, chestPrefabs.Count)];
        GameObject chest = Instantiate(chestPrefab, worldPos, Quaternion.identity, transform);
        spawnedObjects.Add(chest);
    }

    bool IsAdjacentToWallOrTree(Vector3Int pos)
    {
        // Check 8 adjacent tiles
        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                if (offsetX == 0 && offsetY == 0) continue;

                Vector3Int adjacentPos = new Vector3Int(pos.x + offsetX, pos.y + offsetY, 0);
                if (wallPositions.Contains(adjacentPos) || treePositions.Contains(adjacentPos))
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool IsPositionClearForChest(Vector3Int pos)
    {
        // Check a 3x3 area around the chest position
        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                Vector3Int checkPos = new Vector3Int(pos.x + offsetX, pos.y + offsetY, 0);
                if (wallPositions.Contains(checkPos) || treePositions.Contains(checkPos))
                {
                    return false;
                }
            }
        }
        return true;
    }

    void ClearSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
    }

    TileBase RandomWall()
    {
        return WallTiles[Random.Range(0, WallTiles.Count)];
    }

    TileBase RandomFloor()
    {
        return FloorTiles[Random.Range(0, FloorTiles.Count)];
    }

    // Editor helper - regenerate room in play mode
    [ContextMenu("Regenerate Room")]
    void RegenerateRoom()
    {
        GenerateRoom();
    }
}