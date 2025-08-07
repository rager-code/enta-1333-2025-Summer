using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComandPlayerUnits : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Camera mainCamera;               // Assign your main camera
    [SerializeField] private GridManager gridManager;         // Reference to GridManager
    [SerializeField] private UnitInstance controlledUnit;


    // This gets called every frame - we're checking for mouse clicks here
    private void Update()
    {
        // Check if the player clicked the left mouse button
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            // Cast a ray from the camera through the mouse position into the world
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            // See if the ray hits anything in the scene
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                // Figure out which grid node the player clicked on
                GridNode targetNode = gridManager.GetNodeFromWorldPosition(hitInfo.point);

                // Make sure the node exists and the unit can actually walk there
                if (targetNode != null && targetNode.walkable)
                {
                    // Tell our unit to move to that spot
                    controlledUnit.MoveTo(targetNode);
                }
            }
        }
    }
}