using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class BarracksSpawner : MonoBehaviour
{
    // Start is called before the first frame update
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

    // Static list to track ALL units from ALL barracks
    private static List<UnitInstance> allUnits = new List<UnitInstance>();
    // List to track units spawned by THIS specific barracks
    private List<UnitInstance> myUnits = new List<UnitInstance>();

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

    private void Update()
    {
        // Handle mouse click for moving target
        if (enableClickToMove && Input.GetMouseButtonDown(0)) // Left mouse button
        {
            HandleMouseClick();
        }

        // Keyboard controls
        if (Input.GetKeyDown(KeyCode.G))
        {
            UnitPositionTracker(); // Moves ALL units to this barracks
           
              
            
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            UnitSpawn();

        }

        // Move only units from THIS barracks
        if (Input.GetKeyDown(KeyCode.H))
        {
            MoveMyUnitsOnly();
        }

        // Optional: Clear all units from ALL barracks with C key
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearAllUnits();
        }

        // Optional: Clear only units from THIS barracks with V key
        if (Input.GetKeyDown(KeyCode.V))
        {
            ClearMyUnits();
        }
        
    }
   

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

    // Method to move only units spawned by THIS barracks
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

    // Public method to set target position programmatically
    public void SetTargetPosition(Vector3 newPosition)
    {
        endPoint.position = newPosition;
        MoveMyUnitsToNewTarget(newPosition);
    }

    // Method to toggle click-to-move functionality
    public void SetClickToMoveEnabled(bool enabled)
    {
        enableClickToMove = enabled;
        Debug.Log($"Click to move: {(enabled ? "Enabled" : "Disabled")}");
    }

   
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

    // Clear units spawned by THIS barracks only
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

    // Visual debug helper
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