using UnityEngine;


public class GridNodeLinker : MonoBehaviour
{
    [SerializeField] private GridNode linkedNode;

    // Simple getter/setter to access the linked grid node
    public GridNode LinkedNode
    {
        get => linkedNode;
        set => linkedNode = value;
    }

    // Get what type of terrain this node represents
    public TerrainType GetTerrainType()
    {
        return linkedNode?.terrainType;
    }

    // Check if units can walk on this terrain
    public bool IsWalkable()
    {
        return linkedNode?.walkable ?? false;
    }

    // Get how much it costs to move through this terrain
    public int GetMovementCost()
    {
        return linkedNode?.MovementCost ?? 1;
    }

    // Get the exact world position of this grid node
    public Vector3 GetNodeWorldPosition()
    {
        return linkedNode?.WorldPosition ?? Vector3.zero;
    }

    // Check if this component actually has a node connected to it
    public bool HasLinkedNode()
    {
        return linkedNode != null;
    }

    // Automatically update the object name to match the terrain type (helpful for debugging)
    private void OnValidate()
    {
        // Update the name to reflect the linked node
        if (linkedNode != null)
        {
            gameObject.name = $"{linkedNode.terrainType?.TerrainName ?? "Unknown"}_{linkedNode.Name}";
        }
    }

   
}