using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandTargetPath : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private AStarPathfinding pathfindingLogic;
    [SerializeField] private GameObject startPosPrefab;
    [SerializeField] private GameObject endPosPrefab;


    [Header("Tuning")]
    [SerializeField] private float searchDelay = 0.1f;

    private LineRenderer lineRenderer;
    private List<GridNode> pathNodes = new();
    private GameObject startInstance;
    private GameObject endInstance;
    private GridNode startNode;
    private GridNode endNode;
    private Camera mainCamera;
    private Coroutine pathRoutine;

    // Sets up the line renderer and grabs the main camera
    private void Awake()
    {
        SetupLineRenderer();
        mainCamera = Camera.main;
    }

    // Checks for mouse clicks every frame
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TrySetNewTargetFromClick();
    }

    // Figures out what grid square the player clicked on
    public void TrySetNewTargetFromClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GridNode clickedNode = gridManager.GetNodeFromWorldPosition(hit.point);
            if (clickedNode != null && clickedNode.walkable)
            {
                if (pathRoutine != null)
                    StopCoroutine(pathRoutine);

                //if (movingAgent.TryGetComponent(out Player_Targeting mover))//ToDo make moving agent a parameter 
                //mover.StopMoving();

                //pathRoutine = StartCoroutine(GeneratePathTo(clickedNode));
            }
        }
    }

    // Creates a path from a unit to the target and makes the unit start moving
    public IEnumerator GeneratePathTo(GridNode targetNode, UnitInstance movingAgent)
    {
        if (endInstance != null) Destroy(endInstance);
        endNode = targetNode;
        endInstance = Instantiate(endPosPrefab, endNode.WorldPosition, Quaternion.identity);

        startNode = gridManager.GetNodeFromWorldPosition(movingAgent.transform.position);//Make a parameter to make a comand target path 

        if (startInstance == null)
            startInstance = Instantiate(startPosPrefab, startNode.WorldPosition, Quaternion.identity);
        else
            startInstance.transform.position = startNode.WorldPosition;

        pathNodes = pathfindingLogic.FindPath(gridManager, startNode, endNode, 1, 1);

        foreach (GridNode node in pathNodes)
            yield return new WaitForSeconds(searchDelay);

        if (pathNodes.Count > 0)
        {
            DrawPath(pathNodes);


            if (movingAgent.TryGetComponent(out Player_Targeting mover))//Make a parameter to make a comand target path 
                mover.StartMoving(pathNodes);
        }
        else
        {
            Debug.LogWarning("No valid path to clicked location.");
        }

        pathRoutine = null;
    }

    // Draws the path as a colored line in the world
    private void DrawPath(List<GridNode> path)
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
            lineRenderer.SetPosition(i, path[i].WorldPosition + Vector3.up * 0.2f);
    }

    // Sets up the line renderer with colors and settings
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

    // Clears everything - paths, markers, and stops any movement
    public void ResetField()
    {
        if (pathRoutine != null) StopCoroutine(pathRoutine);
        pathRoutine = null;

        if (startInstance != null) Destroy(startInstance);
        if (endInstance != null) Destroy(endInstance);

        if (lineRenderer != null) lineRenderer.positionCount = 0;

        
    }
}