using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridProcedural : MonoBehaviour
{
    [Header("Terrain Generation Settings")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private TerrainType grassTerrain;
    [SerializeField] private TerrainType waterTerrain;
    [SerializeField] private TerrainType rocksTerrain;

    [Header("Prefab Management")]
    [SerializeField] private Transform terrainParent; // Parent object for organization
    [SerializeField] private bool instantiatePrefabs = true;
    [SerializeField] private bool clearExistingPrefabs = true;

    [Header("Noise Settings")]
    [SerializeField] private float noiseScale = 0.1f;
    [SerializeField] private float waterThreshold = 0.3f;
    [SerializeField] private int octaves = 4;
    [SerializeField] private float persistence = 0.5f;
    [SerializeField] private float lacunarity = 2f;

    [Header("Water Body Settings")]
    [SerializeField] private int numberOfLakes = 3;
    [SerializeField] private int minLakeSize = 5;
    [SerializeField] private int maxLakeSize = 15;

    [Header("River Settings")]
    [SerializeField] private int numberOfRivers = 2;
    [SerializeField] private int riverLength = 20;
    [SerializeField] private int riverWidth = 2;

    [Header("Seed Settings")]
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed = 12345;

    [Header("Auto Randomization")]
    [SerializeField] private bool randomizeOnStart = true;
    [SerializeField] private bool randomizeOnEnable = false;

    private float[,] noiseMap;
    private Vector2 noiseOffset;
    private Dictionary<GridNode, GameObject> nodePrefabMap = new Dictionary<GridNode, GameObject>();

    void Start()
    {

        if (gridManager != null && randomizeOnStart)
        {
            StartCoroutine(GenerateTerrainCoroutine());
            //RandomizeSeed();
        }
    }

    void OnEnable()
    {
        if (gridManager != null && randomizeOnEnable && gridManager.IsInitialized)
        {
            GenerateTerrain();
        }
    }

    private IEnumerator GenerateTerrainCoroutine()
    {
        // Wait for grid to be initialized
        while (!gridManager.IsInitialized)
        {
            yield return null;
        }

        GenerateTerrain();
    }

    [ContextMenu("Generate Terrain")]
    public void GenerateTerrain()
    {
        if (gridManager == null || !gridManager.IsInitialized)
        {
            Debug.LogWarning("GridManager not initialized!");
            return;
        }

        // Clear existing prefabs if requested
        if (clearExistingPrefabs)
        {
            ClearAllPrefabs();
        }

        // Always use a new random seed for full randomization
        seed = Random.Range(0, 10000);
        Random.InitState(seed);

        // Generate random offset for noise
        noiseOffset = new Vector2(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));

        // Generate base noise map
        GenerateNoiseMap();

        // Apply base terrain
        ApplyBaseTerrain();

        // Add water features
        GenerateLakes();
        GenerateRivers();

        // Instantiate prefabs for all nodes
        if (instantiatePrefabs)
        {
            InstantiatePrefabsForAllNodes();
        }

        Debug.Log($"Terrain generated with seed: {seed}");
    }

    private void GenerateNoiseMap()
    {
        var settings = gridManager.GridSettings;
        noiseMap = new float[settings.GridSizeX, settings.GridSizeY];

        for (int x = 0; x < settings.GridSizeX; x++)
        {
            for (int y = 0; y < settings.GridSizeY; y++)
            {
                float noiseValue = 0f;
                float amplitude = 1f;
                float frequency = noiseScale;

                // Generate fractal noise using multiple octaves
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + noiseOffset.x) * frequency;
                    float sampleY = (y + noiseOffset.y) * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                    noiseValue += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                noiseMap[x, y] = Mathf.Clamp01(noiseValue);
            }
        }
    }

    private void ApplyBaseTerrain()
    {
        var settings = gridManager.GridSettings;
        var allNodes = gridManager.GetAllNodes();

        foreach (var node in allNodes)
        {
            // Convert world position back to grid coordinates
            int x = settings.UseXZPlane
                ? Mathf.RoundToInt(node.WorldPosition.x / settings.NodeSize)
                : Mathf.RoundToInt(node.WorldPosition.z / settings.NodeSize);
            int y = settings.UseXZPlane
                ? Mathf.RoundToInt(node.WorldPosition.z / settings.NodeSize)
                : Mathf.RoundToInt(node.WorldPosition.y / settings.NodeSize);

            // Clamp coordinates to valid range
            x = Mathf.Clamp(x, 0, settings.GridSizeX - 1);
            y = Mathf.Clamp(y, 0, settings.GridSizeY - 1);

            // Apply terrain based on noise value
            float noiseValue = noiseMap[x, y];

            if (noiseValue < waterThreshold)
            {
                SetNodeTerrain(node, waterTerrain);
            }
            else
            {
                SetNodeTerrain(node, grassTerrain);
            }
        }
    }

    private void GenerateLakes()
    {
        var settings = gridManager.GridSettings;

        for (int i = 0; i < numberOfLakes; i++)
        {
            // Random lake center
            int centerX = Random.Range(maxLakeSize, settings.GridSizeX - maxLakeSize);
            int centerY = Random.Range(maxLakeSize, settings.GridSizeY - maxLakeSize);

            // Random lake size
            int lakeSize = Random.Range(minLakeSize, maxLakeSize);

            // Create roughly circular lake
            for (int x = centerX - lakeSize; x <= centerX + lakeSize; x++)
            {
                for (int y = centerY - lakeSize; y <= centerY + lakeSize; y++)
                {
                    if (x >= 0 && x < settings.GridSizeX && y >= 0 && y < settings.GridSizeY)
                    {
                        float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));

                        // Add some randomness to make lakes more natural
                        float randomFactor = Random.Range(0.7f, 1.3f);

                        if (distance <= lakeSize * randomFactor)
                        {
                            var node = gridManager.GetNode(x, y);
                            if (node != null)
                            {
                                SetNodeTerrain(node, waterTerrain);
                            }
                        }
                    }
                }
            }
        }
    }

    private void GenerateRivers()
    {
        var settings = gridManager.GridSettings;

        for (int i = 0; i < numberOfRivers; i++)
        {
            // Random starting point at edge
            Vector2Int startPos = GetRandomEdgePosition(settings);
            Vector2Int currentPos = startPos;

            // Random direction towards center
            Vector2 targetDirection = (new Vector2(settings.GridSizeX / 2f, settings.GridSizeY / 2f) - new Vector2(startPos.x, startPos.y)).normalized;

            List<Vector2Int> riverPath = new List<Vector2Int>();

            // Generate river path
            for (int step = 0; step < riverLength; step++)
            {
                riverPath.Add(currentPos);

                // Add some randomness to the direction
                Vector2 randomOffset = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
                Vector2 newDirection = (targetDirection + randomOffset).normalized;

                // Move to next position
                Vector2Int nextPos = currentPos + new Vector2Int(
                    Mathf.RoundToInt(newDirection.x),
                    Mathf.RoundToInt(newDirection.y)
                );

                // Clamp to grid bounds
                nextPos.x = Mathf.Clamp(nextPos.x, 0, settings.GridSizeX - 1);
                nextPos.y = Mathf.Clamp(nextPos.y, 0, settings.GridSizeY - 1);

                currentPos = nextPos;

                // Stop if we've reached the edge again or a lake
                if (IsAtEdge(currentPos, settings) || IsWater(currentPos))
                {
                    break;
                }
            }

            // Apply river to terrain
            foreach (var pos in riverPath)
            {
                CreateRiverSegment(pos, riverWidth, settings);
            }
        }
    }

    private void InstantiatePrefabsForAllNodes()
    {
        var allNodes = gridManager.GetAllNodes();

        // Setup parent object if not assigned
        if (terrainParent == null)
        {
            GameObject parentObj = new GameObject("Terrain Prefabs");
            parentObj.transform.SetParent(this.transform);
            terrainParent = parentObj.transform;
        }

        foreach (var node in allNodes)
        {
            InstantiatePrefabForNode(node);
        }

        Debug.Log($"Instantiated {allNodes.Count} terrain prefabs");
    }

    private void InstantiatePrefabForNode(GridNode node)
    {
        if (node.terrainType == null || node.terrainType.TerrainPrefab == null)
        {
            return;
        }

        // Calculate final position with offset
        Vector3 spawnPosition = node.WorldPosition + node.terrainType.PrefabOffset;

        // Instantiate the prefab
        GameObject prefabInstance = Instantiate(
            node.terrainType.TerrainPrefab,
            spawnPosition,
            Quaternion.identity,
            terrainParent
        );

        // Apply scale
        prefabInstance.transform.localScale = node.terrainType.PrefabScale;

        // Name the instance for organization
        prefabInstance.name = $"{node.terrainType.TerrainName}_{node.Name}";

        // Store reference for potential cleanup
        nodePrefabMap[node] = prefabInstance;

        // Add a component to link back to the node if needed
        var nodeLinker = prefabInstance.GetComponent<GridNodeLinker>();
        if (nodeLinker == null)
        {
            nodeLinker = prefabInstance.AddComponent<GridNodeLinker>();
        }
        nodeLinker.LinkedNode = node;
    }

    private void ClearAllPrefabs()
    {
        // Clear from dictionary and destroy GameObjects
        foreach (var kvp in nodePrefabMap)
        {
            if (kvp.Value != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(kvp.Value);
                }
                else
                {
                    DestroyImmediate(kvp.Value);
                }
            }
        }
        nodePrefabMap.Clear();

        // Also clear any remaining children in terrainParent
        if (terrainParent != null)
        {
            int childCount = terrainParent.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = terrainParent.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }

    private Vector2Int GetRandomEdgePosition(GridSettings settings)
    {
        int side = Random.Range(0, 4);
        switch (side)
        {
            case 0: return new Vector2Int(0, Random.Range(0, settings.GridSizeY)); // Left edge
            case 1: return new Vector2Int(settings.GridSizeX - 1, Random.Range(0, settings.GridSizeY)); // Right edge
            case 2: return new Vector2Int(Random.Range(0, settings.GridSizeX), 0); // Bottom edge
            default: return new Vector2Int(Random.Range(0, settings.GridSizeX), settings.GridSizeY - 1); // Top edge
        }
    }

    private bool IsAtEdge(Vector2Int pos, GridSettings settings)
    {
        return pos.x == 0 || pos.x == settings.GridSizeX - 1 || pos.y == 0 || pos.y == settings.GridSizeY - 1;
    }

    private bool IsWater(Vector2Int pos)
    {
        var node = gridManager.GetNode(pos);
        return node != null && node.terrainType == waterTerrain;
    }

    private void CreateRiverSegment(Vector2Int center, int width, GridSettings settings)
    {
        for (int x = center.x - width; x <= center.x + width; x++)
        {
            for (int y = center.y - width; y <= center.y + width; y++)
            {
                if (x >= 0 && x < settings.GridSizeX && y >= 0 && y < settings.GridSizeY)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center.x, center.y));
                    if (distance <= width)
                    {
                        var node = gridManager.GetNode(x, y);
                        if (node != null)
                        {
                            SetNodeTerrain(node, waterTerrain);
                        }
                    }
                }
            }
        }
    }

    private void SetNodeTerrain(GridNode node, TerrainType terrain)
    {
        node.terrainType = terrain;
        node.walkable = terrain.Walkable;
    }

    // Public methods for runtime access
    public GameObject GetPrefabForNode(GridNode node)
    {
        nodePrefabMap.TryGetValue(node, out GameObject prefab);
        return prefab;
    }

    public void UpdateSingleNodePrefab(GridNode node)
    {
        // Remove existing prefab if it exists
        if (nodePrefabMap.ContainsKey(node) && nodePrefabMap[node] != null)
        {
            if (Application.isPlaying)
            {
                Destroy(nodePrefabMap[node]);
            }
            else
            {
                DestroyImmediate(nodePrefabMap[node]);
            }
            nodePrefabMap.Remove(node);
        }

        // Instantiate new prefab
        if (instantiatePrefabs)
        {
            InstantiatePrefabForNode(node);
        }
    }

    // Editor utility methods
    [ContextMenu("Randomize Seed")]
    public void RandomizeSeed()
    {
        seed = Random.Range(0, 10000);
        GenerateTerrain();
    }

    [ContextMenu("Clear All Prefabs")]
    public void ClearPrefabs()
    {
        ClearAllPrefabs();
    }

    [ContextMenu("Regenerate Prefabs Only")]
    public void RegeneratePrefabs()
    {
        ClearAllPrefabs();
        if (instantiatePrefabs)
        {
            InstantiatePrefabsForAllNodes();
        }
    }

    private void OnValidate()
    {
        // Clamp values to reasonable ranges
        noiseScale = Mathf.Clamp(noiseScale, 0.01f, 1f);
        waterThreshold = Mathf.Clamp01(waterThreshold);
        octaves = Mathf.Clamp(octaves, 1, 8);
        persistence = Mathf.Clamp01(persistence);
        lacunarity = Mathf.Max(1f, lacunarity);
        numberOfLakes = Mathf.Max(0, numberOfLakes);
        minLakeSize = Mathf.Max(1, minLakeSize);
        maxLakeSize = Mathf.Max(minLakeSize, maxLakeSize);
        numberOfRivers = Mathf.Max(0, numberOfRivers);
        riverLength = Mathf.Max(1, riverLength);
        riverWidth = Mathf.Max(1, riverWidth);
    }

    private void OnDestroy()
    {
        // Clean up references
        nodePrefabMap.Clear();
    }
}