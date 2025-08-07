using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrefabSpawnInfo
{
    public GameObject prefab;
    [Tooltip("Width in grid units (X-axis)")]
    public int width = 1;
    [Tooltip("Height in grid units (Z-axis)")]
    public int height = 1;
    [Tooltip("Weight for random selection (higher = more likely)")]
    public float spawnWeight = 1f;
}

public class PrefabsOnGrid : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private PrefabSpawnInfo[] prefabsToSpawn;
    [SerializeField] private int numberOfPrefabsToSpawn = 10;
    [SerializeField] private bool spawnOnStart = true;

    [Header("Grid Reference")]
    [SerializeField] private GridManager gridManager;

    private List<GameObject> spawnedPrefabs = new List<GameObject>();
    private Dictionary<GameObject, List<GridNode>> prefabToNodesMap = new Dictionary<GameObject, List<GridNode>>();

    private void Start()
    {
        if (spawnOnStart != null)
        {
            StartCoroutine(SpawnPrefabsWhenGridReady());
        }
    }

    private IEnumerator SpawnPrefabsWhenGridReady()
    {
        // Wait until the grid is initialized
        while (!gridManager.IsInitialized)
        {
            yield return null;
        }

        SpawnPrefabsRandomly();
    }

    public void SpawnPrefabsRandomly()
    {
        if (gridManager == null || !gridManager.IsInitialized)
        {
            Debug.LogWarning("GridManager is not initialized!");
            return;
        }

        if (prefabsToSpawn == null || prefabsToSpawn.Length == 0)
        {
            Debug.LogWarning("No prefabs assigned to spawn!");
            return;
        }

        // Get all walkable nodes
        List<GridNode> availableNodes = GetWalkableNodes();

        if (availableNodes.Count == 0)
        {
            Debug.LogWarning("No walkable nodes available for spawning!");
            return;
        }

        int successfulSpawns = 0;
        int attempts = 0;
        int maxAttempts = numberOfPrefabsToSpawn * 3; // Prevent infinite loops

        while (successfulSpawns < numberOfPrefabsToSpawn && attempts < maxAttempts)
        {
            attempts++;

            // Select a random prefab based on weight
            PrefabSpawnInfo selectedPrefabInfo = GetWeightedRandomPrefab();
            if (selectedPrefabInfo == null || selectedPrefabInfo.prefab == null) continue;

            // Try to find a valid spawn position
            GridNode spawnNode = FindValidSpawnPosition(availableNodes, selectedPrefabInfo);
            if (spawnNode == null) continue;

            // Get all nodes that will be occupied by this prefab
            List<GridNode> occupiedNodes = GetOccupiedNodes(spawnNode, selectedPrefabInfo);
            if (occupiedNodes.Count == 0) continue;

            // Spawn the prefab
            GameObject spawnedPrefab = Instantiate(selectedPrefabInfo.prefab, spawnNode.WorldPosition, Quaternion.identity);
            spawnedPrefab.transform.SetParent(transform);

            // Mark all occupied nodes as non-walkable
            foreach (GridNode node in occupiedNodes)
            {
                node.walkable = false;
                availableNodes.Remove(node); // Remove from available nodes
            }

            // Track the spawned prefab and its occupied nodes
            spawnedPrefabs.Add(spawnedPrefab);
            prefabToNodesMap[spawnedPrefab] = occupiedNodes;

            successfulSpawns++;

            Debug.Log($"Spawned {selectedPrefabInfo.prefab.name} at {spawnNode.WorldPosition} occupying {occupiedNodes.Count} nodes ({selectedPrefabInfo.width}x{selectedPrefabInfo.height})");
        }

        Debug.Log($"Successfully spawned {successfulSpawns} prefabs on the grid! ({attempts} attempts made)");
    }

    private List<GridNode> GetWalkableNodes()
    {
        List<GridNode> walkableNodes = new List<GridNode>();
        List<GridNode> allNodes = gridManager.GetAllNodes();

        foreach (GridNode node in allNodes)
        {
            if (node.walkable)
            {
                walkableNodes.Add(node);
            }
        }

        return walkableNodes;
    }

    public void ClearAllSpawnedPrefabs()
    {
        foreach (GameObject prefab in spawnedPrefabs)
        {
            if (prefab != null)
            {
                // Restore walkability for all nodes occupied by this prefab
                if (prefabToNodesMap.ContainsKey(prefab))
                {
                    foreach (GridNode node in prefabToNodesMap[prefab])
                    {
                        if (node != null)
                        {
                            node.walkable = node.terrainType.Walkable; // Reset to terrain's original walkable state
                        }
                    }
                }

                DestroyImmediate(prefab);
            }
        }

        spawnedPrefabs.Clear();
        prefabToNodesMap.Clear();
        Debug.Log("Cleared all spawned prefabs and restored node walkability!");
    }

    // Editor utility method to manually trigger spawning
    [ContextMenu("Spawn Prefabs")]
    public void ManualSpawn()
    {
        if (Application.isPlaying)
        {
            SpawnPrefabsRandomly();
        }
        else
        {
            Debug.LogWarning("Manual spawn only works in Play Mode!");
        }
    }

    // Editor utility method to clear spawned prefabs
    [ContextMenu("Clear Spawned Prefabs")]
    public void ManualClear()
    {
        ClearAllSpawnedPrefabs();
    }

    private PrefabSpawnInfo GetWeightedRandomPrefab()
    {
        float totalWeight = 0f;
        foreach (var prefabInfo in prefabsToSpawn)
        {
            if (prefabInfo.prefab != null)
                totalWeight += prefabInfo.spawnWeight;
        }

        if (totalWeight <= 0f) return null;

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var prefabInfo in prefabsToSpawn)
        {
            if (prefabInfo.prefab == null) continue;

            currentWeight += prefabInfo.spawnWeight;
            if (randomValue <= currentWeight)
                return prefabInfo;
        }

        return prefabsToSpawn[prefabsToSpawn.Length - 1]; // Fallback
    }

    private GridNode FindValidSpawnPosition(List<GridNode> availableNodes, PrefabSpawnInfo prefabInfo)
    {
        List<GridNode> validNodes = new List<GridNode>();

        foreach (GridNode node in availableNodes)
        {
            if (CanPlacePrefabAt(node, prefabInfo))
            {
                validNodes.Add(node);
            }
        }

        if (validNodes.Count == 0) return null;

        return validNodes[Random.Range(0, validNodes.Count)];
    }

    private bool CanPlacePrefabAt(GridNode startNode, PrefabSpawnInfo prefabInfo)
    {
        List<GridNode> requiredNodes = GetRequiredNodes(startNode, prefabInfo);

        // Check if all required nodes exist and are walkable
        foreach (GridNode node in requiredNodes)
        {
            if (node == null || !node.walkable)
                return false;
        }

        return requiredNodes.Count == prefabInfo.width * prefabInfo.height;
    }

    private List<GridNode> GetRequiredNodes(GridNode startNode, PrefabSpawnInfo prefabInfo)
    {
        List<GridNode> nodes = new List<GridNode>();

        // Convert world position to grid coordinates
        Vector3 startPos = startNode.WorldPosition;
        GridNode startGridNode = gridManager.GetNodeFromWorldPosition(startPos);

        if (startGridNode == null) return nodes;

        // Calculate the starting grid position (this is now the CENTER of the prefab)
        Vector2Int centerGridPos = GetGridPosition(startGridNode);
        if (centerGridPos.x == -1) return nodes; // Invalid position

        // Calculate offset to center the prefab
        int halfWidth = prefabInfo.width / 2;
        int halfHeight = prefabInfo.height / 2;

        // Calculate the actual starting position (bottom-left corner of the prefab area)
        Vector2Int startGridPos = new Vector2Int(
            centerGridPos.x - halfWidth,
            centerGridPos.y - halfHeight
        );

        // Get all nodes in the width x height area
        for (int x = 0; x < prefabInfo.width; x++)
        {
            for (int y = 0; y < prefabInfo.height; y++)
            {
                Vector2Int nodePos = new Vector2Int(startGridPos.x + x, startGridPos.y + y);
                GridNode node = gridManager.GetNode(nodePos);

                if (node != null)
                {
                    nodes.Add(node);
                }
            }
        }

        return nodes;
    }

    private List<GridNode> GetOccupiedNodes(GridNode startNode, PrefabSpawnInfo prefabInfo)
    {
        return GetRequiredNodes(startNode, prefabInfo);
    }

    private Vector2Int GetGridPosition(GridNode node)
    {
        // Find the grid position by searching through all nodes
        List<GridNode> allNodes = gridManager.GetAllNodes();

        for (int x = 0; x < gridManager.GridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridManager.GridSettings.GridSizeY; y++)
            {
                GridNode gridNode = gridManager.GetNode(x, y);
                if (gridNode == node)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return new Vector2Int(-1, -1); // Not found
    }
}