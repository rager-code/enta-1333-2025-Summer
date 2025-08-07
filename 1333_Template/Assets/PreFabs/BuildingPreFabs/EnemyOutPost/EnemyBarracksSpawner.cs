using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

    [Header("Castle Targeting")]
    public bool waitForCastle = true; // Wait for castle before spawning
    public bool onlySpawnAfterCastle = true; // Only spawn units after castle is placed

    private GameObject currentCastle;
    private bool castleFound = false;

    [Header("AI Controls")]
    public Camera playerCamera; // Assign your main camera
    public LayerMask groundLayerMask = 1; // Layer mask for ground/walkable areas
    public bool enableAIMovement = true;
    public float spawnInterval = 15f; // Time between automatic spawns
    public int maxUnits = 10; // Maximum units this barracks can have

    [Header("Target Prefabs")]
    public List<GameObject> targetPrefabs = new List<GameObject>(); // List of target prefabs to attack
    public bool useRandomTargetSelection = true; // Whether to randomly select targets or use all
    public float retargetInterval = 10f; // Time between retargeting units

    [Header("Health Reference")]
    public SimpleHealth targetHealth; // Reference to the castle's health script
    private BuildingHealthAndDamage buildingHealthAndDamage;

    [Header("Combat Settings")]
    public float proximityCheckInterval = 0.5f; // How often to check unit proximity to targets
    public bool destroyOnReachTarget = true; // Enable/disable destroying units when they reach target
    public float destructionDistance = 2f; // Distance in world units to destroy enemies (approximately 1 node)

    [Header("Pathfinding Settings")]
    public int maxSearchRadius = 10; // Maximum radius to search for walkable nodes
    public float searchStepSize = 1f; // Step size for searching (world units)

    // Static list to track ALL enemy units from ALL enemy barracks
    private static List<UnitInstance> allEnemyUnits = new List<UnitInstance>();
    // List to track units spawned by THIS specific enemy barracks
    private List<UnitInstance> myEnemyUnits = new List<UnitInstance>();

    private float lastSpawnTime;
    private float lastRetargetTime;
    private float lastProximityCheckTime;

   public bool castleEnemyDamage = false;

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
        if (buildingHealthAndDamage == null)
        {
            buildingHealthAndDamage = FindAnyObjectByType<BuildingHealthAndDamage>();
        }
        lastSpawnTime = Time.time;
        lastRetargetTime = Time.time;
        lastProximityCheckTime = Time.time;
    }
    
    private void Update()
    {
        bool canSpawn = !onlySpawnAfterCastle || castleFound;

        // Automatic spawning based on interval
        if (enableAIMovement && canSpawn && Time.time - lastSpawnTime > spawnInterval)
        {
            if (myEnemyUnits.Count < maxUnits)
            {
                SpawnEnemyUnit();
                lastSpawnTime = Time.time;
            }
        }

        // Check unit proximity to targets
        if (destroyOnReachTarget && Time.time - lastProximityCheckTime > proximityCheckInterval)
        {
            CheckUnitProximityToTargets();
            lastProximityCheckTime = Time.time;
        }

        // Retarget units periodically
        if (Time.time - lastRetargetTime > retargetInterval)
        {
            //RetargetUnits();
            lastRetargetTime = Time.time;
        }

        // Manual keyboard controls for testing
        if (Input.GetKeyDown(KeyCode.K))
        {
           // MoveAllEnemyUnitsToTarget(currentCastle.transform); // Moves ALL enemy units to this barracks target
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

    /// <summary>
    /// Finds the nearest walkable node to a given target position
    /// </summary>
    /// <param name="targetPosition">The world position to find nearest walkable node for</param>
    /// <returns>The nearest walkable GridNode, or null if none found within search radius</returns>
    private GridNode FindNearestWalkableNode(Vector3 targetPosition)
    {
        GridNode originalNode = gridManager.GetNodeFromWorldPosition(targetPosition);

        // If the original node is walkable, return it
        if (originalNode != null && originalNode.walkable)
        {
            return originalNode;
        }

        Debug.Log($"Target position {targetPosition} is on unwalkable node, searching for nearest walkable node...");

        GridNode bestNode = null;
        float shortestDistance = float.MaxValue;

        // Search in expanding radius around the target position using world units
        for (int radius = 1; radius <= maxSearchRadius; radius++)
        {
            float searchRadius = radius * searchStepSize;

            // Check positions in a circle pattern around the target
            int numPoints = Mathf.Max(8, radius * 8); // More points for larger radii

            for (int i = 0; i < numPoints; i++)
            {
                float angle = (float)i / numPoints * 2f * Mathf.PI;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * searchRadius,
                    0,
                    Mathf.Sin(angle) * searchRadius
                );

                Vector3 checkPosition = targetPosition + offset;
                GridNode checkNode = gridManager.GetNodeFromWorldPosition(checkPosition);

                if (checkNode != null && checkNode.walkable)
                {
                    float distance = Vector3.Distance(targetPosition, checkNode.WorldPosition);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        bestNode = checkNode;
                    }
                }
            }

            // If we found a walkable node at this radius, return it (closest possible)
            if (bestNode != null)
            {
                Debug.Log($"Found nearest walkable node at distance {shortestDistance} from target position");
                return bestNode;
            }
        }

        Debug.LogWarning($"Could not find any walkable node within radius {maxSearchRadius * searchStepSize} world units of target position {targetPosition}");
        return null;
    }

    /// <summary>
    /// Gets a safe target node for a given prefab, automatically finding nearest walkable if needed
    /// </summary>
    /// <param name="targetPrefab">The GameObject to target</param>
    /// <returns>A walkable GridNode near the target, or null if none found</returns>
    private GridNode GetSafeTargetNode(GameObject targetPrefab)
    {
        if (targetPrefab == null) return null;

        return FindNearestWalkableNode(targetPrefab.transform.position);
    }

    // New method to check if units are close to their targets
    private void CheckUnitProximityToTargets()
    {
        // Clean up destroyed units first
        allEnemyUnits.RemoveAll(unit => unit == null);
        myEnemyUnits.RemoveAll(unit => unit == null);

        List<UnitInstance> unitsToDestroy = new List<UnitInstance>();

        foreach (UnitInstance unit in allEnemyUnits)
        {
            if (unit != null)
            {
                Vector3 unitPosition = unit.transform.position;

                // Check distance to castle (primary target)
                if (CastleManager.Instance != null && CastleManager.Instance.Castle != null)
                {
                    Vector3 castlePosition = CastleManager.Instance.Castle.transform.position;
                    float distance = Vector3.Distance(unitPosition, castlePosition);

                    // Check if unit is within destruction distance
                    if (distance <= destructionDistance)
                    {
                        Debug.Log($"Enemy unit {unit.name} reached castle target - destroying!");
                        unitsToDestroy.Add(unit);
                        continue;
                    }
                }

                // Check distance to other target prefabs
                foreach (GameObject targetPrefab in targetPrefabs)
                {
                    if (targetPrefab != null)
                    {
                        Vector3 targetPosition = targetPrefab.transform.position;
                        float distance = Vector3.Distance(unitPosition, targetPosition);

                        // Check if unit is within destruction distance
                        if (distance <= destructionDistance)
                        {
                            
                            Debug.Log($"Enemy unit {unit.name} reached target {targetPrefab.name} - destroying!");
                            unitsToDestroy.Add(unit);
                            break; // No need to check other targets for this unit
                        }
                    }
                }
            }
        }

        // Destroy units that reached their targets
        foreach (UnitInstance unit in unitsToDestroy)
        {
            castleEnemyDamage = true;
            DestroyEnemyUnit(unit);
        }
    }

    // Helper method to properly destroy an enemy unit
    private void DestroyEnemyUnit(UnitInstance unit)
    {
        if (unit != null)
        {
            // Try to get target health if we don't have it
            if (targetHealth == null)
            {
                TryFindTargetHealth();
            }

            // Call EnemyInCastle when unit reaches target (deals damage to castle)
            if (targetHealth != null)
            {
                targetHealth.EnemyInCastle();
                Debug.Log($"Enemy unit {unit.name} reached castle - dealt 50 damage! Castle health: {targetHealth.Health}");
            }
            else
            {
                Debug.LogWarning("No target health found - cannot deal damage! Reasons could be:");
                Debug.LogWarning("1. Castle not spawned yet");
                Debug.LogWarning("2. Castle doesn't have SimpleHealth component");
                Debug.LogWarning("3. CastleManager.Instance.Castle is null");

                // Try one more time to find it
                TryFindTargetHealth();
                if (targetHealth != null)
                {
                    targetHealth.EnemyInCastle();
                    Debug.Log($"Found target health on second try - dealt damage!");
                }
            }

            // Remove from both tracking lists
            allEnemyUnits.Remove(unit);
            myEnemyUnits.Remove(unit);

            // Destroy the game object
            Destroy(unit.gameObject);
            Debug.Log($"Destroyed enemy unit {unit.name} that reached its target");
        }
    }

    // Helper method to find target health
    private void TryFindTargetHealth()
    {
        // Method 1: Try CastleManager
        if (CastleManager.Instance?.Castle != null)
        {
            targetHealth = CastleManager.Instance.Castle.GetComponent<SimpleHealth>();
            if (targetHealth != null)
            {
                Debug.Log("Found target health via CastleManager!");
                return;
            }
        }

        // Method 2: Find any SimpleHealth in scene
        targetHealth = FindObjectOfType<SimpleHealth>();
        if (targetHealth != null)
        {
            Debug.Log($"Found target health via FindObjectOfType on: {targetHealth.gameObject.name}");
            return;
        }

        // Method 3: Search by tag (if your castle has a specific tag)
        GameObject castle = GameObject.FindGameObjectWithTag("Castle");
        if (castle != null)
        {
            targetHealth = castle.GetComponent<SimpleHealth>();
            if (targetHealth != null)
            {
                Debug.Log("Found target health via Castle tag!");
                return;
            }
        }

        Debug.LogError("Could not find target health using any method!");
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

        // Get the castle as target
        GameObject targetPrefab = CastleManager.Instance?.Castle;

        if (targetPrefab != null)
        {
            Debug.Log($"Targeting enemy unit to {targetPrefab.name} at position {targetPrefab.transform.position}");

            // Use the safe target node method
            GridNode safeTargetNode = GetSafeTargetNode(targetPrefab);

            if (safeTargetNode != null)
            {
                unit.MoveTo(safeTargetNode);
                Debug.Log($"Spawned new enemy unit {newEnemyUnit.name} and moving to safe target node at {safeTargetNode.WorldPosition}");
            }
            else
            {
                Debug.LogError($"Could not find a safe walkable node near target {targetPrefab.name}!");
                // Optionally destroy the unit if no valid path can be found
                DestroyEnemyUnit(unit);
            }
        }
        else
        {
            Debug.LogWarning("No target castle found!");
            // Optionally destroy or handle units when no target exists
        }
    }

    // Public method to set target position programmatically
    public void SetEnemyTargetPosition(Vector3 newPosition)
    {
        enemyTargetPoint.position = newPosition;
        //MoveMyEnemyUnitsToNewTarget(newPosition);
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

    // Method to set retarget interval
    public void SetRetargetInterval(float interval)
    {
        retargetInterval = interval;
        Debug.Log($"Enemy retarget interval set to: {interval} seconds");
    }

    // Method to set proximity check interval
    public void SetProximityCheckInterval(float interval)
    {
        proximityCheckInterval = interval;
        Debug.Log($"Enemy proximity check interval set to: {interval} seconds");
    }

    // Method to set destruction distance
    public void SetDestructionDistance(float distance)
    {
        destructionDistance = distance;
        Debug.Log($"Enemy destruction distance set to: {distance} units");
    }

    // Method to set max search radius for finding walkable nodes
    public void SetMaxSearchRadius(int radius)
    {
        maxSearchRadius = radius;
        Debug.Log($"Max search radius set to: {radius} steps");
    }

    // Method to set search step size
    public void SetSearchStepSize(float stepSize)
    {
        searchStepSize = stepSize;
        Debug.Log($"Search step size set to: {stepSize} world units");
    }

    // Method to toggle destroy on reach target
    public void SetDestroyOnReachTarget(bool enabled)
    {
        destroyOnReachTarget = enabled;
        Debug.Log($"Destroy on reach target: {(enabled ? "Enabled" : "Disabled")}");
    }

    // Method to add a target prefab
    public void AddTargetPrefab(GameObject prefab)
    {
        if (prefab != null && !targetPrefabs.Contains(prefab))
        {
            targetPrefabs.Add(prefab);
            Debug.Log($"Added target prefab: {prefab.name}");
        }
    }

    // Method to remove a target prefab
    public void RemoveTargetPrefab(GameObject prefab)
    {
        if (targetPrefabs.Contains(prefab))
        {
            targetPrefabs.Remove(prefab);
            Debug.Log($"Removed target prefab: {prefab.name}");
        }
    }

    // Method to clear all target prefabs
    public void ClearTargetPrefabs()
    {
        targetPrefabs.Clear();
        Debug.Log("Cleared all target prefabs");
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
        }

        // Draw lines to target prefabs
        if (targetPrefabs != null && targetPrefabs.Count > 0)
        {
            Gizmos.color = Color.magenta;
            foreach (GameObject target in targetPrefabs)
            {
                if (target != null)
                {
                    Gizmos.DrawWireSphere(target.transform.position, 0.3f);
                    if (enemySpawnPoint != null)
                    {
                        Gizmos.DrawLine(enemySpawnPoint.position, target.transform.position);
                    }
                }
            }
        }

        // Visualize search radius for nearest walkable node
        if (CastleManager.Instance != null && CastleManager.Instance.Castle != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(CastleManager.Instance.Castle.transform.position, maxSearchRadius * searchStepSize);
        }
    }
}