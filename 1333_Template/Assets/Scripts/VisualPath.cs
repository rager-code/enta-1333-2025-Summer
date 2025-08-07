using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualPath : MonoBehaviour
{
    [SerializeField] private AStarPathfinding pathfindingLogic; // reference to the A* logic (used only for ClearVisualization)

    private LineRenderer lineRenderer;

    // Gets called when the object first wakes up
    private void Awake()
    {
        // set up the line renderer used to draw path lines
        SetupLineRenderer();
    }

    // Wipes the visual stuff clean so we can start fresh
    public void ResetFeild()
    {
        // clear any line and gizmos previously drawn
        lineRenderer.positionCount = 0;
        //pathfindingLogic?.ClearVisualization();
    }

    // Takes a bunch of nodes and draws a line through them
    public void DrawPath(List<GridNode> path)
    {
        // validate the path before drawing
        if (lineRenderer == null || path == null || path.Count == 0) return;

        lineRenderer.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            // draw slightly above ground
            lineRenderer.SetPosition(i, path[i].WorldPosition + Vector3.up * 0.2f);
        }
    }

    // Does the boring setup work for the line renderer
    private void SetupLineRenderer()
    {
        // configure line renderer with basic settings
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.black;
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 0;
    }
}