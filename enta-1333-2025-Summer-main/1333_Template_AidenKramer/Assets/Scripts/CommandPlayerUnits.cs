using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComandPlayerUnits : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Camera mainCamera;               // Assign your main camera
    [SerializeField] private GridManager gridManager;         // Reference to GridManager
    [SerializeField] private UnitInstance controlledUnit;
    

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                // Get the clicked node
                GridNode targetNode = gridManager.GetNodeFromWorldPosition(hitInfo.point);

                if (targetNode != null && targetNode.walkable)
                {
                    // Move the unit to this node
                    controlledUnit.MoveTo(targetNode);
                }
            }
        }
    }
}
