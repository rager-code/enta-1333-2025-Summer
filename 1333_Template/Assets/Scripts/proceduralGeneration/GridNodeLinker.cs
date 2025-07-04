using UnityEngine;

/// <summary>
/// Component that links instantiated terrain prefabs back to their corresponding GridNode
/// This allows you to access the GridNode data from the visual representation
/// </summary>
public class GridNodeLinker : MonoBehaviour
{
    [SerializeField] private GridNode linkedNode;

    public GridNode LinkedNode
    {
        get => linkedNode;
        set => linkedNode = value;
    }

    /// <summary>
    /// Get the terrain type of the linked node
    /// </summary>
    public TerrainType GetTerrainType()
    {
        return linkedNode?.terrainType;
    }

    /// <summary>
    /// Check if the linked node is walkable
    /// </summary>
    public bool IsWalkable()
    {
        return linkedNode?.walkable ?? false;
    }

    /// <summary>
    /// Get the movement cost for the linked node
    /// </summary>
    public int GetMovementCost()
    {
        return linkedNode?.MovementCost ?? 1;
    }

    /// <summary>
    /// Get the world position of the linked node
    /// </summary>
    public Vector3 GetNodeWorldPosition()
    {
        return linkedNode?.WorldPosition ?? Vector3.zero;
    }

    /// <summary>
    /// Check if this prefab has a valid linked node
    /// </summary>
    public bool HasLinkedNode()
    {
        return linkedNode != null;
    }

    private void OnValidate()
    {
        // Update the name to reflect the linked node
        if (linkedNode != null)
        {
            gameObject.name = $"{linkedNode.terrainType?.TerrainName ?? "Unknown"}_{linkedNode.Name}";
        }
    }
    /*
    private void OnDrawGizmosSelected()
    {
        if (linkedNode != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(linkedNode.WorldPosition, Vector3.one * 0.5f);

            // Draw a line connecting the prefab to the node position
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, linkedNode.WorldPosition);
        }
    }
    */
}