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


    [Header("Castle Targeting")]
    public bool waitForCastle = true; // Wait for castle before spawning
    public bool onlySpawnAfterCastle = true; // Only spawn units after castle is placed

    private GameObject currentCastle;
    private bool castleFound = false;


    [Header("AI Controls")]
    public Camera playerCamera; // Assign your main camera
    public LayerMask groundLayerMask = 1; // Layer mask for ground/walkable areas
    public bool enableAIMovement = true;
    public float spawnInterval = 5f; // Time between automatic spawns
    public int maxUnits = 10; // Maximum units this barracks can have

    [Header("Target Prefabs")]
    public List<GameObject> targetPrefabs = new List<GameObject>(); // List of target prefabs to attack
    public bool useRandomTargetSelection = true; // Whether to randomly select targets or use all
    public float retargetInterval = 10f; // Time between retargeting units

    // Static list to track ALL enemy units from ALL enemy barracks
    private static List<UnitInstance> allEnemyUnits = new List<UnitInstance>();
    // List to track units spawned by THIS specific enemy barracks
    private List<UnitInstance> myEnemyUnits = new List<UnitInstance>();

    private float lastSpawnTime;
    private float lastRetargetTime;

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
        lastRetargetTime = Time.time;

        CastleManager.OnCastlePlaced += OnCastlePlaced;
        CastleManager.OnCastleDestroyed += OnCastleDestroyed;

        // Check if castle already exists
        if (CastleManager.HasCastle())
        {
            OnCastlePlaced(CastleManager.CurrentCastle);
        }
    }
    private void FindAndAddCastle()
    {
        // Try to find castle by name first
        GameObject castle = GameObject.Find("Castle");

        // If not found by name, try finding by tag (make sure to tag your castle with "Castle")
        if (castle == null)
        {
            castle = GameObject.FindGameObjectWithTag("Castle");
        }

        // If still not found, try finding any object with "castle" in the name (case insensitive)
        if (castle == null)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("castle"))
                {
                    castle = obj;
                    break;
                }
            }
        }

        if (castle != null)
        {
            // Clear existing targets and add the castle
            targetPrefabs.Clear();
            targetPrefabs.Add(castle);
            Debug.Log($"Found and added castle: {castle.name} at position {castle.transform.position}");
        }
        else
        {
            Debug.LogWarning("Could not find castle prefab! Make sure it exists in the scene and is named 'Castle' or tagged with 'Castle'");
        }
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

        // Retarget units periodically
        if (Time.time - lastRetargetTime > retargetInterval)
        {
            //RetargetUnits();
            lastRetargetTime = Time.time;
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

        // Get a target prefab instead of random node
        GameObject targetPrefab = GetTargetPrefab();

        if (targetPrefab != null)
        {
            GridNode targetNode = gridManager.GetNodeFromWorldPosition(targetPrefab.transform.position);
            if (targetNode != null)
            {
                unit.MoveTo(targetNode);
                Debug.Log($"Spawned new enemy unit {unit.name} and moving to target prefab {targetPrefab.name}");
            }
            else
            {
                Debug.Log($"Couldn't find valid node for target prefab {targetPrefab.name}");
                // Fallback to original target point
                FallbackToOriginalTarget(unit);
            }
        }
        else
        {
            Debug.Log("No target prefabs assigned, using fallback target");
            // Fallback to original target if no prefabs assigned
            FallbackToOriginalTarget(unit);
        }
    }

    private void FallbackToOriginalTarget(UnitInstance unit)
    {
        GridNode fallbackTargetNode = gridManager.GetNodeFromWorldPosition(enemyTargetPoint.position);
        if (fallbackTargetNode != null)
        {
            unit.MoveTo(fallbackTargetNode);
            Debug.Log($"Moving enemy unit {unit.name} to fallback target {fallbackTargetNode}");
        }
        else
        {
            Debug.Log("Couldn't find any valid target node for spawned unit");
        }
    }

    // Get a target prefab based on selection method
    private GameObject GetTargetPrefab()
    {
        if (targetPrefabs == null || targetPrefabs.Count == 0)
        {
            return null;
        }

        // Remove null references
        targetPrefabs.RemoveAll(prefab => prefab == null);

        if (targetPrefabs.Count == 0)
        {
            return null;
        }

        if (useRandomTargetSelection)
        {
            // Return a random target prefab
            int randomIndex = Random.Range(0, targetPrefabs.Count);
            return targetPrefabs[randomIndex];
        }
        else
        {
            // Return the first available target prefab
            return targetPrefabs[0];
        }
    }

    // Retarget existing units to new prefab targets
    /*
    private void RetargetUnits()
    {
        myEnemyUnits.RemoveAll(unit => unit == null);

        foreach (UnitInstance unit in myEnemyUnits)
        {
            if (unit != null)
            {
                GameObject targetPrefab = GetTargetPrefab();
                if (targetPrefab != null)
                {
                    GridNode targetNode = gridManager.GetNodeFromWorldPosition(targetPrefab.transform.position);
                    if (targetNode != null)
                    {
                        unit.MoveTo(targetNode);
                        Debug.Log($"Retargeting enemy unit {unit.name} to prefab {targetPrefab.name}");
                    }
                }
            }
        }
    }
    */
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

    // Method to set retarget interval
    public void SetRetargetInterval(float interval)
    {
        retargetInterval = interval;
        Debug.Log($"Enemy retarget interval set to: {interval} seconds");
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
    }
    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        CastleManager.OnCastlePlaced -= OnCastlePlaced;
        CastleManager.OnCastleDestroyed -= OnCastleDestroyed;
    }

    private void OnCastlePlaced(GameObject castle)
    {
        currentCastle = castle;
        castleFound = true;

        // Clear existing targets and add the castle
        targetPrefabs.Clear();
        targetPrefabs.Add(castle);

        Debug.Log($"Enemy barracks {name} now targeting castle at: {castle.transform.position}");

        // Retarget existing units to the castle
        RetargetUnitsToCurrentCastle();
    }
    private void OnCastleDestroyed()
    {
        currentCastle = null;
        castleFound = false;
        targetPrefabs.Clear();

        Debug.Log($"Enemy barracks {name} lost castle target");
    }
    private void RetargetUnitsToCurrentCastle()
    {
        if (currentCastle == null) return;

        myEnemyUnits.RemoveAll(unit => unit == null);

        GridNode castleNode = gridManager.GetNodeFromWorldPosition(currentCastle.transform.position);
        if (castleNode != null)
        {
            foreach (UnitInstance unit in myEnemyUnits)
            {
                if (unit != null)
                {
                    unit.MoveTo(castleNode);
                    Debug.Log($"Retargeting {unit.name} to newly placed castle");
                }
            }
        }
    }

}