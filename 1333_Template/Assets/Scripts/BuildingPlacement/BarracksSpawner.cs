using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class BarracksSpawner : MonoBehaviour
{
    // Basic setup for spawning units
    public GameObject unitPrefab;
    public Transform spawnPoint;
    public Transform endPoint;
    public GridManager gridManager;
    public UnitType unitType;
    public AStarPathfinding astarPathfinding;
    public VisualTargetPath visualTargetPath;

    [Header("Click Controls")]
    public Camera playerCamera; // Assign your main camera
    public LayerMask groundLayerMask = 1; // Layer mask for ground/walkable areas
    public bool enableClickToMove = true;

    // Keep track of all units from all barracks in the game
    private static List<UnitInstance> allUnits = new List<UnitInstance>();
    // Keep track of units spawned by just this specific barracks
    private List<UnitInstance> myUnits = new List<UnitInstance>();

    // Find all the required components when we start
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
    }

    // Handle input every frame
    private void Update()
    {
        // Handle mouse click for moving target
        if (enableClickToMove && Input.GetMouseButtonDown(0)) // Left mouse button
        {
            HandleMouseClick();
        }

        // G key - Move ALL units from ALL barracks to this barracks' target
        if (Input.GetKeyDown(KeyCode.G))
        {
            UnitPositionTracker(); // Moves ALL units to this barracks
        }

        // F key - Spawn a new unit from this barracks
        if (Input.GetKeyDown(KeyCode.F))
        {
            UnitSpawn();
        }

        // H key - Move only units from THIS barracks
        if (Input.GetKeyDown(KeyCode.H))
        {
            MoveMyUnitsOnly();
        }

        // C key - Delete all units from all barracks
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearAllUnits();
        }

        // V key - Delete only units from this barracks
        if (Input.GetKeyDown(KeyCode.V))
        {
            ClearMyUnits();
        }
    }

    // Handle clicking on the ground to set new target location
    private void HandleMouseClick()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask))
        {
            Vector3 clickPosition = hit.point;

            // Update the endPoint position to the clicked location
            endPoint.position = clickPosition;

            Debug.Log($"Target moved to: {clickPosition}");

            // Automatically move all units from this barracks to the new target
            MoveMyUnitsToNewTarget(clickPosition);
        }
    }

    // Move all units from this barracks to a new target position
    private void MoveMyUnitsToNewTarget(Vector3 newTargetPosition)
    {
        myUnits.RemoveAll(unit => unit == null);

        GridNode targetNode = gridManager.GetNodeFromWorldPosition(newTargetPosition);
        if (targetNode != null)
        {
            foreach (UnitInstance unit in myUnits)
            {
                if (unit != null)
                {
                    unit.MoveTo(targetNode);
                    Debug.Log($"Moving unit {unit.name} to new clicked target {targetNode}");
                }
            }

            if (myUnits.Count > 0)
            {
                Debug.Log($"Moved {myUnits.Count} units to new clicked position");
            }
        }
        else
        {
            Debug.Log($"Couldn't find valid node at clicked position: {newTargetPosition}");
        }
    }

    // Move ALL units from ALL barracks to this barracks' target point
    public void UnitPositionTracker()
    {
        // Clean up destroyed units from both lists
        allUnits.RemoveAll(unit => unit == null);
        myUnits.RemoveAll(unit => unit == null);

        GridNode targetNode = gridManager.GetNodeFromWorldPosition(endPoint.position);
        if (targetNode != null)
        {
            // Move ALL units from ALL barracks to this target position
            foreach (UnitInstance unit in allUnits)
            {
                if (unit != null)
                {
                    unit.MoveTo(targetNode);
                    Debug.Log($"Moving unit {unit.name} to {targetNode}");
                }
            }

            if (allUnits.Count > 0)
            {
                Debug.Log($"Updated position for {allUnits.Count} total units to move to this barracks");
            }
            else
            {
                Debug.Log("No existing units to move");
            }
        }
        else
        {
            Debug.Log("Couldn't find target node");
        }
    }

    // Spawn a new unit and send it to the target location
    public void UnitSpawn()
    {
        GameObject newUnit = Instantiate(unitPrefab, spawnPoint.position, Quaternion.identity);
        UnitInstance unit = newUnit.GetComponent<UnitInstance>();
        unit.Initialize(astarPathfinding, unitType, gridManager, visualTargetPath);

        // Add the new unit to both tracking lists
        allUnits.Add(unit);
        myUnits.Add(unit);

        GridNode targetNode = gridManager.GetNodeFromWorldPosition(endPoint.position);
        if (targetNode != null)
        {
            unit.MoveTo(targetNode);
            Debug.Log($"Spawned new unit {newUnit.name} and moving to {targetNode}");
        }
        else
        {
            Debug.Log("Couldn't find node");
        }
    }

    // Move only the units that were spawned by this specific barracks
    public void MoveMyUnitsOnly()
    {
        myUnits.RemoveAll(unit => unit == null);

        GridNode targetNode = gridManager.GetNodeFromWorldPosition(endPoint.position);
        if (targetNode != null)
        {
            foreach (UnitInstance unit in myUnits)
            {
                if (unit != null)
                {
                    unit.MoveTo(targetNode);
                    Debug.Log($"Moving my unit {unit.name} to {targetNode}");
                }
            }

            Debug.Log($"Moved {myUnits.Count} units from this barracks");
        }
    }

    // Set a new target position from code
    public void SetTargetPosition(Vector3 newPosition)
    {
        endPoint.position = newPosition;
        MoveMyUnitsToNewTarget(newPosition);
    }

    // Enable or disable clicking to move units
    public void SetClickToMoveEnabled(bool enabled)
    {
        enableClickToMove = enabled;
        Debug.Log($"Click to move: {(enabled ? "Enabled" : "Disabled")}");
    }

    // Delete all units from every barracks in the game
    public static void ClearAllUnits()
    {
        foreach (UnitInstance unit in allUnits)
        {
            if (unit != null)
            {
                Destroy(unit.gameObject);
            }
        }
        allUnits.Clear();
        Debug.Log("Cleared all units from all barracks");
    }

    // Delete only the units that came from this specific barracks
    public void ClearMyUnits()
    {
        foreach (UnitInstance unit in myUnits)
        {
            if (unit != null)
            {
                // Remove from global list too
                allUnits.Remove(unit);
                Destroy(unit.gameObject);
            }
        }
        myUnits.Clear();
        Debug.Log("Cleared units from this barracks");
    }

    // Draw visual indicators in the scene view for debugging
    private void OnDrawGizmos()
    {
        if (endPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPoint.position, 0.5f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(endPoint.position, endPoint.position + Vector3.up * 2f);
        }
    }
}