using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBarracksSpawner : MonoBehaviour
{
    [Header("Unit Configuration")]
    public GameObject enemyUnitPrefab;
    public Transform enemySpawnPoint;
    public Transform enemyTargetPoint;
    public GridManager gridManager;
    public UnitType enemyUnitType;
    public AStarPathfinding astarPathfinding;
    public VisualTargetPath visualTargetPath;

    [Header("AI Controls")]
    public Camera playerCamera; // Assign your main camera
    public LayerMask groundLayerMask = 1; // Layer mask for ground/walkable areas
    public bool enableAIMovement = true;
    public float spawnInterval = 5f; // Time between automatic spawns
    public int maxUnits = 10; // Maximum units this barracks can have

    [Header("Random Target Settings")]
    public int randomTargetRange = 15; // Range for random target selection from spawn point

    // Static list to track ALL enemy units from ALL enemy barracks
    private static List<UnitInstance> allEnemyUnits = new List<UnitInstance>();
    // List to track units spawned by THIS specific enemy barracks
    private List<UnitInstance> myEnemyUnits = new List<UnitInstance>();

    private float lastSpawnTime;
    private float lastRandomMoveTime;

    private void Start()
    {
        if (gridManager == null)
        {
            gridManager = FindAnyObjectByType<GridManager>();
        }
        if (astarPathfinding == null)
        {
            astarPathfinding = FindAnyObjectByType<AStarPathfinding>();
        }
        if (visualTargetPath == null)
        {
            visualTargetPath = FindAnyObjectByType<VisualTargetPath>();
        }
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        lastSpawnTime = Time.time;
        lastRandomMoveTime = Time.time;
    }

    private void Update()
    {
        // Automatic spawning based on interval
        if (enableAIMovement && Time.time - lastSpawnTime > spawnInterval)
        {
            if (myEnemyUnits.Count < maxUnits)
            {
                SpawnEnemyUnit();
                lastSpawnTime = Time.time;
            }
        }

        // Random movement every 2 seconds
        if (Time.time - lastRandomMoveTime > 2f)
        {
            MoveUnitsRandomly();
            lastRandomMoveTime = Time.time;
        }

        // Manual keyboard controls for testing
        if (Input.GetKeyDown(KeyCode.K))
        {
            MoveAllEnemyUnitsToTarget(); // Moves ALL enemy units to this barracks target
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            SpawnEnemyUnit();
        }

        // Move only units from THIS enemy barracks
        if (Input.GetKeyDown(KeyCode.L))
        {
            MoveMyEnemyUnitsOnly();
        }

        // Optional: Clear all enemy units from ALL enemy barracks with N key
        if (Input.GetKeyDown(KeyCode.N))
        {
            ClearAllEnemyUnits();
        }

        // Optional: Clear only units from THIS enemy barracks with M key
        if (Input.GetKeyDown(KeyCode.M))
        {
            ClearMyEnemyUnits();
        }
    }

    private void MoveMyEnemyUnitsToNewTarget(Vector3 newTargetPosition)
    {
        myEnemyUnits.RemoveAll(unit => unit == null);

        GridNode targetNode = gridManager.GetNodeFromWorldPosition(newTargetPosition);
        if (targetNode != null)
        {
            foreach (UnitInstance unit in myEnemyUnits)
            {
                if (unit != null)
                {
                    unit.MoveTo(targetNode);
                    Debug.Log($"Moving enemy unit {unit.name} to new target {targetNode}");
                }
            }

            if (myEnemyUnits.Count > 0)
            {
                Debug.Log($"Moved {myEnemyUnits.Count} enemy units to new position");
            }
        }
        else
        {
            Debug.Log($"Couldn't find valid node at target position: {newTargetPosition}");
        }
    }

    public void MoveAllEnemyUnitsToTarget()
    {
        // Clean up destroyed units from both lists
        allEnemyUnits.RemoveAll(unit => unit == null);
        myEnemyUnits.RemoveAll(unit => unit == null);

        GridNode targetNode = gridManager.GetNodeFromWorldPosition(enemyTargetPoint.position);
        if (targetNode != null)
        {
            // Move ALL enemy units from ALL enemy barracks to this target position
            foreach (UnitInstance unit in allEnemyUnits)
            {
                if (unit != null)
                {
                    unit.MoveTo(targetNode);
                    Debug.Log($"Moving enemy unit {unit.name} to {targetNode}");
                }
            }

            if (allEnemyUnits.Count > 0)
            {
                Debug.Log($"Updated position for {allEnemyUnits.Count} total enemy units to move to this enemy barracks");
            }
            else
            {
                Debug.Log("No existing enemy units to move");
            }
        }
        else
        {
            Debug.Log("Couldn't find enemy target node");
        }
    }

    public void SpawnEnemyUnit()
    {
        if (enemyUnitPrefab == null || enemySpawnPoint == null) return;

        GameObject newEnemyUnit = Instantiate(enemyUnitPrefab, enemySpawnPoint.position, Quaternion.identity);
        UnitInstance unit = newEnemyUnit.GetComponent<UnitInstance>();
        unit.Initialize(astarPathfinding, enemyUnitType, gridManager, visualTargetPath);

        // Add the new unit to both tracking lists
        allEnemyUnits.Add(unit);
        myEnemyUnits.Add(unit);

        // Get a random target node instead of the fixed enemy target point
        GridNode spawnNode = gridManager.GetNodeFromWorldPosition(enemySpawnPoint.position);
        GridNode randomTargetNode = GetRandomNodeWithinRange(spawnNode, randomTargetRange);

        if (randomTargetNode != null)
        {
            unit.MoveTo(randomTargetNode);
            Debug.Log($"Spawned new enemy unit {unit.name} and moving to random target {randomTargetNode}");
        }
        else
        {
            // Fallback to original target if no random node found
            GridNode fallbackTargetNode = gridManager.GetNodeFromWorldPosition(enemyTargetPoint.position);
            if (fallbackTargetNode != null)
            {
                unit.MoveTo(fallbackTargetNode);
                Debug.Log($"Spawned new enemy unit {unit.name} and moving to fallback target {fallbackTargetNode}");
            }
            else
            {
                Debug.Log("Couldn't find any valid target node for spawned unit");
            }
        }
    }

    // Method to move only units spawned by THIS enemy barracks
    public void MoveMyEnemyUnitsOnly()
    {
        myEnemyUnits.RemoveAll(unit => unit == null);

        GridNode targetNode = gridManager.GetNodeFromWorldPosition(enemyTargetPoint.position);
        if (targetNode != null)
        {
            foreach (UnitInstance unit in myEnemyUnits)
            {
                if (unit != null)
                {
                    unit.MoveTo(targetNode);
                    Debug.Log($"Moving my enemy unit {unit.name} to {targetNode}");
                }
            }

            Debug.Log($"Moved {myEnemyUnits.Count} enemy units from this barracks");
        }
    }

    // Public method to set target position programmatically
    public void SetEnemyTargetPosition(Vector3 newPosition)
    {
        enemyTargetPoint.position = newPosition;
        MoveMyEnemyUnitsToNewTarget(newPosition);
    }

    // Method to toggle AI movement functionality
    public void SetAIMovementEnabled(bool enabled)
    {
        enableAIMovement = enabled;
        Debug.Log($"Enemy AI movement: {(enabled ? "Enabled" : "Disabled")}");
    }

    // Method to set spawn interval
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = interval;
        Debug.Log($"Enemy spawn interval set to: {interval} seconds");
    }

    // Method to set random target range
    public void SetRandomTargetRange(int range)
    {
        randomTargetRange = range;
        Debug.Log($"Enemy random target range set to: {range} nodes");
    }

    // Move all units randomly within 5 nodes
    private void MoveUnitsRandomly()
    {
        myEnemyUnits.RemoveAll(unit => unit == null);

        foreach (UnitInstance unit in myEnemyUnits)
        {
            if (unit != null)
            {
                GridNode currentNode = gridManager.GetNodeFromWorldPosition(unit.transform.position);
                if (currentNode != null)
                {
                    GridNode randomNode = GetRandomNodeWithinRange(currentNode, 15);
                    if (randomNode != null)
                    {
                        unit.MoveTo(randomNode);
                    }
                }
            }
        }
    }

    // Get random walkable node within range
    private GridNode GetRandomNodeWithinRange(GridNode centerNode, int range)
    {
        if (centerNode == null) return null;

        // Get center position in grid coordinates
        Vector3 centerPos = centerNode.WorldPosition;
        int centerX = Mathf.RoundToInt(centerPos.x / gridManager.GridSettings.NodeSize);
        int centerY = Mathf.RoundToInt(centerPos.z / gridManager.GridSettings.NodeSize);

        for (int i = 0; i < 20; i++) // Max 20 attempts
        {
            int randomX = centerX + Random.Range(-range, range + 1);
            int randomY = centerY + Random.Range(-range, range + 1);

            GridNode node = gridManager.GetNode(randomX, randomY);
            if (node != null && node.walkable)
            {
                return node;
            }
        }
        return null;
    }

    // Optional: Method to get all currently active enemy units from ALL enemy barracks
    public static List<UnitInstance> GetAllActiveEnemyUnits()
    {
        allEnemyUnits.RemoveAll(unit => unit == null);
        return new List<UnitInstance>(allEnemyUnits);
    }

    // Optional: Method to get enemy units from THIS barracks only
    public List<UnitInstance> GetMyEnemyUnits()
    {
        myEnemyUnits.RemoveAll(unit => unit == null);
        return new List<UnitInstance>(myEnemyUnits);
    }

    // Optional: Method to clear all enemy units from ALL enemy barracks
    public static void ClearAllEnemyUnits()
    {
        foreach (UnitInstance unit in allEnemyUnits)
        {
            if (unit != null)
            {
                Destroy(unit.gameObject);
            }
        }
        allEnemyUnits.Clear();
        Debug.Log("Cleared all enemy units from all enemy barracks");
    }

    // Clear enemy units spawned by THIS barracks only
    public void ClearMyEnemyUnits()
    {
        foreach (UnitInstance unit in myEnemyUnits)
        {
            if (unit != null)
            {
                // Remove from global list too
                allEnemyUnits.Remove(unit);
                Destroy(unit.gameObject);
            }
        }
        myEnemyUnits.Clear();
        Debug.Log("Cleared enemy units from this barracks");
    }

    // Visual debug helper
    private void OnDrawGizmos()
    {
        if (enemyTargetPoint != null)
        {
            Gizmos.color = Color.blue; // Different color for enemy targets
            Gizmos.DrawWireSphere(enemyTargetPoint.position, 0.5f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(enemyTargetPoint.position, enemyTargetPoint.position + Vector3.up * 2f);
        }

        if (enemySpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(enemySpawnPoint.position, Vector3.one * 0.5f);

            // Draw range indicator for random target selection
            Gizmos.color = Color.yellow;
            float rangeSize = randomTargetRange * (gridManager != null ? gridManager.GridSettings.NodeSize : 1f);
            Gizmos.DrawWireCube(enemySpawnPoint.position, Vector3.one * rangeSize);
        }
    }
}