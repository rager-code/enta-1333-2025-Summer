using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] public GridSettings gridSettings;
    [SerializeField] private TerrainType defaultTerrainType;
    [SerializeField] private List<TerrainType> terrainTypes = new();

    public GridNode[,] gridNodes;
    public List<GridNode> allNodes = new();

    public List<GridNode> GetAllNodes() => allNodes;
    public bool IsInitialized { get; private set; }


    
    public void InitializeGrid()
    {
        gridNodes = new GridNode[gridSettings.GridSizeX, gridSettings.GridSizeY];
        allNodes.Clear();

        

        for (int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridSettings.GridSizeY; y++)
            {
                Vector3 worldPos = gridSettings.UseXZPlane
                    ? new Vector3(x, 0, y) * gridSettings.NodeSize
                    : new Vector3(x, y, 0) * gridSettings.NodeSize;

                TerrainType chosenTerrain = terrainTypes[Random.Range(0, terrainTypes.Count)];

                GridNode node = new GridNode
                {
                    Name = $"Cell_{x}_{y}",
                    WorldPosition = worldPos,
                    terrainType = chosenTerrain,
                    Walkable = chosenTerrain.IsWalkable
                };
               
                gridNodes[x, y] = node;
                allNodes.Add(node);

            }
        }

        IsInitialized = true;
        
    }
   



    public GridNode GetNode(int x, int y)//new
    {
        return gridNodes[x, y];
    }//new

    private void OnDrawGizmos()
    {
        if (gridNodes == null || gridSettings == null) return;

        for (int x = 0; x < gridSettings.GridSizeX; x++)
        {
            for (int y = 0; y < gridSettings.GridSizeY; y++)
            {
                GridNode node = gridNodes[x, y];
                Gizmos.color = node.Walkable ? node.terrainType.GizmoColor : Color.red;
                Gizmos.DrawWireCube(node.WorldPosition, Vector3.one * gridSettings.NodeSize * 0.9f);
            }
        }
    }

    public GridNode GetNodeFromWorldPosition(Vector3 position)
    {

        // Determine which axes to use based on grid orientation.

        int x = gridSettings.UseXZPlane ? Mathf.RoundToInt(position.x / gridSettings.NodeSize): Mathf.RoundToInt(position.z / gridSettings.NodeSize);

        int y = gridSettings.UseXZPlane ? Mathf.RoundToInt(position.z / gridSettings.NodeSize): Mathf.RoundToInt(position.y / gridSettings.NodeSize);

        // Clamp coordinates to grid bounds.

        x = Mathf.Clamp(x, 0, gridSettings.GridSizeX - 1);

        y = Mathf.Clamp(y, 0, gridSettings.GridSizeY - 1);

        // Return the node at the clamped coordinates.

        return GetNode(x, y);

    }
    public GridNode GetNodeAtCoordinates(Vector3 coords)
    {
        foreach (GridNode node in GetAllNodes())
        {
            if (node.cords == coords)
            {
                return node;
            }
        }
        return null;
    }
    public List<GridNode> GetNeighbors(GridNode node)
    {
        List<GridNode> neighbors = new List<GridNode>();

        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(0, 1),   // Up
        new Vector2Int(1, 0),   // Right
        new Vector2Int(0, -1),  // Down
        new Vector2Int(-1, 0),  // Left
        };

        foreach (var dir in directions)
        {
            Vector3 neighborCoords = new Vector3(
                node.cords.x + dir.x,
                node.cords.y + dir.y,
                node.cords.z // Assuming Z stays same; if using 3D grid, add Z directions too
            );

            GridNode neighborNode = GetNodeAtCoordinates(neighborCoords);
            if (neighborNode != null)
            {
                neighbors.Add(neighborNode);
            }
        }

        return neighbors;
    }


}