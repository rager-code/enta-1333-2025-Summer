using System.Collections.Generic;
using UnityEngine;
using System;

public class AStarPathfinding : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;

    // Settings for checking when units reach their destination
    [Header("Path Completion Settings")]
    [SerializeField] private float destinationThreshold = 0.5f;

    // Keep track of units that are moving and when they finish
    private Dictionary<GameObject, PathTracker> activePathTrackers = new Dictionary<GameObject, PathTracker>();

    // Find the grid manager when we start up
    private void Start()
    {
        // Find GridManager if not assigned
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }
    }

    // Check every frame if any units have reached their destination
    private void Update()
    {
        // Check all active path trackers for completion
        CheckPathCompletions();
    }

    // Main pathfinding function with callback support for when the unit arrives
    public List<GridNode> FindPath(GridManager gridManager, GridNode start, GridNode end, int unitWidth = 1, int unitHeight = 1, GameObject unit = null, Action<GameObject> onPathComplete = null)
    {
        List<GridNode> path = FindPathInternal(gridManager, start, end, unitWidth, unitHeight);

        // If we want to know when the unit finishes moving, start tracking it
        if (onPathComplete != null && unit != null && path.Count > 0)
        {
            StartTrackingPath(unit, path, end, onPathComplete);
        }

        return path;
    }

    // Simpler version that uses the default grid manager
    public List<GridNode> FindPath(GridNode start, GridNode end, int unitWidth = 1, int unitHeight = 1, GameObject unit = null, Action<GameObject> onPathComplete = null)
    {
        return FindPath(gridManager, start, end, unitWidth, unitHeight, unit, onPathComplete);
    }

    // Basic pathfinding without callbacks (for backwards compatibility)
    public List<GridNode> FindPath(GridManager gridManager, GridNode start, GridNode end, int unitWidth = 1, int unitHeight = 1)
    {
        return FindPathInternal(gridManager, start, end, unitWidth, unitHeight);
    }

    // Another basic version using default grid manager
    public List<GridNode> FindPath(GridNode start, GridNode end, int unitWidth = 1, int unitHeight = 1)
    {
        return FindPath(gridManager, start, end, unitWidth, unitHeight);
    }

    // This is where the actual A* pathfinding magic happens
    private List<GridNode> FindPathInternal(GridManager gridManager, GridNode start, GridNode end, int unitWidth = 1, int unitHeight = 1)
    {
        if (start == null || end == null)
        {
            Debug.LogWarning("Start or end node is null!");
            return new List<GridNode>();
        }

        // Reset all nodes before pathfinding
        ResetAllNodes(gridManager);

        List<GridNode> openSet = new() { start };
        HashSet<GridNode> closedSet = new();

        // Initialize start node
        start.GCost = 0;
        start.HCost = CalculateHeuristic(start, end);
        start.CameFromNode = null;

        while (openSet.Count > 0)
        {
            GridNode current = GetLowestFCostNode(openSet);

            if (current == end)
            {
                return ReconstructPath(current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (GridNode neighbor in gridManager.GetNeighborNodes(current))
            {
                if (!IsNodeWalkable(neighbor, unitWidth, unitHeight) || closedSet.Contains(neighbor))
                    continue;

                // Calculate tentative G cost (movement cost from start to neighbor through current)
                int tentativeGCost = current.GCost + GetMovementCost(current, neighbor);

                // If this path to neighbor is better than any previous one
                if (tentativeGCost < neighbor.GCost || !openSet.Contains(neighbor))
                {
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = CalculateHeuristic(neighbor, end);
                    neighbor.CameFromNode = current;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        Debug.LogWarning("A* failed: no path found from " + start.Name + " to " + end.Name);
        return new List<GridNode>();
    }

    // Start watching a unit to see when it reaches its destination
    private void StartTrackingPath(GameObject unit, List<GridNode> path, GridNode destination, Action<GameObject> onComplete)
    {
        if (activePathTrackers.ContainsKey(unit))
        {
            // Update existing tracker
            activePathTrackers[unit] = new PathTracker(path, destination, onComplete);
        }
        else
        {
            // Add new tracker
            activePathTrackers.Add(unit, new PathTracker(path, destination, onComplete));
        }

        Debug.Log($"Started tracking path for {unit.name} to {destination.Name}");
    }

    // Check if any units have reached their destinations
    private void CheckPathCompletions()
    {
        List<GameObject> completedPaths = new List<GameObject>();

        foreach (var kvp in activePathTrackers)
        {
            GameObject unit = kvp.Key;
            PathTracker tracker = kvp.Value;

            // Skip if unit was destroyed
            if (unit == null)
            {
                completedPaths.Add(kvp.Key);
                continue;
            }

            // Check if unit has reached destination
            float distanceToDestination = Vector3.Distance(unit.transform.position, tracker.destination.WorldPosition);

            if (distanceToDestination <= destinationThreshold)
            {
                Debug.Log($"Unit {unit.name} reached destination {tracker.destination.Name}");

                // Trigger callback
                tracker.onComplete?.Invoke(unit);

                // Mark for removal
                completedPaths.Add(unit);
            }
        }

        // Remove completed paths
        foreach (GameObject unit in completedPaths)
        {
            activePathTrackers.Remove(unit);
        }
    }

    // Stop watching a specific unit (useful if you cancel their movement)
    public void StopTrackingUnit(GameObject unit)
    {
        if (activePathTrackers.ContainsKey(unit))
        {
            activePathTrackers.Remove(unit);
            Debug.Log($"Stopped tracking {unit.name}");
        }
    }

    // Check if we're currently watching a unit
    public bool IsTrackingUnit(GameObject unit)
    {
        return activePathTrackers.ContainsKey(unit);
    }

    // Get how many units we're currently tracking
    public int GetTrackedUnitsCount()
    {
        return activePathTrackers.Count;
    }

    // Change how close a unit needs to be to count as "arrived"
    public void SetDestinationThreshold(float threshold)
    {
        destinationThreshold = threshold;
    }

    // Clean up all the pathfinding data before starting a new search
    private void ResetAllNodes(GridManager gridManager)
    {
        foreach (GridNode node in gridManager.GetAllNodes())
        {
            node.GCost = int.MaxValue;
            node.HCost = 0;
            node.CameFromNode = null;
        }
    }

    // Calculate the estimated distance from one node to another (straight line)
    private int CalculateHeuristic(GridNode nodeA, GridNode nodeB)
    {
        Vector2Int posA = WorldToGridPosition(nodeA.WorldPosition);
        Vector2Int posB = WorldToGridPosition(nodeB.WorldPosition);

        // Manhattan distance (good for 4-directional movement)
        int distanceX = Mathf.Abs(posA.x - posB.x);
        int distanceY = Mathf.Abs(posA.y - posB.y);

        return (distanceX + distanceY) * 10; // Multiply by 10 for better precision
    }

    // Figure out how much it costs to move from one node to another
    private int GetMovementCost(GridNode from, GridNode to)
    {
        // Base movement cost is 10 (straight movement)
        int baseCost = 10;

        // Add terrain movement cost
        int terrainCost = to.MovementCost * 10;

        return baseCost + terrainCost;
    }

    // Check if a unit can actually walk on this node (considering unit size)
    private bool IsNodeWalkable(GridNode node, int unitWidth, int unitHeight)
    {
        if (node == null || !node.walkable)
            return false;

        // For units larger than 1x1, check surrounding nodes
        if (unitWidth > 1 || unitHeight > 1)
        {
            return CheckAreaWalkable(node, unitWidth, unitHeight);
        }

        return true;
    }

    // Check if there's enough space for a bigger unit to fit
    private bool CheckAreaWalkable(GridNode centerNode, int width, int height)
    {
        Vector2Int centerPos = WorldToGridPosition(centerNode.WorldPosition);

        // Check all nodes in the unit's footprint
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int checkPos = new Vector2Int(
                    centerPos.x + x - width / 2,
                    centerPos.y + y - height / 2
                );

                GridNode nodeToCheck = gridManager.GetNode(checkPos);
                if (nodeToCheck == null || !nodeToCheck.walkable)
                {
                    return false;
                }
            }
        }

        return true;
    }

    // Convert world coordinates to grid coordinates
    private Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        if (gridManager == null)
        {
            // Fallback to default grid size
            int x = Mathf.RoundToInt(worldPos.x);
            int y = Mathf.RoundToInt(worldPos.z);
            return new Vector2Int(x, y);
        }

        float nodeSize = gridManager.GridSettings.NodeSize;
        int x_grid = Mathf.RoundToInt(worldPos.x / nodeSize);
        int y_grid = Mathf.RoundToInt(worldPos.z / nodeSize);

        return new Vector2Int(x_grid, y_grid);
    }

    // Find the node with the lowest F cost (best option to explore next)
    private GridNode GetLowestFCostNode(List<GridNode> nodes)
    {
        GridNode bestNode = nodes[0];

        foreach (GridNode node in nodes)
        {
            if (node.FCost < bestNode.FCost ||
                (node.FCost == bestNode.FCost && node.HCost < bestNode.HCost))
            {
                bestNode = node;
            }
        }

        return bestNode;
    }

    // Build the final path by working backwards from the destination
    private List<GridNode> ReconstructPath(GridNode endNode)
    {
        List<GridNode> path = new List<GridNode>();
        GridNode currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(currentNode);
            currentNode = currentNode.CameFromNode;
        }

        path.Reverse();
        return path;
    }

    // Helper for debugging - shows the costs for a specific node
    public void DebugNodeCosts(GridNode node)
    {
        if (node != null)
        {
            Debug.Log($"Node: {node.Name} | GCost: {node.GCost} | HCost: {node.HCost} | FCost: {node.FCost}");
        }
    }

    // Get the total movement cost for a complete path
    public int GetPathCost(List<GridNode> path)
    {
        if (path == null || path.Count == 0)
            return 0;

        return path[path.Count - 1].GCost;
    }

    // Helper class to keep track of units and their destinations
    [System.Serializable]
    private class PathTracker
    {
        public List<GridNode> path;
        public GridNode destination;
        public Action<GameObject> onComplete;

        public PathTracker(List<GridNode> path, GridNode destination, Action<GameObject> onComplete)
        {
            this.path = path;
            this.destination = destination;
            this.onComplete = onComplete;
        }
    }
}