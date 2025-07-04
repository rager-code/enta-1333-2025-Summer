using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    
    private void Start()
    {
        // Find GridManager if not assigned
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }
    }
    
    public List<GridNode> FindPath(GridManager gridManager, GridNode start, GridNode end, int unitWidth = 1, int unitHeight = 1)
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
    
    public List<GridNode> FindPath(GridNode start, GridNode end, int unitWidth = 1, int unitHeight = 1)
    {
        return FindPath(gridManager, start, end, unitWidth, unitHeight);
    }
    
    private void ResetAllNodes(GridManager gridManager)
    {
        foreach (GridNode node in gridManager.GetAllNodes())
        {
            node.GCost = int.MaxValue;
            node.HCost = 0;
            node.CameFromNode = null;
        }
    }
    
    private int CalculateHeuristic(GridNode nodeA, GridNode nodeB)
    {
        Vector2Int posA = WorldToGridPosition(nodeA.WorldPosition);
        Vector2Int posB = WorldToGridPosition(nodeB.WorldPosition);
        
        // Manhattan distance (good for 4-directional movement)
        int distanceX = Mathf.Abs(posA.x - posB.x);
        int distanceY = Mathf.Abs(posA.y - posB.y);
        
        return (distanceX + distanceY) * 10; // Multiply by 10 for better precision
    }
    
    private int GetMovementCost(GridNode from, GridNode to)
    {
        // Base movement cost is 10 (straight movement)
        int baseCost = 10;
        
        // Add terrain movement cost
        int terrainCost = to.MovementCost * 10;
        
        return baseCost + terrainCost;
    }
    
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
    
    // Debug method to visualize pathfinding costs
    public void DebugNodeCosts(GridNode node)
    {
        if (node != null)
        {
            Debug.Log($"Node: {node.Name} | GCost: {node.GCost} | HCost: {node.HCost} | FCost: {node.FCost}");
        }
    }
    
    // Method to get the total path cost
    public int GetPathCost(List<GridNode> path)
    {
        if (path == null || path.Count == 0)
            return 0;
        
        return path[path.Count - 1].GCost;
    }
}