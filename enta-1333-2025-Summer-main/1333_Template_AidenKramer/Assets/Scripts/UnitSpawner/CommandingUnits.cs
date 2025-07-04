using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandingUnits : MonoBehaviour
{
    /*
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private CommandTargetPath commandTargetPath;

    [Header("Selection Visual")]
    [SerializeField] private GameObject selectionIndicatorPrefab; // Optional visual indicator
    [SerializeField] private Color selectedUnitColor = Color.yellow;
    [SerializeField] private Color normalUnitColor = Color.white;

    private GameObject selectedUnit;
    private GameObject selectionIndicator;
    private Renderer selectedUnitRenderer;
    private Color originalUnitColor;

    private void Awake()
    {
        // Get main camera if not assigned
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Find components if not assigned
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        if (commandTargetPath == null)
            commandTargetPath = FindObjectOfType<CommandTargetPath>();
    }

    private void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        // Left mouse button - Select unit or move selected unit
        if (Input.GetMouseButtonDown(0))
        {
            HandleLeftClick();
        }

        // Right mouse button - Deselect unit
        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
    }

    private void HandleLeftClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // First check if we hit a unit
            GameObject hitObject = hit.collider.gameObject;

            // Check if the hit object is a unit (has Player_Targeting or UnitIdentifier component)
            if (IsUnit(hitObject))
            {
                SelectUnit(hitObject);
            }
            else if (selectedUnit != null)
            {
                // We have a selected unit and clicked on terrain - try to move
                GridNode targetNode = gridManager.GetNodeFromWorldPosition(hit.point);

                if (targetNode != null && targetNode.walkable)
                {
                    MoveSelectedUnit(targetNode);
                }
                else
                {
                    Debug.Log("Cannot move to that location - not walkable terrain");
                }
            }
        }
    }

    private void HandleRightClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject hitObject = hit.collider.gameObject;

            // If we right-clicked on the same selected unit, deselect it
            if (selectedUnit != null && hitObject == selectedUnit)
            {
                DeselectUnit();
                Debug.Log("UnitDeselected");
            }
        }
        else
        {
            // Right-clicked on empty space - deselect current unit
            if (selectedUnit != null)
            {
                DeselectUnit();
                Debug.Log("UnitDeselected");
            }
        }
    }

    private bool IsUnit(GameObject obj)
    {
        // Check if the object has components that identify it as a unit
        return obj.GetComponent<Player_Targeting>() != null ||
               obj.GetComponent<UnitIdentifier>() != null ||
               obj.CompareTag("Unit"); // Optional: use tags
    }

    private void SelectUnit(GameObject unit)
    {
        // If we're selecting the same unit, do nothing
        if (selectedUnit == unit)
        {
            return;
        }

        // Deselect current unit first
        if (selectedUnit != null)
        {
            DeselectCurrentUnit();
        }

        // Select new unit
        selectedUnit = unit;

        // Store renderer and original color
        selectedUnitRenderer = selectedUnit.GetComponent<Renderer>();
        if (selectedUnitRenderer != null)
        {
            originalUnitColor = selectedUnitRenderer.material.color;
            selectedUnitRenderer.material.color = selectedUnitColor;
        }

        // Show selection indicator
        ShowSelectionIndicator();

        Debug.Log($"Selected unit: {selectedUnit.name}");
    }

    private void DeselectUnit()
    {
        if (selectedUnit == null) return;

        DeselectCurrentUnit();
        selectedUnit = null;

        Debug.Log("Unit deselected");
    }

    private void DeselectCurrentUnit()
    {
        if (selectedUnit == null) return;

        // Restore original color
        if (selectedUnitRenderer != null)
        {
            selectedUnitRenderer.material.color = originalUnitColor;
        }

        // Hide selection indicator
        HideSelectionIndicator();

        // Clear references
        selectedUnitRenderer = null;
    }

    private void ShowSelectionIndicator()
    {
        if (selectedUnit == null) return;

        // Remove previous indicator
        HideSelectionIndicator();

        // Create new selection indicator if prefab is assigned
        if (selectionIndicatorPrefab != null)
        {
            selectionIndicator = Instantiate(selectionIndicatorPrefab,
                selectedUnit.transform.position,
                Quaternion.identity);

            // Parent it to the unit so it follows
            selectionIndicator.transform.SetParent(selectedUnit.transform);
        }
    }

    private void HideSelectionIndicator()
    {
        if (selectionIndicator != null)
        {
            Destroy(selectionIndicator);
            selectionIndicator = null;
        }
    }

    private void MoveSelectedUnit(GridNode targetNode)
    {
        if (selectedUnit == null) return;

        // Get the unit's movement component
        Player_Targeting unitMovement = selectedUnit.GetComponent<Player_Targeting>();

        if (unitMovement != null)
        {
            // Stop current movement
            unitMovement.StopMoving();

            // Use CommandTargetPath to generate and execute the path
            if (commandTargetPath != null)
            {
                StartCoroutine(commandTargetPath.GeneratePathTo(targetNode));
            }
            else
            {
                Debug.LogWarning("CommandTargetPath not found - cannot move unit");
            }

            Debug.Log($"Moving {selectedUnit.name} to {targetNode.WorldPosition}");
        }
        else
        {
            Debug.LogWarning($"Selected unit {selectedUnit.name} does not have Player_Targeting component");
        }
    }

    // Public methods for external access
    public GameObject GetSelectedUnit()
    {
        return selectedUnit;
    }

    public bool HasSelectedUnit()
    {
        return selectedUnit != null;
    }

    public void ClearSelection()
    {
        DeselectUnit();
    }

    // Method to handle unit destruction/removal
    public void OnUnitDestroyed(GameObject unit)
    {
        if (selectedUnit == unit)
        {
            DeselectUnit();
        }
    }
    */
}