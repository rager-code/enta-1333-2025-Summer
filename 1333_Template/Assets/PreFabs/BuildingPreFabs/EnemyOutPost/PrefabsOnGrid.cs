using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stores settings for each prefab that can be spawned
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

    // Start is called on scene load
    private void Start()
    {
        // Wait for grid setup before spawning if needed
        if (spawnOnStart != null)
        {
            StartCoroutine(SpawnPrefabsWhenGridReady());
        }
    }

    // Waits until the grid is initialized before spawning
    private IEnumerator SpawnPrefabsWhenGridReady()
    {
        while (!gridManager.IsInitialized)
        {
            yield return null;
        }

        SpawnPrefabsRandomly();
    }

    // Spawns multiple prefabs onto available walkable nodes
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

        // Get available grid nodes
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

            // Pick a prefab based on weight
            PrefabSpawnInfo selectedPrefabInfo = GetWeightedRandomPrefab();
            if (selectedPrefabInfo == null || selectedPrefabInfo.prefab == null) continue;

            // Find a spawnable position for this prefab
            GridNode spawnNode = FindValidSpawnPosition(availableNodes, selectedPrefabInfo);
            if (spawnNode == null) continue;

            // Get grid nodes the prefab will occupy
            List<GridNode> occupiedNodes = GetOccupiedNodes(spawnNode, selectedPrefabInfo);
            if (occupiedNodes.Count == 0) continue;

            // Spawn the prefab into the scene
            GameObject spawnedPrefab = Instantiate(selectedPrefabInfo.prefab, spawnNode.WorldPosition, Quaternion.identity);
            spawnedPrefab.transform.SetParent(transform);

            // Mark those grid nodes as taken
            foreach (GridNode node in occupiedNodes)
            {
                node.walkable = false;
                availableNodes.Remove(node);
            }

            // Track spawned prefab and its nodes
            spawnedPrefabs.Add(spawnedPrefab);
            prefabToNodesMap[spawnedPrefab] = occupiedNodes;

            successfulSpawns++;

            Debug.Log($"Spawned {selectedPrefabInfo.prefab.name} at {spawnNode.WorldPosition} occupying {occupiedNodes.Count} nodes ({selectedPrefabInfo.width}x{selectedPrefabInfo.height})");
        }

        Debug.Log($"Successfully spawned {successfulSpawns} prefabs on the grid! ({attempts} attempts made)");
    }

    // Gets all nodes on the grid that are walkable
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

    // Removes all spawned prefabs and resets the grid state
    public void ClearAllSpawnedPrefabs()
    {
        foreach (GameObject prefab in spawnedPrefabs)
        {
            if (prefab != null)
            {
                // Restore the walkable state of each node this prefab used
                if (prefabToNodesMap.ContainsKey(prefab))
                {
                    foreach (GridNode node in prefabToNodesMap[prefab])
                    {
                        if (node != null)
                        {
                            node.walkable = node.terrainType.Walkable;
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

    // Lets you trigger prefab spawning via the Unity context menu
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

    // Lets you manually clear spawned prefabs from the Unity context menu
    [ContextMenu("Clear Spawned Prefabs")]
    public void ManualClear()
    {
        ClearAllSpawnedPrefabs();
    }

    // Randomly selects a prefab from the list using weighted odds
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

        return prefabsToSpawn[prefabsToSpawn.Length - 1]; // Fallback option
    }

    // Finds a valid node to place the prefab on
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

    // Checks if the prefab can fit at a given node
    private bool CanPlacePrefabAt(GridNode startNode, PrefabSpawnInfo prefabInfo)
    {
        List<GridNode> requiredNodes = GetRequiredNodes(startNode, prefabInfo);

        foreach (GridNode node in requiredNodes)
        {
            if (node == null || !node.walkable)
                return false;
        }

        return requiredNodes.Count == prefabInfo.width * prefabInfo.height;
    }

    // Gets the grid nodes a prefab would take up based on its size
    private List<GridNode> GetRequiredNodes(GridNode startNode, PrefabSpawnInfo prefabInfo)
    {
        List<GridNode> nodes = new List<GridNode>();

        Vector3 startPos = startNode.WorldPosition;
        GridNode startGridNode = gridManager.GetNodeFromWorldPosition(startPos);

        if (startGridNode == null) return nodes;

        Vector2Int centerGridPos = GetGridPosition(startGridNode);
        if (centerGridPos.x == -1) return nodes;

        int halfWidth = prefabInfo.width / 2;
        int halfHeight = prefabInfo.height / 2;

        Vector2Int startGridPos = new Vector2Int(
            centerGridPos.x - halfWidth,
            centerGridPos.y - halfHeight
        );

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

    // Helper to get occupied nodes — same as required nodes
    private List<GridNode> GetOccupiedNodes(GridNode startNode, PrefabSpawnInfo prefabInfo)
    {
        return GetRequiredNodes(startNode, prefabInfo);
    }

    // Finds the X,Y grid coordinate for a given GridNode
    private Vector2Int GetGridPosition(GridNode node)
    {
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
