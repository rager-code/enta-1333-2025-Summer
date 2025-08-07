using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualTargetPath : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private AStarPathfinding pathfindingLogic;
    [SerializeField] private GameObject startPosPrefab;
    [SerializeField] private GameObject endPosPrefab;
    [SerializeField] private GameObject movingAgent;

    [Header("Tuning")]
    [SerializeField] private float searchDelay = 0.01f;

    private LineRenderer lineRenderer;
    private List<GridNode> pathNodes = new();
    private GameObject startInstance;
    private GameObject endInstance;
    private GridNode startNode;
    private GridNode endNode;
    private Camera mainCamera;
    private Coroutine pathRoutine;

    // Get everything ready when the game starts
    private void Awake()
    {
        SetupLineRenderer();
        mainCamera = Camera.main;
    }
    // Check for mouse clicks every frame
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TrySetNewTargetFromClick();
    }
    // Figure out what the player clicked on and see if we can move there
    private void TrySetNewTargetFromClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("gridmanager:" + gridManager);
            

            GridNode clickedNode = gridManager.GetNodeFromWorldPosition(hit.point);
            if (clickedNode != null && clickedNode.walkable)
            {
                if (pathRoutine != null)
                    StopCoroutine(pathRoutine);
               
            }
        }
    }
    // Create a path from the agent to the target and show it visually
    public IEnumerator GeneratePathTo(GridNode targetNode, UnitBase movingAgent)
    {
        // Clean up old target marker and place a new one
        if (endInstance != null) Destroy(endInstance);
        endNode = targetNode;
        endInstance = Instantiate(endPosPrefab, endNode.WorldPosition, Quaternion.identity);

        // Get where we're starting from
        startNode = gridManager.GetNodeFromWorldPosition(movingAgent.transform.position);

        // Place or move the start marker
        if (startInstance == null)
            startInstance = Instantiate(startPosPrefab, startNode.WorldPosition, Quaternion.identity);
        else
            startInstance.transform.position = startNode.WorldPosition;
        // Actually calculate the path using A*
        pathNodes = pathfindingLogic.FindPath(gridManager, startNode, endNode, 1, 1);

        // Add a tiny delay for each node
        foreach (GridNode node in pathNodes)
            yield return new WaitForSeconds(searchDelay);

        // If we found a valid path, draw it and start moving
        if (pathNodes.Count > 0)
        {
            DrawPath(pathNodes);

            if (movingAgent.TryGetComponent(out Player_Targeting mover))
                mover.StartMoving(pathNodes);
        }
        else
        {
            Debug.LogWarning("No valid path to clicked location.");
        }

        pathRoutine = null;
    }
    // Draw the path as a line in the world
    public void DrawPath(List<GridNode> path)
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
            lineRenderer.SetPosition(i, path[i].WorldPosition + Vector3.up * 0.2f);
    }
    // Set up the line renderer with some nice colors and settings
    private void SetupLineRenderer()
    {
        lineRenderer ??= gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.magenta;
        lineRenderer.endColor = Color.black;
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 0;
    }
    // Clean up everything when we need to reset
    public void ResetField() 
    {
        if (pathRoutine != null) StopCoroutine(pathRoutine);
        pathRoutine = null;

        if (startInstance != null) Destroy(startInstance);
        if (endInstance != null) Destroy(endInstance);

        if (lineRenderer != null) lineRenderer.positionCount = 0;

       
    }
}