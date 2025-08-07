using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [Header("Unit Settings")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private int unitsToSpawn = 5;
    [SerializeField] private float spawnDelay = 5f; // 5 seconds between spawns

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 spawnAreaCenter = Vector3.zero;
    [SerializeField] private float spawnRadius = 5f;

    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private CommandTargetPath commandTargetPath;

    [Header("Debug")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool drawSpawnArea = true;

    private List<GameObject> spawnedUnits = new List<GameObject>();
    private bool isSpawning = false;

    // Gets things ready when the game starts
    void Start()
    {
        // Find components if not assigned
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        if (commandTargetPath == null)
            commandTargetPath = FindObjectOfType<CommandTargetPath>();

        if (spawnOnStart)
        {
            StartSpawning();
        }
    }

    // Checks for keypresses every frame
    void Update()
    {
        // Optional: Press 'S' to start spawning manually
        if (Input.GetKeyDown(KeyCode.S) && !isSpawning)
        {
            StartSpawning();
        }

        // Optional: Press 'R' to reset and clear all units
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAllUnits();
        }
    }

    // Kicks off the whole spawning process
    [ContextMenu("Start Spawning")]
    public void StartSpawning()
    {
        if (isSpawning)
        {
            Debug.LogWarning("Already spawning units!");
            return;
        }

        if (unitPrefab == null)
        {
            Debug.LogError("Unit prefab is not assigned!");
            return;
        }

        // Reset command field
        if (commandTargetPath != null)
        {
            commandTargetPath.ResetField();
            Debug.Log("Command target path field reset.");
        }

        // Clear existing units
        ClearAllUnits();

        // Start spawning coroutine
        StartCoroutine(SpawnUnitsWithDelay());
    }

    // Does the actual spawning work, one unit at a time with delays
    private IEnumerator SpawnUnitsWithDelay()
    {
        isSpawning = true;

        // Wait for grid to be initialized if needed
        while (gridManager != null && !gridManager.IsInitialized)
        {
            Debug.Log("Waiting for grid to initialize...");
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log($"Starting to spawn {unitsToSpawn} units with {spawnDelay} second delays...");

        for (int i = 0; i < unitsToSpawn; i++)
        {
            // Find a valid spawn position
            Vector3 spawnPosition = GetValidSpawnPosition(i);

            // Instantiate the unit
            GameObject newUnit = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);

            // Configure the unit
            SetupUnit(newUnit, i);

            // Add to spawned units list
            spawnedUnits.Add(newUnit);

            Debug.Log($"Spawned Unit {i + 1}/{unitsToSpawn} at position {spawnPosition}");

            // Wait for the specified delay before spawning the next unit
            if (i < unitsToSpawn - 1) // Don't wait after the last unit
            {
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        isSpawning = false;
        Debug.Log($"Finished spawning all {unitsToSpawn} units!");
    }

    // Figures out where to put each unit so they don't overlap
    private Vector3 GetValidSpawnPosition(int unitIndex)
    {
        Vector3 spawnPosition = spawnAreaCenter;

        if (gridManager != null)
        {
            // Try to find a valid position in a circle around the spawn center
            for (int attempts = 0; attempts < 20; attempts++)
            {
                // Calculate position in a spiral pattern
                float angle = unitIndex * 72f + attempts * 36f; // 72 degrees apart, with random offset
                float distance = (unitIndex * 0.5f + attempts * 0.2f) * spawnRadius / unitsToSpawn;
                distance = Mathf.Min(distance, spawnRadius);

                Vector3 offset = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                    0,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * distance
                );

                Vector3 testPosition = spawnAreaCenter + offset;
                GridNode node = gridManager.GetNodeFromWorldPosition(testPosition);

                if (node != null && node.walkable)
                {
                    spawnPosition = node.WorldPosition;
                    break;
                }
            }
        }
        else
        {
            // Fallback: simple circular positioning
            float angle = unitIndex * (360f / unitsToSpawn);
            float distance = spawnRadius * 0.7f;

            spawnPosition = spawnAreaCenter + new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                0,
                Mathf.Sin(angle * Mathf.Deg2Rad) * distance
            );
        }

        return spawnPosition;
    }

    // Gets a unit ready after it's been created
    private void SetupUnit(GameObject unit, int unitIndex)
    {
        // Name the unit
        unit.name = $"Unit_{unitIndex + 1}";

        // Ensure the unit has Player_Targeting component
        Player_Targeting targeting = unit.GetComponent<Player_Targeting>();
        if (targeting == null)
        {
            targeting = unit.AddComponent<Player_Targeting>();
            Debug.Log($"Added Player_Targeting component to {unit.name}");
        }

        // Set initial position slightly above ground to avoid clipping
        unit.transform.position += Vector3.up * 0.1f;

        // Optional: Add other components or initialization here
        InitializeUnitComponents(unit, unitIndex);
    }

    // Adds extra stuff to each unit like colors and IDs
    private void InitializeUnitComponents(GameObject unit, int unitIndex)
    {
        // Add any additional unit initialization here
        // For example:

        // Add a simple identifier component
        UnitIdentifier identifier = unit.GetComponent<UnitIdentifier>();
        if (identifier == null)
        {
            identifier = unit.AddComponent<UnitIdentifier>();
        }
        identifier.SetUnitID(unitIndex);

        // Set unit color for visual distinction (if renderer exists)
        Renderer unitRenderer = unit.GetComponent<Renderer>();
        if (unitRenderer != null)
        {
            Color unitColor = Color.HSVToRGB((unitIndex * 0.2f) % 1f, 0.7f, 0.9f);
            unitRenderer.material.color = unitColor;
        }
    }

    // Destroys all the units we've spawned
    [ContextMenu("Clear All Units")]
    public void ClearAllUnits()
    {
        foreach (GameObject unit in spawnedUnits)
        {
            if (unit != null)
            {
                DestroyImmediate(unit);
            }
        }

        spawnedUnits.Clear();
        Debug.Log("All spawned units cleared.");
    }

    // Wipes everything clean and starts fresh
    public void ResetAllUnits()
    {
        // Stop spawning if in progress
        StopAllCoroutines();
        isSpawning = false;

        // Clear all units
        ClearAllUnits();

        // Reset command field
        if (commandTargetPath != null)
        {
            commandTargetPath.ResetField();
        }

        Debug.Log("All units reset and command field cleared.");
    }

    // Returns a copy of all the units we've made
    public List<GameObject> GetSpawnedUnits()
    {
        // Remove null references (destroyed units)
        spawnedUnits.RemoveAll(unit => unit == null);
        return new List<GameObject>(spawnedUnits);
    }

    // Tells you if we're currently making units
    public bool IsSpawning()
    {
        return isSpawning;
    }

    // Counts how many units are still alive
    public int GetSpawnedUnitCount()
    {
        spawnedUnits.RemoveAll(unit => unit == null);
        return spawnedUnits.Count;
    }

    // Lets you change how many units to spawn and the delay between them
    public void SetSpawnSettings(int unitCount, float delay)
    {
        unitsToSpawn = Mathf.Max(1, unitCount);
        spawnDelay = Mathf.Max(0.1f, delay);
    }

    // Changes where units spawn and how spread out they are
    public void SetSpawnArea(Vector3 center, float radius)
    {
        spawnAreaCenter = center;
        spawnRadius = Mathf.Max(1f, radius);
    }

    // Draws helpful stuff in the scene view so you can see the spawn area
    private void OnDrawGizmos()
    {
        if (!drawSpawnArea) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(spawnAreaCenter, spawnRadius);

        // Draw spawn center
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(spawnAreaCenter, Vector3.one * 0.5f);

        // Draw unit positions if spawned
        if (Application.isPlaying && spawnedUnits.Count > 0)
        {
            Gizmos.color = Color.green;
            foreach (GameObject unit in spawnedUnits)
            {
                if (unit != null)
                {
                    Gizmos.DrawWireSphere(unit.transform.position, 0.3f);
                }
            }
        }
    }
}

// Helper component for unit identification
public class UnitIdentifier : MonoBehaviour
{
    [SerializeField] private int unitID;

    // Gets this unit's unique ID number
    public int GetUnitID() => unitID;

    // Sets this unit's unique ID number
    public void SetUnitID(int id) => unitID = id;
}